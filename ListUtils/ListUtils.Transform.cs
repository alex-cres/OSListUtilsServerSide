using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ListUtils;

public partial class ListUtils
{
    public void List_Chunk(
        string SourceListJson,
        int ChunkSize,
        out List<string> ChunksListJson)
    {
        ChunksListJson = new List<string>();
        if (string.IsNullOrEmpty(SourceListJson) || ChunkSize <= 0)
            return;

        var array = JsonNode.Parse(SourceListJson)!.AsArray();
        JsonArray current = new();
        for (int i = 0; i < array.Count; i++)
        {
            if (current.Count == ChunkSize)
            {
                ChunksListJson.Add(current.ToJsonString(JsonOptions));
                current = new JsonArray();
            }
            current.Add(array[i]!.DeepClone());
        }
        if (current.Count > 0)
            ChunksListJson.Add(current.ToJsonString(JsonOptions));
    }

    public void List_DistinctBy(
        string SourceListJson,
        string PropertyName,
        bool CaseSensitive,
        out string DistinctListJson)
    {
        if (string.IsNullOrEmpty(SourceListJson))
        {
            DistinctListJson = "[]";
            return;
        }

        var array = JsonNode.Parse(SourceListJson)!.AsArray();
        var result = new JsonArray();

        if (string.IsNullOrEmpty(PropertyName))
        {
            // No key → dedupe on the item's own JSON representation.
            var seenRaw = new HashSet<string>(CaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase);
            foreach (var item in array)
            {
                var raw = item?.ToJsonString(JsonOptions) ?? "null";
                if (seenRaw.Add(raw))
                    result.Add(item!.DeepClone());
            }
            DistinctListJson = result.ToJsonString(JsonOptions);
            return;
        }

        var cmp = CaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;
        var seen = new HashSet<string>(cmp);
        bool nullSeen = false;

        foreach (var item in array)
        {
            var key = GetPropertyValue(item!, PropertyName);
            if (key == null)
            {
                if (nullSeen) continue;
                nullSeen = true;
                result.Add(item!.DeepClone());
            }
            else if (seen.Add(key))
            {
                result.Add(item!.DeepClone());
            }
        }

        DistinctListJson = result.ToJsonString(JsonOptions);
    }

    public void List_Slice(
        string SourceListJson,
        int Start,
        int End,
        int Step,
        out string SliceListJson)
    {
        if (string.IsNullOrEmpty(SourceListJson))
        {
            SliceListJson = "[]";
            return;
        }

        var array = JsonNode.Parse(SourceListJson)!.AsArray();
        int n = array.Count;
        if (n == 0)
        {
            SliceListJson = "[]";
            return;
        }

        int step = Step == 0 ? 1 : Step;
        // End == 0 is treated as "unspecified" (Python default). For step > 0
        // that means "to end of list"; for step < 0 that means "past beginning
        // of list" (so the reverse walk includes index 0).
        bool endIsSentinel = End == 0;

        int start = NormalizeSliceStart(Start, n, step);
        int end = endIsSentinel
            ? (step > 0 ? n : -1)
            : NormalizeSliceEnd(End, n, step);

        var result = new JsonArray();
        if (step > 0)
        {
            for (int i = start; i < end && i < n; i += step)
            {
                if (i < 0) continue;
                result.Add(array[i]!.DeepClone());
            }
        }
        else
        {
            for (int i = start; i > end && i >= 0; i += step)
            {
                if (i >= n) continue;
                result.Add(array[i]!.DeepClone());
            }
        }

        SliceListJson = result.ToJsonString(JsonOptions);
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

    public void List_Shuffle(
        string SourceListJson,
        int Seed,
        out string ShuffledListJson)
    {
        if (string.IsNullOrEmpty(SourceListJson))
        {
            ShuffledListJson = "[]";
            return;
        }

        var source = JsonNode.Parse(SourceListJson)!.AsArray();
        var buffer = new JsonNode?[source.Count];
        for (int i = 0; i < source.Count; i++)
            buffer[i] = source[i]!.DeepClone();

        // Fisher-Yates. Non-zero seed → deterministic; zero → cryptographic RNG.
        if (Seed != 0)
        {
            var rng = new Random(Seed);
            for (int i = buffer.Length - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (buffer[i], buffer[j]) = (buffer[j], buffer[i]);
            }
        }
        else
        {
            using var crypto = RandomNumberGenerator.Create();
            var word = new byte[4];
            for (int i = buffer.Length - 1; i > 0; i--)
            {
                crypto.GetBytes(word);
                uint rand = System.BitConverter.ToUInt32(word, 0);
                int j = (int)(rand % (uint)(i + 1));
                (buffer[i], buffer[j]) = (buffer[j], buffer[i]);
            }
        }

        var result = new JsonArray();
        foreach (var node in buffer)
            result.Add(node);

        ShuffledListJson = result.ToJsonString(JsonOptions);
    }

    public void List_UpdateAt(
        string SourceListJson,
        int Index,
        string PropertyName,
        string NewValueJson,
        out string UpdatedListJson,
        out string PreviousValueJson)
    {
        if (string.IsNullOrEmpty(SourceListJson))
        {
            UpdatedListJson = "[]";
            PreviousValueJson = "null";
            return;
        }

        var array = JsonNode.Parse(SourceListJson)!.AsArray();
        int n = array.Count;
        int i = Index < 0 ? n + Index : Index;

        if (i < 0 || i >= n || string.IsNullOrEmpty(PropertyName))
        {
            UpdatedListJson = SourceListJson;
            PreviousValueJson = "null";
            return;
        }

        var item = array[i];
        if (item is not JsonObject)
        {
            UpdatedListJson = SourceListJson;
            PreviousValueJson = "null";
            return;
        }

        var newValue = ParseValueOrString(NewValueJson);
        var previous = SetPropertyValue(item!, PropertyName, newValue);
        PreviousValueJson = previous?.ToJsonString(JsonOptions) ?? "null";
        UpdatedListJson = array.ToJsonString(JsonOptions);
    }

    private static JsonNode? ParseValueOrString(string? raw)
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

    // Walks the property path, creating missing intermediate objects, and
    // returns the previous value (or null when the property did not exist).
    // Array indices must exist — this helper never grows arrays.
    private static JsonNode? SetPropertyValue(JsonNode root, string propertyPath, JsonNode? newValue)
    {
        var segments = propertyPath.Split('.');
        JsonNode? current = root;
        for (int idx = 0; idx < segments.Length - 1; idx++)
        {
            current = NavigateOrCreateSegment(current, segments[idx]);
            if (current == null) return null;
        }

        var last = segments[^1];
        return SetTerminalSegment(current, last, newValue);
    }

    private static JsonNode? NavigateOrCreateSegment(JsonNode? current, string segment)
    {
        if (current == null || string.IsNullOrEmpty(segment)) return null;

        (string name, int? index) = ParseSegment(segment);

        JsonNode? next = current;
        if (!string.IsNullOrEmpty(name))
        {
            if (current is not JsonObject obj) return null;
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
                    // Create a new object at this hop so callers can set nested paths.
                    var created = new JsonObject();
                    obj[name] = created;
                    next = created;
                }
            }
        }

        if (index.HasValue)
        {
            if (next is not JsonArray arr) return null;
            int i = index.Value < 0 ? arr.Count + index.Value : index.Value;
            if (i < 0 || i >= arr.Count) return null;
            return arr[i];
        }

        return next;
    }

    private static JsonNode? SetTerminalSegment(JsonNode? current, string segment, JsonNode? newValue)
    {
        if (current == null || string.IsNullOrEmpty(segment)) return null;

        (string name, int? index) = ParseSegment(segment);

        JsonNode? target = current;
        JsonObject? parentObj = null;
        string effectiveName = name;
        if (!string.IsNullOrEmpty(name))
        {
            if (current is not JsonObject obj) return null;
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
            if (target is not JsonArray arr) return null;
            int i = index.Value < 0 ? arr.Count + index.Value : index.Value;
            if (i < 0 || i >= arr.Count) return null;
            var prev = arr[i]?.DeepClone();
            arr[i] = newValue?.DeepClone();
            return prev;
        }

        if (parentObj == null) return null;
        JsonNode? previous = null;
        if (parentObj.TryGetPropertyValue(effectiveName, out var existing))
            previous = existing?.DeepClone();
        parentObj[effectiveName] = newValue?.DeepClone();
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

    public void List_Reverse(
        string SourceListJson,
        out string ReversedListJson)
    {
        if (string.IsNullOrEmpty(SourceListJson))
        {
            ReversedListJson = "[]";
            return;
        }

        var array = JsonNode.Parse(SourceListJson)!.AsArray();
        var result = new JsonArray();
        for (int i = array.Count - 1; i >= 0; i--)
            result.Add(array[i]!.DeepClone());

        ReversedListJson = result.ToJsonString(JsonOptions);
    }

    public void List_Flatten(
        List<string> ChunksListJson,
        out string FlatListJson)
    {
        var result = new JsonArray();
        if (ChunksListJson != null)
        {
            foreach (var entry in ChunksListJson)
            {
                if (string.IsNullOrEmpty(entry)) continue;
                JsonNode? parsed;
                try { parsed = JsonNode.Parse(entry); }
                catch (JsonException) { continue; }
                if (parsed is not JsonArray inner) continue;
                foreach (var item in inner)
                    result.Add(item?.DeepClone());
            }
        }
        FlatListJson = result.ToJsonString(JsonOptions);
    }

    public void List_Sample(
        string SourceListJson,
        int SampleSize,
        int Seed,
        out string SampleListJson)
    {
        if (string.IsNullOrEmpty(SourceListJson) || SampleSize <= 0)
        {
            SampleListJson = "[]";
            return;
        }

        var source = JsonNode.Parse(SourceListJson)!.AsArray();
        int n = source.Count;
        if (n == 0)
        {
            SampleListJson = "[]";
            return;
        }

        int take = SampleSize > n ? n : SampleSize;
        var buffer = new JsonNode?[n];
        for (int i = 0; i < n; i++) buffer[i] = source[i]!.DeepClone();

        // Partial Fisher-Yates: shuffle just the first `take` slots.
        if (Seed != 0)
        {
            var rng = new Random(Seed);
            for (int i = 0; i < take; i++)
            {
                int j = i + rng.Next(n - i);
                (buffer[i], buffer[j]) = (buffer[j], buffer[i]);
            }
        }
        else
        {
            using var crypto = System.Security.Cryptography.RandomNumberGenerator.Create();
            var word = new byte[4];
            for (int i = 0; i < take; i++)
            {
                crypto.GetBytes(word);
                uint rand = System.BitConverter.ToUInt32(word, 0);
                int j = i + (int)(rand % (uint)(n - i));
                (buffer[i], buffer[j]) = (buffer[j], buffer[i]);
            }
        }

        var result = new JsonArray();
        for (int i = 0; i < take; i++) result.Add(buffer[i]);

        SampleListJson = result.ToJsonString(JsonOptions);
    }

    public void List_ReplaceWhere(
        string SourceListJson,
        List<Condition> Conditions,
        string LogicalOperator,
        string UpdateProperty,
        string NewValueJson,
        out string UpdatedListJson,
        out int MatchCount)
    {
        UpdatedListJson = SourceListJson ?? "[]";
        MatchCount = 0;
        if (string.IsNullOrEmpty(SourceListJson) || string.IsNullOrEmpty(UpdateProperty))
            return;

        if (Conditions == null || Conditions.Count == 0) return;

        var array = JsonNode.Parse(SourceListJson)!.AsArray();
        var newValue = ParseValueOrString(NewValueJson);

        foreach (var item in array)
        {
            if (!EvaluateConditions(item!, Conditions, LogicalOperator)) continue;
            if (item is not JsonObject) continue;
            SetPropertyValue(item!, UpdateProperty, newValue);
            MatchCount++;
        }

        UpdatedListJson = array.ToJsonString(JsonOptions);
    }

    public void List_UpdateMultipleAt(
        string SourceListJson,
        string IndicesToUpdate,
        string PropertyName,
        string NewValueJson,
        out string UpdatedListJson,
        out int UpdatedCount)
    {
        UpdatedListJson = SourceListJson ?? "[]";
        UpdatedCount = 0;
        if (string.IsNullOrEmpty(SourceListJson) || string.IsNullOrEmpty(PropertyName) || string.IsNullOrEmpty(IndicesToUpdate))
            return;

        var array = JsonNode.Parse(SourceListJson)!.AsArray();
        int n = array.Count;
        var newValue = ParseValueOrString(NewValueJson);

        var seen = new HashSet<int>();
        foreach (var raw in IndicesToUpdate.Split(','))
        {
            if (!int.TryParse(raw.Trim(), out var idx)) continue;
            int actual = idx < 0 ? n + idx : idx;
            if (actual < 0 || actual >= n) continue;
            if (!seen.Add(actual)) continue;
            if (array[actual] is not JsonObject) continue;
            SetPropertyValue(array[actual]!, PropertyName, newValue);
            UpdatedCount++;
        }

        UpdatedListJson = array.ToJsonString(JsonOptions);
    }
}
