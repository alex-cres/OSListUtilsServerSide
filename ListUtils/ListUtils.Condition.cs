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
        var segments = SplitPath(PropertyName);
        JsonNode? matchedNode = null;
        int matchedIndex = -1;

        if (SearchFromEnd)
        {
            for (int i = array.Count - 1; i >= 0; i--)
            {
                var value = GetPropertyValue(array[i]!, segments);
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
                var value = GetPropertyValue(array[i]!, segments);
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
        var segments = SplitPath(PropertyName);

        foreach (var item in DrainToArray(originalArray))
        {
            var value = GetPropertyValue(item!, segments);
            if (value != null && MatchesCondition(value, TargetValue, ComparisonOperator, CaseSensitive))
                poppedArray.Add(item);
            else
                keptArray.Add(item);
        }

        UpdatedListJson = keptArray.ToJsonString(JsonOptions);
        PoppedElementsJson = poppedArray.ToJsonString(JsonOptions);
    }

    public void List_PopByConditions(
        string SourceListJson,
        List<Condition> Conditions,
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

        var array = JsonNode.Parse(SourceListJson)!.AsArray();

        if (Conditions == null || Conditions.Count == 0)
        {
            UpdatedListJson = SourceListJson;
            PoppedElementJson = "{}";
            return;
        }

        int matchedIndex = -1;
        var compiled = CompileConditions(Conditions);
        if (SearchFromEnd)
        {
            for (int i = array.Count - 1; i >= 0; i--)
            {
                if (EvaluateConditions(array[i]!, compiled, LogicalOperator))
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
                if (EvaluateConditions(array[i]!, compiled, LogicalOperator))
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
        List<Condition> Conditions,
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

        var originalArray = JsonNode.Parse(SourceListJson)!.AsArray();

        if (Conditions == null || Conditions.Count == 0)
        {
            UpdatedListJson = SourceListJson;
            PoppedElementsJson = "[]";
            return;
        }

        var keptArray = new JsonArray();
        var poppedArray = new JsonArray();
        var compiled = CompileConditions(Conditions);

        foreach (var item in DrainToArray(originalArray))
        {
            if (EvaluateConditions(item!, compiled, LogicalOperator))
                poppedArray.Add(item);
            else
                keptArray.Add(item);
        }

        UpdatedListJson = keptArray.ToJsonString(JsonOptions);
        PoppedElementsJson = poppedArray.ToJsonString(JsonOptions);
    }

    public void List_Partition(
        string SourceListJson,
        string PropertyName,
        string TargetValue,
        string ComparisonOperator,
        bool CaseSensitive,
        out string MatchingListJson,
        out string NonMatchingListJson)
    {
        MatchingListJson = "[]";
        NonMatchingListJson = "[]";
        if (string.IsNullOrEmpty(SourceListJson)) return;

        var array = JsonNode.Parse(SourceListJson)!.AsArray();
        if (string.IsNullOrEmpty(PropertyName))
        {
            NonMatchingListJson = SourceListJson;
            return;
        }

        var matching = new JsonArray();
        var nonMatching = new JsonArray();
        var segments = SplitPath(PropertyName);
        foreach (var item in DrainToArray(array))
        {
            var value = GetPropertyValue(item!, segments);
            bool match = value != null && MatchesCondition(value, TargetValue, ComparisonOperator, CaseSensitive);
            (match ? matching : nonMatching).Add(item);
        }

        MatchingListJson = matching.ToJsonString(JsonOptions);
        NonMatchingListJson = nonMatching.ToJsonString(JsonOptions);
    }

    public void List_PartitionByConditions(
        string SourceListJson,
        List<Condition> Conditions,
        string LogicalOperator,
        out string MatchingListJson,
        out string NonMatchingListJson)
    {
        MatchingListJson = "[]";
        NonMatchingListJson = "[]";
        if (string.IsNullOrEmpty(SourceListJson)) return;

        var array = JsonNode.Parse(SourceListJson)!.AsArray();
        if (Conditions == null || Conditions.Count == 0)
        {
            NonMatchingListJson = SourceListJson;
            return;
        }

        var matching = new JsonArray();
        var nonMatching = new JsonArray();
        var compiled = CompileConditions(Conditions);
        foreach (var item in DrainToArray(array))
        {
            bool match = EvaluateConditions(item!, compiled, LogicalOperator);
            (match ? matching : nonMatching).Add(item);
        }

        MatchingListJson = matching.ToJsonString(JsonOptions);
        NonMatchingListJson = nonMatching.ToJsonString(JsonOptions);
    }
}
