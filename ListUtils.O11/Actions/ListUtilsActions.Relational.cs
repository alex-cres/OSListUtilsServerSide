using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace OutSystems.NssListUtils;

public partial class CssListUtils
{
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
        int minCount = Math.Min(arrA.Count, arrB.Count);
        var pickedA = DrainToArray(arrA);
        var pickedB = DrainToArray(arrB);
        var result = new JsonArray();

        for (int i = 0; i < minCount; i++)
        {
            var pair = new JsonObject
            {
                [ssKeyNameA] = pickedA[i],
                [ssKeyNameB] = pickedB[i]
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

        foreach (var item in DrainToArray(array))
        {
            string key = GetPropertyValue(item!, ssPropertyName) ?? "Unknown";
            if (!groups.ContainsKey(key))
            {
                groups[key] = new JsonArray();
                groupOrder.Add(key);
            }
            groups[key].Add(item);
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

    public void MssList_ZipGroupBy(
        string ssListAJson,
        string ssListBJson,
        string ssKeyPropertyA,
        string ssKeyPropertyB,
        string ssKeyNameA,
        string ssKeyNameB,
        bool ssCaseSensitive,
        out string ssGroupedListJson)
    {
        var cmp = ssCaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;
        var groupsA = new Dictionary<string, JsonArray>(cmp);
        var groupsB = new Dictionary<string, JsonArray>(cmp);
        var groupOrder = new List<string>();
        var seen = new HashSet<string>(cmp);

        if (!string.IsNullOrEmpty(ssListAJson))
        {
            var arrA = JsonNode.Parse(ssListAJson)!.AsArray();
            foreach (var item in DrainToArray(arrA))
            {
                string key = GetPropertyValue(item!, ssKeyPropertyA) ?? "Unknown";
                JsonArray bucket;
                if (!groupsA.TryGetValue(key, out bucket))
                {
                    bucket = new JsonArray();
                    groupsA[key] = bucket;
                }
                bucket.Add(item);
                if (seen.Add(key)) groupOrder.Add(key);
            }
        }

        if (!string.IsNullOrEmpty(ssListBJson))
        {
            var arrB = JsonNode.Parse(ssListBJson)!.AsArray();
            foreach (var item in DrainToArray(arrB))
            {
                string key = GetPropertyValue(item!, ssKeyPropertyB) ?? "Unknown";
                JsonArray bucket;
                if (!groupsB.TryGetValue(key, out bucket))
                {
                    bucket = new JsonArray();
                    groupsB[key] = bucket;
                }
                bucket.Add(item);
                if (seen.Add(key)) groupOrder.Add(key);
            }
        }

        string nameA = string.IsNullOrEmpty(ssKeyNameA) ? "ItemsA" : ssKeyNameA;
        string nameB = string.IsNullOrEmpty(ssKeyNameB) ? "ItemsB" : ssKeyNameB;

        var result = new JsonArray();
        foreach (var key in groupOrder)
        {
            JsonArray listA, listB;
            var groupObj = new JsonObject
            {
                ["Key"] = key,
                [nameA] = groupsA.TryGetValue(key, out listA) ? listA : new JsonArray(),
                [nameB] = groupsB.TryGetValue(key, out listB) ? listB : new JsonArray(),
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
        bool ssCaseSensitive,
        out string ssDifferenceListJson)
    {
        if (string.IsNullOrEmpty(ssListAJson)) { ssDifferenceListJson = "[]"; return; }
        if (string.IsNullOrEmpty(ssListBJson)) { ssDifferenceListJson = ssListAJson; return; }

        var arrA = JsonNode.Parse(ssListAJson)!.AsArray();
        var arrB = JsonNode.Parse(ssListBJson)!.AsArray();

        var normalizedOp = (ssComparisonOperator ?? "").Trim().ToUpperInvariant();
        var strCmp = ssCaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;
        var invariant = System.Globalization.CultureInfo.InvariantCulture;
        var numStyle = System.Globalization.NumberStyles.Any;

        var bValues = new List<string>();
        foreach (var b in arrB)
        {
            var k = GetPropertyValue(b!, ssMatchKey);
            if (k != null) bValues.Add(k);
        }

        Func<string, bool> matchedAny;

        bool isEquals = normalizedOp.Length == 0 || normalizedOp == "EQUALS";
        bool isNotEquals = normalizedOp == "NOTEQUALS" || normalizedOp == "!=";
        bool isStartsWith = normalizedOp == "STARTSWITH";
        bool isEndsWith = normalizedOp == "ENDSWITH";
        bool isGt = normalizedOp == "GREATERTHAN" || normalizedOp == ">";
        bool isLt = normalizedOp == "LESSTHAN" || normalizedOp == "<";
        bool isGe = normalizedOp == "GREATEROREQUAL" || normalizedOp == ">=";
        bool isLe = normalizedOp == "LESSOREQUAL" || normalizedOp == "<=";
        bool isNumeric = isGt || isLt || isGe || isLe;

        if (isEquals)
        {
            var bSet = new HashSet<string>(bValues, strCmp);
            matchedAny = key => bSet.Contains(key);
        }
        else if (isNotEquals)
        {
            var bSet = new HashSet<string>(bValues, strCmp);
            matchedAny = key => !(bSet.Count == 1 && bSet.Contains(key));
        }
        else if (isStartsWith)
        {
            var bSet = new HashSet<string>(bValues, strCmp);
            matchedAny = key =>
            {
                for (int len = 0; len <= key.Length; len++)
                    if (bSet.Contains(key.Substring(0, len))) return true;
                return false;
            };
        }
        else if (isEndsWith)
        {
            var bSet = new HashSet<string>(bValues, strCmp);
            matchedAny = key =>
            {
                for (int len = 0; len <= key.Length; len++)
                    if (bSet.Contains(key.Substring(key.Length - len, len))) return true;
                return false;
            };
        }
        else if (isNumeric)
        {
            decimal? minB = null, maxB = null;
            foreach (var v in bValues)
            {
                if (!decimal.TryParse(v, numStyle, invariant, out var d)) continue;
                if (minB == null || d < minB.Value) minB = d;
                if (maxB == null || d > maxB.Value) maxB = d;
            }
            if (minB == null)
            {
                matchedAny = _ => false;
            }
            else
            {
                decimal lo = minB.Value, hi = maxB!.Value;
                if (isGt)      matchedAny = key => decimal.TryParse(key, numStyle, invariant, out var d) && d > lo;
                else if (isLt) matchedAny = key => decimal.TryParse(key, numStyle, invariant, out var d) && d < hi;
                else if (isGe) matchedAny = key => decimal.TryParse(key, numStyle, invariant, out var d) && d >= lo;
                else           matchedAny = key => decimal.TryParse(key, numStyle, invariant, out var d) && d <= hi;
            }
        }
        else
        {
            var opCopy = ssComparisonOperator ?? "";
            var csCopy = ssCaseSensitive;
            matchedAny = key => bValues.Any(bv => MatchesCondition(key, bv, opCopy, csCopy));
        }

        var result = new JsonArray();
        foreach (var item in DrainToArray(arrA))
        {
            var key = GetPropertyValue(item!, ssMatchKey);
            if (key == null || !matchedAny(key))
                result.Add(item);
        }

        ssDifferenceListJson = result.ToJsonString(JsonOptions);
    }

    public void MssList_Intersect(
        string ssListAJson,
        string ssListBJson,
        string ssMatchKey,
        string ssComparisonOperator,
        bool ssCaseSensitive,
        out string ssIntersectionListJson)
    {
        if (string.IsNullOrEmpty(ssListAJson)) { ssIntersectionListJson = "[]"; return; }
        if (string.IsNullOrEmpty(ssListBJson)) { ssIntersectionListJson = "[]"; return; }

        var arrA = JsonNode.Parse(ssListAJson)!.AsArray();
        var arrB = JsonNode.Parse(ssListBJson)!.AsArray();
        var normalizedOp = (ssComparisonOperator ?? "").Trim().ToUpperInvariant();
        bool isEquals = normalizedOp.Length == 0 || normalizedOp == "EQUALS";
        var strCmp = ssCaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;

        var bValues = new List<string>();
        foreach (var b in arrB)
        {
            var k = GetPropertyValue(b!, ssMatchKey);
            if (k != null) bValues.Add(k);
        }

        HashSet<string>? bSet = isEquals ? new HashSet<string>(bValues, strCmp) : null;

        var result = new JsonArray();
        foreach (var item in DrainToArray(arrA))
        {
            var key = GetPropertyValue(item!, ssMatchKey);
            if (key == null) continue;
            bool match = isEquals
                ? bSet!.Contains(key)
                : bValues.Any(bv => MatchesCondition(key, bv, ssComparisonOperator ?? "", ssCaseSensitive));
            if (match) result.Add(item);
        }

        ssIntersectionListJson = result.ToJsonString(JsonOptions);
    }

    public void MssList_Union(
        string ssListAJson,
        string ssListBJson,
        string ssMatchKey,
        bool ssCaseSensitive,
        out string ssUnionListJson)
    {
        if (string.IsNullOrEmpty(ssListAJson) && string.IsNullOrEmpty(ssListBJson)) { ssUnionListJson = "[]"; return; }

        var arrA = string.IsNullOrEmpty(ssListAJson) ? new JsonArray() : JsonNode.Parse(ssListAJson)!.AsArray();
        var arrB = string.IsNullOrEmpty(ssListBJson) ? new JsonArray() : JsonNode.Parse(ssListBJson)!.AsArray();
        var cmp = ssCaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;
        var seen = new HashSet<string>(cmp);
        bool nullKeySeen = false;
        var result = new JsonArray();

        void Consume(JsonArray src)
        {
            foreach (var item in DrainToArray(src))
            {
                string? key;
                if (string.IsNullOrEmpty(ssMatchKey))
                    key = item == null ? "null" : item.ToJsonString(JsonOptions);
                else
                    key = GetPropertyValue(item!, ssMatchKey);

                if (key == null)
                {
                    if (nullKeySeen) continue;
                    nullKeySeen = true;
                    result.Add(item);
                }
                else if (seen.Add(key))
                {
                    result.Add(item);
                }
            }
        }

        Consume(arrA);
        Consume(arrB);

        ssUnionListJson = result.ToJsonString(JsonOptions);
    }

    public void MssList_GroupByMultiple(
        string ssSourceListJson,
        List<string> ssPropertyPaths,
        List<string> ssKeyNames,
        string ssItemsFieldName,
        bool ssCaseSensitive,
        out string ssGroupedListJson)
    {
        if (string.IsNullOrEmpty(ssSourceListJson) || ssPropertyPaths == null || ssPropertyPaths.Count == 0)
        {
            ssGroupedListJson = "[]";
            return;
        }

        var array = JsonNode.Parse(ssSourceListJson)!.AsArray();
        var cmp = ssCaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;
        var groups = new Dictionary<string, JsonArray>(cmp);
        var parts = new Dictionary<string, string[]>(cmp);
        var order = new List<string>();

        foreach (var item in DrainToArray(array))
        {
            BuildCompositeKey(item!, ssPropertyPaths, out var composite, out var keyValues);
            if (!groups.TryGetValue(composite, out var bucket))
            {
                bucket = new JsonArray();
                groups[composite] = bucket;
                parts[composite] = keyValues;
                order.Add(composite);
            }
            bucket.Add(item);
        }

        string itemsField = string.IsNullOrEmpty(ssItemsFieldName) ? "Items" : ssItemsFieldName;

        var result = new JsonArray();
        foreach (var composite in order)
        {
            var keyValues = parts[composite];
            var obj = new JsonObject();
            for (int i = 0; i < keyValues.Length; i++)
            {
                obj[KeyLabel(ssKeyNames, i)] = keyValues[i];
            }
            obj[itemsField] = groups[composite];
            result.Add(obj);
        }

        ssGroupedListJson = result.ToJsonString(JsonOptions);
    }

    public void MssList_ZipGroupByMultiple(
        string ssListAJson,
        string ssListBJson,
        List<string> ssKeyPropertiesA,
        List<string> ssKeyPropertiesB,
        List<string> ssKeyNames,
        string ssKeyNameA,
        string ssKeyNameB,
        bool ssCaseSensitive,
        out string ssGroupedListJson)
    {
        var cmp = ssCaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;
        var groupsA = new Dictionary<string, JsonArray>(cmp);
        var groupsB = new Dictionary<string, JsonArray>(cmp);
        var parts = new Dictionary<string, string[]>(cmp);
        var order = new List<string>();
        var seen = new HashSet<string>(cmp);

        void Consume(string listJson, List<string> keyPaths, Dictionary<string, JsonArray> target)
        {
            if (string.IsNullOrEmpty(listJson)) return;
            var effectivePaths = keyPaths ?? new List<string>();
            var arr = JsonNode.Parse(listJson)!.AsArray();
            foreach (var item in DrainToArray(arr))
            {
                BuildCompositeKey(item!, effectivePaths, out var composite, out var keyValues);
                if (!target.TryGetValue(composite, out var bucket))
                    target[composite] = bucket = new JsonArray();
                bucket.Add(item);
                if (seen.Add(composite))
                {
                    parts[composite] = keyValues;
                    order.Add(composite);
                }
            }
        }

        Consume(ssListAJson, ssKeyPropertiesA, groupsA);
        Consume(ssListBJson, ssKeyPropertiesB, groupsB);

        string nameA = string.IsNullOrEmpty(ssKeyNameA) ? "ItemsA" : ssKeyNameA;
        string nameB = string.IsNullOrEmpty(ssKeyNameB) ? "ItemsB" : ssKeyNameB;

        var result = new JsonArray();
        foreach (var composite in order)
        {
            var keyValues = parts[composite];
            var obj = new JsonObject();
            for (int i = 0; i < keyValues.Length; i++)
            {
                obj[KeyLabel(ssKeyNames, i)] = keyValues[i];
            }
            obj[nameA] = groupsA.TryGetValue(composite, out var listA) ? listA : new JsonArray();
            obj[nameB] = groupsB.TryGetValue(composite, out var listB) ? listB : new JsonArray();
            result.Add(obj);
        }

        ssGroupedListJson = result.ToJsonString(JsonOptions);
    }
}
