# OSListUtilsServerSide — Architecture

Structural reference for the solution, the runtime component, and the test
projects. Keep this file in sync with the code — update it whenever the
solution layout, Server Action surface, dependency set, or project structure
changes.

> **Runtime behaviour** and **Server Action signatures** live in
> [README.md](./README.md) and the [docs/platform/](./docs/platform) Forge
> copies. This document describes **structure only** — how the code is
> organised, why it is split the way it is, and where each concern lives.

---

## 1. Solution layout

```
OSListUtilsServerSide/                           ← repo root
├── ListUtils.sln                                ← solution (loads all four .csproj files)
├── ListUtils/                                   ← ODC External Library (net10.0)
├── ListUtils.Tests/                             ← ODC xUnit suite (net10.0)
├── ListUtils.O11/                               ← O11 Integration Studio extension (net48)
├── ListUtils.O11.Tests/                         ← O11 xUnit suite (net48)
└── docs/                                        ← README/Forge copies, versioned changelogs
```

---

## 2. ODC External Library — `ListUtils/`

Target framework: **`net10.0`**. Namespace: `ListUtils`.
Primary class: `ListUtils`, implementing `IListUtils`.

### Public surface

| File | Type | Purpose |
|------|------|---------|
| [IListUtils.cs](ListUtils/IListUtils.cs) | `[OSInterface]` | Declares 28 actions: 14 base actions (`List_Pop`, `List_PopMultiple`, `List_PopByCondition`, `List_PopMultipleByCondition`, `List_PopByConditions`, `List_PopMultipleByConditions`, `List_Zip`, `List_GroupBy`, `List_Difference`, `List_Chunk`, `List_DistinctBy`, `List_Slice`, `List_Shuffle`, `List_UpdateAt`) + 14 `*InPlace` variants that use `ref string` on the primary list parameter (mapped to OutSystems Input/Output) |

No `[OSStructure]` types — every exposed parameter is a primitive (`string`, `int`, `bool`). All list data is passed as JSON strings (`Text`), which keeps the interface generic across any consumer Structure.

### Implementation files (partial class)

`ListUtils` is a `public partial class` split across seven files by action group. The shell holds the class declaration + `IListUtils` implementation marker; each partial contributes one concern.

| File | Responsibility |
|------|---------------|
| [ListUtils.cs](ListUtils/ListUtils.cs) | Partial-class shell — declares `public partial class ListUtils : IListUtils`, no members |
| [ListUtils.Index.cs](ListUtils/ListUtils.Index.cs) | Index-based pops: `List_Pop`, `List_PopMultiple` |
| [ListUtils.Condition.cs](ListUtils/ListUtils.Condition.cs) | Condition-based actions: `List_PopByCondition`, `List_PopMultipleByCondition`, `List_PopByConditions`, `List_PopMultipleByConditions` |
| [ListUtils.Relational.cs](ListUtils/ListUtils.Relational.cs) | Relational / set actions: `List_Zip`, `List_GroupBy`, `List_Difference` (including all fast-path branches) |
| [ListUtils.Transform.cs](ListUtils/ListUtils.Transform.cs) | Transformation / randomization actions: `List_Chunk`, `List_DistinctBy`, `List_Slice`, `List_Shuffle`, `List_UpdateAt` (plus private helpers for slice normalization, CSPRNG-vs-seeded shuffle, and nested-path write-back with object-only auto-creation) |
| [ListUtils.InPlace.cs](ListUtils/ListUtils.InPlace.cs) | The 14 `*InPlace` variants. Each is a thin delegate: call the base action, then assign its output list back to the `ref` parameter. Secondary outputs (`PoppedElementJson`, `PoppedElementsJson`, `PreviousValueJson`) flow through unchanged. |
| [ListUtils.Helpers.cs](ListUtils/ListUtils.Helpers.cs) | `GetPropertyValue` + `NavigateSegment` path walker, `MatchesCondition` operator evaluator, `ParseConditions` + `EvaluateConditions` multi-condition engine, `TryCompareNumeric` numeric comparator, `ToCamelCase` fallback, nested `Condition` type, shared `JsonSerializerOptions` |

### Runtime dependencies

Declared in [ListUtils.csproj](ListUtils/ListUtils.csproj).

| Package | License | Purpose |
|---------|---------|---------|
| `OutSystems.ExternalLibraries.SDK` | OutSystems proprietary | `[OSInterface]`, `[OSAction]`, `[OSParameter]` attributes |

`System.Text.Json` is used but ships as part of the net10.0 BCL — no NuGet required. `System.Security.Cryptography.RandomNumberGenerator` (used by `List_Shuffle` when `Seed == 0`) also ships with the BCL.

### ODC processing flow

```mermaid
flowchart LR
    In[inputs] --> Type{Action type?}

    Type -->|List_Pop / List_PopMultiple| IndexOp["JsonNode.Parse → JsonArray<br/>RemoveAt by index<br/>(reverse-sorted for PopMultiple)"]

    Type -->|PopByCondition /<br/>PopMultipleByCondition| ParseSingle[JsonNode.Parse → JsonArray]
    ParseSingle --> SingleScan{Search from end?}
    SingleScan -->|"false / N/A for Multiple"| ScanFwd[Forward scan i = 0..N]
    SingleScan -->|"true (Pop only)"| ScanBwd[Backward scan i = N-1..0]
    ScanFwd --> Eval1["For each item:<br/>GetPropertyValue(path) via NavigateSegment<br/>then MatchesCondition(value, target, op, caseSensitive)"]
    ScanBwd --> Eval1

    Type -->|PopByConditions /<br/>PopMultipleByConditions| ParseMulti["ParseConditions(conditionsJson)<br/>→ List&lt;Condition&gt;<br/>+ JsonNode.Parse → JsonArray"]
    ParseMulti --> MultiScan{Search from end?}
    MultiScan -->|"false / N/A for Multiple"| ScanFwdM[Forward scan]
    MultiScan -->|"true (Pop only)"| ScanBwdM[Backward scan]
    ScanFwdM --> Eval2["EvaluateConditions(item, conditions, AND/OR)<br/>AND: all must match<br/>OR: any must match"]
    ScanBwdM --> Eval2

    Type -->|List_Zip| Zip[Parse both arrays<br/>→ index-paired JsonObjects<br/>truncate to shorter]

    Type -->|List_GroupBy| GroupBy["Parse source array<br/>→ Dictionary&lt;string, JsonArray&gt;<br/>keyed by GetPropertyValue(path)<br/>preserves first-seen order"]

    Type -->|List_Difference| Diff["Parse both arrays<br/>Build B-value list once<br/>Equals / NotEquals: HashSet lookup - O(A+B)<br/>StartsWith / EndsWith: HashSet + prefix/suffix scan - O(A·L)<br/>Numeric ops: precompute min(B) / max(B) - O(A+B)<br/>Contains: linear scan via MatchesCondition - O(A*B)"]

    Type -->|List_Chunk| Chunk["Parse source array<br/>Slide window of ChunkSize<br/>ChunkSize &lt;= 0 or empty source returns '[]'"]

    Type -->|List_DistinctBy| Distinct["Parse source array<br/>Key = GetPropertyValue(PropertyName) or full-item JSON<br/>HashSet dedupe (Ordinal / OrdinalIgnoreCase)<br/>Missing keys share one 'null-key' bucket<br/>First occurrence wins"]

    Type -->|List_Slice| Slice["Parse source array<br/>Normalize Start/End (negative counts from end, clamped)<br/>End == 0 sentinel: forward = end of list, backward = past beginning<br/>Step == 0 treated as 1; negative Step reverses"]

    Type -->|List_Shuffle| Shuffle["Parse source array (DeepClone, source not mutated)<br/>Fisher-Yates in-place<br/>Seed == 0: RandomNumberGenerator per swap (CSPRNG)<br/>Seed != 0: System.Random(Seed) for reproducibility"]

    Type -->|List_UpdateAt| UpdateAt["Parse source array<br/>Normalize Index (negative counts from end)<br/>Walk PropertyName path; auto-create missing objects<br/>(arrays are NOT auto-created \u2014 missing/non-array short-circuits)<br/>Parse NewValueJson; fallback to raw string on JsonException<br/>Emit PreviousValueJson = 'null' when index OOB, empty PropertyName,<br/>non-object item, missing property, or existing JSON null"]

    Eval1 --> Serialize["ToJsonString(JsonOptions)"]
    Eval2 --> Serialize
    Zip --> Serialize
    GroupBy --> Serialize
    Diff --> Serialize
    IndexOp --> Serialize
    Chunk --> Serialize
    Distinct --> Serialize
    Slice --> Serialize
    Shuffle --> Serialize
    UpdateAt --> Serialize

    Serialize --> Out[Output parameters]

    subgraph PathNav[GetPropertyValue / NavigateSegment]
        direction TB
        Path["Split path on '.'"] --> Seg["Per segment:<br/>parse 'Name[index]' or 'Name'"]
        Seg --> Prop["TryGetPropertyValue(name)<br/>fallback camelCase"]
        Prop --> Idx{"Has [index]?"}
        Idx -->|yes| Arr["Array indexing<br/>(negative counts from end)"]
        Idx -->|no| Next[Continue to next segment]
    end

    subgraph OpMatch[MatchesCondition]
        direction TB
        Op[Normalize operator] --> Cmp{Operator type?}
        Cmp -->|Equals / NotEquals| StrEq[String cmp<br/>Ordinal / OrdinalIgnoreCase]
        Cmp -->|Contains / StartsWith / EndsWith| StrPat[String match<br/>per caseSensitive]
        Cmp -->|GreaterThan / LessThan / GreaterOrEqual / LessOrEqual| Num[TryCompareNumeric<br/>decimal / InvariantCulture]
    end
```

Helper flow (used by all condition-based and grouping actions):

- **`GetPropertyValue(node, path)`** — splits `path` on `.`, then for each segment calls `NavigateSegment`. Each segment can be `Name` or `Name[index]`. Falls back to camelCase at every step. Returns `null` if any hop fails.
- **`MatchesCondition(actual, target, op, caseSensitive)`** — case-insensitive by default. Numeric operators parse with `InvariantCulture`; non-numeric input returns `false`.
- **`ParseConditions(json)`** — reads `[{path, operator, value, caseSensitive?}]` array. Missing fields default to empty strings / `false`.
- **`EvaluateConditions(item, conditions, logicalOp)`** — AND short-circuits on first miss; OR short-circuits on first hit.

### Build & package

`ListUtils/generate_upload_package.ps1` runs
`dotnet publish -c Release -r linux-x64 --self-contained false` and zips the
publish folder into `ExternalLibrary.zip`. The 90 MB ODC upload ceiling is
enforced by the script.

---

## 3. O11 Integration Studio Extension — `ListUtils.O11/`

Target framework: **`net48`**, `LangVersion=10`. Namespace: `OutSystems.NssListUtils`.

### Public surface (Integration Studio–generated names)

| File | Type | Purpose |
|------|------|---------|
| [IssListUtils.cs](ListUtils.O11/IssListUtils.cs) | Interface | Declares 28 methods: 14 base `MssList_*` (`MssList_Pop`, `MssList_PopMultiple`, `MssList_PopByCondition`, `MssList_PopMultipleByCondition`, `MssList_PopByConditions`, `MssList_PopMultipleByConditions`, `MssList_Zip`, `MssList_GroupBy`, `MssList_Difference`, `MssList_Chunk`, `MssList_DistinctBy`, `MssList_Slice`, `MssList_Shuffle`, `MssList_UpdateAt`) + 14 `MssList_*InPlace` variants using `ref string` on the primary list parameter |

No record types — every exposed parameter is a primitive (`string`, `int`, `bool`). Lists cross the boundary as JSON strings, identical to the ODC surface.

### Implementation files (partial class)

`CssListUtils` is a `public partial class` split in the same shape as the ODC side. Every partial mirrors its ODC counterpart line-for-line except for the platform-specific tweaks listed below.

| File | Responsibility |
|------|---------------|
| [Actions/ListUtilsActions.cs](ListUtils.O11/Actions/ListUtilsActions.cs) | Partial-class shell — declares `public partial class CssListUtils : IssListUtils`, no members |
| [Actions/ListUtilsActions.Index.cs](ListUtils.O11/Actions/ListUtilsActions.Index.cs) | `MssList_Pop`, `MssList_PopMultiple` |
| [Actions/ListUtilsActions.Condition.cs](ListUtils.O11/Actions/ListUtilsActions.Condition.cs) | `MssList_PopByCondition`, `MssList_PopMultipleByCondition`, `MssList_PopByConditions`, `MssList_PopMultipleByConditions` |
| [Actions/ListUtilsActions.Relational.cs](ListUtils.O11/Actions/ListUtilsActions.Relational.cs) | `MssList_Zip`, `MssList_GroupBy`, `MssList_Difference` (including all fast-path branches) |
| [Actions/ListUtilsActions.Transform.cs](ListUtils.O11/Actions/ListUtilsActions.Transform.cs) | `MssList_Chunk`, `MssList_DistinctBy`, `MssList_Slice`, `MssList_Shuffle`, `MssList_UpdateAt` — mirrors the ODC `ListUtils.Transform.cs` |
| [Actions/ListUtilsActions.InPlace.cs](ListUtils.O11/Actions/ListUtilsActions.InPlace.cs) | The 14 `MssList_*InPlace` delegating implementations. Each calls the base `MssList_*` method with a local `out` variable, then writes it back to the `ref` parameter. |
| [Actions/ListUtilsActions.Helpers.cs](ListUtils.O11/Actions/ListUtilsActions.Helpers.cs) | Path walker, condition evaluator, multi-condition engine, `TryCompareNumeric`, `ToCamelCase`, nested `Condition` type, shared `JsonSerializerOptions` |

Logic is functionally identical to the ODC implementation. Platform-specific differences:
- `ss`-prefixed parameter names
- `Mss` method prefix
- `str.Substring(1)` instead of `str[1..]` (net48 C# 10 lacks range syntax)
- Explicit `new JsonSerializerOptions { ... }` instead of target-typed `new()`
- `actual.IndexOf(target, cmp) >= 0` instead of `actual.Contains(target, cmp)` (net48 `String.Contains` has no `StringComparison` overload)
- Explicit `using System;`, `using System.Collections.Generic;`, `using System.Linq;`, `using System.Text.Json;`, `using System.Text.Json.Nodes;` on each partial (O11 project sets `<ImplicitUsings>disable</ImplicitUsings>`)

### Runtime dependencies

Declared in [ListUtils.O11.csproj](ListUtils.O11/ListUtils.O11.csproj).

| Package | Version | License | Purpose |
|---------|---------|---------|---------|
| `System.Text.Json` | 8.0.5 | MIT | `JsonNode` / `JsonArray` / `JsonObject` for dynamic JSON manipulation. Not part of net48 BCL — must be added as NuGet. CVE-2024-43484 resolved at 8.0.5. |

---

## 4. Test projects

190 functional tests + 207 load tests per platform × 2 = **794 tests total**. 13 test files per project.

Load tests use a shared 10,000-element complex JSON structure (nested objects, arrays, mixed types) and assert each Server Action completes in under **300 ms** in Release. Every load test also verifies the **result correctness** (expected element count or the invariant `updated + popped = source`) parsed outside the stopwatch so it does not count against the timing budget. `List_Difference` with `Contains` uses a 1,000-element pair because the substring operator is inherently O(A×B).

`List_Difference` fast paths cover every operator except `Contains`:

| Operator | Complexity | Mechanism |
|----------|------------|-----------|
| Equals (default) | O(A+B) | `HashSet<string>` from B, `Contains(keyA)` per A |
| NotEquals / != | O(A+B) | Same set, keep A iff `bSet == {keyA}` |
| StartsWith | O(A·L) | Iterate all prefixes of `keyA` against the B set |
| EndsWith | O(A·L) | Iterate all suffixes of `keyA` against the B set |
| GreaterThan / > | O(A+B) | Precompute `min(B)`, keep A iff `keyA ≤ min` (or not numeric) |
| LessThan / < | O(A+B) | Precompute `max(B)` |
| GreaterOrEqual / >= | O(A+B) | Precompute `min(B)` |
| LessOrEqual / <= | O(A+B) | Precompute `max(B)` |
| Contains | O(A·B) | Linear scan via `MatchesCondition` — no set-based shortcut without a suffix trie |

All actions clone JSON nodes with `JsonNode.DeepClone()` (System.Text.Json 8.0+) instead of the `JsonNode.Parse(node.ToJsonString())` round-trip.

### `ListUtils.Tests/` (ODC, net10.0)

| File | Responsibility |
|------|---------------|
| Usings.cs | `global using Xunit;` |
| ListPopTests.cs | Index-based pop tests: valid index, OOB, negative, null list, first/last element, multiple indices, null/empty indices, OOB ignored, null source, object elements |
| ListJsonTests.cs | JSON-based tests: PopByCondition/PopMultipleByCondition (match, no match, null, camelCase), Zip (equal, unequal, empty), GroupBy (groups, empty), Difference (removes, null A/B, case-insensitive) |
| ComplexStructureTests.cs | Nested objects, deep nesting, mixed value types, large structures with many fields, unicode text, special characters |
| OperatorTests.cs | Contains, StartsWith, EndsWith, NotEquals, GreaterThan, LessThan, GreaterOrEqual, LessOrEqual, symbol aliases (`!=`, `>`, `<`, `>=`, `<=`), non-numeric guard |
| NestedPathTests.cs | Dot-separated property paths (2-level and 3-level), path miss, path with operator, camelCase fallback per segment |
| CaseSensitiveTests.cs | Case-sensitive Equals, NotEquals, Contains, StartsWith on strings; case-insensitive vs case-sensitive Difference |
| ArrayIndexPathTests.cs | Fixed indices, negative indices (from end), array + dot combined, out-of-range guard, nested arrays inside arrays |
| MultiConditionTests.cs | AND/OR combinations, nested-path in condition, empty conditions guard, per-condition case sensitivity, mixed operators |
| SearchDirectionTests.cs | SearchFromEnd on List_PopByCondition and List_PopByConditions — pops last match vs first match; verifies list order preserved after removal |
| TransformTests.cs | 20 functional tests — `List_Chunk` (regular / uneven / oversized / negative / empty), `List_DistinctBy` (property key, nested path, empty PropertyName full-item dedupe, null-key bucket, case-sensitive vs insensitive), `List_Slice` (positive / negative Start / negative End / `End == 0` sentinel / `Step == 0` / negative Step reverse), `List_Shuffle` (`Seed != 0` reproducibility, `Seed == 0` CSPRNG variance, source not mutated), `List_UpdateAt` (positive / negative Index, nested path, auto-created missing objects, missing array short-circuit, PreviousValueJson for missing property vs JSON `null`) |
| InPlaceTests.cs | 45 functional tests — 20 ref-mutation identity + 11 full parity matrix (every `*InPlace` variant vs its base action, asserting byte-equal primary + secondary output) + 14 ref-specific behaviour tests (chained pops reduce the list sequentially; chained shuffle with the same seed is shuffle-of-the-shuffle; fresh input + same seed is fully deterministic across three calls; `List_ChunkInPlace` then `List_SliceInPlace` composes; `List_DistinctByInPlace` then `List_GroupByInPlace` composes; `SearchFromEnd` toggle across chained pops; secondary output is independent of the ref value; `UpdateAtInPlace` PreviousValueJson is a snapshot before the mutation; chained UpdateAt + Pop composes; malformed JSON on `ShuffleInPlace` / `SliceInPlace` throws `JsonException`; `ZipInPlace` / `DifferenceInPlace` do not touch ListB; ref assignment produces a new string reference, not in-place mutation of the caller's original snapshot). |
| LoadTests.cs | 207 load tests — 165 base + 42 InPlace (3 per InPlace variant across 14 variants) — driven by a shared 10,000-element complex JSON list. Each test asserts elapsed time < 300 ms in Release. `List_Difference` with `Contains` uses a 1,000-element pair (slow-path). InPlace load tests take an O(1) local copy of the shared static input, then pass it by `ref` — the shared data is never mutated across tests. |

Test data is inline string literals — **no binary test files are committed**.

### `ListUtils.O11.Tests/` (O11, net48)

| File | Responsibility |
|------|---------------|
| TestHelpers.cs | O11 adapter types: `IListUtils` interface + `ListUtils` wrapper class that calls `CssListUtils.MssList_*` methods and strips `ss`/`Mss` prefixes |
| Usings.cs | `global using Xunit;` |
| ListPopTests.cs | Byte-for-byte identical to ODC |
| ListJsonTests.cs | Byte-for-byte identical to ODC |
| ComplexStructureTests.cs | Byte-for-byte identical to ODC |
| OperatorTests.cs | Byte-for-byte identical to ODC |
| NestedPathTests.cs | Byte-for-byte identical to ODC |
| CaseSensitiveTests.cs | Byte-for-byte identical to ODC |
| ArrayIndexPathTests.cs | Byte-for-byte identical to ODC |
| MultiConditionTests.cs | Byte-for-byte identical to ODC |
| SearchDirectionTests.cs | Byte-for-byte identical to ODC |
| TransformTests.cs | Byte-for-byte identical to ODC |
| InPlaceTests.cs | Byte-for-byte identical to ODC |
| LoadTests.cs | Byte-for-byte identical to ODC |

The adapter pattern ensures `new ListUtils()` resolves to the wrapper in the
O11 test namespace, delegating to `CssListUtils` internally. This allows all
test files to compile unchanged on both platforms. The `internal IListUtils`
interface and its wrapper in `ListUtils.O11.Tests/TestHelpers.cs` include
adapter methods for all 28 actions (14 base + 14 `*InPlace`), with the InPlace
adapters forwarding the `ref string` parameter through to the corresponding
`MssList_*InPlace` methods.

---

## 5. Naming conventions

| Concern | ODC (`net10.0`) | O11 (`net48`) |
|---------|----------------|---------------|
| Interface | `IListUtils` | `IssListUtils` |
| Implementation | `ListUtils` | `CssListUtils` |
| Namespace | `ListUtils` | `OutSystems.NssListUtils` |
| Method names | `List_Pop`, `List_Zip`, etc. | `MssList_Pop`, `MssList_Zip`, etc. |
| Parameter naming | PascalCase (`SourceListJson`, `PropertyName`, `CaseSensitive`) | `ss`-prefixed camelCase (`ssSourceListJson`, `ssPropertyName`) |

---

## 6. Implementation patterns

### JSON manipulation (System.Text.Json.Nodes)

All JSON-based actions use the `System.Text.Json.Nodes` API:
- `JsonNode.Parse(input)!.AsArray()` for parsing
- `JsonObject` / `JsonArray` construction for output
- `item.ToJsonString()` → `JsonNode.Parse(...)` for node cloning (required because `JsonNode` tracks parent ownership)
- `ToJsonString(JsonOptions)` with `WriteIndented = false` for compact output

### Property lookup

`GetPropertyValue(JsonNode, string path)` is a shared static helper that walks a **dot-separated path** with optional **array indexing** at any segment. For each segment (`Address`, `Items[0]`, `Tags[-1]`):

1. Parse the segment into `name` and optional `[index]`.
2. On the current object, try `TryGetPropertyValue(name)`; fall back to `ToCamelCase(name)` (first letter lowered).
3. If `[index]` is present, cast the value to `JsonArray` and index into it — negative indices count from the end.
4. Return `null` if any hop fails (missing property, wrong node type, out-of-range index).

CamelCase fallback runs at **every** path segment, not just the top-level property. This handles the OutSystems `JSON Serialize` behaviour which produces camelCase keys from PascalCase attribute names, even for nested structures.

### Condition evaluation

`MatchesCondition(actual, target, op, caseSensitive)` is the single truth check used by every condition-based action:

- **String operators** (`Equals`, `NotEquals`, `Contains`, `StartsWith`, `EndsWith`) honour the `caseSensitive` flag (`StringComparison.Ordinal` vs `OrdinalIgnoreCase`).
- **Numeric operators** (`GreaterThan`, `LessThan`, `GreaterOrEqual`, `LessOrEqual`) call `TryCompareNumeric`, which parses both values as `decimal` with `InvariantCulture`. Non-numeric values evaluate as no-match.
- Empty / unknown operator defaults to `Equals`.
- Symbol aliases (`!=`, `>`, `<`, `>=`, `<=`) map to their named operators.

### Multi-condition evaluation

`ParseConditions(json)` reads a `[{path, operator, value, caseSensitive?}]` JSON array into a `List<Condition>`. Missing fields default to empty strings and `false`.

`EvaluateConditions(item, conditions, logicalOp)`:
- **AND** (default): short-circuits `false` on the first missing match.
- **OR**: short-circuits `true` on the first hit.
- Empty conditions list returns `false` (no items pop).

Per-condition `caseSensitive` overrides — different fields in the same query can use different case sensitivity.

### Search direction

`List_PopByCondition` and `List_PopByConditions` accept a `SearchFromEnd` bool:
- `false` (default) — forward loop `for (i = 0; i < N; i++)`, pops first match.
- `true` — reverse loop `for (i = N-1; i >= 0; i--)`, pops last match.

The `PopMultiple*` variants always iterate the whole list (they pop every match), so direction is irrelevant to them.

### Index pop strategy

`List_PopMultiple` reverse-sorts indices before iterating. This ensures each `RemoveAt` operates on the correct position — removing index 5 before index 2 means index 2 is still valid.

### Transform semantics

`List_Chunk` walks the source array with a rolling `JsonArray` buffer, emitting each buffer when it reaches `ChunkSize` and one final smaller buffer for the remainder. Empty source or `ChunkSize <= 0` short-circuits to `"[]"`.

`List_DistinctBy` uses the shared `GetPropertyValue` walker so nested paths (`Address.City`) and array indexing (`Tags[0]`) both work as uniqueness keys. Empty `PropertyName` dedupes on the entire item's serialised JSON. Missing keys collapse into a single "null-key" bucket so that at most one keyless item survives. First occurrence wins; source order is preserved.

`List_Slice` normalises Python/JavaScript slice semantics:
- `Start` and `End` accept negative values (count from the end) and are clamped to the array bounds.
- `End == 0` is a sentinel meaning "unspecified" — for positive `Step` it becomes the end of the list; for negative `Step` it becomes "past the beginning" (Python default).
- `Step == 0` is treated as `1`. Negative `Step` walks the array in reverse.

`List_Shuffle` runs Fisher-Yates against a `DeepClone` of the source so the input is never mutated. `Seed != 0` uses `System.Random(Seed)` for reproducible permutations (used in tests). `Seed == 0` calls `RandomNumberGenerator.GetInt32` per swap — a CSPRNG source suitable for security-sensitive workloads (e.g. randomised experiment cohorts, non-guessable order in game hands).

`List_UpdateAt` normalises negative `Index` (counts from the end) and walks the property path segment-by-segment. Object hops that hit a missing key are auto-created; array indexing steps are not — if a segment expects an array but finds a missing or non-array node, the action returns the source unchanged. `NewValueJson` is parsed with `JsonNode.Parse` and falls back to a raw string on `JsonException`, matching the behaviour a Service Studio developer would expect when passing plain text. `PreviousValueJson` is `"null"` for: out-of-range index, empty `PropertyName`, non-object item, missing property, or property present with JSON `null` value — the last two cases are indistinguishable by design (both represent "no previous non-null value to restore").
