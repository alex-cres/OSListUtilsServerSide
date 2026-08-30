using System.Text.Json.Nodes;

namespace ListUtils;

public partial class ListUtils
{
    public void List_Pop(
        string SourceListJson,
        int Index,
        out string UpdatedListJson,
        out string PoppedElementJson)
    {
        if (string.IsNullOrEmpty(SourceListJson))
        {
            UpdatedListJson = "[]";
            PoppedElementJson = "null";
            return;
        }

        var array = JsonNode.Parse(SourceListJson)!.AsArray();

        if (Index < 0 || Index >= array.Count)
        {
            UpdatedListJson = SourceListJson;
            PoppedElementJson = "null";
            return;
        }

        var popped = array[Index];
        PoppedElementJson = popped?.ToJsonString(JsonOptions) ?? "null";
        array.RemoveAt(Index);
        UpdatedListJson = array.ToJsonString(JsonOptions);
    }

    public void List_PopMultiple(
        string SourceListJson,
        string IndicesToPop,
        out string UpdatedListJson,
        out string PoppedElementsJson)
    {
        if (string.IsNullOrEmpty(SourceListJson))
        {
            UpdatedListJson = "[]";
            PoppedElementsJson = "[]";
            return;
        }

        var array = JsonNode.Parse(SourceListJson)!.AsArray();

        if (string.IsNullOrEmpty(IndicesToPop))
        {
            UpdatedListJson = SourceListJson;
            PoppedElementsJson = "[]";
            return;
        }

        var indices = IndicesToPop.Split(',')
            .Select(s => int.TryParse(s.Trim(), out var v) ? v : -1)
            .Where(i => i >= 0)
            .Distinct()
            .OrderByDescending(i => i)
            .ToList();

        var poppedArray = new JsonArray();

        foreach (int idx in indices)
        {
            if (idx < array.Count)
            {
                var item = array[idx];
                poppedArray.Add(item!.DeepClone());
                array.RemoveAt(idx);
            }
        }

        var ordered = new JsonArray();
        for (int i = poppedArray.Count - 1; i >= 0; i--)
            ordered.Add(poppedArray[i]!.DeepClone());

        UpdatedListJson = array.ToJsonString(JsonOptions);
        PoppedElementsJson = ordered.ToJsonString(JsonOptions);
    }

    public void List_SplitAt(
        string SourceListJson,
        int Index,
        out string LeftListJson,
        out string RightListJson)
    {
        if (string.IsNullOrEmpty(SourceListJson))
        {
            LeftListJson = "[]";
            RightListJson = "[]";
            return;
        }

        var array = JsonNode.Parse(SourceListJson)!.AsArray();
        int n = array.Count;
        int split = Index < 0 ? n + Index : Index;
        if (split < 0) split = 0;
        if (split > n) split = n;

        var left = new JsonArray();
        var right = new JsonArray();
        for (int i = 0; i < n; i++)
        {
            (i < split ? left : right).Add(array[i]!.DeepClone());
        }

        LeftListJson = left.ToJsonString(JsonOptions);
        RightListJson = right.ToJsonString(JsonOptions);
    }
}
