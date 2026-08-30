using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace OutSystems.NssListUtils;

public partial class CssListUtils
{
    public void MssList_ZipMany(
        List<string> ssListsJson,
        List<string> ssKeyNamesJson,
        out string ssZippedListJson)
    {
        ssZippedListJson = "[]";
        if (ssListsJson == null || ssListsJson.Count == 0) return;

        var arrays = new JsonArray[ssListsJson.Count];
        int minLen = int.MaxValue;
        for (int i = 0; i < ssListsJson.Count; i++)
        {
            var s = ssListsJson[i];
            if (string.IsNullOrEmpty(s)) { minLen = 0; arrays[i] = new JsonArray(); continue; }
            arrays[i] = JsonNode.Parse(s)!.AsArray();
            if (arrays[i].Count < minLen) minLen = arrays[i].Count;
        }

        var result = new JsonArray();
        for (int pos = 0; pos < minLen; pos++)
        {
            var obj = new JsonObject();
            for (int listIdx = 0; listIdx < arrays.Length; listIdx++)
            {
                string keyName = KeyName(ssKeyNamesJson, listIdx);
                obj[keyName] = arrays[listIdx][pos]!.DeepClone();
            }
            result.Add(obj);
        }

        ssZippedListJson = result.ToJsonString(JsonOptions);
    }

    public void MssList_ZipManyGroupBy(
        List<string> ssListsJson,
        List<string> ssKeyPropertiesJson,
        List<string> ssKeyNamesJson,
        bool ssCaseSensitive,
        out string ssGroupedListJson)
    {
        ssGroupedListJson = "[]";
        if (ssListsJson == null || ssListsJson.Count == 0) return;

        int n = ssListsJson.Count;
        var arrays = new JsonArray[n];
        for (int i = 0; i < n; i++)
        {
            var s = ssListsJson[i];
            arrays[i] = string.IsNullOrEmpty(s) ? new JsonArray() : JsonNode.Parse(s)!.AsArray();
        }

        var cmp = ssCaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;
        var groups = new Dictionary<string, JsonArray[]>(cmp);
        var groupOrder = new List<string>();
        const string UnknownKey = "Unknown";

        for (int listIdx = 0; listIdx < n; listIdx++)
        {
            var arr = arrays[listIdx];
            string keyPath = listIdx < (ssKeyPropertiesJson == null ? 0 : ssKeyPropertiesJson.Count) ? ssKeyPropertiesJson![listIdx] : "";
            foreach (var item in arr)
            {
                string key = (string.IsNullOrEmpty(keyPath)
                    ? null
                    : GetPropertyValue(item!, keyPath)) ?? UnknownKey;

                if (!groups.TryGetValue(key, out var buckets))
                {
                    buckets = new JsonArray[n];
                    for (int k = 0; k < n; k++) buckets[k] = new JsonArray();
                    groups[key] = buckets;
                    groupOrder.Add(key);
                }
                buckets[listIdx].Add(item!.DeepClone());
            }
        }

        var result = new JsonArray();
        foreach (var key in groupOrder)
        {
            var buckets = groups[key];
            var obj = new JsonObject { ["Key"] = key };
            for (int listIdx = 0; listIdx < n; listIdx++)
            {
                obj[KeyName(ssKeyNamesJson, listIdx)] = buckets[listIdx];
            }
            result.Add(obj);
        }

        ssGroupedListJson = result.ToJsonString(JsonOptions);
    }

    private static string KeyName(List<string>? keyNames, int index)
    {
        if (keyNames != null && index < keyNames.Count)
        {
            var raw = keyNames[index];
            if (!string.IsNullOrEmpty(raw)) return raw;
        }
        return "Items" + index.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }
}
