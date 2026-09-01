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

        var picked = new JsonNode?[arrays.Length][];
        for (int i = 0; i < arrays.Length; i++) picked[i] = DrainToArray(arrays[i]);

        var result = new JsonArray();
        for (int pos = 0; pos < minLen; pos++)
        {
            var obj = new JsonObject();
            for (int listIdx = 0; listIdx < arrays.Length; listIdx++)
            {
                string keyName = KeyName(KeyNamesJson, listIdx);
                obj[keyName] = picked[listIdx][pos];
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
            foreach (var item in DrainToArray(arr))
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
                buckets[listIdx].Add(item);
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

    public void List_ZipManyGroupByMultiple(
        List<string> ListsJson,
        int KeyCount,
        List<string> KeyProperties,
        List<string> KeyNames,
        List<string> ItemsFieldNames,
        bool CaseSensitive,
        out string GroupedListJson)
    {
        GroupedListJson = "[]";
        if (ListsJson == null || ListsJson.Count == 0 || KeyCount <= 0) return;

        int m = ListsJson.Count;
        int n = KeyCount;
        var arrays = new JsonArray[m];
        for (int i = 0; i < m; i++)
        {
            var s = ListsJson[i];
            arrays[i] = string.IsNullOrEmpty(s) ? new JsonArray() : JsonNode.Parse(s)!.AsArray();
        }

        var cmp = CaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;
        var groups = new Dictionary<string, JsonArray[]>(cmp);
        var parts = new Dictionary<string, string[]>(cmp);
        var order = new List<string>();

        int totalPaths = KeyProperties?.Count ?? 0;
        var scratch = new List<string>(n);

        for (int listIdx = 0; listIdx < m; listIdx++)
        {
            scratch.Clear();
            for (int k = 0; k < n; k++)
            {
                int flatIdx = listIdx * n + k;
                scratch.Add(flatIdx < totalPaths ? KeyProperties![flatIdx] : "");
            }

            foreach (var item in DrainToArray(arrays[listIdx]))
            {
                var (composite, keyValues) = BuildCompositeKey(item!, scratch);
                if (!groups.TryGetValue(composite, out var buckets))
                {
                    buckets = new JsonArray[m];
                    for (int k = 0; k < m; k++) buckets[k] = new JsonArray();
                    groups[composite] = buckets;
                    parts[composite] = keyValues;
                    order.Add(composite);
                }
                buckets[listIdx].Add(item);
            }
        }

        var result = new JsonArray();
        foreach (var composite in order)
        {
            var keyValues = parts[composite];
            var buckets = groups[composite];
            var obj = new JsonObject();
            for (int i = 0; i < keyValues.Length; i++)
            {
                obj[KeyLabel(KeyNames, i)] = keyValues[i];
            }
            for (int listIdx = 0; listIdx < m; listIdx++)
            {
                obj[KeyName(ItemsFieldNames, listIdx)] = buckets[listIdx];
            }
            result.Add(obj);
        }

        GroupedListJson = result.ToJsonString(JsonOptions);
    }
}
