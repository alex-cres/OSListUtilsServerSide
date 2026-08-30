using System.Text.Json.Nodes;

namespace ListUtils;

public partial class ListUtils
{
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
                [KeyNameA] = arrA[i]!.DeepClone(),
                [KeyNameB] = arrB[i]!.DeepClone()
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

        GroupedListJson = result.ToJsonString(JsonOptions);
    }

    public void List_ZipGroupBy(
        string ListAJson,
        string ListBJson,
        string KeyPropertyA,
        string KeyPropertyB,
        string KeyNameA,
        string KeyNameB,
        bool CaseSensitive,
        out string GroupedListJson)
    {
        var cmp = CaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;
        var groupsA = new Dictionary<string, JsonArray>(cmp);
        var groupsB = new Dictionary<string, JsonArray>(cmp);
        var groupOrder = new List<string>();
        var seen = new HashSet<string>(cmp);

        if (!string.IsNullOrEmpty(ListAJson))
        {
            var arrA = JsonNode.Parse(ListAJson)!.AsArray();
            foreach (var item in arrA)
            {
                string key = GetPropertyValue(item!, KeyPropertyA) ?? "Unknown";
                if (!groupsA.TryGetValue(key, out var bucket))
                    groupsA[key] = bucket = new JsonArray();
                bucket.Add(item!.DeepClone());
                if (seen.Add(key)) groupOrder.Add(key);
            }
        }

        if (!string.IsNullOrEmpty(ListBJson))
        {
            var arrB = JsonNode.Parse(ListBJson)!.AsArray();
            foreach (var item in arrB)
            {
                string key = GetPropertyValue(item!, KeyPropertyB) ?? "Unknown";
                if (!groupsB.TryGetValue(key, out var bucket))
                    groupsB[key] = bucket = new JsonArray();
                bucket.Add(item!.DeepClone());
                if (seen.Add(key)) groupOrder.Add(key);
            }
        }

        // Fall back to sensible names when the caller passes blank labels.
        string nameA = string.IsNullOrEmpty(KeyNameA) ? "ItemsA" : KeyNameA;
        string nameB = string.IsNullOrEmpty(KeyNameB) ? "ItemsB" : KeyNameB;

        var result = new JsonArray();
        foreach (var key in groupOrder)
        {
            var groupObj = new JsonObject
            {
                ["Key"] = key,
                [nameA] = groupsA.TryGetValue(key, out var listA) ? listA : new JsonArray(),
                [nameB] = groupsB.TryGetValue(key, out var listB) ? listB : new JsonArray(),
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

        var normalizedOp = (ComparisonOperator ?? "").Trim().ToUpperInvariant();
        var strCmp = CaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;
        var invariant = System.Globalization.CultureInfo.InvariantCulture;
        var numStyle = System.Globalization.NumberStyles.Any;

        var bValues = new List<string>();
        foreach (var b in arrB)
        {
            var k = GetPropertyValue(b!, MatchKey);
            if (k != null) bValues.Add(k);
        }

        // Build the matchedAny predicate once. All fast paths are O(A+B) or O(A*L)
        // where L is the string length. Contains stays O(A*B) — needs a suffix
        // trie / Aho-Corasick to beat that and is not worth the complexity yet.
        Func<string, bool> matchedAny;

        bool isEquals = normalizedOp.Length == 0 || normalizedOp == "EQUALS";
        bool isNotEquals = normalizedOp is "NOTEQUALS" or "!=";
        bool isStartsWith = normalizedOp == "STARTSWITH";
        bool isEndsWith = normalizedOp == "ENDSWITH";
        bool isGt = normalizedOp is "GREATERTHAN" or ">";
        bool isLt = normalizedOp is "LESSTHAN" or "<";
        bool isGe = normalizedOp is "GREATEROREQUAL" or ">=";
        bool isLe = normalizedOp is "LESSOREQUAL" or "<=";
        bool isNumeric = isGt || isLt || isGe || isLe;

        if (isEquals)
        {
            var bSet = new HashSet<string>(bValues, strCmp);
            matchedAny = key => bSet.Contains(key);
        }
        else if (isNotEquals)
        {
            var bSet = new HashSet<string>(bValues, strCmp);
            // ∃b: key != b  is true unless every b equals key (i.e. bSet == {key}).
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
                if (minB is null || d < minB.Value) minB = d;
                if (maxB is null || d > maxB.Value) maxB = d;
            }
            if (minB is null)
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
            // Slow path: Contains and any unknown operator fall back to O(A*B).
            var opCopy = ComparisonOperator;
            var csCopy = CaseSensitive;
            matchedAny = key => bValues.Any(bv => MatchesCondition(key, bv, opCopy, csCopy));
        }

        var result = new JsonArray();
        foreach (var item in arrA)
        {
            var key = GetPropertyValue(item!, MatchKey);
            if (key == null || !matchedAny(key))
                result.Add(item!.DeepClone());
        }

        DifferenceListJson = result.ToJsonString(JsonOptions);
    }

    public void List_Intersect(
        string ListAJson,
        string ListBJson,
        string MatchKey,
        string ComparisonOperator,
        bool CaseSensitive,
        out string IntersectionListJson)
    {
        if (string.IsNullOrEmpty(ListAJson)) { IntersectionListJson = "[]"; return; }
        if (string.IsNullOrEmpty(ListBJson)) { IntersectionListJson = "[]"; return; }

        var arrA = JsonNode.Parse(ListAJson)!.AsArray();
        var arrB = JsonNode.Parse(ListBJson)!.AsArray();
        var normalizedOp = (ComparisonOperator ?? "").Trim().ToUpperInvariant();
        bool isEquals = normalizedOp.Length == 0 || normalizedOp == "EQUALS";
        var strCmp = CaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;

        var bValues = new List<string>();
        foreach (var b in arrB)
        {
            var k = GetPropertyValue(b!, MatchKey);
            if (k != null) bValues.Add(k);
        }

        HashSet<string>? bSet = isEquals ? new HashSet<string>(bValues, strCmp) : null;

        var result = new JsonArray();
        foreach (var item in arrA)
        {
            var key = GetPropertyValue(item!, MatchKey);
            if (key == null) continue;
            bool match = isEquals
                ? bSet!.Contains(key)
                : bValues.Any(bv => MatchesCondition(key, bv, ComparisonOperator, CaseSensitive));
            if (match) result.Add(item!.DeepClone());
        }

        IntersectionListJson = result.ToJsonString(JsonOptions);
    }

    public void List_Union(
        string ListAJson,
        string ListBJson,
        string MatchKey,
        bool CaseSensitive,
        out string UnionListJson)
    {
        if (string.IsNullOrEmpty(ListAJson) && string.IsNullOrEmpty(ListBJson)) { UnionListJson = "[]"; return; }

        var arrA = string.IsNullOrEmpty(ListAJson) ? new JsonArray() : JsonNode.Parse(ListAJson)!.AsArray();
        var arrB = string.IsNullOrEmpty(ListBJson) ? new JsonArray() : JsonNode.Parse(ListBJson)!.AsArray();
        var cmp = CaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;
        var seen = new HashSet<string>(cmp);
        bool nullKeySeen = false;
        var result = new JsonArray();

        void Consume(JsonArray src)
        {
            foreach (var item in src)
            {
                string? key;
                if (string.IsNullOrEmpty(MatchKey))
                    key = item?.ToJsonString(JsonOptions) ?? "null";
                else
                    key = GetPropertyValue(item!, MatchKey);

                if (key == null)
                {
                    if (nullKeySeen) continue;
                    nullKeySeen = true;
                    result.Add(item!.DeepClone());
                }
                else if (seen.Add(key))
                {
                    result.Add(item!.DeepClone());
                }
            }
        }

        Consume(arrA);
        Consume(arrB);

        UnionListJson = result.ToJsonString(JsonOptions);
    }
}
