# O11 Dashboard Timing Split — ListUtils — 2026-10-29

## Scope

Separate the dashboard's browser time into ListUtils extension time, full server-action time,
and network/request overhead.

## Measurement method

| Column | Calculation |
|--------|-------------|
| ListUtils | SQL Server clock immediately before and after the extension call |
| Server action | SQL Server clock at the start and end of the whole `Test_List_*` action, including 5,000-element data generation, input preparation, ListUtils, and fingerprint checks |
| Network/request | Browser ms minus server-action ms |
| Browser | `performance.now()` before and after the dashboard calls `Test_List_*` |

`ActionMs` was added to `TestResult`. The dashboard sums ListUtils ms, server-action ms,
network/request ms, and browser ms.

The SQL Server clock on this environment has about 15–16 ms resolution. `0 ms` means within one
clock step.

## Measurements

| Run | Result |
|-----|--------|
| First run after publish | `33 / 33 passed - ListUtils 1083 ms - server actions 2455 ms - network/request 10930 ms - browser 13385 ms` |
| Warm rerun | `33 / 33 passed - ListUtils 579 ms - server actions 1837 ms - network/request 8621 ms - browser 10458 ms` |

On the warm rerun, network/request accounted for **82.4%** of browser time.

| Measure | Min | Median | Max |
|---------|----:|-------:|----:|
| ListUtils ms | 0 | 16 | 63 |
| Server-action ms | 16 | 47 | 125 |
| Network/request ms | 158 | 202 | 471 |
| Browser ms | 184 | 299 | 565 |

| Action | ListUtils ms | Server action ms | Network/request ms | Browser ms |
|--------|-------------:|-----------------:|-------------------:|-----------:|
| `List_Pop` | 16 | 46 | 178 | 224 |
| `List_PopMultiple` | 0 | 62 | 336 | 398 |
| `List_PopByCondition` | 15 | 125 | 174 | 299 |
| `List_PopMultipleByCondition` | 15 | 48 | 350 | 398 |
| `List_PopByConditions` | 0 | 47 | 392 | 439 |
| `List_PopMultipleByConditions` | 16 | 63 | 166 | 229 |
| `List_Zip` | 16 | 62 | 342 | 404 |
| `List_GroupBy` | 15 | 63 | 359 | 422 |
| `List_ZipGroupBy` | 31 | 78 | 348 | 426 |
| `List_Difference` | 32 | 78 | 190 | 268 |
| `List_Chunk` | 0 | 31 | 339 | 370 |
| `List_DistinctBy` | 15 | 31 | 348 | 379 |
| `List_Slice` | 0 | 47 | 160 | 207 |
| `List_Shuffle` | 16 | 32 | 158 | 190 |
| `List_UpdateAt` | 16 | 47 | 173 | 220 |
| `List_MinBy` | 16 | 37 | 161 | 198 |
| `List_MaxBy` | 16 | 31 | 332 | 363 |
| `List_Aggregate` | 15 | 31 | 168 | 199 |
| `List_Intersect` | 16 | 32 | 356 | 388 |
| `List_Union` | 31 | 62 | 165 | 227 |
| `List_SplitAt` | 0 | 16 | 334 | 350 |
| `List_Partition` | 16 | 62 | 326 | 388 |
| `List_PartitionByConditions` | 16 | 62 | 178 | 240 |
| `List_Reverse` | 0 | 32 | 189 | 221 |
| `List_Flatten` | 16 | 16 | 342 | 358 |
| `List_Sample` | 0 | 16 | 168 | 184 |
| `List_ReplaceWhere` | 16 | 47 | 202 | 249 |
| `List_UpdateMultipleAt` | 15 | 47 | 170 | 217 |
| `List_ZipMany` | 47 | 94 | 471 | 565 |
| `List_ZipManyGroupBy` | 31 | 78 | 340 | 418 |
| `List_GroupByMultiple` | 63 | 110 | 166 | 276 |
| `List_ZipGroupByMultiple` | 31 | 94 | 163 | 257 |
| `List_ZipManyGroupByMultiple` | 31 | 110 | 377 | 487 |

Evidence: [2026-10-29-dashboard-timing-split.png](./2026-10-29-dashboard-timing-split.png)

## Verdict

**PASS** — all 33 actions pass, and the dashboard shows where each browser round trip is spent.
For these tests, most time is network/request overhead rather than ListUtils or server-side test
work.
