using System.Text.Json;
using System.Text.Json.Nodes;

namespace ListUtils;

public class ListUtils : IListUtils
{
    public void List_Pop(
        string SourceListJson,
        int Index,
        out string UpdatedListJson,
        out string PoppedElementJson)
    {
        if (string.IsNullOrEmpty(SourceListJson))
        {
            UpdatedListJson = "[]";
            PoppedElementJson = "null";
            return;
        }

        var array = JsonNode.Parse(SourceListJson)!.AsArray();

        if (Index < 0 || Index >= array.Count)
        {
            UpdatedListJson = SourceListJson;
            PoppedElementJson = "null";
            return;
        }

        var popped = array[Index];
        PoppedElementJson = popped?.ToJsonString(JsonOptions) ?? "null";
        array.RemoveAt(Index);
        UpdatedListJson = array.ToJsonString(JsonOptions);
    }

    public void List_PopMultiple(
        string SourceListJson,
        string IndicesToPop,
        out string UpdatedListJson,
        out string PoppedElementsJson)
    {
        if (string.IsNullOrEmpty(SourceListJson))
        {
            UpdatedListJson = "[]";
            PoppedElementsJson = "[]";
            return;
        }

        var array = JsonNode.Parse(SourceListJson)!.AsArray();

        if (string.IsNullOrEmpty(IndicesToPop))
        {
            UpdatedListJson = SourceListJson;
            PoppedElementsJson = "[]";
            return;
        }

        var indices = IndicesToPop.Split(',')
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

        UpdatedListJson = array.ToJsonString(JsonOptions);
        PoppedElementsJson = ordered.ToJsonString(JsonOptions);
    }

    public void List_PopByCondition(
        string SourceListJson,
        string PropertyName,
        string TargetValue,
        string ComparisonOperator,
        bool CaseSensitive,
        bool SearchFromEnd,
        out string UpdatedListJson,
        out string PoppedElementJson)
    {
        if (string.IsNullOrEmpty(SourceListJson) || string.IsNullOrEmpty(PropertyName))
        {
            UpdatedListJson = SourceListJson ?? "[]";
            PoppedElementJson = "{}";
            return;
        }

        var array = JsonNode.Parse(SourceListJson)!.AsArray();
        JsonNode? matchedNode = null;
        int matchedIndex = -1;

        if (SearchFromEnd)
        {
            for (int i = array.Count - 1; i >= 0; i--)
            {
                var value = GetPropertyValue(array[i]!, PropertyName);
                if (value != null && MatchesCondition(value, TargetValue, ComparisonOperator, CaseSensitive))
                {
                    matchedIndex = i;
                    break;
                }
            }
        }
        else
        {
            for (int i = 0; i < array.Count; i++)
            {
                var value = GetPropertyValue(array[i]!, PropertyName);
                if (value != null && MatchesCondition(value, TargetValue, ComparisonOperator, CaseSensitive))
                {
                    matchedIndex = i;
                    break;
                }
            }
        }

        if (matchedIndex >= 0)
        {
            matchedNode = array[matchedIndex];
            array.RemoveAt(matchedIndex);
        }

        UpdatedListJson = array.ToJsonString(JsonOptions);
        PoppedElementJson = matchedNode?.ToJsonString(JsonOptions) ?? "{}";
    }

    public void List_PopMultipleByCondition(
        string SourceListJson,
        string PropertyName,
        string TargetValue,
        string ComparisonOperator,
        bool CaseSensitive,
        out string UpdatedListJson,
        out string PoppedElementsJson)
    {
        if (string.IsNullOrEmpty(SourceListJson) || string.IsNullOrEmpty(PropertyName))
        {
            UpdatedListJson = SourceListJson ?? "[]";
            PoppedElementsJson = "[]";
            return;
        }

        var originalArray = JsonNode.Parse(SourceListJson)!.AsArray();
        var keptArray = new JsonArray();
        var poppedArray = new JsonArray();

        foreach (var item in originalArray)
        {
            var value = GetPropertyValue(item!, PropertyName);
            if (value != null && MatchesCondition(value, TargetValue, ComparisonOperator, CaseSensitive))
                poppedArray.Add(JsonNode.Parse(item!.ToJsonString())!);
            else
                keptArray.Add(JsonNode.Parse(item!.ToJsonString())!);
        }

        UpdatedListJson = keptArray.ToJsonString(JsonOptions);
        PoppedElementsJson = poppedArray.ToJsonString(JsonOptions);
    }

    public void List_PopByConditions(
        string SourceListJson,
        string ConditionsJson,
        string LogicalOperator,
        bool SearchFromEnd,
        out string UpdatedListJson,
        out string PoppedElementJson)
    {
        if (string.IsNullOrEmpty(SourceListJson))
        {
            UpdatedListJson = "[]";
            PoppedElementJson = "{}";
            return;
        }

        var conditions = ParseConditions(ConditionsJson);
        var array = JsonNode.Parse(SourceListJson)!.AsArray();

        if (conditions.Count == 0)
        {
            UpdatedListJson = SourceListJson;
            PoppedElementJson = "{}";
            return;
        }

        int matchedIndex = -1;
        if (SearchFromEnd)
        {
            for (int i = array.Count - 1; i >= 0; i--)
            {
                if (EvaluateConditions(array[i]!, conditions, LogicalOperator))
                {
                    matchedIndex = i;
                    break;
                }
            }
        }
        else
        {
            for (int i = 0; i < array.Count; i++)
            {
                if (EvaluateConditions(array[i]!, conditions, LogicalOperator))
                {
                    matchedIndex = i;
                    break;
                }
            }
        }

        JsonNode? matchedNode = null;
        if (matchedIndex >= 0)
        {
            matchedNode = array[matchedIndex];
            array.RemoveAt(matchedIndex);
        }

        UpdatedListJson = array.ToJsonString(JsonOptions);
        PoppedElementJson = matchedNode?.ToJsonString(JsonOptions) ?? "{}";
    }

    public void List_PopMultipleByConditions(
        string SourceListJson,
        string ConditionsJson,
        string LogicalOperator,
        out string UpdatedListJson,
        out string PoppedElementsJson)
    {
        if (string.IsNullOrEmpty(SourceListJson))
        {
            UpdatedListJson = "[]";
            PoppedElementsJson = "[]";
            return;
        }

        var conditions = ParseConditions(ConditionsJson);
        var originalArray = JsonNode.Parse(SourceListJson)!.AsArray();

        if (conditions.Count == 0)
        {
            UpdatedListJson = SourceListJson;
            PoppedElementsJson = "[]";
            return;
        }

        var keptArray = new JsonArray();
        var poppedArray = new JsonArray();

        foreach (var item in originalArray)
        {
            if (EvaluateConditions(item!, conditions, LogicalOperator))
                poppedArray.Add(JsonNode.Parse(item!.ToJsonString())!);
            else
                keptArray.Add(JsonNode.Parse(item!.ToJsonString())!);
        }

        UpdatedListJson = keptArray.ToJsonString(JsonOptions);
        PoppedElementsJson = poppedArray.ToJsonString(JsonOptions);
    }

    public void List_Zip(
        string ListAJson,
        string ListBJson,
        string KeyNameA,
        string KeyNameB,
        out string ZippedListJson)
    {
        if (string.IsNullOrEmpty(ListAJson) || string.IsNullOrEmpty(ListBJson))
        {
            ZippedListJson = "[]";
            return;
        }

        var arrA = JsonNode.Parse(ListAJson)!.AsArray();
        var arrB = JsonNode.Parse(ListBJson)!.AsArray();
        var result = new JsonArray();

        int minCount = Math.Min(arrA.Count, arrB.Count);
        for (int i = 0; i < minCount; i++)
        {
            var pair = new JsonObject
            {
                [KeyNameA] = JsonNode.Parse(arrA[i]!.ToJsonString()),
                [KeyNameB] = JsonNode.Parse(arrB[i]!.ToJsonString())
            };
            result.Add(pair);
        }

        ZippedListJson = result.ToJsonString(JsonOptions);
    }

    public void List_GroupBy(
        string SourceListJson,
        string PropertyName,
        out string GroupedListJson)
    {
        if (string.IsNullOrEmpty(SourceListJson))
        {
            GroupedListJson = "[]";
            return;
        }

        var array = JsonNode.Parse(SourceListJson)!.AsArray();
        var groups = new Dictionary<string, JsonArray>(StringComparer.Ordinal);
        var groupOrder = new List<string>();

        foreach (var item in array)
        {
            string key = GetPropertyValue(item!, PropertyName) ?? "Unknown";
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

        GroupedListJson = result.ToJsonString(JsonOptions);
    }

    public void List_Difference(
        string ListAJson,
        string ListBJson,
        string MatchKey,
        string ComparisonOperator,
        bool CaseSensitive,
        out string DifferenceListJson)
    {
        if (string.IsNullOrEmpty(ListAJson)) { DifferenceListJson = "[]"; return; }
        if (string.IsNullOrEmpty(ListBJson)) { DifferenceListJson = ListAJson; return; }

        var arrA = JsonNode.Parse(ListAJson)!.AsArray();
        var arrB = JsonNode.Parse(ListBJson)!.AsArray();

        var bValues = new List<string>();
        foreach (var b in arrB)
        {
            var k = GetPropertyValue(b!, MatchKey);
            if (k != null) bValues.Add(k);
        }

        var result = new JsonArray();
        foreach (var item in arrA)
        {
            var key = GetPropertyValue(item!, MatchKey);
            bool matchedAny = key != null && bValues.Any(bv => MatchesCondition(key, bv, ComparisonOperator, CaseSensitive));
            if (!matchedAny)
                result.Add(JsonNode.Parse(item!.ToJsonString())!);
        }

        DifferenceListJson = result.ToJsonString(JsonOptions);
    }

    private static string? GetPropertyValue(JsonNode node, string propertyPath)
    {
        if (string.IsNullOrEmpty(propertyPath)) return null;

        JsonNode? current = node;
        foreach (var segment in propertyPath.Split('.'))
        {
            current = NavigateSegment(current, segment);
            if (current == null) return null;
        }
        return current?.ToString();
    }

    private static JsonNode? NavigateSegment(JsonNode? current, string segment)
    {
        if (current == null || string.IsNullOrEmpty(segment)) return null;

        string name = segment;
        int? index = null;
        var bracketStart = segment.IndexOf('[');
        if (bracketStart >= 0)
        {
            var bracketEnd = segment.IndexOf(']', bracketStart);
            if (bracketEnd > bracketStart)
            {
                var idxStr = segment.Substring(bracketStart + 1, bracketEnd - bracketStart - 1);
                if (int.TryParse(idxStr, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var idx))
                {
                    name = segment.Substring(0, bracketStart);
                    index = idx;
                }
            }
        }

        JsonNode? next = current;
        if (!string.IsNullOrEmpty(name))
        {
            if (current is not JsonObject obj) return null;
            if (obj.TryGetPropertyValue(name, out var val) && val != null)
                next = val;
            else
            {
                string camel = ToCamelCase(name);
                if (obj.TryGetPropertyValue(camel, out val) && val != null)
                    next = val;
                else
                    return null;
            }
        }

        if (index.HasValue)
        {
            if (next is not JsonArray arr) return null;
            int i = index.Value < 0 ? arr.Count + index.Value : index.Value;
            if (i < 0 || i >= arr.Count) return null;
            return arr[i];
        }

        return next;
    }

    private static bool MatchesCondition(string actual, string target, string? op, bool caseSensitive)
    {
        var normalized = (op ?? "").Trim();
        var cmp = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
        switch (normalized.ToUpperInvariant())
        {
            case "NOTEQUALS":
            case "!=":
                return !actual.Equals(target, cmp);
            case "CONTAINS":
                return actual.Contains(target, cmp);
            case "STARTSWITH":
                return actual.StartsWith(target, cmp);
            case "ENDSWITH":
                return actual.EndsWith(target, cmp);
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
                return actual.Equals(target, cmp);
        }
    }

    private sealed class Condition
    {
        public string Path { get; set; } = "";
        public string Operator { get; set; } = "";
        public string Value { get; set; } = "";
        public bool CaseSensitive { get; set; }
    }

    private static List<Condition> ParseConditions(string conditionsJson)
    {
        var list = new List<Condition>();
        if (string.IsNullOrWhiteSpace(conditionsJson)) return list;
        var arr = JsonNode.Parse(conditionsJson)!.AsArray();
        foreach (var node in arr)
        {
            if (node is not JsonObject obj) continue;
            list.Add(new Condition
            {
                Path = obj["path"]?.ToString() ?? obj["Path"]?.ToString() ?? "",
                Operator = obj["operator"]?.ToString() ?? obj["Operator"]?.ToString() ?? "",
                Value = obj["value"]?.ToString() ?? obj["Value"]?.ToString() ?? "",
                CaseSensitive = (obj["caseSensitive"]?.GetValue<bool>() ?? obj["CaseSensitive"]?.GetValue<bool>()) ?? false,
            });
        }
        return list;
    }

    private static bool EvaluateConditions(JsonNode item, List<Condition> conditions, string logicalOperator)
    {
        if (conditions.Count == 0) return false;
        bool useOr = (logicalOperator ?? "").Trim().Equals("OR", StringComparison.OrdinalIgnoreCase);
        foreach (var c in conditions)
        {
            var actual = GetPropertyValue(item, c.Path);
            bool match = actual != null && MatchesCondition(actual, c.Value, c.Operator, c.CaseSensitive);
            if (useOr && match) return true;
            if (!useOr && !match) return false;
        }
        return !useOr;
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
