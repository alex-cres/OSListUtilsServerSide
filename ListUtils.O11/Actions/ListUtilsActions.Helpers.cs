using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace OutSystems.NssListUtils;

public partial class CssListUtils
{
    // Splits a dotted property path once so hot loops can walk it without allocating per row.
    private static string[]? SplitPath(string? propertyPath)
        => string.IsNullOrEmpty(propertyPath) ? null : propertyPath!.Split('.');

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

    // Pre-split overload — walks cached segments without a per-call Split allocation.
    private static string? GetPropertyValue(JsonNode node, string[]? segments)
    {
        if (segments == null || segments.Length == 0) return null;
        JsonNode? current = node;
        for (int i = 0; i < segments.Length; i++)
        {
            current = NavigateSegment(current, segments[i]);
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
        var oc = StringComparison.OrdinalIgnoreCase;

        if (normalized.Equals(Operators.NotEquals, oc) || normalized == "!=")
            return !actual.Equals(target, cmp);
        if (normalized.Equals(Operators.Contains, oc))
            return actual.IndexOf(target, cmp) >= 0;
        if (normalized.Equals(Operators.StartsWith, oc))
            return actual.StartsWith(target, cmp);
        if (normalized.Equals(Operators.EndsWith, oc))
            return actual.EndsWith(target, cmp);
        if (normalized.Equals(Operators.GreaterThan, oc) || normalized == ">")
            return TryCompareNumeric(actual, target, out int gt) && gt > 0;
        if (normalized.Equals(Operators.LessThan, oc) || normalized == "<")
            return TryCompareNumeric(actual, target, out int lt) && lt < 0;
        if (normalized.Equals(Operators.GreaterOrEqual, oc) || normalized == ">=")
            return TryCompareNumeric(actual, target, out int ge) && ge >= 0;
        if (normalized.Equals(Operators.LessOrEqual, oc) || normalized == "<=")
            return TryCompareNumeric(actual, target, out int le) && le <= 0;
        // Empty / unknown → Equals (documented default).
        return actual.Equals(target, cmp);
    }

    private static bool EvaluateConditions(JsonNode item, List<Condition> conditions, string logicalOperator)
    {
        if (conditions == null || conditions.Count == 0) return false;
        bool useOr = (logicalOperator ?? "").Trim().Equals(LogicalOperators.OR, StringComparison.OrdinalIgnoreCase);
        foreach (var c in conditions)
        {
            var actual = GetPropertyValue(item, c.Path);
            bool match = actual != null && MatchesCondition(actual, c.Value, c.Operator, c.CaseSensitive);
            if (useOr && match) return true;
            if (!useOr && !match) return false;
        }
        return !useOr;
    }

    // Path-cached mirror of the Condition tuple. Callers pre-compile once per action call.
    private readonly struct CompiledCondition
    {
        public readonly string[]? Path;
        public readonly string Value;
        public readonly string Operator;
        public readonly bool CaseSensitive;

        public CompiledCondition(string[]? path, string value, string op, bool cs)
        {
            Path = path;
            Value = value;
            Operator = op;
            CaseSensitive = cs;
        }
    }

    private static CompiledCondition[]? CompileConditions(List<Condition>? conditions)
    {
        if (conditions == null || conditions.Count == 0) return null;
        var arr = new CompiledCondition[conditions.Count];
        for (int i = 0; i < conditions.Count; i++)
        {
            var c = conditions[i];
            arr[i] = new CompiledCondition(SplitPath(c.Path), c.Value, c.Operator, c.CaseSensitive);
        }
        return arr;
    }

    private static bool EvaluateConditions(JsonNode item, CompiledCondition[]? conditions, string logicalOperator)
    {
        if (conditions == null || conditions.Length == 0) return false;
        bool useOr = (logicalOperator ?? "").Trim().Equals(LogicalOperators.OR, StringComparison.OrdinalIgnoreCase);
        for (int i = 0; i < conditions.Length; i++)
        {
            var c = conditions[i];
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

    // ASCII Unit Separator — safe internal joiner for composite N-key group keys.
    private const char CompositeKeySeparator = '\u001F';
    private const string UnknownKey = "Unknown";

    private static void BuildCompositeKey(JsonNode item, List<string> keyPaths, out string composite, out string[] parts)
    {
        int n = keyPaths == null ? 0 : keyPaths.Count;
        if (n == 0) { composite = UnknownKey; parts = new string[0]; return; }

        parts = new string[n];
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < n; i++)
        {
            var path = keyPaths[i];
            var val = string.IsNullOrEmpty(path) ? null : GetPropertyValue(item, path);
            parts[i] = val ?? UnknownKey;
            if (i > 0) sb.Append(CompositeKeySeparator);
            sb.Append(parts[i]);
        }
        composite = sb.ToString();
    }

    // Pre-split overload — accepts a jagged array of path segments so the caller can compile once per action call.
    private static void BuildCompositeKey(JsonNode item, string[]?[] keySegments, out string composite, out string[] parts)
    {
        int n = keySegments.Length;
        if (n == 0) { composite = UnknownKey; parts = new string[0]; return; }

        parts = new string[n];
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < n; i++)
        {
            var val = GetPropertyValue(item, keySegments[i]);
            parts[i] = val ?? UnknownKey;
            if (i > 0) sb.Append(CompositeKeySeparator);
            sb.Append(parts[i]);
        }
        composite = sb.ToString();
    }

    private static string[]?[] CompileKeyPaths(List<string>? keyPaths)
    {
        int n = keyPaths == null ? 0 : keyPaths.Count;
        var arr = new string[]?[n];
        for (int i = 0; i < n; i++) arr[i] = SplitPath(keyPaths![i]);
        return arr;
    }

    private static string KeyLabel(List<string> keyNames, int index)
    {
        if (keyNames != null && index < keyNames.Count)
        {
            var raw = keyNames[index];
            if (!string.IsNullOrEmpty(raw)) return raw;
        }
        return "Key" + index.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }

    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions { WriteIndented = false };

    // Removes every element from `array` and returns the detached refs in original order.
    // Tail-drain keeps each RemoveAt at O(1). Callers can re-parent the refs into a fresh
    // JsonArray without cloning, since RemoveAt sets each node's Parent to null.
    private static JsonNode?[] DrainToArray(JsonArray array)
    {
        int n = array.Count;
        var picked = new JsonNode?[n];
        for (int i = n - 1; i >= 0; i--)
        {
            picked[i] = array[i];
            array.RemoveAt(i);
        }
        return picked;
    }
}
