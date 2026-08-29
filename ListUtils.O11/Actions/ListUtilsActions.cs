using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace OutSystems.NssListUtils;

public class CssListUtils : IssListUtils
{
    public void MssList_Pop(
        string ssSourceListJson,
        int ssIndex,
        out string ssUpdatedListJson,
        out string ssPoppedElementJson)
    {
        if (string.IsNullOrEmpty(ssSourceListJson))
        {
            ssUpdatedListJson = "[]";
            ssPoppedElementJson = "null";
            return;
        }

        var array = JsonNode.Parse(ssSourceListJson)!.AsArray();

        if (ssIndex < 0 || ssIndex >= array.Count)
        {
            ssUpdatedListJson = ssSourceListJson;
            ssPoppedElementJson = "null";
            return;
        }

        var popped = array[ssIndex];
        ssPoppedElementJson = popped?.ToJsonString(JsonOptions) ?? "null";
        array.RemoveAt(ssIndex);
        ssUpdatedListJson = array.ToJsonString(JsonOptions);
    }

    public void MssList_PopMultiple(
        string ssSourceListJson,
        string ssIndicesToPop,
        out string ssUpdatedListJson,
        out string ssPoppedElementsJson)
    {
        if (string.IsNullOrEmpty(ssSourceListJson))
        {
            ssUpdatedListJson = "[]";
            ssPoppedElementsJson = "[]";
            return;
        }

        var array = JsonNode.Parse(ssSourceListJson)!.AsArray();

        if (string.IsNullOrEmpty(ssIndicesToPop))
        {
            ssUpdatedListJson = ssSourceListJson;
            ssPoppedElementsJson = "[]";
            return;
        }

        var indices = ssIndicesToPop.Split(',')
            .Select(s => int.TryParse(s.Trim(), out var v) ? v : -1)
            .Where(i => i >= 0)
            .Distinct()
            .OrderByDescending(i => i)
            .ToList();

        var poppedArray = new JsonArray();

        foreach (int idx in indices)
        {
            if (idx < array.Count)
            {
                var item = array[idx];
                poppedArray.Add(JsonNode.Parse(item!.ToJsonString())!);
                array.RemoveAt(idx);
            }
        }

        var ordered = new JsonArray();
        for (int i = poppedArray.Count - 1; i >= 0; i--)
            ordered.Add(JsonNode.Parse(poppedArray[i]!.ToJsonString())!);

        ssUpdatedListJson = array.ToJsonString(JsonOptions);
        ssPoppedElementsJson = ordered.ToJsonString(JsonOptions);
    }

    public void MssList_PopByCondition(
        string ssSourceListJson,
        string ssPropertyName,
        string ssTargetValue,
        string ssComparisonOperator,
        out string ssUpdatedListJson,
        out string ssPoppedElementJson)
    {
        if (string.IsNullOrEmpty(ssSourceListJson) || string.IsNullOrEmpty(ssPropertyName))
        {
            ssUpdatedListJson = ssSourceListJson ?? "[]";
            ssPoppedElementJson = "{}";
            return;
        }

        var array = JsonNode.Parse(ssSourceListJson)!.AsArray();
        JsonNode? matchedNode = null;

        for (int i = 0; i < array.Count; i++)
        {
            var value = GetPropertyValue(array[i]!, ssPropertyName);
            if (value != null && MatchesCondition(value, ssTargetValue, ssComparisonOperator))
            {
                matchedNode = array[i];
                array.RemoveAt(i);
                break;
            }
        }

        ssUpdatedListJson = array.ToJsonString(JsonOptions);
        ssPoppedElementJson = matchedNode?.ToJsonString(JsonOptions) ?? "{}";
    }

    public void MssList_PopMultipleByCondition(
        string ssSourceListJson,
        string ssPropertyName,
        string ssTargetValue,
        string ssComparisonOperator,
        out string ssUpdatedListJson,
        out string ssPoppedElementsJson)
    {
        if (string.IsNullOrEmpty(ssSourceListJson) || string.IsNullOrEmpty(ssPropertyName))
        {
            ssUpdatedListJson = ssSourceListJson ?? "[]";
            ssPoppedElementsJson = "[]";
            return;
        }

        var originalArray = JsonNode.Parse(ssSourceListJson)!.AsArray();
        var keptArray = new JsonArray();
        var poppedArray = new JsonArray();

        foreach (var item in originalArray)
        {
            var value = GetPropertyValue(item!, ssPropertyName);
            if (value != null && MatchesCondition(value, ssTargetValue, ssComparisonOperator))
            {
                poppedArray.Add(JsonNode.Parse(item!.ToJsonString())!);
            }
            else
            {
                keptArray.Add(JsonNode.Parse(item!.ToJsonString())!);
            }
        }

        ssUpdatedListJson = keptArray.ToJsonString(JsonOptions);
        ssPoppedElementsJson = poppedArray.ToJsonString(JsonOptions);
    }

    public void MssList_Zip(
        string ssListAJson,
        string ssListBJson,
        string ssKeyNameA,
        string ssKeyNameB,
        out string ssZippedListJson)
    {
        if (string.IsNullOrEmpty(ssListAJson) || string.IsNullOrEmpty(ssListBJson))
        {
            ssZippedListJson = "[]";
            return;
        }

        var arrA = JsonNode.Parse(ssListAJson)!.AsArray();
        var arrB = JsonNode.Parse(ssListBJson)!.AsArray();
        var result = new JsonArray();

        int minCount = Math.Min(arrA.Count, arrB.Count);
        for (int i = 0; i < minCount; i++)
        {
            var pair = new JsonObject
            {
                [ssKeyNameA] = JsonNode.Parse(arrA[i]!.ToJsonString()),
                [ssKeyNameB] = JsonNode.Parse(arrB[i]!.ToJsonString())
            };
            result.Add(pair);
        }

        ssZippedListJson = result.ToJsonString(JsonOptions);
    }

    public void MssList_GroupBy(
        string ssSourceListJson,
        string ssPropertyName,
        out string ssGroupedListJson)
    {
        if (string.IsNullOrEmpty(ssSourceListJson))
        {
            ssGroupedListJson = "[]";
            return;
        }

        var array = JsonNode.Parse(ssSourceListJson)!.AsArray();
        var groups = new Dictionary<string, JsonArray>(StringComparer.Ordinal);
        var groupOrder = new List<string>();

        foreach (var item in array)
        {
            string key = GetPropertyValue(item!, ssPropertyName) ?? "Unknown";
            if (!groups.ContainsKey(key))
            {
                groups[key] = new JsonArray();
                groupOrder.Add(key);
            }
            groups[key].Add(JsonNode.Parse(item!.ToJsonString())!);
        }

        var result = new JsonArray();
        foreach (var key in groupOrder)
        {
            var groupObj = new JsonObject
            {
                ["Key"] = key,
                ["Items"] = groups[key]
            };
            result.Add(groupObj);
        }

        ssGroupedListJson = result.ToJsonString(JsonOptions);
    }

    public void MssList_Difference(
        string ssListAJson,
        string ssListBJson,
        string ssMatchKey,
        string ssComparisonOperator,
        out string ssDifferenceListJson)
    {
        if (string.IsNullOrEmpty(ssListAJson)) { ssDifferenceListJson = "[]"; return; }
        if (string.IsNullOrEmpty(ssListBJson)) { ssDifferenceListJson = ssListAJson; return; }

        var arrA = JsonNode.Parse(ssListAJson)!.AsArray();
        var arrB = JsonNode.Parse(ssListBJson)!.AsArray();

        var bValues = new List<string>();
        foreach (var b in arrB)
        {
            var k = GetPropertyValue(b!, ssMatchKey);
            if (k != null) bValues.Add(k);
        }

        var result = new JsonArray();
        foreach (var item in arrA)
        {
            var key = GetPropertyValue(item!, ssMatchKey);
            bool matchedAny = key != null && bValues.Any(bv => MatchesCondition(key, bv, ssComparisonOperator));
            if (!matchedAny)
            {
                result.Add(JsonNode.Parse(item!.ToJsonString())!);
            }
        }

        ssDifferenceListJson = result.ToJsonString(JsonOptions);
    }

    private static string? GetPropertyValue(JsonNode node, string propertyPath)
    {
        if (string.IsNullOrEmpty(propertyPath)) return null;

        JsonNode? current = node;
        foreach (var segment in propertyPath.Split('.'))
        {
            if (current is not JsonObject obj) return null;
            if (obj.TryGetPropertyValue(segment, out var val) && val != null)
            {
                current = val;
                continue;
            }
            string camel = ToCamelCase(segment);
            if (obj.TryGetPropertyValue(camel, out val) && val != null)
            {
                current = val;
                continue;
            }
            return null;
        }

        return current?.ToString();
    }

    private static bool MatchesCondition(string actual, string target, string op)
    {
        var normalized = (op ?? "").Trim();
        switch (normalized.ToUpperInvariant())
        {
            case "NOTEQUALS":
            case "!=":
                return !actual.Equals(target, StringComparison.OrdinalIgnoreCase);
            case "CONTAINS":
                return actual.IndexOf(target, StringComparison.OrdinalIgnoreCase) >= 0;
            case "STARTSWITH":
                return actual.StartsWith(target, StringComparison.OrdinalIgnoreCase);
            case "ENDSWITH":
                return actual.EndsWith(target, StringComparison.OrdinalIgnoreCase);
            case "GREATERTHAN":
            case ">":
                return TryCompareNumeric(actual, target, out int gt) && gt > 0;
            case "LESSTHAN":
            case "<":
                return TryCompareNumeric(actual, target, out int lt) && lt < 0;
            case "GREATEROREQUAL":
            case ">=":
                return TryCompareNumeric(actual, target, out int ge) && ge >= 0;
            case "LESSOREQUAL":
            case "<=":
                return TryCompareNumeric(actual, target, out int le) && le <= 0;
            default:
                return actual.Equals(target, StringComparison.OrdinalIgnoreCase);
        }
    }

    private static bool TryCompareNumeric(string a, string b, out int result)
    {
        result = 0;
        if (!decimal.TryParse(a, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var da)) return false;
        if (!decimal.TryParse(b, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var db)) return false;
        result = da.CompareTo(db);
        return true;
    }

    private static string ToCamelCase(string str)
    {
        if (string.IsNullOrEmpty(str) || !char.IsUpper(str[0])) return str;
        return char.ToLowerInvariant(str[0]) + str.Substring(1);
    }

    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions { WriteIndented = false };
}
