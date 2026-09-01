using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace OutSystems.NssListUtils;

public partial class CssListUtils
{
    public void MssList_Pop(
        string ssSourceListJson,
        int ssIndex,
        out string ssUpdatedListJson,
        out string ssPoppedElementJson)
    {
        if (string.IsNullOrEmpty(ssSourceListJson))
        {
            ssUpdatedListJson = "[]";
            ssPoppedElementJson = "null";
            return;
        }

        var array = JsonNode.Parse(ssSourceListJson)!.AsArray();

        if (ssIndex < 0 || ssIndex >= array.Count)
        {
            ssUpdatedListJson = ssSourceListJson;
            ssPoppedElementJson = "null";
            return;
        }

        var popped = array[ssIndex];
        ssPoppedElementJson = popped?.ToJsonString(JsonOptions) ?? "null";
        array.RemoveAt(ssIndex);
        ssUpdatedListJson = array.ToJsonString(JsonOptions);
    }

    public void MssList_PopMultiple(
        string ssSourceListJson,
        string ssIndicesToPop,
        out string ssUpdatedListJson,
        out string ssPoppedElementsJson)
    {
        if (string.IsNullOrEmpty(ssSourceListJson))
        {
            ssUpdatedListJson = "[]";
            ssPoppedElementsJson = "[]";
            return;
        }

        var array = JsonNode.Parse(ssSourceListJson)!.AsArray();

        if (string.IsNullOrEmpty(ssIndicesToPop))
        {
            ssUpdatedListJson = ssSourceListJson;
            ssPoppedElementsJson = "[]";
            return;
        }

        var indices = ssIndicesToPop.Split(',')
            .Select(s => int.TryParse(s.Trim(), out var v) ? v : -1)
            .Where(i => i >= 0)
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

        ssUpdatedListJson = array.ToJsonString(JsonOptions);
        ssPoppedElementsJson = poppedArray.ToJsonString(JsonOptions);
    }

    public void MssList_SplitAt(
        string ssSourceListJson,
        int ssIndex,
        out string ssLeftListJson,
        out string ssRightListJson)
    {
        if (string.IsNullOrEmpty(ssSourceListJson))
        {
            ssLeftListJson = "[]";
            ssRightListJson = "[]";
            return;
        }

        var array = JsonNode.Parse(ssSourceListJson)!.AsArray();
        int n = array.Count;
        int split = ssIndex < 0 ? n + ssIndex : ssIndex;
        if (split < 0) split = 0;
        if (split > n) split = n;

        var picked = DrainToArray(array);
        var left = new JsonArray();
        var right = new JsonArray();
        for (int i = 0; i < n; i++)
        {
            (i < split ? left : right).Add(picked[i]);
        }

        ssLeftListJson = left.ToJsonString(JsonOptions);
        ssRightListJson = right.ToJsonString(JsonOptions);
    }
}
