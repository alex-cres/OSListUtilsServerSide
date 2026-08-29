using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace OutSystems.NssListUtils;

public partial class CssListUtils
{
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

    private static bool MatchesCondition(string actual, string target, string op, bool caseSensitive)
    {
        var normalized = (op ?? "").Trim();
        var cmp = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
        switch (normalized.ToUpperInvariant())
        {
            case "NOTEQUALS":
            case "!=":
                return !actual.Equals(target, cmp);
            case "CONTAINS":
                return actual.IndexOf(target, cmp) >= 0;
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
        return char.ToLowerInvariant(str[0]) + str.Substring(1);
    }

    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions { WriteIndented = false };
}
