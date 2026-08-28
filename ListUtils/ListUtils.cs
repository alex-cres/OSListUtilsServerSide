using System.Text.Json;
using System.Text.Json.Nodes;

namespace ListUtils;

public class ListUtils : IListUtils
{
    public void List_Pop(
        List<string> sourceList,
        int index,
        out List<string> updatedList,
        out string poppedElement)
    {
        if (sourceList == null || index < 0 || index >= sourceList.Count)
        {
            updatedList = sourceList ?? new List<string>();
            poppedElement = "";
            return;
        }

        var resultList = new List<string>(sourceList);
        poppedElement = resultList[index];
        resultList.RemoveAt(index);
        updatedList = resultList;
    }

    public void List_PopMultiple(
        List<string> sourceList,
        List<int> indicesToPop,
        out List<string> updatedList,
        out List<string> poppedElements)
    {
        updatedList = new List<string>();
        poppedElements = new List<string>();

        if (sourceList == null) return;

        var resultList = new List<string>(sourceList);
        if (indicesToPop == null || indicesToPop.Count == 0)
        {
            updatedList = resultList;
            return;
        }

        var sortedIndices = new List<int>(indicesToPop);
        sortedIndices.Sort();
        sortedIndices.Reverse();

        foreach (int index in sortedIndices)
        {
            if (index >= 0 && index < resultList.Count)
            {
                poppedElements.Insert(0, resultList[index]);
                resultList.RemoveAt(index);
            }
        }

        updatedList = resultList;
    }

    public void List_PopByCondition(
        string sourceListJson,
        string propertyName,
        string targetValue,
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
            if (value != null && value.Equals(targetValue, StringComparison.OrdinalIgnoreCase))
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
            if (value != null && value.Equals(targetValue, StringComparison.OrdinalIgnoreCase))
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
        out string differenceListJson)
    {
        if (string.IsNullOrEmpty(listAJson)) { differenceListJson = "[]"; return; }
        if (string.IsNullOrEmpty(listBJson)) { differenceListJson = listAJson; return; }

        var arrA = JsonNode.Parse(listAJson)!.AsArray();
        var arrB = JsonNode.Parse(listBJson)!.AsArray();

        var keysInB = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var b in arrB)
        {
            var k = GetPropertyValue(b!, matchKey);
            if (k != null) keysInB.Add(k);
        }

        var result = new JsonArray();
        foreach (var item in arrA)
        {
            var key = GetPropertyValue(item!, matchKey);
            if (key == null || !keysInB.Contains(key))
            {
                result.Add(JsonNode.Parse(item!.ToJsonString())!);
            }
        }

        differenceListJson = result.ToJsonString(JsonOptions);
    }

    private static string? GetPropertyValue(JsonNode node, string propertyName)
    {
        if (node is not JsonObject obj) return null;
        if (obj.TryGetPropertyValue(propertyName, out var val) && val != null)
            return val.ToString();
        // Try camelCase fallback
        string camel = ToCamelCase(propertyName);
        if (obj.TryGetPropertyValue(camel, out val) && val != null)
            return val.ToString();
        return null;
    }

    private static string ToCamelCase(string str)
    {
        if (string.IsNullOrEmpty(str) || !char.IsUpper(str[0])) return str;
        return char.ToLowerInvariant(str[0]) + str[1..];
    }

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = false };
}
