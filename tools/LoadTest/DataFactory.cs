using System.Text;
using System.Text.Json;

namespace LoadTest;

// Per-iteration input bundle. One instance is built at the target list size before every timed call.
internal sealed class Data
{
    public required int          Size;
    public required string       ListJson;    // single list (people)
    public required string       ListAJson;   // list A for two-list operations
    public required string       ListBJson;   // list B for two-list operations (partial overlap with A)
    public required List<string> ListsJson;   // three lists for ZipMany*
    public required List<string> ChunksJson;  // pre-chunked output of ListJson (for List_Flatten)
    public required string       IndicesCsv;  // 5% of indices for pop/update multi
}

internal static class DataFactory
{
    private static readonly string[] Cities =
    [
        "Lisbon", "Porto", "Braga", "Coimbra", "Faro", "Aveiro", "Evora", "Setubal",
        "Madrid", "Barcelona", "Paris", "Berlin", "Rome", "Amsterdam", "Vienna", "Prague"
    ];

    private static readonly string[] Statuses = ["Active", "Inactive", "Pending", "Suspended"];

    // Build all inputs for one iteration.
    public static Data BuildData(int size, Random rng)
    {
        if (size < 1) size = 1;

        var listJson  = BuildPeopleJson(size, rng, idOffset: 0);
        var listAJson = listJson; // reuse — cheap
        var listBJson = BuildPeopleJson(size, rng, idOffset: size / 2); // overlaps A's second half

        var lists = new List<string>(3)
        {
            listJson,
            BuildPeopleJson(size, rng, idOffset: size),
            BuildPeopleJson(size, rng, idOffset: 2 * size),
        };

        // Pre-chunk listJson for List_Flatten's input.
        int chunkSize = Math.Max(1, size / 20); // ~20 chunks
        var chunks = ChunkArrayText(listJson, chunkSize);

        // Sparse index set: 5% of positions, evenly spaced.
        int idxCount = Math.Max(1, size / 20);
        var sb = new StringBuilder(idxCount * 6);
        for (int i = 0; i < idxCount; i++)
        {
            if (i > 0) sb.Append(',');
            sb.Append(((long)i * size / idxCount).ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        return new Data
        {
            Size       = size,
            ListJson   = listJson,
            ListAJson  = listAJson,
            ListBJson  = listBJson,
            ListsJson  = lists,
            ChunksJson = chunks,
            IndicesCsv = sb.ToString(),
        };
    }

    // Hand-rolled JSON writer — orders of magnitude faster than JsonSerializer for the load-test inputs.
    private static string BuildPeopleJson(int count, Random rng, int idOffset)
    {
        var sb = new StringBuilder(count * 96);
        sb.Append('[');
        for (int i = 0; i < count; i++)
        {
            if (i > 0) sb.Append(',');
            int id     = idOffset + i;
            int age    = 18 + rng.Next(63);
            int score  = rng.Next(0, 10_001); // 0..10000 (kept as int; used as numeric in aggregations)
            string city   = Cities[rng.Next(Cities.Length)];
            string status = Statuses[rng.Next(Statuses.Length)];

            sb.Append("{\"Id\":").Append(id);
            sb.Append(",\"Name\":\"user_").Append(id).Append('"');
            sb.Append(",\"Age\":").Append(age);
            sb.Append(",\"Score\":").Append(score);
            sb.Append(",\"City\":\"").Append(city).Append('"');
            sb.Append(",\"Status\":\"").Append(status).Append('"');
            sb.Append('}');
        }
        sb.Append(']');
        return sb.ToString();
    }

    // Cheap chunker over a JSON array string: reparse once, group into JSON-string chunks.
    private static List<string> ChunkArrayText(string arrayJson, int chunkSize)
    {
        using var doc = JsonDocument.Parse(arrayJson);
        var root = doc.RootElement;
        int total = root.GetArrayLength();
        var chunks = new List<string>((total + chunkSize - 1) / chunkSize);
        var sb = new StringBuilder();
        int i = 0;
        while (i < total)
        {
            int end = Math.Min(i + chunkSize, total);
            sb.Clear();
            sb.Append('[');
            for (int j = i; j < end; j++)
            {
                if (j > i) sb.Append(',');
                sb.Append(root[j].GetRawText());
            }
            sb.Append(']');
            chunks.Add(sb.ToString());
            i = end;
        }
        return chunks;
    }
}
