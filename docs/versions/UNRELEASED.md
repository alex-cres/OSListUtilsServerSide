# Unreleased

Changes in progress — not yet published to OutSystems Forge.

> When publishing a Forge release, tell the OutSystems Extension Builder agent the version number.

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
- 188 tests (94 ODC + 94 O11) covering pop, JSON, complex structure, operator, nested-path, array indexing, case-sensitive, multi-condition, and search-direction scenarios
- 180 load tests (90 ODC + 90 O11) — 10 per Server Action — using a 10,000-element complex JSON structure (nested objects, arrays, mixed types). Every assertion enforces a 300 ms budget in Release configuration. `List_Difference` with `Contains` uses a 1,000-element pair because that operator is inherently O(A×B).
- 30 additional functional and load tests (15 ODC + 15 O11) covering the new `List_Difference` fast paths for `NotEquals`, `StartsWith`, `EndsWith`, `GreaterThan`, `LessThan`, `GreaterOrEqual`, `LessOrEqual`, plus edge cases (non-numeric key, no-parseable-B). Ten new functional tests per platform verify the fast paths return the same results as the slow path; five new load tests per platform prove each fast path stays under the 300 ms budget on 10k-element inputs.
- 21 additional edge-case tests per platform (42 total) in a new `EdgeCasesTests.cs` file covering empty/single-element inputs, ordering preservation, null/empty parameter names, contract behaviour for `PoppedElementJson` on no match ("{}" for condition-based, "null" for index-based), default `LogicalOperator`, empty `KeyName` in Zip, and numeric-vs-non-numeric operand handling.
- Load tests now assert **correctness of the result** in addition to timing: every load test parses its output outside the stopwatch and asserts the expected element count (or invariant `updated + popped = source` when the exact count depends on data alignment). Helpers: `AssertPopSingle`, `AssertPopMany`, `AssertPopManyInvariant`, `AssertZip`, `AssertGroupBy`, `AssertDifference`.

## Changed

- `List_Pop` and `List_PopMultiple` inputs/outputs changed to JSON strings for consistency with all other actions
- `List_Difference` fast paths — every operator except `Contains` now runs in O(A+B) or O(A·L):
  - `Equals` and `NotEquals` use a `HashSet<string>` for O(1) lookup per element.
  - `StartsWith` and `EndsWith` scan the O(|keyA|) prefixes/suffixes of each A key against a `HashSet` of B values.
  - `GreaterThan`, `LessThan`, `GreaterOrEqual`, `LessOrEqual` precompute `min(B)` / `max(B)` once and reduce each per-A test to a single decimal parse plus compare.
  - `Contains` remains O(A×B) — this is the only operator that would need a suffix trie / Aho-Corasick to beat.
- Replaced every `JsonNode.Parse(node.ToJsonString())` round-trip clone with `JsonNode.DeepClone()` across all Server Actions. Halves the runtime of `List_Zip`, `List_GroupBy`, `List_Pop`, `List_PopMultiple`, and the `Pop*ByCondition*` family on 10k-element inputs.

## Fixed

*(nothing yet)*

## Removed

*(nothing yet)*
