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

        ssUpdatedListJson = array.ToJsonString(JsonOptions);
        ssPoppedElementsJson = ordered.ToJsonString(JsonOptions);
    }
}
