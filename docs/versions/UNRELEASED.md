# Unreleased

Changes in progress — not yet published to OutSystems Forge.

---

## Added

- Initial component scaffold with 9 Server Actions: List_Pop, List_PopMultiple, List_PopByCondition, List_PopMultipleByCondition, List_PopByConditions, List_PopMultipleByConditions, List_Zip, List_GroupBy, List_Difference
- ODC external library (net10.0) with `OutSystems.ExternalLibraries.SDK`
- O11 extension (net48) with `System.Text.Json`
- Component icons (64x64 PNG for ODC, 32x32 ICO for O11)
- `ComparisonOperator` parameter on condition-based single-condition actions — supports Equals (default), NotEquals, Contains, StartsWith, EndsWith, GreaterThan, LessThan, GreaterOrEqual, LessOrEqual (plus symbol aliases `!=`, `>`, `<`, `>=`, `<=`)
- `CaseSensitive` boolean parameter on List_PopByCondition, List_PopMultipleByCondition, and List_Difference — toggles case-sensitive string comparison
- `SearchFromEnd` boolean parameter on List_PopByCondition and List_PopByConditions — pops the LAST match instead of the first
- Nested property path support on all condition-based actions and List_GroupBy — dot-separated paths (e.g. `Address.City`) with camelCase fallback per segment
- Array indexing in property paths — `Tags[0]`, `Items[-1].Name`, `Data.Values[0]`. Negative indices count from the end.
- Two new multi-condition actions: `List_PopByConditions` and `List_PopMultipleByConditions` accepting a JSON conditions array with per-condition path, operator, value, and caseSensitive fields; combined with `AND` or `OR` logical operator
- All Server Action parameter names use PascalCase for consistency with OutSystems naming conventions
- 250 functional tests (125 ODC + 125 O11) covering pop, JSON, complex structure, operator, nested-path, array indexing, case-sensitive, multi-condition, search-direction, and edge-case scenarios
- 190 load tests (95 ODC + 95 O11) — driven by a shared 10,000-element complex JSON structure (nested objects, arrays, mixed types). Every assertion enforces a 300 ms budget in Release configuration. `List_Difference` with `Contains` uses a 1,000-element pair because that operator is inherently O(A×B). Every load test also verifies the result correctness (expected element count or the invariant `updated + popped = source`) parsed outside the stopwatch.
- Total suite: 440 tests (220 ODC + 220 O11)
- Load tests now assert **correctness of the result** in addition to timing: every load test parses its output outside the stopwatch and asserts the expected element count (or invariant `updated + popped = source` when the exact count depends on data alignment). Helpers: `AssertPopSingle`, `AssertPopMany`, `AssertPopManyInvariant`, `AssertZip`, `AssertGroupBy`, `AssertDifference`.
- Five new Server Actions covering list transformation and randomization, taking the surface from 9 to 14:
  - `List_Chunk(SourceListJson, ChunkSize) → ChunksListJson` — splits a JSON list into an array of fixed-size sublists (the last chunk may be smaller). `ChunkSize <= 0` or an empty source returns `"[]"`.
  - `List_DistinctBy(SourceListJson, PropertyName, CaseSensitive) → DistinctListJson` — filters to unique elements by property key (first occurrence wins). Empty `PropertyName` dedupes on the entire item's JSON. Missing keys share a single "null-key" bucket. Uses `GetPropertyValue` so nested paths and array indexing are supported.
  - `List_Slice(SourceListJson, Start, End, Step) → SliceListJson` — Python/JavaScript-style slicing. Negative `Start`/`End` count from the end; `End == 0` is treated as "unspecified" ("to end of list" for positive `Step`, "past the beginning" for negative `Step`); `Step == 0` is treated as `1`; negative `Step` reverses the walk.
  - `List_Shuffle(SourceListJson, Seed) → ShuffledListJson` — Fisher-Yates shuffle. `Seed == 0` uses `RandomNumberGenerator` (CSPRNG per swap); `Seed != 0` uses `System.Random(Seed)` for reproducibility. Source list is not mutated.
  - `List_UpdateAt(SourceListJson, Index, PropertyName, NewValueJson) → UpdatedListJson, PreviousValueJson` — sets a property on the item at `Index`. Negative `Index` counts from the end. `PropertyName` supports nested paths + array indexing; missing intermediate objects are created (arrays must already exist). `NewValueJson` is parsed as JSON, falling back to a raw string. `PreviousValueJson` is `"null"` when the index is out of range, `PropertyName` is empty, the item is not an object, the property did not exist, OR the property existed with a JSON `null` value (the last two cases are indistinguishable).
- New partial `ListUtils/ListUtils.Transform.cs` (ODC) hosting the five implementations plus seven private helpers.
- New partial `ListUtils.O11/Actions/ListUtilsActions.Transform.cs` (O11) mirroring the ODC implementation with `ss`-prefixed parameters and `Mss` method names.
- 40 new functional tests (20 ODC + 20 O11) in `TransformTests.cs` covering chunk sizes, distinct-by nested key with camelCase fallback, slice reverse walks and `End == 0` sentinel, shuffle determinism vs CSPRNG, and update-at path creation / missing-array short-circuit.
- 100 new load tests (10 per new action × 5 actions × 2 platforms) appended to `LoadTests.cs`, driven by the shared 10,000-element complex JSON structure with the same 300 ms budget.
- Adapter methods for the five new actions added to `internal IListUtils` and its wrapper in `ListUtils.O11.Tests/TestHelpers.cs`.
- Total suite grew from 440 to **620 tests** (310 ODC + 310 O11).
- Fourteen in-place (Input/Output) variants of every existing action — one per action, taking the surface from 14 to **28 Server Actions**. Each accepts the primary list parameter by C# `ref` (mapped to Input/Output in OutSystems), mutates the caller's variable directly, and delegates to the corresponding non-InPlace action for the actual logic. Secondary outputs (`PoppedElementJson`, `PoppedElementsJson`, `PreviousValueJson`) stay as `out`. This lets OutSystems consumers avoid the `MyList = ListUtils.Action(MyList).OutputList` reassignment pattern.
  - Pop family: `List_PopInPlace`, `List_PopMultipleInPlace`, `List_PopByConditionInPlace`, `List_PopMultipleByConditionInPlace`, `List_PopByConditionsInPlace`, `List_PopMultipleByConditionsInPlace`
  - Relational: `List_ZipInPlace` (mutates `ListAJson` with the paired result), `List_GroupByInPlace` (mutates `SourceListJson` with the grouped `{Key, Items}` array), `List_DifferenceInPlace` (mutates `ListAJson` with A − B)
  - Transform: `List_ChunkInPlace`, `List_DistinctByInPlace`, `List_SliceInPlace`, `List_ShuffleInPlace`, `List_UpdateAtInPlace`
- New partial `ListUtils/ListUtils.InPlace.cs` (ODC) and `ListUtils.O11/Actions/ListUtilsActions.InPlace.cs` (O11) hosting all 14 delegating implementations.
- 40 new functional tests (20 ODC + 20 O11) in `InPlaceTests.cs` covering: ref-mutation identity for every InPlace action, delegation-parity spot checks vs the base action (Slice, Shuffle, Chunk), and null/empty/out-of-range short-circuits.
- 50 additional functional tests (25 ODC + 25 O11) added to `InPlaceTests.cs` — the full parity matrix (11 tests) covers every remaining `*InPlace` variant against its base action asserting byte-equal primary + secondary output; plus 14 ref-semantics tests covering chained pops, chained-same-seed shuffle, fresh-input determinism, `Chunk→Slice` and `DistinctBy→GroupBy` composition, `SearchFromEnd` toggle across chained pops, secondary-output independence from the ref, `UpdateAt` PreviousValueJson snapshot semantics, malformed-JSON exception propagation on `Shuffle` / `Slice`, `Zip` / `Difference` leaving `ListB` untouched, and ref-assignment new-reference semantics.
- 84 new load tests (42 ODC + 42 O11) appended to `LoadTests.cs` — 3 per InPlace variant × 14 variants — driven by the same shared 10,000-element complex JSON input with the same 300 ms budget. Each takes an O(1) local copy of the shared static input so tests remain isolated when run in parallel.
- Adapter methods for the 14 new InPlace actions added to `internal IListUtils` and its wrapper in `ListUtils.O11.Tests/TestHelpers.cs`, forwarding `ref` through to the `MssList_*InPlace` methods.
- Total suite grew from 620 to **794 tests** (397 ODC + 397 O11).

## Changed

- `List_Pop` and `List_PopMultiple` inputs/outputs changed to JSON strings for consistency with all other actions
- Refactored `ListUtils` (ODC, `net10.0`) and `CssListUtils` (O11, `net48`) into partial classes split by action group. New files: `ListUtils.Index.cs` / `ListUtilsActions.Index.cs` (`List_Pop`, `List_PopMultiple`); `ListUtils.Condition.cs` / `ListUtilsActions.Condition.cs` (`PopByCondition`, `PopMultipleByCondition`, `PopByConditions`, `PopMultipleByConditions`); `ListUtils.Relational.cs` / `ListUtilsActions.Relational.cs` (`Zip`, `GroupBy`, `Difference`); `ListUtils.Helpers.cs` / `ListUtilsActions.Helpers.cs` (`GetPropertyValue`, `NavigateSegment`, `MatchesCondition`, `ParseConditions`, `EvaluateConditions`, `TryCompareNumeric`, `ToCamelCase`, `Condition`, `JsonOptions`). The original `ListUtils.cs` and `Actions/ListUtilsActions.cs` are now partial-class shells (no members). No behavioural change; all 440 tests pass.
- `List_Difference` fast paths — every operator except `Contains` now runs in O(A+B) or O(A·L):
  - `Equals` and `NotEquals` use a `HashSet<string>` for O(1) lookup per element.
  - `StartsWith` and `EndsWith` scan the O(|keyA|) prefixes/suffixes of each A key against a `HashSet` of B values.
  - `GreaterThan`, `LessThan`, `GreaterOrEqual`, `LessOrEqual` precompute `min(B)` / `max(B)` once and reduce each per-A test to a single decimal parse plus compare.
  - `Contains` remains O(A×B) — this is the only operator that would need a suffix trie / Aho-Corasick to beat.
- Replaced every `JsonNode.Parse(node.ToJsonString())` round-trip clone with `JsonNode.DeepClone()` across all Server Actions. Halves the runtime of `List_Zip`, `List_GroupBy`, `List_Pop`, `List_PopMultiple`, and the `Pop*ByCondition*` family on 10k-element inputs.
- Updated the shell-comment enumeration in `ListUtils/ListUtils.cs` and `ListUtils.O11/Actions/ListUtilsActions.cs` from "nine implementations" to "fourteen implementations", and added the new `ListUtils.Transform.cs` / `ListUtilsActions.Transform.cs` partial to the partial-file inventory in each shell.
- Refined the `[OSParameter]` descriptions on `List_UpdateAt` in `IListUtils.cs`: `PropertyName` now states that missing intermediate objects are created but arrays are NOT auto-created (a missing/non-array indexing step returns the source unchanged); `PreviousValueJson` now states that `"null"` is also returned when the property existed with a JSON `null` value (indistinguishable from "property did not exist").

## Fixed

*(nothing yet)*

## Removed

*(nothing yet)*
