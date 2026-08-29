using System.Text.Json.Nodes;

namespace ListUtils;

public partial class ListUtils
{
    public void List_PopByCondition(
        string SourceListJson,
        string PropertyName,
        string TargetValue,
        string ComparisonOperator,
        bool CaseSensitive,
        bool SearchFromEnd,
        out string UpdatedListJson,
        out string PoppedElementJson)
    {
        if (string.IsNullOrEmpty(SourceListJson) || string.IsNullOrEmpty(PropertyName))
        {
            UpdatedListJson = SourceListJson ?? "[]";
            PoppedElementJson = "{}";
            return;
        }

        var array = JsonNode.Parse(SourceListJson)!.AsArray();
        JsonNode? matchedNode = null;
        int matchedIndex = -1;

        if (SearchFromEnd)
        {
            for (int i = array.Count - 1; i >= 0; i--)
            {
                var value = GetPropertyValue(array[i]!, PropertyName);
                if (value != null && MatchesCondition(value, TargetValue, ComparisonOperator, CaseSensitive))
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
                var value = GetPropertyValue(array[i]!, PropertyName);
                if (value != null && MatchesCondition(value, TargetValue, ComparisonOperator, CaseSensitive))
                {
                    matchedIndex = i;
                    break;
                }
            }
        }

        if (matchedIndex >= 0)
        {
            matchedNode = array[matchedIndex];
            array.RemoveAt(matchedIndex);
        }

        UpdatedListJson = array.ToJsonString(JsonOptions);
        PoppedElementJson = matchedNode?.ToJsonString(JsonOptions) ?? "{}";
    }

    public void List_PopMultipleByCondition(
        string SourceListJson,
        string PropertyName,
        string TargetValue,
        string ComparisonOperator,
        bool CaseSensitive,
        out string UpdatedListJson,
        out string PoppedElementsJson)
    {
        if (string.IsNullOrEmpty(SourceListJson) || string.IsNullOrEmpty(PropertyName))
        {
            UpdatedListJson = SourceListJson ?? "[]";
            PoppedElementsJson = "[]";
            return;
        }

        var originalArray = JsonNode.Parse(SourceListJson)!.AsArray();
        var keptArray = new JsonArray();
        var poppedArray = new JsonArray();

        foreach (var item in originalArray)
        {
            var value = GetPropertyValue(item!, PropertyName);
            if (value != null && MatchesCondition(value, TargetValue, ComparisonOperator, CaseSensitive))
                poppedArray.Add(item!.DeepClone());
            else
                keptArray.Add(item!.DeepClone());
        }

        UpdatedListJson = keptArray.ToJsonString(JsonOptions);
        PoppedElementsJson = poppedArray.ToJsonString(JsonOptions);
    }

    public void List_PopByConditions(
        string SourceListJson,
        string ConditionsJson,
        string LogicalOperator,
        bool SearchFromEnd,
        out string UpdatedListJson,
        out string PoppedElementJson)
    {
        if (string.IsNullOrEmpty(SourceListJson))
        {
            UpdatedListJson = "[]";
            PoppedElementJson = "{}";
            return;
        }

        var conditions = ParseConditions(ConditionsJson);
        var array = JsonNode.Parse(SourceListJson)!.AsArray();

        if (conditions.Count == 0)
        {
            UpdatedListJson = SourceListJson;
            PoppedElementJson = "{}";
            return;
        }

        int matchedIndex = -1;
        if (SearchFromEnd)
        {
            for (int i = array.Count - 1; i >= 0; i--)
            {
                if (EvaluateConditions(array[i]!, conditions, LogicalOperator))
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
                if (EvaluateConditions(array[i]!, conditions, LogicalOperator))
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

        UpdatedListJson = array.ToJsonString(JsonOptions);
        PoppedElementJson = matchedNode?.ToJsonString(JsonOptions) ?? "{}";
    }

    public void List_PopMultipleByConditions(
        string SourceListJson,
        string ConditionsJson,
        string LogicalOperator,
        out string UpdatedListJson,
        out string PoppedElementsJson)
    {
        if (string.IsNullOrEmpty(SourceListJson))
        {
            UpdatedListJson = "[]";
            PoppedElementsJson = "[]";
            return;
        }

        var conditions = ParseConditions(ConditionsJson);
        var originalArray = JsonNode.Parse(SourceListJson)!.AsArray();

        if (conditions.Count == 0)
        {
            UpdatedListJson = SourceListJson;
            PoppedElementsJson = "[]";
            return;
        }

        var keptArray = new JsonArray();
        var poppedArray = new JsonArray();

        foreach (var item in originalArray)
        {
            if (EvaluateConditions(item!, conditions, LogicalOperator))
                poppedArray.Add(item!.DeepClone());
            else
                keptArray.Add(item!.DeepClone());
        }

        UpdatedListJson = keptArray.ToJsonString(JsonOptions);
        PoppedElementsJson = poppedArray.ToJsonString(JsonOptions);
    }
}
