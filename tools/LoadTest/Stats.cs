namespace LoadTest;

internal static class Stats
{
    // Linear-interpolation percentile on a copy-sorted array (Excel PERCENTILE.INC / numpy default).
    public static double Percentile(double[] values, double p)
    {
        if (values.Length == 0) return double.NaN;
        var sorted = (double[])values.Clone();
        Array.Sort(sorted);
        return PercentileSorted(sorted, p);
    }

    public static double PercentileSorted(double[] sorted, double p)
    {
        int n = sorted.Length;
        if (n == 0) return double.NaN;
        if (n == 1) return sorted[0];
        double rank = p * (n - 1);
        int lo = (int)Math.Floor(rank);
        int hi = (int)Math.Ceiling(rank);
        if (lo == hi) return sorted[lo];
        double frac = rank - lo;
        return sorted[lo] + frac * (sorted[hi] - sorted[lo]);
    }

    public static Summary Compute(double[] timings)
    {
        int n = timings.Length;
        if (n == 0) return new Summary();

        var sorted = (double[])timings.Clone();
        Array.Sort(sorted);

        double sum = 0;
        for (int i = 0; i < n; i++) sum += sorted[i];
        double mean = sum / n;

        double variance = 0;
        if (n > 1)
        {
            for (int i = 0; i < n; i++)
            {
                double d = sorted[i] - mean;
                variance += d * d;
            }
            variance /= (n - 1);
        }

        return new Summary
        {
            Count  = n,
            Min    = sorted[0],
            Max    = sorted[n - 1],
            Mean   = mean,
            StdDev = Math.Sqrt(variance),
            Q1     = PercentileSorted(sorted, 0.25),
            Median = PercentileSorted(sorted, 0.50),
            Q3     = PercentileSorted(sorted, 0.75),
            P80    = PercentileSorted(sorted, 0.80),
            P95    = PercentileSorted(sorted, 0.95),
            Sum    = sum,
        };
    }
}

internal struct Summary
{
    public int    Count;
    public double Min;
    public double Max;
    public double Mean;
    public double StdDev;
    public double Q1;
    public double Median;
    public double Q3;
    public double P80;
    public double P95;
    public double Sum;
}
