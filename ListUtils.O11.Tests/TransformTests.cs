using System.Text.Json.Nodes;

namespace ListUtils.Tests;

public class TransformTests
{
    private readonly ListUtils _sut = new();

    // ── List_Chunk ────────────────────────────────────────────────────────────

    [Fact]
    public void List_Chunk_EvenSplit_ReturnsBalancedChunks()
    {
        string json = """[1,2,3,4,5,6]""";

        _sut.List_Chunk(json, 2, out var chunks);

        Assert.Equal(3, chunks.Count);
        var first = JsonNode.Parse(chunks[0])!.AsArray();
        var last = JsonNode.Parse(chunks[2])!.AsArray();
        Assert.Equal(2, first.Count);
        Assert.Equal("1", first[0]!.ToString());
        Assert.Equal("6", last[1]!.ToString());
    }

    [Fact]
    public void List_Chunk_UnevenSplit_LastChunkShorter()
    {
        string json = """["a","b","c","d","e"]""";

        _sut.List_Chunk(json, 2, out var chunks);

        Assert.Equal(3, chunks.Count);
        Assert.Equal(2, JsonNode.Parse(chunks[0])!.AsArray().Count);
        Assert.Equal(2, JsonNode.Parse(chunks[1])!.AsArray().Count);
        var tail = JsonNode.Parse(chunks[2])!.AsArray();
        Assert.Single(tail);
        Assert.Equal("e", tail[0]!.ToString());
    }

    [Fact]
    public void List_Chunk_SingleChunk_WhenSizeExceedsList()
    {
        string json = """[1,2,3]""";

        _sut.List_Chunk(json, 10, out var chunks);

        Assert.Single(chunks);
        Assert.Equal(3, JsonNode.Parse(chunks[0])!.AsArray().Count);
    }

    [Fact]
    public void List_Chunk_ZeroOrNegativeSize_ReturnsEmpty()
    {
        string json = """[1,2,3]""";

        _sut.List_Chunk(json, 0, out var zero);
        _sut.List_Chunk(json, -5, out var negative);

        Assert.Empty(zero);
        Assert.Empty(negative);
    }

    [Fact]
    public void List_Chunk_EmptyOrNullSource_ReturnsEmpty()
    {
        _sut.List_Chunk("", 3, out var empty);
        _sut.List_Chunk(null!, 3, out var nullSrc);
        _sut.List_Chunk("[]", 3, out var emptyArr);

        Assert.Empty(empty);
        Assert.Empty(nullSrc);
        Assert.Empty(emptyArr);
    }

    [Fact]
    public void List_Chunk_Objects_PreservesStructure()
    {
        string json = """[{"Id":1},{"Id":2},{"Id":3},{"Id":4}]""";

        _sut.List_Chunk(json, 2, out var chunks);

        Assert.Equal(2, chunks.Count);
        var first = JsonNode.Parse(chunks[0])!.AsArray();
        var second = JsonNode.Parse(chunks[1])!.AsArray();
        Assert.Equal("1", first[0]!["Id"]!.ToString());
        Assert.Equal("4", second[1]!["Id"]!.ToString());
    }

    [Fact]
    public void List_Chunk_EachEntryIsStandaloneJsonArray_ReadyForDeserialize()
    {
        // Contract: every entry in the output list is a self-contained JSON
        // array string that the OutSystems caller can JSON Deserialize into
        // their target Structure List without any pre-processing.
        string json = """[{"Id":1,"Name":"A"},{"Id":2,"Name":"B"},{"Id":3,"Name":"C"}]""";

        _sut.List_Chunk(json, 2, out var chunks);

        Assert.Equal(2, chunks.Count);
        foreach (var entry in chunks)
        {
            Assert.StartsWith("[", entry);
            Assert.EndsWith("]", entry);
            var parsed = JsonNode.Parse(entry);
            Assert.NotNull(parsed);
            Assert.IsType<JsonArray>(parsed);
        }
    }

    // ── List_DistinctBy ───────────────────────────────────────────────────────

    [Fact]
    public void List_DistinctBy_TopLevelKey_KeepsFirstOccurrence()
    {
        string json = """[{"Code":"A","V":1},{"Code":"B","V":2},{"Code":"A","V":3},{"Code":"C","V":4}]""";

        _sut.List_DistinctBy(json, "Code", false, out var result);

        var arr = JsonNode.Parse(result)!.AsArray();
        Assert.Equal(3, arr.Count);
        Assert.Equal("1", arr[0]!["V"]!.ToString());
        Assert.Equal("2", arr[1]!["V"]!.ToString());
        Assert.Equal("4", arr[2]!["V"]!.ToString());
    }

    [Fact]
    public void List_DistinctBy_NestedPath_Works()
    {
        string json = """[{"Meta":{"Region":"EU"}},{"Meta":{"Region":"US"}},{"Meta":{"Region":"EU"}}]""";

        _sut.List_DistinctBy(json, "Meta.Region", false, out var result);

        var arr = JsonNode.Parse(result)!.AsArray();
        Assert.Equal(2, arr.Count);
    }

    [Fact]
    public void List_DistinctBy_CaseInsensitiveByDefault()
    {
        string json = """[{"K":"foo"},{"K":"FOO"},{"K":"bar"}]""";

        _sut.List_DistinctBy(json, "K", false, out var ci);
        _sut.List_DistinctBy(json, "K", true, out var cs);

        Assert.Equal(2, JsonNode.Parse(ci)!.AsArray().Count);
        Assert.Equal(3, JsonNode.Parse(cs)!.AsArray().Count);
    }

    [Fact]
    public void List_DistinctBy_MissingKey_TreatedAsSingleBucket()
    {
        string json = """[{"K":"a"},{"NotK":"x"},{"K":"a"},{"NotK":"y"}]""";

        _sut.List_DistinctBy(json, "K", false, out var result);

        // First "a", then first item without K, then dupes dropped.
        var arr = JsonNode.Parse(result)!.AsArray();
        Assert.Equal(2, arr.Count);
    }

    [Fact]
    public void List_DistinctBy_EmptyPropertyName_DedupesByWholeItem()
    {
        string json = """[{"A":1},{"B":2},{"A":1},{"B":2}]""";

        _sut.List_DistinctBy(json, "", false, out var result);

        var arr = JsonNode.Parse(result)!.AsArray();
        Assert.Equal(2, arr.Count);
    }

    [Fact]
    public void List_DistinctBy_NullOrEmptySource_ReturnsEmpty()
    {
        _sut.List_DistinctBy("", "K", false, out var empty);
        _sut.List_DistinctBy(null!, "K", false, out var nullSrc);

        Assert.Equal("[]", empty);
        Assert.Equal("[]", nullSrc);
    }

    // ── List_Slice ────────────────────────────────────────────────────────────

    [Fact]
    public void List_Slice_PositiveRange_ReturnsInclusiveExclusive()
    {
        string json = """[10,20,30,40,50]""";

        _sut.List_Slice(json, 1, 4, 1, out var result);

        var arr = JsonNode.Parse(result)!.AsArray();
        Assert.Equal(3, arr.Count);
        Assert.Equal("20", arr[0]!.ToString());
        Assert.Equal("40", arr[2]!.ToString());
    }

    [Fact]
    public void List_Slice_EndZero_MeansToEndOfList()
    {
        string json = """[10,20,30,40,50]""";

        _sut.List_Slice(json, 2, 0, 1, out var result);

        var arr = JsonNode.Parse(result)!.AsArray();
        Assert.Equal(3, arr.Count);
        Assert.Equal("30", arr[0]!.ToString());
        Assert.Equal("50", arr[2]!.ToString());
    }

    [Fact]
    public void List_Slice_NegativeStart_CountsFromEnd()
    {
        string json = """[10,20,30,40,50]""";

        _sut.List_Slice(json, -2, 0, 1, out var result);

        var arr = JsonNode.Parse(result)!.AsArray();
        Assert.Equal(2, arr.Count);
        Assert.Equal("40", arr[0]!.ToString());
        Assert.Equal("50", arr[1]!.ToString());
    }

    [Fact]
    public void List_Slice_NegativeEnd_ExcludesTail()
    {
        string json = """[10,20,30,40,50]""";

        _sut.List_Slice(json, 0, -1, 1, out var result);

        var arr = JsonNode.Parse(result)!.AsArray();
        Assert.Equal(4, arr.Count);
        Assert.Equal("40", arr[3]!.ToString());
    }

    [Fact]
    public void List_Slice_StepTwo_TakesEveryOther()
    {
        string json = """[0,1,2,3,4,5,6,7,8,9]""";

        _sut.List_Slice(json, 0, 0, 2, out var result);

        var arr = JsonNode.Parse(result)!.AsArray();
        Assert.Equal(5, arr.Count);
        Assert.Equal("0", arr[0]!.ToString());
        Assert.Equal("8", arr[4]!.ToString());
    }

    [Fact]
    public void List_Slice_NegativeStep_ReversesList()
    {
        string json = """[1,2,3,4,5]""";

        _sut.List_Slice(json, -1, 0, -1, out var result);

        var arr = JsonNode.Parse(result)!.AsArray();
        Assert.Equal(5, arr.Count);
        Assert.Equal("5", arr[0]!.ToString());
        Assert.Equal("1", arr[4]!.ToString());
    }

    [Fact]
    public void List_Slice_StepZero_TreatedAsOne()
    {
        string json = """[1,2,3]""";

        _sut.List_Slice(json, 0, 0, 0, out var result);

        var arr = JsonNode.Parse(result)!.AsArray();
        Assert.Equal(3, arr.Count);
    }

    [Fact]
    public void List_Slice_StartGreaterThanEnd_ReturnsEmpty()
    {
        string json = """[1,2,3,4,5]""";

        _sut.List_Slice(json, 4, 1, 1, out var result);

        Assert.Equal("[]", result);
    }

    [Fact]
    public void List_Slice_OutOfBounds_Clamped()
    {
        string json = """[1,2,3]""";

        _sut.List_Slice(json, -100, 100, 1, out var result);

        var arr = JsonNode.Parse(result)!.AsArray();
        Assert.Equal(3, arr.Count);
    }

    [Fact]
    public void List_Slice_EmptyOrNullSource_ReturnsEmpty()
    {
        _sut.List_Slice("", 0, 3, 1, out var empty);
        _sut.List_Slice(null!, 0, 3, 1, out var nullSrc);
        _sut.List_Slice("[]", 0, 3, 1, out var emptyArr);

        Assert.Equal("[]", empty);
        Assert.Equal("[]", nullSrc);
        Assert.Equal("[]", emptyArr);
    }

    // ── List_Shuffle ──────────────────────────────────────────────────────────

    [Fact]
    public void List_Shuffle_DeterministicSeed_ProducesSamePermutation()
    {
        string json = """[1,2,3,4,5,6,7,8,9,10]""";

        _sut.List_Shuffle(json, 42, out var first);
        _sut.List_Shuffle(json, 42, out var second);

        Assert.Equal(first, second);
    }

    [Fact]
    public void List_Shuffle_DifferentSeeds_ProduceDifferentPermutations()
    {
        // 100 elements → collision probability between two seeds is negligible.
        var jsonArr = new JsonArray();
        for (int i = 0; i < 100; i++) jsonArr.Add(i);
        string json = jsonArr.ToJsonString();

        _sut.List_Shuffle(json, 1, out var a);
        _sut.List_Shuffle(json, 2, out var b);

        Assert.NotEqual(a, b);
    }

    [Fact]
    public void List_Shuffle_PreservesLengthAndElements()
    {
        string json = """[1,2,3,4,5,6,7,8,9,10]""";

        _sut.List_Shuffle(json, 7, out var shuffled);

        var arr = JsonNode.Parse(shuffled)!.AsArray();
        Assert.Equal(10, arr.Count);
        var set = new HashSet<string>();
        foreach (var n in arr) set.Add(n!.ToString());
        for (int i = 1; i <= 10; i++)
            Assert.Contains(i.ToString(), set);
    }

    [Fact]
    public void List_Shuffle_EmptyList_ReturnsEmpty()
    {
        _sut.List_Shuffle("[]", 1, out var result);
        Assert.Equal("[]", result);
    }

    [Fact]
    public void List_Shuffle_SingleElement_ReturnsUnchanged()
    {
        _sut.List_Shuffle("""[42]""", 1, out var result);

        var arr = JsonNode.Parse(result)!.AsArray();
        Assert.Single(arr);
        Assert.Equal("42", arr[0]!.ToString());
    }

    [Fact]
    public void List_Shuffle_NullOrEmptySource_ReturnsEmpty()
    {
        _sut.List_Shuffle("", 1, out var empty);
        _sut.List_Shuffle(null!, 1, out var nullSrc);

        Assert.Equal("[]", empty);
        Assert.Equal("[]", nullSrc);
    }

    [Fact]
    public void List_Shuffle_RandomSeed_IsNotDeterministic()
    {
        // 50 elements → 50! outcomes; collision odds effectively zero.
        var jsonArr = new JsonArray();
        for (int i = 0; i < 50; i++) jsonArr.Add(i);
        string json = jsonArr.ToJsonString();

        _sut.List_Shuffle(json, 0, out var a);
        _sut.List_Shuffle(json, 0, out var b);

        Assert.NotEqual(a, b);
    }

    // ── List_UpdateAt ─────────────────────────────────────────────────────────

    [Fact]
    public void List_UpdateAt_ExistingProperty_ReplacesAndReturnsPrevious()
    {
        string json = """[{"Id":1,"Status":"Pending"},{"Id":2,"Status":"Pending"}]""";

        _sut.List_UpdateAt(json, 0, "Status", "\"Active\"", out var updated, out var previous);

        Assert.Equal("\"Pending\"", previous);
        var arr = JsonNode.Parse(updated)!.AsArray();
        Assert.Equal("Active", arr[0]!["Status"]!.ToString());
        Assert.Equal("Pending", arr[1]!["Status"]!.ToString());
    }

    [Fact]
    public void List_UpdateAt_MissingProperty_CreatesItAndReturnsNull()
    {
        string json = """[{"Id":1}]""";

        _sut.List_UpdateAt(json, 0, "Status", "\"New\"", out var updated, out var previous);

        Assert.Equal("null", previous);
        var arr = JsonNode.Parse(updated)!.AsArray();
        Assert.Equal("New", arr[0]!["Status"]!.ToString());
    }

    [Fact]
    public void List_UpdateAt_NestedPath_CreatesIntermediateObjects()
    {
        string json = """[{"Id":1}]""";

        _sut.List_UpdateAt(json, 0, "Address.City", "\"Lisbon\"", out var updated, out var previous);

        Assert.Equal("null", previous);
        var arr = JsonNode.Parse(updated)!.AsArray();
        Assert.Equal("Lisbon", arr[0]!["Address"]!["City"]!.ToString());
    }

    [Fact]
    public void List_UpdateAt_NegativeIndex_TargetsFromEnd()
    {
        string json = """[{"V":1},{"V":2},{"V":3}]""";

        _sut.List_UpdateAt(json, -1, "V", "99", out var updated, out var previous);

        Assert.Equal("3", previous);
        var arr = JsonNode.Parse(updated)!.AsArray();
        Assert.Equal("99", arr[2]!["V"]!.ToString());
    }

    [Fact]
    public void List_UpdateAt_OutOfRangeIndex_ReturnsSourceUnchanged()
    {
        string json = """[{"V":1}]""";

        _sut.List_UpdateAt(json, 5, "V", "42", out var updated, out var previous);

        Assert.Equal(json, updated);
        Assert.Equal("null", previous);
    }

    [Fact]
    public void List_UpdateAt_NonObjectItem_ReturnsSourceUnchanged()
    {
        string json = """[1,2,3]""";

        _sut.List_UpdateAt(json, 0, "V", "42", out var updated, out var previous);

        Assert.Equal(json, updated);
        Assert.Equal("null", previous);
    }

    [Fact]
    public void List_UpdateAt_ObjectValue_ReplacesEntireSubtree()
    {
        string json = """[{"Id":1,"Meta":{"Old":true}}]""";

        _sut.List_UpdateAt(json, 0, "Meta", """{"New":42}""", out var updated, out var previous);

        var prev = JsonNode.Parse(previous)!.AsObject();
        Assert.True(prev["Old"]!.GetValue<bool>());

        var arr = JsonNode.Parse(updated)!.AsArray();
        Assert.Equal("42", arr[0]!["Meta"]!["New"]!.ToString());
    }

    [Fact]
    public void List_UpdateAt_RawString_FallsBackToStringValue()
    {
        // "not-valid-json" is not parseable — should be stored as a raw string.
        string json = """[{"K":"old"}]""";

        _sut.List_UpdateAt(json, 0, "K", "not valid json {", out var updated, out var previous);

        Assert.Equal("\"old\"", previous);
        var arr = JsonNode.Parse(updated)!.AsArray();
        Assert.Equal("not valid json {", arr[0]!["K"]!.ToString());
    }

    [Fact]
    public void List_UpdateAt_EmptyPropertyName_ReturnsSourceUnchanged()
    {
        string json = """[{"K":1}]""";

        _sut.List_UpdateAt(json, 0, "", "42", out var updated, out var previous);

        Assert.Equal(json, updated);
        Assert.Equal("null", previous);
    }

    [Fact]
    public void List_UpdateAt_NullOrEmptySource_ReturnsEmpty()
    {
        _sut.List_UpdateAt("", 0, "K", "1", out var empty, out var prevEmpty);
        _sut.List_UpdateAt(null!, 0, "K", "1", out var nullSrc, out var prevNull);

        Assert.Equal("[]", empty);
        Assert.Equal("[]", nullSrc);
        Assert.Equal("null", prevEmpty);
        Assert.Equal("null", prevNull);
    }

    [Fact]
    public void List_UpdateAt_NullValue_SetsPropertyToNull()
    {
        string json = """[{"K":1}]""";

        _sut.List_UpdateAt(json, 0, "K", "null", out var updated, out var previous);

        Assert.Equal("1", previous);
        var arr = JsonNode.Parse(updated)!.AsArray();
        Assert.Null(arr[0]!["K"]);
    }
}
