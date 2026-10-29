# O11 Test App Manifest — OSListUtilsServerSide

## Current target

| Field | Value |
|-------|-------|
| Application | `ListUtilsTester` |
| Module | `ListUtilsTester` |
| Module type | Reactive Web App |
| Module key | `66a12dab-a06a-47e7-8ad2-752c894030a1` |
| Extension reference | `ListUtils` (`Extension`) |
| Created | 2026-10-28 |
| Status | Published; anonymous access approved for `ListUtilsTests` |
| Runtime URL | `https://alexandre-crespo-realinho.outsystemscloud.com/ListUtilsTester/ListUtilsTests` |
| Latest evidence | [2026-10-29 dashboard timing split](./2026-10-29-dashboard-timing-split.md) |

## Test surface

| Element | Purpose |
|---------|---------|
| `ListUtilsTests` screen | Runs all 5,000-element tests on ready and from **Run all tests** by calling each test separately; shows pass count plus ListUtils, server-action, network/request, and browser totals; each row shows the same four timings, Expected/Actual fingerprints and Error for failures; links to every per-action screen |
| `<Action>Test` screens | One screen per ListUtils action (for example `PopTest`, `ZipManyTest`). Inputs are prefilled with the regression sample and editable. **Run** calls the extension directly; **Reset sample** restores defaults; results show every output or the exception message plus browser round-trip ms |
| `RunAllTests` server action | Server-side batch runner retained for compatibility; the dashboard no longer uses it because per-action timing requires separate calls |
| `Test_List_*` server actions | One deterministic 5,000-element runtime case per ListUtils Server Action; generates inputs, times only the ListUtils call, fingerprints outputs, and captures exceptions instead of aborting the run |
| `TestResult` structure | `Action`, `CaseName`, `Expected`, `Actual`, `Passed`, `ErrorMessage`, `ElapsedMs`, `ServerMs`, `ActionMs` |
| `GenPeople`, `GenCustomers`, `GenOrders`, `GenNumbers`, `GenChunks` | Deterministic server-side 5,000-element input generators |
| `FP` / `ServerClockMs` | Output fingerprint (length + first/last 80 chars) and SQL Server millisecond clock |
| `NowMs` client action | Reads `performance.now()` for millisecond browser round-trip timing |
| `Q` / `QC` functions | Decode sample JSON literals on the server and client without OutSystems string-escaping ambiguity |
| `LinesToTextItems` client action | Converts editable one-value-per-line input into the extension's `TextItem` list |

Editable input types: Text uses an input or text area, Integer uses a number input, Boolean uses
a checkbox, `ComparisonOperator` / `LogicalOperator` / `Operation` use radio groups, `TextItem`
lists use one value per line, and `Condition` lists use a JSON array.

Dashboard expected fingerprints were generated from the local `ListUtils.O11` implementation
using the same 5,000-element data formulas. Multi-output actions concatenate outputs with ` || `
before fingerprinting; `List_Chunk` records chunk count plus first and last chunk. Editable
action screens keep small editable samples.

## Coverage

| Area | Actions | Cases built | Runtime result |
|------|---------|-------------|----------------|
| Index | `List_Pop`, `List_PopMultiple`, `List_SplitAt` | 3 | PASS (dashboard + editable screens) |
| Condition | `List_PopByCondition`, `List_PopMultipleByCondition`, `List_PopByConditions`, `List_PopMultipleByConditions`, `List_Partition`, `List_PartitionByConditions` | 6 | PASS (dashboard + editable screens) |
| Relational | `List_Zip`, `List_GroupBy`, `List_ZipGroupBy`, `List_GroupByMultiple`, `List_ZipGroupByMultiple`, `List_Difference`, `List_Intersect`, `List_Union` | 8 | PASS (dashboard + editable screens) |
| Transform | `List_Chunk`, `List_DistinctBy`, `List_Slice`, `List_Shuffle`, `List_UpdateAt`, `List_Reverse`, `List_Flatten`, `List_Sample`, `List_ReplaceWhere`, `List_UpdateMultipleAt` | 10 | PASS (dashboard + editable screens) |
| Aggregate | `List_MinBy`, `List_MaxBy`, `List_Aggregate` | 3 | PASS (dashboard + editable screens) |
| Multi-list | `List_ZipMany`, `List_ZipManyGroupBy`, `List_ZipManyGroupByMultiple` | 3 | PASS (dashboard + editable screens) |
| **Total** | **33 / 33 exposed actions** | **33** | **33 / 33 PASS on both surfaces** |
