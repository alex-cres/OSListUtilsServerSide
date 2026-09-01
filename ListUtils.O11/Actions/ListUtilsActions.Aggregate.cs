using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json.Nodes;

namespace OutSystems.NssListUtils;

public partial class CssListUtils
{
    public void MssList_MinBy(
        string ssSourceListJson,
        string ssPropertyName,
        bool ssNumericMode,
        out string ssElementJson,
        out string ssMinValue,
        out int ssMinIndex)
    {
        MinByOrMaxBy(ssSourceListJson, ssPropertyName, ssNumericMode, isMax: false, out ssElementJson, out ssMinValue, out ssMinIndex);
    }

    public void MssList_MaxBy(
        string ssSourceListJson,
        string ssPropertyName,
        bool ssNumericMode,
        out string ssElementJson,
        out string ssMaxValue,
        out int ssMaxIndex)
    {
        MinByOrMaxBy(ssSourceListJson, ssPropertyName, ssNumericMode, isMax: true, out ssElementJson, out ssMaxValue, out ssMaxIndex);
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
        var segments = SplitPath(propertyName);

        decimal bestNumeric = 0m;
        string? bestText = null;
        JsonNode? bestNode = null;
        int bestIdx = -1;

        for (int i = 0; i < array.Count; i++)
        {
            var value = GetPropertyValue(array[i]!, segments);
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

        elementJson = bestNode == null ? "null" : bestNode.ToJsonString(JsonOptions);
        boundaryValue = bestText ?? "";
        boundaryIndex = bestIdx;
    }

    public void MssList_Aggregate(
        string ssSourceListJson,
        string ssPropertyName,
        string ssOperation,
        out string ssResultValue,
        out int ssMatchedCount)
    {
        ssResultValue = "";
        ssMatchedCount = 0;

        if (string.IsNullOrEmpty(ssSourceListJson))
            return;

        var array = JsonNode.Parse(ssSourceListJson)!.AsArray();
        var normalized = (ssOperation ?? "").Trim().ToUpperInvariant();
        if (normalized.Length == 0) normalized = "SUM";
        var invariant = CultureInfo.InvariantCulture;
        var numStyle = NumberStyles.Any;
        var segments = SplitPath(ssPropertyName);

        if (normalized == "COUNT" || normalized == "COUNTDISTINCT")
        {
            var seen = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < array.Count; i++)
            {
                var v = GetPropertyValue(array[i]!, segments);
                if (v == null) continue;
                ssMatchedCount++;
                if (normalized == "COUNTDISTINCT") seen.Add(v);
            }
            int outCount = normalized == "COUNT" ? ssMatchedCount : seen.Count;
            ssResultValue = outCount.ToString(invariant);
            return;
        }

        decimal sum = 0m;
        decimal min = 0m;
        decimal max = 0m;
        for (int i = 0; i < array.Count; i++)
        {
            var v = GetPropertyValue(array[i]!, segments);
            if (v == null) continue;
            if (!decimal.TryParse(v, numStyle, invariant, out var d)) continue;
            if (ssMatchedCount == 0) { min = max = d; }
            else { if (d < min) min = d; if (d > max) max = d; }
            sum += d;
            ssMatchedCount++;
        }

        if (ssMatchedCount == 0) return;

        decimal result;
        switch (normalized)
        {
            case "SUM": result = sum; break;
            case "AVG": result = sum / ssMatchedCount; break;
            case "MIN": result = min; break;
            case "MAX": result = max; break;
            default: result = sum; break;
        }
        ssResultValue = result.ToString(invariant);
    }
}
