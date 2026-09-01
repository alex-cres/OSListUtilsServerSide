# LoadTest — ListUtils micro-benchmark harness

Runs every `[OSAction]` in `ListUtils` **N** times per scenario and reports
per-action wall-clock statistics.

Two scenarios are executed back-to-back:

| Scenario     | List size per iteration                                              |
|--------------|----------------------------------------------------------------------|
| **Normal**   | Truncated `N(mean = 10 000, stdev = 5 000)` clamped to `[1, 20 000]` |
| **Worst**    | Constant `20 000` (upper bound on every call)                        |

For each action and scenario the tool reports:

- `N` — iteration count
- `Err` — number of iterations that threw
- `Min`, `Mean`, `StdDev`, `Max`
- `Q1 (25 %)`, `Median (50 %)`, `Q3 (75 %)`
- `P80`, `P95`
- `Total(ms)` — wall time for all iterations (includes input-JSON generation)

A final **comparison table** shows Normal vs Worst medians, P95s, and their
ratios so you can spot actions whose worst-case cost blows up (e.g. anything
with an `O(n²)` inner join).

## Running

From `OSListUtilsServerSide/`:

```powershell
dotnet run -c Release --project tools/LoadTest -- --iterations 1000
```

Options:

| Flag                       | Default | Purpose                                              |
|----------------------------|--------:|------------------------------------------------------|
| `-n`, `--iterations <N>`   |  `1000` | Iterations per action per scenario                   |
| `--seed <N>`               |    `42` | RNG seed for both the size draws and the sample data |
| `--csv <path>`             |    none | Also write a machine-readable CSV                    |
| `--only <a,b,c>`           |    none | Comma-separated action names to run (case-insensitive) |
| `--worst-only`             |   `off` | Skip the Normal scenario; run only the Worst-case scenario |
| `-h`, `--help`             |         | Show usage                                           |

Examples:

```powershell
# Quick smoke run (~7 min on a fast desktop)
dotnet run -c Release --project tools/LoadTest -- --iterations 20

# Full 1 000-iteration run, CSV alongside
dotnet run -c Release --project tools/LoadTest -- --iterations 1000 --csv perf.csv

# Focus on the joins only
dotnet run -c Release --project tools/LoadTest -- -n 500 --only List_ZipGroupBy,List_ZipGroupByMultiple,List_ZipManyGroupByMultiple
```

## Timing notes

- Configuration is forced to **Release** and server GC is enabled in the
  `.csproj` so JIT and GC behave like the production ODC runtime.
- One JIT warm-up pass over every action runs before timing starts.
- The timer only wraps the SUT call. Input JSON is built **before** the
  stopwatch starts, so per-iteration numbers reflect ListUtils cost, not JSON
  generation.
- `Total(ms)` includes generation time (it is wall time of the whole loop),
  so `Total ≠ Sum(iteration timings)`.

## Expected runtime

On a 28-core desktop (Zen 4, .NET 10, Server GC):

| Iterations | Normal scenario | Worst scenario | Total       |
|-----------:|----------------:|---------------:|------------:|
|         20 |         ≈ 100 s |        ≈ 335 s |     ≈ 7 min |
|      1 000 |       ≈ 85 min  |      ≈ 280 min |    ≈ 6 hrs  |

The joins (`List_ZipManyGroupBy`, `List_ZipManyGroupByMultiple`,
`List_ZipGroupByMultiple`) dominate — the median for those is ~50–120 ms at
the 20 000-element worst case; everything else fits under ~30 ms.

## Interpreting the comparison table

`Δmed×` / `Δp95×` = worst / normal ratio. Values close to `1.0×` mean the
action's cost is roughly constant across the size range (typically because
its complexity is dominated by JSON parse/serialise rather than the algorithm
itself). Ratios above `2.5×` mean the algorithm cost dominates and the
worst-case sizing hurts you.

## Files

| File               | Purpose                                                                  |
|--------------------|--------------------------------------------------------------------------|
| `Program.cs`       | CLI + scenario driver + truncated-normal sampler                         |
| `Benchmarks.cs`    | The `Action<Data>` per `[OSAction]`. Add new actions here.               |
| `DataFactory.cs`   | Builds JSON list inputs (people records, list-pair, three-list bundle)   |
| `Stats.cs`         | Percentile / mean / stdev                                                |
| `Reporter.cs`      | Console tables + CSV writer                                              |
