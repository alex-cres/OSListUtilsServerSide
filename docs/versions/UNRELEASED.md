# Unreleased

Changes in progress — not yet published to OutSystems Forge.

> When publishing a Forge release, tell the OutSystems Extension Builder agent the version number.

---

## Added

- Initial component scaffold with 7 Server Actions: List_Pop, List_PopMultiple, List_PopByCondition, List_PopMultipleByCondition, List_Zip, List_GroupBy, List_Difference
- ODC external library (net10.0) with `OutSystems.ExternalLibraries.SDK`
- O11 extension (net48) with `System.Text.Json`
- Component icons (64x64 PNG for ODC, 32x32 ICO for O11)
- `comparisonOperator` parameter on List_PopByCondition, List_PopMultipleByCondition, and List_Difference — supports Equals (default), NotEquals, Contains, StartsWith, EndsWith, GreaterThan, LessThan, GreaterOrEqual, LessOrEqual (plus symbol aliases `!=`, `>`, `<`, `>=`, `<=`)
- Nested property path support on all condition-based actions and List_GroupBy — dot-separated paths (e.g. `Address.City`, `Meta.Status.Value`) walk into nested JSON objects with camelCase fallback at each segment
- 118 tests (59 ODC + 59 O11) covering pop, JSON, complex structure, operator, and nested-path scenarios

## Changed

- `List_Pop` and `List_PopMultiple` inputs/outputs changed to JSON strings for consistency with all other actions

## Fixed

*(nothing yet)*

## Removed

*(nothing yet)*
