using System.Text.Json.Nodes;

namespace ListUtils;

public partial class ListUtils
{
    public void List_ZipMany(
        List<string> ListsJson,
        List<string> KeyNamesJson,
        out string ZippedListJson)
    {
        ZippedListJson = "[]";
        if (ListsJson == null || ListsJson.Count == 0) return;

        var arrays = new JsonArray[ListsJson.Count];
        int minLen = int.MaxValue;
        for (int i = 0; i < ListsJson.Count; i++)
        {
            var s = ListsJson[i];
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
                string keyName = KeyName(KeyNamesJson, listIdx);
                obj[keyName] = arrays[listIdx][pos]!.DeepClone();
            }
            result.Add(obj);
        }

        ZippedListJson = result.ToJsonString(JsonOptions);
    }

    public void List_ZipManyGroupBy(
        List<string> ListsJson,
        List<string> KeyPropertiesJson,
        List<string> KeyNamesJson,
        bool CaseSensitive,
        out string GroupedListJson)
    {
        GroupedListJson = "[]";
        if (ListsJson == null || ListsJson.Count == 0) return;

        int n = ListsJson.Count;
        var arrays = new JsonArray[n];
        for (int i = 0; i < n; i++)
        {
            var s = ListsJson[i];
            arrays[i] = string.IsNullOrEmpty(s) ? new JsonArray() : JsonNode.Parse(s)!.AsArray();
        }

        var cmp = CaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;
        // groups[key] holds one JsonArray per input list (index-aligned).
        var groups = new Dictionary<string, JsonArray[]>(cmp);
        var groupOrder = new List<string>();
        const string UnknownKey = "Unknown";

        for (int listIdx = 0; listIdx < n; listIdx++)
        {
            var arr = arrays[listIdx];
            string keyPath = listIdx < (KeyPropertiesJson?.Count ?? 0) ? KeyPropertiesJson![listIdx] : "";
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
                obj[KeyName(KeyNamesJson, listIdx)] = buckets[listIdx];
            }
            result.Add(obj);
        }

        GroupedListJson = result.ToJsonString(JsonOptions);
    }

    private static string KeyName(List<string>? keyNames, int index)
    {
        if (keyNames != null && index < keyNames.Count)
        {
            var raw = keyNames[index];
            if (!string.IsNullOrEmpty(raw)) return raw;
        }
        return $"Items{index}";
    }
}
