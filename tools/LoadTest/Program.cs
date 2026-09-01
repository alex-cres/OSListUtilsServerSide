using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.Json;
using ListUtils;

namespace LoadTest;

// Load-test harness for every [OSAction] in ListUtils.
//
//  * Normal scenario  — list sizes drawn from a truncated Normal(mean=MEAN, stdev=STDEV) clamped to [MIN, MAX].
//  * Worst-case scenario — list sizes fixed at MAX, same iteration count.
//  * Reports per-action Min / Mean / StdDev / Q1 / Median / Q3 / P80 / P95 / Max / TotalMs.
//
// Usage:
//   dotnet run -c Release --project tools/LoadTest -- --iterations 1000
//   dotnet run -c Release --project tools/LoadTest -- --iterations 1000 --csv out.csv
//   dotnet run -c Release --project tools/LoadTest -- --iterations 1000 --only List_GroupBy,List_ZipMany
internal static class Program
{
    private const int MinSize = 1;
    private const int MaxSize = 20_000;
    private const int MeanSize = 10_000;
    private const int StdDevSize = 5_000;

    private static int Main(string[] args)
    {
        int iterations = 1000;
        int seed = 42;
        string? csvPath = null;
        HashSet<string>? only = null;
        bool worstOnly = false;

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--iterations" or "-n" when i + 1 < args.Length:
                    iterations = int.Parse(args[++i], CultureInfo.InvariantCulture);
                    break;
                case "--seed" when i + 1 < args.Length:
                    seed = int.Parse(args[++i], CultureInfo.InvariantCulture);
                    break;
                case "--csv" when i + 1 < args.Length:
                    csvPath = args[++i];
                    break;
                case "--only" when i + 1 < args.Length:
                    only = new HashSet<string>(args[++i].Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries), StringComparer.OrdinalIgnoreCase);
                    break;
                case "--worst-only":
                    worstOnly = true;
                    break;
                case "--help" or "-h":
                    PrintUsage();
                    return 0;
                default:
                    Console.Error.WriteLine($"Unknown argument: {args[i]}");
                    PrintUsage();
                    return 1;
            }
        }

        if (iterations < 1)
        {
            Console.Error.WriteLine("--iterations must be >= 1");
            return 1;
        }

        var sut = new global::ListUtils.ListUtils();
        var benchmarks = Benchmarks.All(sut);
        if (only is not null)
        {
            benchmarks = benchmarks.Where(b => only.Contains(b.Name)).ToList();
            if (benchmarks.Count == 0)
            {
                Console.Error.WriteLine("--only filter matched no benchmarks.");
                return 1;
            }
        }

        Console.WriteLine($"ListUtils load test — {benchmarks.Count} action(s), {iterations} iteration(s) per scenario");
        Console.WriteLine($"  Normal scenario:  list size ~ N(mean={MeanSize}, stdev={StdDevSize}) clamped to [{MinSize}, {MaxSize}]");
        Console.WriteLine($"  Worst  scenario:  list size = {MaxSize} (constant)");
        Console.WriteLine($"  Seed: {seed}    Runtime: {Environment.Version}    Cores: {Environment.ProcessorCount}    GC server: {System.Runtime.GCSettings.IsServerGC}");
        Console.WriteLine();

        var rngNormal = new Random(seed);
        var rngWorst  = new Random(seed ^ 0x5A5A5A5A);

        // JIT warm-up — one pass at moderate size for every action, timings discarded.
        Console.Write("Warming up ... ");
        var warmSw = Stopwatch.StartNew();
        var warmData = DataFactory.BuildData(1_000, new Random(seed + 1));
        foreach (var b in benchmarks)
        {
            try { b.Invoke(warmData); } catch { /* warm-up errors are surfaced during the real run */ }
        }
        warmSw.Stop();
        Console.WriteLine($"done ({warmSw.Elapsed.TotalMilliseconds:F0} ms)");
        Console.WriteLine();

        var normalResults = worstOnly
            ? new List<ScenarioResult>()
            : RunScenario("Normal", benchmarks, iterations, rngNormal, useNormalDistribution: true);
        var worstResults  = RunScenario("Worst-case", benchmarks, iterations, rngWorst,  useNormalDistribution: false);

        Console.WriteLine();
        if (!worstOnly)
        {
            Reporter.PrintFullTable("NORMAL SCENARIO — sizes ~ N(10k, 5k) clamped to [1, 20k]", normalResults);
            Console.WriteLine();
        }
        Reporter.PrintFullTable($"WORST-CASE SCENARIO — sizes = {MaxSize}", worstResults);
        if (!worstOnly)
        {
            Console.WriteLine();
            Reporter.PrintComparison(normalResults, worstResults);
        }

        if (csvPath is not null)
        {
            Reporter.WriteCsv(csvPath, normalResults, worstResults);
            Console.WriteLine();
            Console.WriteLine($"CSV written to {csvPath}");
        }

        return 0;
    }

    private static List<ScenarioResult> RunScenario(
        string label,
        IReadOnlyList<Benchmark> benchmarks,
        int iterations,
        Random rng,
        bool useNormalDistribution)
    {
        Console.WriteLine($"[{label}] running {benchmarks.Count} action(s) × {iterations} iteration(s) ...");
        var results = new List<ScenarioResult>(benchmarks.Count);

        // Pre-generate size sequence once so both scenarios can be reproduced deterministically per action.
        var sizes = new int[iterations];
        for (int i = 0; i < iterations; i++)
        {
            sizes[i] = useNormalDistribution
                ? SampleTruncatedNormal(rng, MeanSize, StdDevSize, MinSize, MaxSize)
                : MaxSize;
        }

        int completed = 0;
        var swAll = Stopwatch.StartNew();
        foreach (var b in benchmarks)
        {
            var timings = new double[iterations];
            var actualSizes = new int[iterations];
            int failures = 0;

            var swAction = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                int size = sizes[i];
                actualSizes[i] = size;
                var data = DataFactory.BuildData(size, rng);

                var swIter = Stopwatch.StartNew();
                try
                {
                    b.Invoke(data);
                }
                catch
                {
                    failures++;
                }
                swIter.Stop();
                timings[i] = swIter.Elapsed.TotalMilliseconds;
            }
            swAction.Stop();

            results.Add(new ScenarioResult(b.Name, timings, actualSizes, failures, swAction.Elapsed.TotalMilliseconds));
            completed++;
            Console.Write($"  [{completed,2}/{benchmarks.Count}] {b.Name,-32}  median={Stats.Percentile(timings, 0.50):F3} ms  p95={Stats.Percentile(timings, 0.95):F3} ms");
            if (failures > 0) Console.Write($"  ({failures} error(s))");
            Console.WriteLine();
        }
        swAll.Stop();
        Console.WriteLine($"[{label}] total wall time: {swAll.Elapsed.TotalSeconds:F1} s");
        return results;
    }

    // Box-Muller truncated normal.
    private static int SampleTruncatedNormal(Random rng, double mean, double stdev, int lo, int hi)
    {
        for (int attempt = 0; attempt < 32; attempt++)
        {
            double u1 = 1.0 - rng.NextDouble();
            double u2 = 1.0 - rng.NextDouble();
            double z  = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
            double x  = mean + stdev * z;
            int rounded = (int)Math.Round(x, MidpointRounding.AwayFromZero);
            if (rounded >= lo && rounded <= hi) return rounded;
        }
        // Fallback: hard clamp on the last draw.
        double fallback = mean + stdev * (rng.NextDouble() * 2.0 - 1.0);
        return Math.Clamp((int)Math.Round(fallback), lo, hi);
    }

    private static void PrintUsage()
    {
        Console.WriteLine("Usage: dotnet run -c Release --project tools/LoadTest -- [options]");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  -n, --iterations <N>   Iterations per action per scenario (default 1000)");
        Console.WriteLine("      --seed <N>         RNG seed (default 42)");
        Console.WriteLine("      --csv <path>       Also write results to CSV");
        Console.WriteLine("      --only <a,b,c>     Comma-separated list of action names to run");
        Console.WriteLine("      --worst-only       Skip the Normal scenario; run only the Worst-case scenario");
        Console.WriteLine("  -h, --help             Show this help");
    }
}
