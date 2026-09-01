using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace OutSystems.NssListUtils;

public partial class CssListUtils
{
    public void MssList_Chunk(
        string ssSourceListJson,
        int ssChunkSize,
        out List<string> ssChunksListJson)
    {
        ssChunksListJson = new List<string>();
        if (string.IsNullOrEmpty(ssSourceListJson) || ssChunkSize <= 0)
            return;

        var array = JsonNode.Parse(ssSourceListJson!)!.AsArray();
        int n = array.Count;
        var picked = DrainToArray(array);

        JsonArray current = new JsonArray();
        for (int i = 0; i < n; i++)
        {
            if (current.Count == ssChunkSize)
            {
                ssChunksListJson.Add(current.ToJsonString(JsonOptions));
                current = new JsonArray();
            }
            current.Add(picked[i]);
        }
        if (current.Count > 0)
            ssChunksListJson.Add(current.ToJsonString(JsonOptions));
    }

    public void MssList_DistinctBy(
        string ssSourceListJson,
        string ssPropertyName,
        bool ssCaseSensitive,
        out string ssDistinctListJson)
    {
        if (string.IsNullOrEmpty(ssSourceListJson))
        {
            ssDistinctListJson = "[]";
            return;
        }

        var array = JsonNode.Parse(ssSourceListJson!)!.AsArray();
        var picked = DrainToArray(array);
        var result = new JsonArray();

        if (string.IsNullOrEmpty(ssPropertyName))
        {
            var seenRaw = new HashSet<string>(ssCaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase);
            foreach (var item in picked)
            {
                var raw = item?.ToJsonString(JsonOptions) ?? "null";
                if (seenRaw.Add(raw))
                    result.Add(item);
            }
            ssDistinctListJson = result.ToJsonString(JsonOptions);
            return;
        }

        var cmp = ssCaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;
        var seen = new HashSet<string>(cmp);
        bool nullSeen = false;
        var segments = SplitPath(ssPropertyName);

        foreach (var item in picked)
        {
            var key = GetPropertyValue(item!, segments);
            if (key == null)
            {
                if (nullSeen) continue;
                nullSeen = true;
                result.Add(item);
            }
            else if (seen.Add(key))
            {
                result.Add(item);
            }
        }

        ssDistinctListJson = result.ToJsonString(JsonOptions);
    }

    public void MssList_Slice(
        string ssSourceListJson,
        int ssStart,
        int ssEnd,
        int ssStep,
        out string ssSliceListJson)
    {
        if (string.IsNullOrEmpty(ssSourceListJson))
        {
            ssSliceListJson = "[]";
            return;
        }

        var array = JsonNode.Parse(ssSourceListJson!)!.AsArray();
        int n = array.Count;
        if (n == 0)
        {
            ssSliceListJson = "[]";
            return;
        }

        int step = ssStep == 0 ? 1 : ssStep;
        bool endIsSentinel = ssEnd == 0;

        int start = NormalizeSliceStart(ssStart, n, step);
        int end = endIsSentinel
            ? (step > 0 ? n : -1)
            : NormalizeSliceEnd(ssEnd, n, step);

        var picked = DrainToArray(array);
        var result = new JsonArray();
        if (step > 0)
        {
            for (int i = start; i < end && i < n; i += step)
            {
                if (i < 0) continue;
                result.Add(picked[i]);
            }
        }
        else
        {
            for (int i = start; i > end && i >= 0; i += step)
            {
                if (i >= n) continue;
                result.Add(picked[i]);
            }
        }

        ssSliceListJson = result.ToJsonString(JsonOptions);
    }

    private static int NormalizeSliceStart(int raw, int length, int step)
    {
        int i = raw < 0 ? length + raw : raw;
        if (step > 0)
        {
            if (i < 0) i = 0;
            if (i > length) i = length;
        }
        else
        {
            if (i < 0) i = -1;
            if (i >= length) i = length - 1;
        }
        return i;
    }

    private static int NormalizeSliceEnd(int raw, int length, int step)
    {
        int i = raw < 0 ? length + raw : raw;
        if (step > 0)
        {
            if (i < 0) i = 0;
            if (i > length) i = length;
        }
        else
        {
            if (i < -1) i = -1;
            if (i >= length) i = length;
        }
        return i;
    }

    public void MssList_Shuffle(
        string ssSourceListJson,
        int ssSeed,
        out string ssShuffledListJson)
    {
        if (string.IsNullOrEmpty(ssSourceListJson))
        {
            ssShuffledListJson = "[]";
            return;
        }

        var source = JsonNode.Parse(ssSourceListJson!)!.AsArray();
        var buffer = DrainToArray(source);

        if (ssSeed != 0)
        {
            var rng = new Random(ssSeed);
            for (int i = buffer.Length - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                var tmp = buffer[i];
                buffer[i] = buffer[j];
                buffer[j] = tmp;
            }
        }
        else
        {
            using (var crypto = RandomNumberGenerator.Create())
            {
                var word = new byte[4];
                for (int i = buffer.Length - 1; i > 0; i--)
                {
                    crypto.GetBytes(word);
                    uint rand = BitConverter.ToUInt32(word, 0);
                    int j = (int)(rand % (uint)(i + 1));
                    var tmp = buffer[i];
                    buffer[i] = buffer[j];
                    buffer[j] = tmp;
                }
            }
        }

        var result = new JsonArray();
        foreach (var node in buffer)
            result.Add(node);

        ssShuffledListJson = result.ToJsonString(JsonOptions);
    }

    public void MssList_UpdateAt(
        string ssSourceListJson,
        int ssIndex,
        string ssPropertyName,
        string ssNewValueJson,
        out string ssUpdatedListJson,
        out string ssPreviousValueJson)
    {
        if (string.IsNullOrEmpty(ssSourceListJson))
        {
            ssUpdatedListJson = "[]";
            ssPreviousValueJson = "null";
            return;
        }

        var array = JsonNode.Parse(ssSourceListJson!)!.AsArray();
        int n = array.Count;
        int i = ssIndex < 0 ? n + ssIndex : ssIndex;

        if (i < 0 || i >= n || string.IsNullOrEmpty(ssPropertyName))
        {
            ssUpdatedListJson = ssSourceListJson;
            ssPreviousValueJson = "null";
            return;
        }

        var item = array[i];
        if (!(item is JsonObject))
        {
            ssUpdatedListJson = ssSourceListJson;
            ssPreviousValueJson = "null";
            return;
        }

        var newValue = ParseValueOrString(ssNewValueJson);
        var previous = SetPropertyValue(item!, ssPropertyName, newValue);
        ssPreviousValueJson = previous == null ? "null" : previous.ToJsonString(JsonOptions);
        ssUpdatedListJson = array.ToJsonString(JsonOptions);
    }

    private static JsonNode? ParseValueOrString(string raw)
    {
        if (raw == null) return null;
        try
        {
            return JsonNode.Parse(raw);
        }
        catch (JsonException)
        {
            return JsonValue.Create(raw);
        }
    }

    private static JsonNode? SetPropertyValue(JsonNode root, string propertyPath, JsonNode? newValue)
    {
        var segments = propertyPath.Split('.');
        JsonNode? current = root;
        for (int idx = 0; idx < segments.Length - 1; idx++)
        {
            current = NavigateOrCreateSegment(current, segments[idx]);
            if (current == null) return null;
        }

        var last = segments[segments.Length - 1];
        return SetTerminalSegment(current, last, newValue);
    }

    private static JsonNode? NavigateOrCreateSegment(JsonNode? current, string segment)
    {
        if (current == null || string.IsNullOrEmpty(segment)) return null;

        var parsed = ParseSegment(segment);
        string name = parsed.name;
        int? index = parsed.index;

        JsonNode? next = current;
        if (!string.IsNullOrEmpty(name))
        {
            if (!(current is JsonObject obj)) return null;
            if (obj.TryGetPropertyValue(name, out var val) && val != null)
            {
                next = val;
            }
            else
            {
                string camel = ToCamelCase(name);
                if (!name.Equals(camel, StringComparison.Ordinal)
                    && obj.TryGetPropertyValue(camel, out var camelVal) && camelVal != null)
                {
                    next = camelVal;
                }
                else
                {
                    var created = new JsonObject();
                    obj[name] = created;
                    next = created;
                }
            }
        }

        if (index.HasValue)
        {
            if (!(next is JsonArray arr)) return null;
            int i = index.Value < 0 ? arr.Count + index.Value : index.Value;
            if (i < 0 || i >= arr.Count) return null;
            return arr[i];
        }

        return next;
    }

    private static JsonNode? SetTerminalSegment(JsonNode? current, string segment, JsonNode? newValue)
    {
        if (current == null || string.IsNullOrEmpty(segment)) return null;

        var parsed = ParseSegment(segment);
        string name = parsed.name;
        int? index = parsed.index;

        JsonNode? target = current;
        JsonObject? parentObj = null;
        string effectiveName = name;
        if (!string.IsNullOrEmpty(name))
        {
            if (!(current is JsonObject obj)) return null;
            parentObj = obj;
            if (!obj.TryGetPropertyValue(name, out var val))
            {
                string camel = ToCamelCase(name);
                if (!name.Equals(camel, StringComparison.Ordinal) && obj.ContainsKey(camel))
                {
                    val = obj[camel];
                    effectiveName = camel;
                }
            }
            target = val;
        }

        if (index.HasValue)
        {
            if (!(target is JsonArray arr)) return null;
            int i = index.Value < 0 ? arr.Count + index.Value : index.Value;
            if (i < 0 || i >= arr.Count) return null;
            var prev = arr[i] == null ? null : arr[i]!.DeepClone();
            arr[i] = newValue == null ? null : newValue.DeepClone();
            return prev;
        }

        if (parentObj == null) return null;
        JsonNode? previous = null;
        if (parentObj.TryGetPropertyValue(effectiveName, out var existing))
            previous = existing == null ? null : existing.DeepClone();
        parentObj[effectiveName] = newValue == null ? null : newValue.DeepClone();
        return previous;
    }

    private static (string name, int? index) ParseSegment(string segment)
    {
        var bracketStart = segment.IndexOf('[');
        if (bracketStart < 0) return (segment, null);
        var bracketEnd = segment.IndexOf(']', bracketStart);
        if (bracketEnd <= bracketStart) return (segment, null);
        var idxStr = segment.Substring(bracketStart + 1, bracketEnd - bracketStart - 1);
        if (!int.TryParse(idxStr, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var idx))
            return (segment, null);
        return (segment.Substring(0, bracketStart), idx);
    }

    public void MssList_Reverse(
        string ssSourceListJson,
        out string ssReversedListJson)
    {
        if (string.IsNullOrEmpty(ssSourceListJson))
        {
            ssReversedListJson = "[]";
            return;
        }

        var array = JsonNode.Parse(ssSourceListJson!)!.AsArray();
        int n = array.Count;
        var result = new JsonArray();
        // Tail-drain → items detach in reverse order; add each directly.
        for (int i = n - 1; i >= 0; i--)
        {
            var item = array[i];
            array.RemoveAt(i);
            result.Add(item);
        }

        ssReversedListJson = result.ToJsonString(JsonOptions);
    }

    public void MssList_Flatten(
        List<string> ssChunksListJson,
        out string ssFlatListJson)
    {
        var result = new JsonArray();
        if (ssChunksListJson != null)
        {
            foreach (var entry in ssChunksListJson)
            {
                if (string.IsNullOrEmpty(entry)) continue;
                JsonNode? parsed;
                try { parsed = JsonNode.Parse(entry!); }
                catch (JsonException) { continue; }
                if (!(parsed is JsonArray inner)) continue;
                var picked = DrainToArray(inner);
                for (int i = 0; i < picked.Length; i++)
                    result.Add(picked[i]);
            }
        }
        ssFlatListJson = result.ToJsonString(JsonOptions);
    }

    public void MssList_Sample(
        string ssSourceListJson,
        int ssSampleSize,
        int ssSeed,
        out string ssSampleListJson)
    {
        if (string.IsNullOrEmpty(ssSourceListJson) || ssSampleSize <= 0)
        {
            ssSampleListJson = "[]";
            return;
        }

        var source = JsonNode.Parse(ssSourceListJson!)!.AsArray();
        int n = source.Count;
        if (n == 0)
        {
            ssSampleListJson = "[]";
            return;
        }

        int take = ssSampleSize > n ? n : ssSampleSize;
        var buffer = DrainToArray(source);

        if (ssSeed != 0)
        {
            var rng = new Random(ssSeed);
            for (int i = 0; i < take; i++)
            {
                int j = i + rng.Next(n - i);
                var tmp = buffer[i]; buffer[i] = buffer[j]; buffer[j] = tmp;
            }
        }
        else
        {
            using (var crypto = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                var word = new byte[4];
                for (int i = 0; i < take; i++)
                {
                    crypto.GetBytes(word);
                    uint rand = BitConverter.ToUInt32(word, 0);
                    int j = i + (int)(rand % (uint)(n - i));
                    var tmp = buffer[i]; buffer[i] = buffer[j]; buffer[j] = tmp;
                }
            }
        }

        var result = new JsonArray();
        for (int i = 0; i < take; i++) result.Add(buffer[i]);

        ssSampleListJson = result.ToJsonString(JsonOptions);
    }

    public void MssList_ReplaceWhere(
        string ssSourceListJson,
        List<Condition> ssConditions,
        string ssLogicalOperator,
        string ssUpdateProperty,
        string ssNewValueJson,
        out string ssUpdatedListJson,
        out int ssMatchCount)
    {
        ssUpdatedListJson = ssSourceListJson ?? "[]";
        ssMatchCount = 0;
        if (string.IsNullOrEmpty(ssSourceListJson) || string.IsNullOrEmpty(ssUpdateProperty))
            return;

        if (ssConditions == null || ssConditions.Count == 0) return;

        var array = JsonNode.Parse(ssSourceListJson!)!.AsArray();
        var newValue = ParseValueOrString(ssNewValueJson);
        var compiled = CompileConditions(ssConditions);

        foreach (var item in array)
        {
            if (!EvaluateConditions(item!, compiled, ssLogicalOperator)) continue;
            if (!(item is JsonObject)) continue;
            SetPropertyValue(item!, ssUpdateProperty, newValue);
            ssMatchCount++;
        }

        ssUpdatedListJson = array.ToJsonString(JsonOptions);
    }

    public void MssList_UpdateMultipleAt(
        string ssSourceListJson,
        string ssIndicesToUpdate,
        string ssPropertyName,
        string ssNewValueJson,
        out string ssUpdatedListJson,
        out int ssUpdatedCount)
    {
        ssUpdatedListJson = ssSourceListJson ?? "[]";
        ssUpdatedCount = 0;
        if (string.IsNullOrEmpty(ssSourceListJson) || string.IsNullOrEmpty(ssPropertyName) || string.IsNullOrEmpty(ssIndicesToUpdate))
            return;

        var array = JsonNode.Parse(ssSourceListJson!)!.AsArray();
        int n = array.Count;
        var newValue = ParseValueOrString(ssNewValueJson);

        var seen = new HashSet<int>();
        foreach (var raw in ssIndicesToUpdate.Split(','))
        {
            if (!int.TryParse(raw.Trim(), out var idx)) continue;
            int actual = idx < 0 ? n + idx : idx;
            if (actual < 0 || actual >= n) continue;
            if (!seen.Add(actual)) continue;
            if (!(array[actual] is JsonObject)) continue;
            SetPropertyValue(array[actual]!, ssPropertyName, newValue);
            ssUpdatedCount++;
        }

        ssUpdatedListJson = array.ToJsonString(JsonOptions);
    }
}

