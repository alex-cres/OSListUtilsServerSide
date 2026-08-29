# OSListUtilsServerSide — Architecture

Structural reference for the solution, the runtime component, and the test
projects. Keep this file in sync with the code — the `documentation-updater`
agent updates it as part of the change cycle whenever the solution layout,
Server Action surface, dependency set, or project structure changes.

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
| [IListUtils.cs](ListUtils/IListUtils.cs) | `[OSInterface]` | Declares 9 actions: `List_Pop`, `List_PopMultiple`, `List_PopByCondition`, `List_PopMultipleByCondition`, `List_PopByConditions`, `List_PopMultipleByConditions`, `List_Zip`, `List_GroupBy`, `List_Difference` |

No `[OSStructure]` types — every exposed parameter is a primitive (`string`, `int`, `bool`). All list data is passed as JSON strings (`Text`), which keeps the interface generic across any consumer Structure.

### Implementation files (partial class)

`ListUtils` is a `public partial class` split across five files by action group. The shell holds the class declaration + `IListUtils` implementation marker; each partial contributes one concern.

| File | Responsibility |
|------|---------------|
| [ListUtils.cs](ListUtils/ListUtils.cs) | Partial-class shell — declares `public partial class ListUtils : IListUtils`, no members |
| [ListUtils.Index.cs](ListUtils/ListUtils.Index.cs) | Index-based pops: `List_Pop`, `List_PopMultiple` |
| [ListUtils.Condition.cs](ListUtils/ListUtils.Condition.cs) | Condition-based actions: `List_PopByCondition`, `List_PopMultipleByCondition`, `List_PopByConditions`, `List_PopMultipleByConditions` |
| [ListUtils.Relational.cs](ListUtils/ListUtils.Relational.cs) | Relational / set actions: `List_Zip`, `List_GroupBy`, `List_Difference` (including all fast-path branches) |
| [ListUtils.Helpers.cs](ListUtils/ListUtils.Helpers.cs) | `GetPropertyValue` + `NavigateSegment` path walker, `MatchesCondition` operator evaluator, `ParseConditions` + `EvaluateConditions` multi-condition engine, `TryCompareNumeric` numeric comparator, `ToCamelCase` fallback, nested `Condition` type, shared `JsonSerializerOptions` |

### Runtime dependencies

Declared in [ListUtils.csproj](ListUtils/ListUtils.csproj).

| Package | License | Purpose |
|---------|---------|---------|
| `OutSystems.ExternalLibraries.SDK` | OutSystems proprietary | `[OSInterface]`, `[OSAction]`, `[OSParameter]` attributes |

`System.Text.Json` is used but ships as part of the net10.0 BCL — no NuGet required.

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

    Eval1 --> Serialize["ToJsonString(JsonOptions)"]
    Eval2 --> Serialize
    Zip --> Serialize
    GroupBy --> Serialize
    Diff --> Serialize
    IndexOp --> Serialize

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
| [IssListUtils.cs](ListUtils.O11/IssListUtils.cs) | Interface | Declares `MssList_Pop`, `MssList_PopMultiple`, `MssList_PopByCondition`, `MssList_PopMultipleByCondition`, `MssList_PopByConditions`, `MssList_PopMultipleByConditions`, `MssList_Zip`, `MssList_GroupBy`, `MssList_Difference` |

No record types — every exposed parameter is a primitive (`string`, `int`, `bool`). Lists cross the boundary as JSON strings, identical to the ODC surface.

### Implementation files (partial class)

`CssListUtils` is a `public partial class` split in the same shape as the ODC side. Every partial mirrors its ODC counterpart line-for-line except for the platform-specific tweaks listed below.

| File | Responsibility |
|------|---------------|
| [Actions/ListUtilsActions.cs](ListUtils.O11/Actions/ListUtilsActions.cs) | Partial-class shell — declares `public partial class CssListUtils : IssListUtils`, no members |
| [Actions/ListUtilsActions.Index.cs](ListUtils.O11/Actions/ListUtilsActions.Index.cs) | `MssList_Pop`, `MssList_PopMultiple` |
| [Actions/ListUtilsActions.Condition.cs](ListUtils.O11/Actions/ListUtilsActions.Condition.cs) | `MssList_PopByCondition`, `MssList_PopMultipleByCondition`, `MssList_PopByConditions`, `MssList_PopMultipleByConditions` |
| [Actions/ListUtilsActions.Relational.cs](ListUtils.O11/Actions/ListUtilsActions.Relational.cs) | `MssList_Zip`, `MssList_GroupBy`, `MssList_Difference` (including all fast-path branches) |
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

125 functional tests + 95 load tests per platform × 2 = **440 tests total**. 11 test files per project.

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
| LoadTests.cs | 90 load tests — 10 per Server Action — driven by a shared 10,000-element complex JSON list. Each test asserts elapsed time < 300 ms in Release. `List_Difference` with `Contains` uses a 1,000-element pair (slow-path). |

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
| LoadTests.cs | Byte-for-byte identical to ODC |

The adapter pattern ensures `new ListUtils()` resolves to the wrapper in the
O11 test namespace, delegating to `CssListUtils` internally. This allows all
test files to compile unchanged on both platforms.

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
