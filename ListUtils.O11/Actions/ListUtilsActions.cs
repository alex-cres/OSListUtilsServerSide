using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace OutSystems.NssListUtils;

public class CssListUtils : IssListUtils
{
    public void MssList_Pop(
        List<string> ssSourceList,
        int ssIndex,
        out List<string> ssUpdatedList,
        out string ssPoppedElement)
    {
        if (ssSourceList == null || ssIndex < 0 || ssIndex >= ssSourceList.Count)
        {
            ssUpdatedList = ssSourceList ?? new List<string>();
            ssPoppedElement = "";
            return;
        }

        var resultList = new List<string>(ssSourceList);
        ssPoppedElement = resultList[ssIndex];
        resultList.RemoveAt(ssIndex);
        ssUpdatedList = resultList;
    }

    public void MssList_PopMultiple(
        List<string> ssSourceList,
        List<int> ssIndicesToPop,
        out List<string> ssUpdatedList,
        out List<string> ssPoppedElements)
    {
        ssUpdatedList = new List<string>();
        ssPoppedElements = new List<string>();

        if (ssSourceList == null) return;

        var resultList = new List<string>(ssSourceList);
        if (ssIndicesToPop == null || ssIndicesToPop.Count == 0)
        {
            ssUpdatedList = resultList;
            return;
        }

        var sortedIndices = new List<int>(ssIndicesToPop);
        sortedIndices.Sort();
        sortedIndices.Reverse();

        foreach (int index in sortedIndices)
        {
            if (index >= 0 && index < resultList.Count)
            {
                ssPoppedElements.Insert(0, resultList[index]);
                resultList.RemoveAt(index);
            }
        }

        ssUpdatedList = resultList;
    }

    public void MssList_PopByCondition(
        string ssSourceListJson,
        string ssPropertyName,
        string ssTargetValue,
        out string ssUpdatedListJson,
        out string ssPoppedElementJson)
    {
        if (string.IsNullOrEmpty(ssSourceListJson) || string.IsNullOrEmpty(ssPropertyName))
        {
            ssUpdatedListJson = ssSourceListJson ?? "[]";
            ssPoppedElementJson = "{}";
            return;
        }

        var array = JsonNode.Parse(ssSourceListJson)!.AsArray();
        JsonNode? matchedNode = null;

        for (int i = 0; i < array.Count; i++)
        {
            var value = GetPropertyValue(array[i]!, ssPropertyName);
            if (value != null && value.Equals(ssTargetValue, StringComparison.OrdinalIgnoreCase))
            {
                matchedNode = array[i];
                array.RemoveAt(i);
                break;
            }
        }

        ssUpdatedListJson = array.ToJsonString(JsonOptions);
        ssPoppedElementJson = matchedNode?.ToJsonString(JsonOptions) ?? "{}";
    }

    public void MssList_PopMultipleByCondition(
        string ssSourceListJson,
        string ssPropertyName,
        string ssTargetValue,
        out string ssUpdatedListJson,
        out string ssPoppedElementsJson)
    {
        if (string.IsNullOrEmpty(ssSourceListJson) || string.IsNullOrEmpty(ssPropertyName))
        {
            ssUpdatedListJson = ssSourceListJson ?? "[]";
            ssPoppedElementsJson = "[]";
            return;
        }

        var originalArray = JsonNode.Parse(ssSourceListJson)!.AsArray();
        var keptArray = new JsonArray();
        var poppedArray = new JsonArray();

        foreach (var item in originalArray)
        {
            var value = GetPropertyValue(item!, ssPropertyName);
            if (value != null && value.Equals(ssTargetValue, StringComparison.OrdinalIgnoreCase))
            {
                poppedArray.Add(JsonNode.Parse(item!.ToJsonString())!);
            }
            else
            {
                keptArray.Add(JsonNode.Parse(item!.ToJsonString())!);
            }
        }

        ssUpdatedListJson = keptArray.ToJsonString(JsonOptions);
        ssPoppedElementsJson = poppedArray.ToJsonString(JsonOptions);
    }

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
                [ssKeyNameA] = JsonNode.Parse(arrA[i]!.ToJsonString()),
                [ssKeyNameB] = JsonNode.Parse(arrB[i]!.ToJsonString())
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

        ssGroupedListJson = result.ToJsonString(JsonOptions);
    }

    public void MssList_Difference(
        string ssListAJson,
        string ssListBJson,
        string ssMatchKey,
        out string ssDifferenceListJson)
    {
        if (string.IsNullOrEmpty(ssListAJson)) { ssDifferenceListJson = "[]"; return; }
        if (string.IsNullOrEmpty(ssListBJson)) { ssDifferenceListJson = ssListAJson; return; }

        var arrA = JsonNode.Parse(ssListAJson)!.AsArray();
        var arrB = JsonNode.Parse(ssListBJson)!.AsArray();

        var keysInB = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var b in arrB)
        {
            var k = GetPropertyValue(b!, ssMatchKey);
            if (k != null) keysInB.Add(k);
        }

        var result = new JsonArray();
        foreach (var item in arrA)
        {
            var key = GetPropertyValue(item!, ssMatchKey);
            if (key == null || !keysInB.Contains(key))
            {
                result.Add(JsonNode.Parse(item!.ToJsonString())!);
            }
        }

        ssDifferenceListJson = result.ToJsonString(JsonOptions);
    }

    private static string? GetPropertyValue(JsonNode node, string propertyName)
    {
        if (node is not JsonObject obj) return null;
        if (obj.TryGetPropertyValue(propertyName, out var val) && val != null)
            return val.ToString();
        string camel = ToCamelCase(propertyName);
        if (obj.TryGetPropertyValue(camel, out val) && val != null)
            return val.ToString();
        return null;
    }

    private static string ToCamelCase(string str)
    {
        if (string.IsNullOrEmpty(str) || !char.IsUpper(str[0])) return str;
        return char.ToLowerInvariant(str[0]) + str.Substring(1);
    }

    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions { WriteIndented = false };
}
