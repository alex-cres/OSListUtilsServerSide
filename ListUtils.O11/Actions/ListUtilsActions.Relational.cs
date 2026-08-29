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
        var result = new JsonArray();

        int minCount = Math.Min(arrA.Count, arrB.Count);
        for (int i = 0; i < minCount; i++)
        {
            var pair = new JsonObject
            {
                [ssKeyNameA] = arrA[i]!.DeepClone(),
                [ssKeyNameB] = arrB[i]!.DeepClone()
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
            groups[key].Add(item!.DeepClone());
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
        foreach (var item in arrA)
        {
            var key = GetPropertyValue(item!, ssMatchKey);
            if (key == null || !matchedAny(key))
                result.Add(item!.DeepClone());
        }

        ssDifferenceListJson = result.ToJsonString(JsonOptions);
    }
}
