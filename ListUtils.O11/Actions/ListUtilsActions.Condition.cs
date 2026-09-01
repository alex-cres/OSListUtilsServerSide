using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace OutSystems.NssListUtils;

public partial class CssListUtils
{
    public void MssList_PopByCondition(
        string ssSourceListJson,
        string ssPropertyName,
        string ssTargetValue,
        string ssComparisonOperator,
        bool ssCaseSensitive,
        bool ssSearchFromEnd,
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
        var segments = SplitPath(ssPropertyName);
        int matchedIndex = -1;

        if (ssSearchFromEnd)
        {
            for (int i = array.Count - 1; i >= 0; i--)
            {
                var value = GetPropertyValue(array[i]!, segments);
                if (value != null && MatchesCondition(value, ssTargetValue, ssComparisonOperator, ssCaseSensitive))
                {
                    matchedIndex = i;
                    break;
                }
            }
        }
        else
        {
            for (int i = 0; i < array.Count; i++)
            {
                var value = GetPropertyValue(array[i]!, segments);
                if (value != null && MatchesCondition(value, ssTargetValue, ssComparisonOperator, ssCaseSensitive))
                {
                    matchedIndex = i;
                    break;
                }
            }
        }

        JsonNode? matchedNode = null;
        if (matchedIndex >= 0)
        {
            matchedNode = array[matchedIndex];
            array.RemoveAt(matchedIndex);
        }

        ssUpdatedListJson = array.ToJsonString(JsonOptions);
        ssPoppedElementJson = matchedNode?.ToJsonString(JsonOptions) ?? "{}";
    }

    public void MssList_PopMultipleByCondition(
        string ssSourceListJson,
        string ssPropertyName,
        string ssTargetValue,
        string ssComparisonOperator,
        bool ssCaseSensitive,
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
        var segments = SplitPath(ssPropertyName);

        foreach (var item in DrainToArray(originalArray))
        {
            var value = GetPropertyValue(item!, segments);
            if (value != null && MatchesCondition(value, ssTargetValue, ssComparisonOperator, ssCaseSensitive))
                poppedArray.Add(item);
            else
                keptArray.Add(item);
        }

        ssUpdatedListJson = keptArray.ToJsonString(JsonOptions);
        ssPoppedElementsJson = poppedArray.ToJsonString(JsonOptions);
    }

    public void MssList_PopByConditions(
        string ssSourceListJson,
        List<Condition> ssConditions,
        string ssLogicalOperator,
        bool ssSearchFromEnd,
        out string ssUpdatedListJson,
        out string ssPoppedElementJson)
    {
        if (string.IsNullOrEmpty(ssSourceListJson))
        {
            ssUpdatedListJson = "[]";
            ssPoppedElementJson = "{}";
            return;
        }

        var array = JsonNode.Parse(ssSourceListJson!)!.AsArray();

        if (ssConditions == null || ssConditions.Count == 0)
        {
            ssUpdatedListJson = ssSourceListJson;
            ssPoppedElementJson = "{}";
            return;
        }

        int matchedIndex = -1;
        var compiled = CompileConditions(ssConditions);
        if (ssSearchFromEnd)
        {
            for (int i = array.Count - 1; i >= 0; i--)
            {
                if (EvaluateConditions(array[i]!, compiled, ssLogicalOperator))
                {
                    matchedIndex = i;
                    break;
                }
            }
        }
        else
        {
            for (int i = 0; i < array.Count; i++)
            {
                if (EvaluateConditions(array[i]!, compiled, ssLogicalOperator))
                {
                    matchedIndex = i;
                    break;
                }
            }
        }

        JsonNode? matchedNode = null;
        if (matchedIndex >= 0)
        {
            matchedNode = array[matchedIndex];
            array.RemoveAt(matchedIndex);
        }

        ssUpdatedListJson = array.ToJsonString(JsonOptions);
        ssPoppedElementJson = matchedNode == null ? "{}" : matchedNode.ToJsonString(JsonOptions);
    }

    public void MssList_PopMultipleByConditions(
        string ssSourceListJson,
        List<Condition> ssConditions,
        string ssLogicalOperator,
        out string ssUpdatedListJson,
        out string ssPoppedElementsJson)
    {
        if (string.IsNullOrEmpty(ssSourceListJson))
        {
            ssUpdatedListJson = "[]";
            ssPoppedElementsJson = "[]";
            return;
        }

        var originalArray = JsonNode.Parse(ssSourceListJson!)!.AsArray();

        if (ssConditions == null || ssConditions.Count == 0)
        {
            ssUpdatedListJson = ssSourceListJson;
            ssPoppedElementsJson = "[]";
            return;
        }

        var keptArray = new JsonArray();
        var poppedArray = new JsonArray();
        var compiled = CompileConditions(ssConditions);

        foreach (var item in DrainToArray(originalArray))
        {
            if (EvaluateConditions(item!, compiled, ssLogicalOperator))
                poppedArray.Add(item);
            else
                keptArray.Add(item);
        }

        ssUpdatedListJson = keptArray.ToJsonString(JsonOptions);
        ssPoppedElementsJson = poppedArray.ToJsonString(JsonOptions);
    }

    public void MssList_Partition(
        string ssSourceListJson,
        string ssPropertyName,
        string ssTargetValue,
        string ssComparisonOperator,
        bool ssCaseSensitive,
        out string ssMatchingListJson,
        out string ssNonMatchingListJson)
    {
        ssMatchingListJson = "[]";
        ssNonMatchingListJson = "[]";
        if (string.IsNullOrEmpty(ssSourceListJson)) return;

        var array = JsonNode.Parse(ssSourceListJson)!.AsArray();
        if (string.IsNullOrEmpty(ssPropertyName))
        {
            ssNonMatchingListJson = ssSourceListJson;
            return;
        }

        var matching = new JsonArray();
        var nonMatching = new JsonArray();
        var segments = SplitPath(ssPropertyName);
        foreach (var item in DrainToArray(array))
        {
            var value = GetPropertyValue(item!, segments);
            bool match = value != null && MatchesCondition(value, ssTargetValue, ssComparisonOperator, ssCaseSensitive);
            (match ? matching : nonMatching).Add(item);
        }

        ssMatchingListJson = matching.ToJsonString(JsonOptions);
        ssNonMatchingListJson = nonMatching.ToJsonString(JsonOptions);
    }

    public void MssList_PartitionByConditions(
        string ssSourceListJson,
        List<Condition> ssConditions,
        string ssLogicalOperator,
        out string ssMatchingListJson,
        out string ssNonMatchingListJson)
    {
        ssMatchingListJson = "[]";
        ssNonMatchingListJson = "[]";
        if (string.IsNullOrEmpty(ssSourceListJson)) return;

        var array = JsonNode.Parse(ssSourceListJson!)!.AsArray();
        if (ssConditions == null || ssConditions.Count == 0)
        {
            ssNonMatchingListJson = ssSourceListJson;
            return;
        }

        var matching = new JsonArray();
        var nonMatching = new JsonArray();
        var compiled = CompileConditions(ssConditions);
        foreach (var item in DrainToArray(array))
        {
            bool match = EvaluateConditions(item!, compiled, ssLogicalOperator);
            (match ? matching : nonMatching).Add(item);
        }

        ssMatchingListJson = matching.ToJsonString(JsonOptions);
        ssNonMatchingListJson = nonMatching.ToJsonString(JsonOptions);
    }
}
