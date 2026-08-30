using ListUtils;

namespace LoadTest;

internal sealed record Benchmark(string Name, Action<Data> Invoke);

internal static class Benchmarks
{
    // Common condition list used by ByConditions / PartitionByConditions / ReplaceWhere.
    private static readonly List<Condition> StandardConditions =
    [
        new Condition { Path = "Age",    Operator = ">=",       Value = "30",       CaseSensitive = false },
        new Condition { Path = "Status", Operator = "Equals",   Value = "Active",   CaseSensitive = false },
    ];

    private static readonly List<string> ZipManyKeyNames    = ["A", "B", "C"];
    private static readonly List<string> ZipManyKeyPaths    = ["City", "City", "City"];

    public static IReadOnlyList<Benchmark> All(global::ListUtils.ListUtils sut) =>
    [
        // ── Index-based (3) ────────────────────────────────────────────
        new("List_Pop", d =>
            sut.List_Pop(d.ListJson, d.Size / 2, out _, out _)),

        new("List_PopMultiple", d =>
            sut.List_PopMultiple(d.ListJson, d.IndicesCsv, out _, out _)),

        new("List_SplitAt", d =>
            sut.List_SplitAt(d.ListJson, d.Size / 2, out _, out _)),

        // ── Condition-based (6) ────────────────────────────────────────
        new("List_PopByCondition", d =>
            sut.List_PopByCondition(d.ListJson, "Status", "Active", "Equals", false, false, out _, out _)),

        new("List_PopMultipleByCondition", d =>
            sut.List_PopMultipleByCondition(d.ListJson, "Status", "Active", "Equals", false, out _, out _)),

        new("List_PopByConditions", d =>
            sut.List_PopByConditions(d.ListJson, StandardConditions, "AND", false, out _, out _)),

        new("List_PopMultipleByConditions", d =>
            sut.List_PopMultipleByConditions(d.ListJson, StandardConditions, "AND", out _, out _)),

        new("List_Partition", d =>
            sut.List_Partition(d.ListJson, "Status", "Active", "Equals", false, out _, out _)),

        new("List_PartitionByConditions", d =>
            sut.List_PartitionByConditions(d.ListJson, StandardConditions, "AND", out _, out _)),

        // ── Relational (7) ─────────────────────────────────────────────
        new("List_Zip", d =>
            sut.List_Zip(d.ListAJson, d.ListBJson, "A", "B", out _)),

        new("List_GroupBy", d =>
            sut.List_GroupBy(d.ListJson, "City", out _)),

        new("List_ZipGroupBy", d =>
            sut.List_ZipGroupBy(d.ListAJson, d.ListBJson, "City", "City", "AItems", "BItems", false, out _)),

        new("List_Difference", d =>
            sut.List_Difference(d.ListAJson, d.ListBJson, "Id", "Equals", false, out _)),

        new("List_Intersect", d =>
            sut.List_Intersect(d.ListAJson, d.ListBJson, "Id", "Equals", false, out _)),

        new("List_Union", d =>
            sut.List_Union(d.ListAJson, d.ListBJson, "Id", false, out _)),

        new("List_GroupByMultiple", d =>
            sut.List_GroupByMultiple(d.ListJson, ["City", "Status"], ["City", "Status"], "Items", false, out _)),

        new("List_ZipGroupByMultiple", d =>
            sut.List_ZipGroupByMultiple(
                d.ListAJson, d.ListBJson,
                ["City", "Status"], ["City", "Status"],
                ["City", "Status"], "AItems", "BItems", false, out _)),

        // ── Transform (10) ─────────────────────────────────────────────
        new("List_Chunk", d =>
            sut.List_Chunk(d.ListJson, 100, out _)),

        new("List_DistinctBy", d =>
            sut.List_DistinctBy(d.ListJson, "City", false, out _)),

        new("List_Slice", d =>
            sut.List_Slice(d.ListJson, 0, d.Size, 2, out _)),

        new("List_Shuffle", d =>
            sut.List_Shuffle(d.ListJson, 12345, out _)),

        new("List_UpdateAt", d =>
            sut.List_UpdateAt(d.ListJson, d.Size / 2, "Status", "\"Updated\"", out _, out _)),

        new("List_Reverse", d =>
            sut.List_Reverse(d.ListJson, out _)),

        new("List_Flatten", d =>
            sut.List_Flatten(d.ChunksJson, out _)),

        new("List_Sample", d =>
            sut.List_Sample(d.ListJson, Math.Max(1, d.Size / 10), 12345, out _)),

        new("List_ReplaceWhere", d =>
            sut.List_ReplaceWhere(d.ListJson, StandardConditions, "AND", "Status", "\"Reviewed\"", out _, out _)),

        new("List_UpdateMultipleAt", d =>
            sut.List_UpdateMultipleAt(d.ListJson, d.IndicesCsv, "Status", "\"Reviewed\"", out _, out _)),

        // ── Aggregate (3) ──────────────────────────────────────────────
        new("List_MinBy", d =>
            sut.List_MinBy(d.ListJson, "Score", true, out _, out _, out _)),

        new("List_MaxBy", d =>
            sut.List_MaxBy(d.ListJson, "Score", true, out _, out _, out _)),

        new("List_Aggregate", d =>
            sut.List_Aggregate(d.ListJson, "Score", "Sum", out _, out _)),

        // ── Multi-list (3) ─────────────────────────────────────────────
        new("List_ZipMany", d =>
            sut.List_ZipMany(d.ListsJson, ZipManyKeyNames, out _)),

        new("List_ZipManyGroupBy", d =>
            sut.List_ZipManyGroupBy(d.ListsJson, ZipManyKeyPaths, ZipManyKeyNames, false, out _)),

        new("List_ZipManyGroupByMultiple", d =>
            sut.List_ZipManyGroupByMultiple(
                d.ListsJson,
                KeyCount: 2,
                // Flattened key-property list: 2 paths per input list × 3 lists = 6 entries.
                KeyProperties: ["City", "Status", "City", "Status", "City", "Status"],
                KeyNames:      ["City", "Status"],
                ItemsFieldNames: ZipManyKeyNames,
                CaseSensitive: false,
                out _)),
    ];
}
