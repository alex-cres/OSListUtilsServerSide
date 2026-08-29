using System.Text.Json;
using System.Text.Json.Nodes;

namespace ListUtils;

public class ListUtils : IListUtils
{
    public void List_Pop(
        string sourceListJson,
        int index,
        out string updatedListJson,
        out string poppedElementJson)
    {
        if (string.IsNullOrEmpty(sourceListJson))
        {
            updatedListJson = "[]";
            poppedElementJson = "null";
            return;
        }

        var array = JsonNode.Parse(sourceListJson)!.AsArray();

        if (index < 0 || index >= array.Count)
        {
            updatedListJson = sourceListJson;
            poppedElementJson = "null";
            return;
        }

        var popped = array[index];
        poppedElementJson = popped?.ToJsonString(JsonOptions) ?? "null";
        array.RemoveAt(index);
        updatedListJson = array.ToJsonString(JsonOptions);
    }

    public void List_PopMultiple(
        string sourceListJson,
        string indicesToPop,
        out string updatedListJson,
        out string poppedElementsJson)
    {
        if (string.IsNullOrEmpty(sourceListJson))
        {
            updatedListJson = "[]";
            poppedElementsJson = "[]";
            return;
        }

        var array = JsonNode.Parse(sourceListJson)!.AsArray();

        if (string.IsNullOrEmpty(indicesToPop))
        {
            updatedListJson = sourceListJson;
            poppedElementsJson = "[]";
            return;
        }

        var indices = indicesToPop.Split(',')
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

        // Reverse so popped elements are in original order
        var ordered = new JsonArray();
        for (int i = poppedArray.Count - 1; i >= 0; i--)
            ordered.Add(JsonNode.Parse(poppedArray[i]!.ToJsonString())!);

        updatedListJson = array.ToJsonString(JsonOptions);
        poppedElementsJson = ordered.ToJsonString(JsonOptions);
    }

    public void List_PopByCondition(
        string sourceListJson,
        string propertyName,
        string targetValue,
        string comparisonOperator,
        out string updatedListJson,
        out string poppedElementJson)
    {
        if (string.IsNullOrEmpty(sourceListJson) || string.IsNullOrEmpty(propertyName))
        {
            updatedListJson = sourceListJson ?? "[]";
            poppedElementJson = "{}";
            return;
        }

        var array = JsonNode.Parse(sourceListJson)!.AsArray();
        JsonNode? matchedNode = null;

        for (int i = 0; i < array.Count; i++)
        {
            var value = GetPropertyValue(array[i]!, propertyName);
            if (value != null && MatchesCondition(value, targetValue, comparisonOperator))
            {
                matchedNode = array[i];
                array.RemoveAt(i);
                break;
            }
        }

        updatedListJson = array.ToJsonString(JsonOptions);
        poppedElementJson = matchedNode?.ToJsonString(JsonOptions) ?? "{}";
    }

    public void List_PopMultipleByCondition(
        string sourceListJson,
        string propertyName,
        string targetValue,
        string comparisonOperator,
        out string updatedListJson,
        out string poppedElementsJson)
    {
        if (string.IsNullOrEmpty(sourceListJson) || string.IsNullOrEmpty(propertyName))
        {
            updatedListJson = sourceListJson ?? "[]";
            poppedElementsJson = "[]";
            return;
        }

        var originalArray = JsonNode.Parse(sourceListJson)!.AsArray();
        var keptArray = new JsonArray();
        var poppedArray = new JsonArray();

        foreach (var item in originalArray)
        {
            var value = GetPropertyValue(item!, propertyName);
            if (value != null && MatchesCondition(value, targetValue, comparisonOperator))
            {
                poppedArray.Add(JsonNode.Parse(item!.ToJsonString())!);
            }
            else
            {
                keptArray.Add(JsonNode.Parse(item!.ToJsonString())!);
            }
        }

        updatedListJson = keptArray.ToJsonString(JsonOptions);
        poppedElementsJson = poppedArray.ToJsonString(JsonOptions);
    }

    public void List_Zip(
        string listAJson,
        string listBJson,
        string keyNameA,
        string keyNameB,
        out string zippedListJson)
    {
        if (string.IsNullOrEmpty(listAJson) || string.IsNullOrEmpty(listBJson))
        {
            zippedListJson = "[]";
            return;
        }

        var arrA = JsonNode.Parse(listAJson)!.AsArray();
        var arrB = JsonNode.Parse(listBJson)!.AsArray();
        var result = new JsonArray();

        int minCount = Math.Min(arrA.Count, arrB.Count);
        for (int i = 0; i < minCount; i++)
        {
            var pair = new JsonObject
            {
                [keyNameA] = JsonNode.Parse(arrA[i]!.ToJsonString()),
                [keyNameB] = JsonNode.Parse(arrB[i]!.ToJsonString())
            };
            result.Add(pair);
        }

        zippedListJson = result.ToJsonString(JsonOptions);
    }

    public void List_GroupBy(
        string sourceListJson,
        string propertyName,
        out string groupedListJson)
    {
        if (string.IsNullOrEmpty(sourceListJson))
        {
            groupedListJson = "[]";
            return;
        }

        var array = JsonNode.Parse(sourceListJson)!.AsArray();
        var groups = new Dictionary<string, JsonArray>(StringComparer.Ordinal);
        var groupOrder = new List<string>();

        foreach (var item in array)
        {
            string key = GetPropertyValue(item!, propertyName) ?? "Unknown";
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

        groupedListJson = result.ToJsonString(JsonOptions);
    }

    public void List_Difference(
        string listAJson,
        string listBJson,
        string matchKey,
        string comparisonOperator,
        out string differenceListJson)
    {
        if (string.IsNullOrEmpty(listAJson)) { differenceListJson = "[]"; return; }
        if (string.IsNullOrEmpty(listBJson)) { differenceListJson = listAJson; return; }

        var arrA = JsonNode.Parse(listAJson)!.AsArray();
        var arrB = JsonNode.Parse(listBJson)!.AsArray();

        var bValues = new List<string>();
        foreach (var b in arrB)
        {
            var k = GetPropertyValue(b!, matchKey);
            if (k != null) bValues.Add(k);
        }

        var result = new JsonArray();
        foreach (var item in arrA)
        {
            var key = GetPropertyValue(item!, matchKey);
            bool matchedAny = key != null && bValues.Any(bv => MatchesCondition(key, bv, comparisonOperator));
            if (!matchedAny)
            {
                result.Add(JsonNode.Parse(item!.ToJsonString())!);
            }
        }

        differenceListJson = result.ToJsonString(JsonOptions);
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

    private static bool MatchesCondition(string actual, string target, string? op)
    {
        var normalized = (op ?? "").Trim();
        switch (normalized.ToUpperInvariant())
        {
            case "NOTEQUALS":
            case "!=":
                return !actual.Equals(target, StringComparison.OrdinalIgnoreCase);
            case "CONTAINS":
                return actual.Contains(target, StringComparison.OrdinalIgnoreCase);
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
        return char.ToLowerInvariant(str[0]) + str[1..];
    }

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = false };
}
