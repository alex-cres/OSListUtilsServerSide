# Unreleased

Changes in progress — not yet published to OutSystems Forge.

---

## Added

### Server Actions (3 new → 32 total)

Three new grouping actions that generalise the existing single-key group-by / cogroup family to an arbitrary number of key columns (composite keys). All three reuse the existing `GetPropertyValue` path walker plus two new shared helpers (`BuildCompositeKey`, `KeyLabel`) — no new NuGet dependencies.

Composite-key encoding uses the ASCII **Unit Separator** (`\u001F`) between key values internally so distinct tuples cannot collide even when their string representations share prefixes. Missing key values fall back to the string `"Unknown"`. Output labels default to `Key0`, `Key1`, … when the caller does not supply enough `KeyNames` entries.

- **`List_GroupByMultiple(SourceListJson, PropertyPaths : List<Text>, KeyNames : List<Text>, ItemsFieldName, CaseSensitive) → GroupedListJson`** — groups a JSON list by N property paths. Each output group emits one field per key (labelled by `KeyNames[i]`, defaulting to `Key0`, `Key1`, …) plus an items array (`ItemsFieldName`, defaulting to `Items`). Generalisation of `List_GroupBy` from a single property path to any number of key columns.
- **`List_ZipGroupByMultiple(ListAJson, ListBJson, KeyPropertiesA : List<Text>, KeyPropertiesB : List<Text>, KeyNames : List<Text>, KeyNameA, KeyNameB, CaseSensitive) → GroupedListJson`** — cogroup two lists by an N-key composite. Each output group emits one field per key plus one named array per list (`KeyNameA` / `KeyNameB`, defaulting to `ItemsA` / `ItemsB`). Generalisation of `List_ZipGroupBy`.
- **`List_ZipManyGroupByMultiple(ListsJson : List<Text>[M], KeyCount, KeyProperties : List<Text>[M*N list-major], KeyNames : List<Text>[N], ItemsFieldNames : List<Text>[M], CaseSensitive) → GroupedListJson`** — cogroup M lists by an N-key composite. `KeyProperties` is a **flat** list in list-major order: entries `[i*KeyCount .. i*KeyCount + KeyCount - 1]` supply the key paths for `ListsJson[i]`. Each output group emits one field per key plus one named array per list (`ItemsFieldNames[i]`, defaulting to `Items0`, `Items1`, …). Generalisation of `List_ZipManyGroupBy`.

### Implementation Files

- ODC: `ListUtils.Relational.cs` gains `List_GroupByMultiple` and `List_ZipGroupByMultiple`. `ListUtils.ZipMany.cs` gains `List_ZipManyGroupByMultiple`. `ListUtils.Helpers.cs` gains two new shared static helpers: `BuildCompositeKey(node, paths, caseSensitive)` (renders the Unit-Separator-joined composite key, falling back to `"Unknown"` per missing segment) and `KeyLabel(keyNames, i)` (returns `keyNames[i]` when present, or `"Key{i}"` as the default).
- O11: parallel additions in `Actions/ListUtilsActions.Relational.cs`, `Actions/ListUtilsActions.ZipMany.cs`, and `Actions/ListUtilsActions.Helpers.cs`. All net48-safe (no target-typed `new()`, no range syntax, explicit `System.Collections.Generic` / `System.Text.Json` usings).

### Test Suite Growth

- 32 new functional tests (16 ODC + 16 O11) in `V060Tests.cs` — happy paths (2-key and 3-key grouping, cogroup with disjoint keys, M=3 lists / N=2 keys), edge cases (empty source, empty key list, `KeyNames` shorter than `PropertyPaths`, `null!` KeyNames, mismatched `KeyPropertiesA` / `KeyPropertiesB` length), and semantic contracts (composite-key collision safety via Unit Separator, `"Unknown"` bucket for missing values, per-list case sensitivity, default label fallback `Key0`, `Key1`, …).
- 30 new load tests (15 ODC + 15 O11) appended to `LoadTests.cs` — 5 per new action — driven by the shared 10 000-element JSON list. Each asserts elapsed time < 300 ms in Release. Nested-key composite grouping uses the 1 000-element `SlowPathList` pair to stay comfortably under budget on both platforms.
- Adapter methods for all three new actions added to `internal IListUtils` and its wrapper in `ListUtils.O11.Tests/TestHelpers.cs` so ODC and O11 test files remain byte-for-byte identical.
- Total suite grew from 922 to **984 tests** (492 ODC + 492 O11).

### Micro-benchmark harness (`tools/LoadTest/`)

- New standalone console tool `tools/LoadTest/LoadTest.csproj` (net10.0, project reference to `ListUtils`) that exercises every one of the 33 `[OSAction]` methods for a configurable iteration count (default 1 000) and reports **Min / Mean / StdDev / Q1 / Median / Q3 / P80 / P95 / Max** per action.
- Two scenarios per run: (a) **Normal** — list sizes drawn from a truncated `N(mean=10 000, stdev=5 000)` clamped to `[1, 20 000]`; (b) **Worst-case** — list sizes fixed at 20 000. A final comparison table prints Normal vs Worst medians, P95s and their ratios so quadratic behaviour stands out.
- Files: `Program.cs` (CLI + Box-Muller truncated-normal sampler + scenario driver), `Benchmarks.cs` (one entry per `[OSAction]`, add new actions here), `DataFactory.cs` (hand-rolled JSON list generator + pre-chunked input for `List_Flatten`), `Stats.cs` (linear-interpolation percentiles), `Reporter.cs` (console tables + optional CSV output).
- CLI: `--iterations`, `--seed`, `--csv <path>`, `--only <a,b,c>`. Detailed usage in `tools/LoadTest/README.md`.
- Non-shipping — the tool is not referenced by `ListUtils.sln`, is Release-mode / server-GC on the harness side only, and produces no ZIP artifact.

## Changed

*(nothing yet)*

## Fixed

*(nothing yet)*

## Removed

*(nothing yet)*
