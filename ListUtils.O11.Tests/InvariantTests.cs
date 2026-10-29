using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ListUtils.Tests;

// Cross-cutting viewpoints not tied to a single action:
//  1. Round-trip and equivalence invariants between actions, checked on seeded random data.
//  2. Independence from the server's current culture (decimal separator, Turkish casing).
//  3. Concurrent use of one instance (OutSystems serves requests in parallel).
//  4. The malformed-input contract: invalid JSON or a non-array root raises an exception.
// Shared by ODC and O11 — the O11 copy must stay byte-for-byte identical.
public class InvariantTests
{
    private readonly ListUtils _sut = new();

    public static IEnumerable<object[]> Seeds => new[] { new object[] { 1 }, new object[] { 42 }, new object[] { 2026 } };

    private static JsonArray Arr(string json) => JsonNode.Parse(json)!.AsArray();

    private static List<string> Items(string json) => Arr(json).Select(n => n?.ToJsonString() ?? "null").ToList();

    private static List<string> SortedById(IEnumerable<string> items) =>
        items.OrderBy(s => JsonNode.Parse(s)!["I"]!.GetValue<int>()).ToList();

    // Objects with a unique "I", a lowercase "S" in a..e (missing ~1 in 6), and a signed "V".
    private static string RandomList(int seed, int count = 40, int idOffset = 0)
    {
        var rng = new Random(seed);
        var parts = new List<string>();
        for (int i = 0; i < count; i++)
        {
            string s = rng.Next(6) == 0 ? "" : "\"S\":\"" + (char)('a' + rng.Next(5)) + "\",";
            parts.Add("{" + s + "\"V\":" + rng.Next(-50, 50) + ",\"I\":" + (i + idOffset) + "}");
        }
        return "[" + string.Join(",", parts) + "]";
    }

    #region Round-trip invariants

    [Theory]
    [MemberData(nameof(Seeds))]
    public void Chunk_ThenFlatten_IsIdentity(int seed)
    {
        string source = RandomList(seed);
        for (int size = 1; size <= 7; size++)
        {
            _sut.List_Chunk(source, size, out var chunks);
            _sut.List_Flatten(chunks, out var flat);
            Assert.Equal(source, flat);
        }
    }

    [Theory]
    [MemberData(nameof(Seeds))]
    public void SplitAt_LeftPlusRight_IsSource(int seed)
    {
        string source = RandomList(seed);
        var expected = Items(source);
        for (int index = -45; index <= 45; index += 5)
        {
            _sut.List_SplitAt(source, index, out var left, out var right);
            Assert.Equal(expected, Items(left).Concat(Items(right)).ToList());
        }
    }

    [Theory]
    [MemberData(nameof(Seeds))]
    public void Reverse_Twice_IsIdentity_AndEqualsReverseSlice(int seed)
    {
        string source = RandomList(seed);

        _sut.List_Reverse(source, out var once);
        _sut.List_Reverse(once, out var twice);
        _sut.List_Slice(source, -1, 0, -1, out var sliced);

        Assert.Equal(source, twice);
        Assert.Equal(once, sliced);
    }

    [Theory]
    [MemberData(nameof(Seeds))]
    public void Shuffle_AndFullSample_ArePermutations(int seed)
    {
        string source = RandomList(seed);
        var expected = Items(source).OrderBy(s => s, StringComparer.Ordinal).ToList();

        _sut.List_Shuffle(source, seed, out var shuffled);
        _sut.List_Sample(source, 1000, seed, out var sample);

        Assert.Equal(expected, Items(shuffled).OrderBy(s => s, StringComparer.Ordinal).ToList());
        Assert.Equal(expected, Items(sample).OrderBy(s => s, StringComparer.Ordinal).ToList());
    }

    [Theory]
    [MemberData(nameof(Seeds))]
    public void Slice_DefaultArguments_IsIdentity(int seed)
    {
        string source = RandomList(seed);

        _sut.List_Slice(source, 0, 0, 1, out var sliced);

        Assert.Equal(source, sliced);
    }

    #endregion

    #region Partition / pop equivalences

    [Theory]
    [MemberData(nameof(Seeds))]
    public void Partition_EqualsPopMultipleByCondition_AndCoversSourceInOrder(int seed)
    {
        string source = RandomList(seed);
        foreach (var (op, target) in new[] { ("Equals", "c"), ("NotEquals", "a"), ("GreaterThan", "0"), ("LessOrEqual", "-10") })
        {
            string path = op is "Equals" or "NotEquals" ? "S" : "V";
            _sut.List_Partition(source, path, target, op, false, out var matching, out var nonMatching);
            _sut.List_PopMultipleByCondition(source, path, target, op, false, out var updated, out var popped);

            Assert.Equal(popped, matching);
            Assert.Equal(updated, nonMatching);
            Assert.Equal(Items(source), SortedById(Items(matching).Concat(Items(nonMatching))));
        }
    }

    [Theory]
    [MemberData(nameof(Seeds))]
    public void PartitionByConditions_EqualsPopMultipleByConditions(int seed)
    {
        string source = RandomList(seed);
        var cond = new List<Condition>
        {
            new() { Path = "S", Operator = Operators.NotEquals, Value = "b" },
            new() { Path = "V", Operator = Operators.GreaterOrEqual, Value = "-5" }
        };
        foreach (var logical in new[] { LogicalOperators.AND, LogicalOperators.OR })
        {
            _sut.List_PartitionByConditions(source, cond, logical, out var matching, out var nonMatching);
            _sut.List_PopMultipleByConditions(source, cond, logical, out var updated, out var popped);

            Assert.Equal(popped, matching);
            Assert.Equal(updated, nonMatching);
        }
    }

    [Theory]
    [MemberData(nameof(Seeds))]
    public void PopByCondition_Repeated_EqualsPopMultipleByCondition(int seed)
    {
        string source = RandomList(seed);
        _sut.List_PopMultipleByCondition(source, "S", "d", "Equals", false, out var expectedRemaining, out var expectedPopped);

        string current = source;
        var popped = new List<string>();
        while (true)
        {
            _sut.List_PopByCondition(current, "S", "d", "Equals", false, false, out var next, out var one);
            if (one == "{}") break;
            popped.Add(one);
            current = next;
        }

        Assert.Equal(Items(expectedPopped), popped);
        Assert.Equal(expectedRemaining, current);
    }

    [Theory]
    [MemberData(nameof(Seeds))]
    public void Pop_SingleIndex_EqualsPopMultiple(int seed)
    {
        string source = RandomList(seed, 10);
        for (int index = -10; index < 10; index++)
        {
            _sut.List_Pop(source, index, out var updated, out var popped);
            _sut.List_PopMultiple(source, index.ToString(CultureInfo.InvariantCulture), out var updatedMany, out var poppedMany);

            Assert.Equal(Items(updated), Items(updatedMany));
            Assert.Equal(new List<string> { popped }, Items(poppedMany));
        }
    }

    #endregion

    #region Set-operation invariants

    [Theory]
    [MemberData(nameof(Seeds))]
    public void Difference_AndIntersect_PartitionKeyedA(int seed)
    {
        string a = RandomList(seed);
        string b = RandomList(seed + 1, 8, 1000);

        _sut.List_Difference(a, b, "S", "Equals", false, out var diff);
        _sut.List_Intersect(a, b, "S", "Equals", false, out var inter);

        var diffItems = Items(diff);
        var interItems = Items(inter);
        Assert.Empty(diffItems.Intersect(interItems));
        // Keyless A items are kept by Difference and excluded by Intersect, so together they cover A.
        Assert.Equal(Items(a), SortedById(diffItems.Concat(interItems)));
    }

    [Theory]
    [MemberData(nameof(Seeds))]
    public void Union_OfListWithItself_EqualsDistinctBy(int seed)
    {
        string source = RandomList(seed);
        foreach (var caseSensitive in new[] { false, true })
        {
            _sut.List_Union(source, source, "S", caseSensitive, out var union);
            _sut.List_DistinctBy(source, "S", caseSensitive, out var distinct);
            Assert.Equal(distinct, union);
        }
    }

    [Theory]
    [MemberData(nameof(Seeds))]
    public void DistinctBy_IsIdempotent(int seed)
    {
        string source = RandomList(seed);

        _sut.List_DistinctBy(source, "S", false, out var once);
        _sut.List_DistinctBy(once, "S", false, out var twice);

        Assert.Equal(once, twice);
    }

    #endregion

    #region Grouping equivalences

    [Theory]
    [MemberData(nameof(Seeds))]
    public void GroupBy_ItemsCoverSource_AndEqualSingleKeyGroupByMultiple(int seed)
    {
        string source = RandomList(seed);

        _sut.List_GroupBy(source, "S", out var grouped);
        _sut.List_GroupByMultiple(source, new List<string> { "S" }, new List<string> { "Key" }, "Items", true, out var multi);

        var all = Arr(grouped).SelectMany(g => g!["Items"]!.AsArray().Select(n => n!.ToJsonString()));
        Assert.Equal(Items(source), SortedById(all));
        Assert.Equal(grouped, multi);
    }

    [Theory]
    [MemberData(nameof(Seeds))]
    public void ZipGroupBy_EqualsTwoListZipManyGroupBy_AndSingleKeyMultipleVariants(int seed)
    {
        string a = RandomList(seed);
        string b = RandomList(seed + 1, 25, 1000);
        var keys = new List<string> { "S" };
        var names = new List<string> { "A", "B" };

        _sut.List_ZipGroupBy(a, b, "S", "S", "A", "B", false, out var zipGroup);
        _sut.List_ZipManyGroupBy(new List<string> { a, b }, new List<string> { "S", "S" }, names, false, out var zipMany);
        _sut.List_ZipGroupByMultiple(a, b, keys, keys, new List<string> { "Key" }, "A", "B", false, out var zipMulti);
        _sut.List_ZipManyGroupByMultiple(new List<string> { a, b }, 1, new List<string> { "S", "S" }, new List<string> { "Key" }, names, false, out var zipManyMulti);

        Assert.Equal(zipGroup, zipMany);
        Assert.Equal(zipGroup, zipMulti);
        Assert.Equal(zipGroup, zipManyMulti);
        int total = Arr(zipGroup).Sum(g => g!["A"]!.AsArray().Count + g["B"]!.AsArray().Count);
        Assert.Equal(Arr(a).Count + Arr(b).Count, total);
    }

    [Theory]
    [MemberData(nameof(Seeds))]
    public void Zip_EqualsTwoListZipMany(int seed)
    {
        string a = RandomList(seed);
        string b = RandomList(seed + 1, 25, 1000);

        _sut.List_Zip(a, b, "A", "B", out var zip);
        _sut.List_ZipMany(new List<string> { a, b }, new List<string> { "A", "B" }, out var zipMany);

        Assert.Equal(zip, zipMany);
        Assert.Equal(25, Arr(zip).Count);
    }

    #endregion

    #region Aggregation and update equivalences

    [Theory]
    [MemberData(nameof(Seeds))]
    public void Aggregate_MatchesMinByMaxByAndManualTotals(int seed)
    {
        string source = RandomList(seed);
        var values = Arr(source).Select(n => n!["V"]!.GetValue<int>()).ToList();
        int withS = Arr(source).Count(n => n!["S"] != null);

        _sut.List_Aggregate(source, "V", "Sum", out var sum, out _);
        _sut.List_Aggregate(source, "V", "Min", out var min, out _);
        _sut.List_Aggregate(source, "V", "Max", out var max, out _);
        _sut.List_Aggregate(source, "S", "Count", out var count, out _);
        _sut.List_MinBy(source, "V", true, out _, out var minBy, out var minIdx);
        _sut.List_MaxBy(source, "V", true, out _, out var maxBy, out var maxIdx);

        Assert.Equal(values.Sum().ToString(CultureInfo.InvariantCulture), sum);
        Assert.Equal(values.Min().ToString(CultureInfo.InvariantCulture), min);
        Assert.Equal(values.Max().ToString(CultureInfo.InvariantCulture), max);
        Assert.Equal(withS.ToString(CultureInfo.InvariantCulture), count);
        Assert.Equal(min, minBy);
        Assert.Equal(max, maxBy);
        Assert.Equal(values.IndexOf(values.Min()), minIdx);
        Assert.Equal(values.IndexOf(values.Max()), maxIdx);
    }

    [Theory]
    [MemberData(nameof(Seeds))]
    public void UpdateMultipleAt_AllIndices_EqualsReplaceWhereAlwaysTrue(int seed)
    {
        string source = RandomList(seed, 15);
        string allIndices = string.Join(",", Enumerable.Range(0, 15));
        var always = new List<Condition> { new() { Path = "I", Operator = Operators.GreaterOrEqual, Value = "0" } };

        _sut.List_UpdateMultipleAt(source, allIndices, "Meta.Flag", "true", out var viaIndices, out var updatedCount);
        _sut.List_ReplaceWhere(source, always, LogicalOperators.AND, "Meta.Flag", "true", out var viaCondition, out var matchCount);

        Assert.Equal(viaIndices, viaCondition);
        Assert.Equal(15, updatedCount);
        Assert.Equal(15, matchCount);
    }

    #endregion

    #region Culture independence

    private static void WithCulture(string name, Action action)
    {
        var previous = CultureInfo.CurrentCulture;
        var previousUi = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo(name);
            CultureInfo.CurrentUICulture = new CultureInfo(name);
            action();
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
            CultureInfo.CurrentUICulture = previousUi;
        }
    }

    [Theory]
    [InlineData("pt-PT")]
    [InlineData("de-DE")]
    [InlineData("fr-FR")]
    public void NumericParsingAndFormatting_UseInvariantCulture(string culture)
    {
        WithCulture(culture, () =>
        {
            _sut.List_Partition("""[{"V":1.25},{"V":2}]""", "V", "1.5", "GreaterThan", false, out var matching, out _);
            _sut.List_Aggregate("""[{"V":1},{"V":2}]""", "V", "Avg", out var avg, out _);
            _sut.List_MinBy("""[{"V":"2.5"},{"V":"10.25"}]""", "V", true, out _, out var minVal, out _);
            _sut.List_PopMultiple("""["a","b","c"]""", "-1", out _, out var popped);

            Assert.Equal("""[{"V":2}]""", matching);
            Assert.Equal("1.5", avg);
            Assert.Equal("2.5", minVal);
            Assert.Equal("""["c"]""", popped);
        });
    }

    [Fact]
    public void CaseInsensitiveMatching_IsNotAffectedByTurkishCulture()
    {
        WithCulture("tr-TR", () =>
        {
            _sut.List_PopMultipleByCondition("""[{"S":"I"},{"S":"i"},{"S":"x"}]""", "S", "i", "Equals", false, out _, out var popped);
            _sut.List_DistinctBy("""[{"S":"TITLE"},{"S":"title"}]""", "S", false, out var distinct);

            Assert.Equal(2, Arr(popped).Count);
            Assert.Single(Arr(distinct));
        });
    }

    #endregion

    #region Concurrency

    [Fact]
    public void ConcurrentCallsOnOneInstance_MatchSequentialResults()
    {
        var inputs = Enumerable.Range(0, 64).Select(i => RandomList(100 + i, 30)).ToList();
        string Run(string s)
        {
            _sut.List_GroupBy(s, "S", out var g);
            _sut.List_Shuffle(s, 7, out var sh);
            _sut.List_PopMultipleByCondition(s, "V", "0", "GreaterThan", false, out var u, out var p);
            _sut.List_DistinctBy(s, "S", false, out var d);
            return g + "|" + sh + "|" + u + "|" + p + "|" + d;
        }

        var sequential = inputs.Select(Run).ToArray();
        var parallel = new string[inputs.Count];
        Parallel.For(0, inputs.Count, new ParallelOptions { MaxDegreeOfParallelism = 8 }, i => parallel[i] = Run(inputs[i]));

        Assert.Equal(sequential, parallel);
    }

    #endregion

    #region Malformed input contract

    public static IEnumerable<object[]> JsonConsumingActions => new[]
    {
        "Pop", "PopMultiple", "PopByCondition", "PopMultipleByCondition", "PopByConditions", "PopMultipleByConditions",
        "Partition", "PartitionByConditions", "SplitAt", "Zip", "GroupBy", "ZipGroupBy", "GroupByMultiple",
        "ZipGroupByMultiple", "Difference", "Intersect", "Union", "Chunk", "DistinctBy", "Slice", "Shuffle",
        "UpdateAt", "UpdateMultipleAt", "ReplaceWhere", "Reverse", "Sample", "MinBy", "MaxBy", "Aggregate",
        "ZipMany", "ZipManyGroupBy", "ZipManyGroupByMultiple"
    }.Select(a => new object[] { a });

    private void Invoke(string action, string json)
    {
        var cond = new List<Condition> { new() { Path = "S", Operator = Operators.Equals, Value = "a" } };
        var keys = new List<string> { "S" };
        var lists = new List<string> { json, "[]" };
        switch (action)
        {
            case "Pop": _sut.List_Pop(json, 0, out _, out _); break;
            case "PopMultiple": _sut.List_PopMultiple(json, "0", out _, out _); break;
            case "PopByCondition": _sut.List_PopByCondition(json, "S", "a", "Equals", false, false, out _, out _); break;
            case "PopMultipleByCondition": _sut.List_PopMultipleByCondition(json, "S", "a", "Equals", false, out _, out _); break;
            case "PopByConditions": _sut.List_PopByConditions(json, cond, "AND", false, out _, out _); break;
            case "PopMultipleByConditions": _sut.List_PopMultipleByConditions(json, cond, "AND", out _, out _); break;
            case "Partition": _sut.List_Partition(json, "S", "a", "Equals", false, out _, out _); break;
            case "PartitionByConditions": _sut.List_PartitionByConditions(json, cond, "AND", out _, out _); break;
            case "SplitAt": _sut.List_SplitAt(json, 1, out _, out _); break;
            case "Zip": _sut.List_Zip(json, "[]", "A", "B", out _); break;
            case "GroupBy": _sut.List_GroupBy(json, "S", out _); break;
            case "ZipGroupBy": _sut.List_ZipGroupBy(json, "[]", "S", "S", "A", "B", false, out _); break;
            case "GroupByMultiple": _sut.List_GroupByMultiple(json, keys, keys, "Items", false, out _); break;
            case "ZipGroupByMultiple": _sut.List_ZipGroupByMultiple(json, "[]", keys, keys, keys, "A", "B", false, out _); break;
            case "Difference": _sut.List_Difference(json, "[]", "S", "Equals", false, out _); break;
            case "Intersect": _sut.List_Intersect(json, "[]", "S", "Equals", false, out _); break;
            case "Union": _sut.List_Union(json, "[]", "S", false, out _); break;
            case "Chunk": _sut.List_Chunk(json, 2, out _); break;
            case "DistinctBy": _sut.List_DistinctBy(json, "S", false, out _); break;
            case "Slice": _sut.List_Slice(json, 0, 0, 1, out _); break;
            case "Shuffle": _sut.List_Shuffle(json, 1, out _); break;
            case "UpdateAt": _sut.List_UpdateAt(json, 0, "S", "1", out _, out _); break;
            case "UpdateMultipleAt": _sut.List_UpdateMultipleAt(json, "0", "S", "1", out _, out _); break;
            case "ReplaceWhere": _sut.List_ReplaceWhere(json, cond, "AND", "S", "1", out _, out _); break;
            case "Reverse": _sut.List_Reverse(json, out _); break;
            case "Sample": _sut.List_Sample(json, 1, 1, out _); break;
            case "MinBy": _sut.List_MinBy(json, "V", true, out _, out _, out _); break;
            case "MaxBy": _sut.List_MaxBy(json, "V", true, out _, out _, out _); break;
            case "Aggregate": _sut.List_Aggregate(json, "V", "Sum", out _, out _); break;
            case "ZipMany": _sut.List_ZipMany(lists, keys, out _); break;
            case "ZipManyGroupBy": _sut.List_ZipManyGroupBy(lists, new List<string> { "S", "S" }, keys, false, out _); break;
            case "ZipManyGroupByMultiple": _sut.List_ZipManyGroupByMultiple(lists, 1, new List<string> { "S", "S" }, keys, keys, false, out _); break;
            default: throw new ArgumentOutOfRangeException(nameof(action), action, null);
        }
    }

    [Theory]
    [MemberData(nameof(JsonConsumingActions))]
    public void InvalidJson_ThrowsJsonException(string action)
    {
        Assert.ThrowsAny<JsonException>(() => Invoke(action, "[{\"S\":\"a\","));
    }

    [Theory]
    [MemberData(nameof(JsonConsumingActions))]
    public void NonArrayJsonRoot_ThrowsInvalidOperationException(string action)
    {
        Assert.Throws<InvalidOperationException>(() => Invoke(action, "{\"S\":\"a\"}"));
    }

    [Fact]
    public void Flatten_InvalidOrNonArrayEntries_AreSkippedNotThrown()
    {
        _sut.List_Flatten(new List<string> { "[1,", "{\"a\":1}", "[2]" }, out var flat);

        Assert.Equal("[2]", flat);
    }

    #endregion
}
