using System.Globalization;
using System.Text;

namespace LoadTest;

internal sealed record ScenarioResult(
    string   Name,
    double[] Timings,
    int[]    Sizes,
    int      Failures,
    double   TotalWallMs)
{
    public Summary Summary { get; } = Stats.Compute(Timings);
}

internal static class Reporter
{
    private static readonly (string Header, int Width)[] Columns =
    [
        ("Action",     35),
        ("N",           6),
        ("Err",         4),
        ("Min",         9),
        ("Mean",        9),
        ("StdDev",      9),
        ("Q1(25%)",     9),
        ("Median",      9),
        ("Q3(75%)",     9),
        ("P80",         9),
        ("P95",         9),
        ("Max",         9),
        ("Total(ms)", 11),
    ];

    public static void PrintFullTable(string title, IReadOnlyList<ScenarioResult> results)
    {
        Console.WriteLine(title);
        Console.WriteLine(new string('=', title.Length));
        PrintHeader();
        foreach (var r in results)
        {
            var s = r.Summary;
            PrintRow(
                r.Name,
                s.Count.ToString(CultureInfo.InvariantCulture),
                r.Failures.ToString(CultureInfo.InvariantCulture),
                F(s.Min), F(s.Mean), F(s.StdDev),
                F(s.Q1), F(s.Median), F(s.Q3),
                F(s.P80), F(s.P95), F(s.Max),
                r.TotalWallMs.ToString("F1", CultureInfo.InvariantCulture));
        }
    }

    public static void PrintComparison(IReadOnlyList<ScenarioResult> normal, IReadOnlyList<ScenarioResult> worst)
    {
        Console.WriteLine("COMPARISON — Normal vs Worst-case (all times in ms)");
        Console.WriteLine("====================================================");
        var byName = worst.ToDictionary(r => r.Name);

        var head = new[]
        {
            ("Action",   35),
            ("N.med",     9),
            ("W.med",     9),
            ("Δmed×",     8),
            ("N.p95",     9),
            ("W.p95",     9),
            ("Δp95×",     8),
            ("N.max",     9),
            ("W.max",     9),
        };
        var sb = new StringBuilder();
        foreach (var (h, w) in head) sb.Append(h.PadRight(w)).Append("  ");
        Console.WriteLine(sb.ToString().TrimEnd());
        Console.WriteLine(new string('-', sb.Length - 2));

        foreach (var n in normal)
        {
            if (!byName.TryGetValue(n.Name, out var w)) continue;
            var ns = n.Summary;
            var ws = w.Summary;
            double rMed = ns.Median > 0 ? ws.Median / ns.Median : double.NaN;
            double rP95 = ns.P95    > 0 ? ws.P95    / ns.P95    : double.NaN;

            sb.Clear();
            sb.Append(Truncate(n.Name, 35).PadRight(35)).Append("  ");
            sb.Append(F(ns.Median).PadLeft(9)).Append("  ");
            sb.Append(F(ws.Median).PadLeft(9)).Append("  ");
            sb.Append(FormatRatio(rMed).PadLeft(8)).Append("  ");
            sb.Append(F(ns.P95).PadLeft(9)).Append("  ");
            sb.Append(F(ws.P95).PadLeft(9)).Append("  ");
            sb.Append(FormatRatio(rP95).PadLeft(8)).Append("  ");
            sb.Append(F(ns.Max).PadLeft(9)).Append("  ");
            sb.Append(F(ws.Max).PadLeft(9));
            Console.WriteLine(sb.ToString());
        }
    }

    public static void WriteCsv(string path, IReadOnlyList<ScenarioResult> normal, IReadOnlyList<ScenarioResult> worst)
    {
        using var w = new StreamWriter(path);
        w.WriteLine("scenario,action,iterations,failures,min_ms,mean_ms,stdev_ms,q1_ms,median_ms,q3_ms,p80_ms,p95_ms,max_ms,total_wall_ms");
        WriteRows(w, "normal", normal);
        WriteRows(w, "worst",  worst);
    }

    private static void WriteRows(StreamWriter w, string scenario, IReadOnlyList<ScenarioResult> results)
    {
        foreach (var r in results)
        {
            var s = r.Summary;
            var ci = CultureInfo.InvariantCulture;
            w.WriteLine(string.Join(",",
                scenario,
                r.Name,
                s.Count.ToString(ci),
                r.Failures.ToString(ci),
                s.Min.ToString("F4", ci),
                s.Mean.ToString("F4", ci),
                s.StdDev.ToString("F4", ci),
                s.Q1.ToString("F4", ci),
                s.Median.ToString("F4", ci),
                s.Q3.ToString("F4", ci),
                s.P80.ToString("F4", ci),
                s.P95.ToString("F4", ci),
                s.Max.ToString("F4", ci),
                r.TotalWallMs.ToString("F1", ci)));
        }
    }

    private static void PrintHeader()
    {
        var sb = new StringBuilder();
        foreach (var (h, wid) in Columns) sb.Append(PadColumn(h, wid)).Append("  ");
        Console.WriteLine(sb.ToString().TrimEnd());
        Console.WriteLine(new string('-', sb.Length - 2));
    }

    private static void PrintRow(params string[] cells)
    {
        var sb = new StringBuilder();
        for (int i = 0; i < cells.Length; i++)
        {
            var (_, wid) = Columns[i];
            sb.Append(PadColumn(cells[i], wid, leftAlign: i == 0)).Append("  ");
        }
        Console.WriteLine(sb.ToString().TrimEnd());
    }

    private static string PadColumn(string value, int width, bool leftAlign = false)
    {
        value = Truncate(value, width);
        return leftAlign ? value.PadRight(width) : value.PadLeft(width);
    }

    private static string Truncate(string s, int max) => s.Length <= max ? s : s[..max];
    private static string F(double v) => v.ToString("F3", CultureInfo.InvariantCulture);
    private static string FormatRatio(double v) => double.IsNaN(v) || double.IsInfinity(v) ? "—" : $"{v:F2}×";
}
