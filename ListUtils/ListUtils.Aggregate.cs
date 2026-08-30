using System.Globalization;
using System.Text.Json.Nodes;

namespace ListUtils;

public partial class ListUtils
{
    public void List_MinBy(
        string SourceListJson,
        string PropertyName,
        bool NumericMode,
        out string ElementJson,
        out string MinValue,
        out int MinIndex)
    {
        MinByOrMaxBy(SourceListJson, PropertyName, NumericMode, isMax: false, out ElementJson, out MinValue, out MinIndex);
    }

    public void List_MaxBy(
        string SourceListJson,
        string PropertyName,
        bool NumericMode,
        out string ElementJson,
        out string MaxValue,
        out int MaxIndex)
    {
        MinByOrMaxBy(SourceListJson, PropertyName, NumericMode, isMax: true, out ElementJson, out MaxValue, out MaxIndex);
    }

    private static void MinByOrMaxBy(
        string sourceListJson,
        string propertyName,
        bool numericMode,
        bool isMax,
        out string elementJson,
        out string boundaryValue,
        out int boundaryIndex)
    {
        elementJson = "null";
        boundaryValue = "";
        boundaryIndex = -1;

        if (string.IsNullOrEmpty(sourceListJson) || string.IsNullOrEmpty(propertyName))
            return;

        var array = JsonNode.Parse(sourceListJson)!.AsArray();
        var invariant = CultureInfo.InvariantCulture;
        var numStyle = NumberStyles.Any;
        var cmp = StringComparer.OrdinalIgnoreCase;

        decimal bestNumeric = 0m;
        string? bestText = null;
        JsonNode? bestNode = null;
        int bestIdx = -1;

        for (int i = 0; i < array.Count; i++)
        {
            var value = GetPropertyValue(array[i]!, propertyName);
            if (value == null) continue;

            if (numericMode)
            {
                if (!decimal.TryParse(value, numStyle, invariant, out var d)) continue;
                bool take = bestIdx < 0
                    || (isMax ? d > bestNumeric : d < bestNumeric);
                if (take)
                {
                    bestNumeric = d;
                    bestText = value;
                    bestNode = array[i];
                    bestIdx = i;
                }
            }
            else
            {
                if (bestText == null)
                {
                    bestText = value;
                    bestNode = array[i];
                    bestIdx = i;
                }
                else
                {
                    int c = cmp.Compare(value, bestText);
                    if (isMax ? c > 0 : c < 0)
                    {
                        bestText = value;
                        bestNode = array[i];
                        bestIdx = i;
                    }
                }
            }
        }

        if (bestIdx < 0) return;

        elementJson = bestNode?.ToJsonString(JsonOptions) ?? "null";
        boundaryValue = bestText ?? "";
        boundaryIndex = bestIdx;
    }

    public void List_Aggregate(
        string SourceListJson,
        string PropertyName,
        string Operation,
        out string ResultValue,
        out int MatchedCount)
    {
        ResultValue = "";
        MatchedCount = 0;

        if (string.IsNullOrEmpty(SourceListJson))
            return;

        var array = JsonNode.Parse(SourceListJson)!.AsArray();
        var normalized = (Operation ?? "").Trim().ToUpperInvariant();
        if (normalized.Length == 0) normalized = "SUM";
        var invariant = CultureInfo.InvariantCulture;
        var numStyle = NumberStyles.Any;

        if (normalized == "COUNT" || normalized == "COUNTDISTINCT")
        {
            var seen = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < array.Count; i++)
            {
                var v = GetPropertyValue(array[i]!, PropertyName);
                if (v == null) continue;
                MatchedCount++;
                if (normalized == "COUNTDISTINCT") seen.Add(v);
            }
            int outCount = normalized == "COUNT" ? MatchedCount : seen.Count;
            ResultValue = outCount.ToString(invariant);
            return;
        }

        decimal sum = 0m;
        decimal min = 0m;
        decimal max = 0m;
        for (int i = 0; i < array.Count; i++)
        {
            var v = GetPropertyValue(array[i]!, PropertyName);
            if (v == null) continue;
            if (!decimal.TryParse(v, numStyle, invariant, out var d)) continue;
            if (MatchedCount == 0) { min = max = d; }
            else { if (d < min) min = d; if (d > max) max = d; }
            sum += d;
            MatchedCount++;
        }

        if (MatchedCount == 0) return;

        decimal result = normalized switch
        {
            "SUM" => sum,
            "AVG" => sum / MatchedCount,
            "MIN" => min,
            "MAX" => max,
            _ => sum,
        };
        ResultValue = result.ToString(invariant);
    }
}
