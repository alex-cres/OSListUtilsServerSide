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
        int index = Index < 0 ? array.Count + Index : Index;

        if (index < 0 || index >= array.Count)
        {
            UpdatedListJson = SourceListJson;
            PoppedElementJson = "null";
            return;
        }

        var popped = array[index];
        PoppedElementJson = popped?.ToJsonString(JsonOptions) ?? "null";
        array.RemoveAt(index);
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

        // Negative indices count from the end; non-numeric and out-of-range entries are ignored.
        // Normalise before Distinct so "-1" and "N-1" collapse to one removal.
        int count = array.Count;
        var indices = IndicesToPop.Split(',')
            .Select(s => int.TryParse(s.Trim(), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var v) ? (v < 0 ? count + v : v) : -1)
            .Where(i => i >= 0 && i < count)
            .Distinct()
            .OrderByDescending(i => i)
            .ToList();

        // Detach in descending-index order (indices remain valid), stash refs,
        // then re-parent them into `poppedArray` in ascending order — no clones.
        var picked = new List<JsonNode?>(indices.Count);
        foreach (int idx in indices)
        {
            if (idx < array.Count)
            {
                var item = array[idx];
                array.RemoveAt(idx);
                picked.Add(item);
            }
        }

        var poppedArray = new JsonArray();
        for (int i = picked.Count - 1; i >= 0; i--)
            poppedArray.Add(picked[i]);

        UpdatedListJson = array.ToJsonString(JsonOptions);
        PoppedElementsJson = poppedArray.ToJsonString(JsonOptions);
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

        var picked = DrainToArray(array);
        var left = new JsonArray();
        var right = new JsonArray();
        for (int i = 0; i < n; i++)
        {
            (i < split ? left : right).Add(picked[i]);
        }

        LeftListJson = left.ToJsonString(JsonOptions);
        RightListJson = right.ToJsonString(JsonOptions);
    }
}
