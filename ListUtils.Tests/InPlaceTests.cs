using System.Text.Json.Nodes;

namespace ListUtils.Tests;

// Every InPlace action delegates to its non-InPlace counterpart, so the
// full behavioural matrix is already covered elsewhere. These tests verify:
// 1) The ref parameter is actually mutated (identity contract).
// 2) The delegation preserves the base action's output.
// 3) Secondary outputs (popped element, previous value) still flow through.
public class InPlaceTests
{
    private readonly ListUtils _sut = new();

    // ── Ref mutation identity ────────────────────────────────────────────────

    [Fact]
    public void PopInPlace_MutatesRefAndReturnsPopped()
    {
        string list = """[1,2,3]""";
        _sut.List_PopInPlace(ref list, 1, out var popped);

        Assert.Equal("2", popped);
        var arr = JsonNode.Parse(list)!.AsArray();
        Assert.Equal(2, arr.Count);
        Assert.Equal("1", arr[0]!.ToString());
        Assert.Equal("3", arr[1]!.ToString());
    }

    [Fact]
    public void PopMultipleInPlace_MutatesRef()
    {
        string list = """["a","b","c","d","e"]""";
        _sut.List_PopMultipleInPlace(ref list, "1,3", out var popped);

        Assert.Equal(2, JsonNode.Parse(popped)!.AsArray().Count);
        Assert.Equal(3, JsonNode.Parse(list)!.AsArray().Count);
    }

    [Fact]
    public void PopByConditionInPlace_MutatesRef()
    {
        string list = """[{"S":"A"},{"S":"B"},{"S":"A"}]""";
        _sut.List_PopByConditionInPlace(ref list, "S", "A", "Equals", false, false, out var popped);

        Assert.Contains("\"S\":\"A\"", popped);
        Assert.Equal(2, JsonNode.Parse(list)!.AsArray().Count);
    }

    [Fact]
    public void PopMultipleByConditionInPlace_MutatesRef()
    {
        string list = """[{"S":"A"},{"S":"B"},{"S":"A"}]""";
        _sut.List_PopMultipleByConditionInPlace(ref list, "S", "A", "Equals", false, out var popped);

        Assert.Equal(2, JsonNode.Parse(popped)!.AsArray().Count);
        Assert.Single(JsonNode.Parse(list)!.AsArray());
    }

    [Fact]
    public void PopByConditionsInPlace_MutatesRef()
    {
        string list = """[{"A":1,"B":10},{"A":2,"B":20}]""";
        string conditions = """[{"path":"A","operator":"Equals","value":"2"}]""";
        _sut.List_PopByConditionsInPlace(ref list, conditions, "AND", false, out var popped);

        Assert.Contains("\"A\":2", popped);
        Assert.Single(JsonNode.Parse(list)!.AsArray());
    }

    [Fact]
    public void PopMultipleByConditionsInPlace_MutatesRef()
    {
        string list = """[{"A":1},{"A":2},{"A":3}]""";
        string conditions = """[{"path":"A","operator":"GreaterThan","value":"1"}]""";
        _sut.List_PopMultipleByConditionsInPlace(ref list, conditions, "AND", out var popped);

        Assert.Equal(2, JsonNode.Parse(popped)!.AsArray().Count);
        Assert.Single(JsonNode.Parse(list)!.AsArray());
    }

    [Fact]
    public void ZipInPlace_ReplacesListAWithZipped()
    {
        string listA = """[1,2,3]""";
        string listB = """["a","b","c"]""";
        _sut.List_ZipInPlace(ref listA, listB, "N", "L");

        var arr = JsonNode.Parse(listA)!.AsArray();
        Assert.Equal(3, arr.Count);
        Assert.Equal("1", arr[0]!["N"]!.ToString());
        Assert.Equal("a", arr[0]!["L"]!.ToString());
    }

    [Fact]
    public void GroupByInPlace_ReplacesSourceWithGrouped()
    {
        string list = """[{"K":"x"},{"K":"y"},{"K":"x"}]""";
        _sut.List_GroupByInPlace(ref list, "K");

        var arr = JsonNode.Parse(list)!.AsArray();
        Assert.Equal(2, arr.Count);
        Assert.Equal("x", arr[0]!["Key"]!.ToString());
        Assert.Equal(2, arr[0]!["Items"]!.AsArray().Count);
    }

    [Fact]
    public void DifferenceInPlace_ReplacesListAWithDifference()
    {
        string listA = """[{"Id":1},{"Id":2},{"Id":3}]""";
        string listB = """[{"Id":2}]""";
        _sut.List_DifferenceInPlace(ref listA, listB, "Id", "Equals", false);

        var arr = JsonNode.Parse(listA)!.AsArray();
        Assert.Equal(2, arr.Count);
    }

    [Fact]
    public void ChunkInPlace_ReplacesSourceWithChunks()
    {
        string list = """[1,2,3,4,5]""";
        _sut.List_ChunkInPlace(ref list, 2);

        var arr = JsonNode.Parse(list)!.AsArray();
        Assert.Equal(3, arr.Count);
        Assert.Equal(2, arr[0]!.AsArray().Count);
        Assert.Single(arr[2]!.AsArray());
    }

    [Fact]
    public void DistinctByInPlace_ReplacesSource()
    {
        string list = """[{"K":"a"},{"K":"b"},{"K":"a"}]""";
        _sut.List_DistinctByInPlace(ref list, "K", false);

        Assert.Equal(2, JsonNode.Parse(list)!.AsArray().Count);
    }

    [Fact]
    public void SliceInPlace_ReplacesSource()
    {
        string list = """[10,20,30,40,50]""";
        _sut.List_SliceInPlace(ref list, 1, 4, 1);

        var arr = JsonNode.Parse(list)!.AsArray();
        Assert.Equal(3, arr.Count);
        Assert.Equal("20", arr[0]!.ToString());
    }

    [Fact]
    public void ShuffleInPlace_PreservesCountAndReplacesSource()
    {
        string list = """[1,2,3,4,5,6,7,8,9,10]""";
        string original = list;
        _sut.List_ShuffleInPlace(ref list, 42);

        Assert.NotSame(original, list);
        var arr = JsonNode.Parse(list)!.AsArray();
        Assert.Equal(10, arr.Count);
    }

    [Fact]
    public void UpdateAtInPlace_MutatesRefAndReturnsPrevious()
    {
        string list = """[{"S":"Old"}]""";
        _sut.List_UpdateAtInPlace(ref list, 0, "S", "\"New\"", out var previous);

        Assert.Equal("\"Old\"", previous);
        var arr = JsonNode.Parse(list)!.AsArray();
        Assert.Equal("New", arr[0]!["S"]!.ToString());
    }

    // ── Delegation parity — spot-check that InPlace matches base ─────────────

    [Fact]
    public void SliceInPlace_MatchesBaseSlice()
    {
        string input = """[1,2,3,4,5,6,7,8,9,10]""";

        string refCopy = input;
        _sut.List_SliceInPlace(ref refCopy, 2, 8, 2);
        _sut.List_Slice(input, 2, 8, 2, out var baseOut);

        Assert.Equal(baseOut, refCopy);
    }

    [Fact]
    public void ShuffleInPlace_DeterministicSeed_MatchesBaseShuffle()
    {
        string input = """[1,2,3,4,5,6,7,8,9,10]""";

        string refCopy = input;
        _sut.List_ShuffleInPlace(ref refCopy, 42);
        _sut.List_Shuffle(input, 42, out var baseOut);

        Assert.Equal(baseOut, refCopy);
    }

    [Fact]
    public void ChunkInPlace_MatchesBaseChunk()
    {
        string input = """[1,2,3,4,5]""";

        string refCopy = input;
        _sut.List_ChunkInPlace(ref refCopy, 2);
        _sut.List_Chunk(input, 2, out var baseOut);

        Assert.Equal(baseOut, refCopy);
    }

    // ── Null / empty input passthrough ───────────────────────────────────────

    [Fact]
    public void ShuffleInPlace_EmptyRef_BecomesEmpty()
    {
        string list = "";
        _sut.List_ShuffleInPlace(ref list, 1);
        Assert.Equal("[]", list);
    }

    [Fact]
    public void UpdateAtInPlace_OutOfRange_LeavesRefUnchanged()
    {
        string list = """[{"S":"A"}]""";
        string original = list;
        _sut.List_UpdateAtInPlace(ref list, 5, "S", "\"B\"", out var previous);

        Assert.Equal(original, list);
        Assert.Equal("null", previous);
    }

    [Fact]
    public void PopInPlace_OutOfRange_LeavesRefUnchanged()
    {
        string list = """[1,2]""";
        string original = list;
        _sut.List_PopInPlace(ref list, 99, out var popped);

        Assert.Equal(original, list);
        Assert.Equal("null", popped);
    }

    // ── Full parity matrix — every InPlace produces byte-identical output ───
    // to its base action on the same input (both primary + secondary out).

    [Fact]
    public void PopInPlace_MatchesBasePop()
    {
        string input = """[10,20,30,40,50]""";

        string refCopy = input;
        _sut.List_PopInPlace(ref refCopy, 2, out var refPopped);
        _sut.List_Pop(input, 2, out var baseUpdated, out var basePopped);

        Assert.Equal(baseUpdated, refCopy);
        Assert.Equal(basePopped, refPopped);
    }

    [Fact]
    public void PopMultipleInPlace_MatchesBasePopMultiple()
    {
        string input = """[1,2,3,4,5,6,7,8]""";
        const string indices = "0,3,5,7";

        string refCopy = input;
        _sut.List_PopMultipleInPlace(ref refCopy, indices, out var refPopped);
        _sut.List_PopMultiple(input, indices, out var baseUpdated, out var basePopped);

        Assert.Equal(baseUpdated, refCopy);
        Assert.Equal(basePopped, refPopped);
    }

    [Fact]
    public void PopByConditionInPlace_MatchesBase()
    {
        string input = """[{"S":"A"},{"S":"B"},{"S":"A"}]""";

        string refCopy = input;
        _sut.List_PopByConditionInPlace(ref refCopy, "S", "A", "Equals", false, true, out var refPopped);
        _sut.List_PopByCondition(input, "S", "A", "Equals", false, true, out var baseUpdated, out var basePopped);

        Assert.Equal(baseUpdated, refCopy);
        Assert.Equal(basePopped, refPopped);
    }

    [Fact]
    public void PopMultipleByConditionInPlace_MatchesBase()
    {
        string input = """[{"K":1},{"K":2},{"K":3},{"K":2}]""";

        string refCopy = input;
        _sut.List_PopMultipleByConditionInPlace(ref refCopy, "K", "2", "Equals", false, out var refPopped);
        _sut.List_PopMultipleByCondition(input, "K", "2", "Equals", false, out var baseUpdated, out var basePopped);

        Assert.Equal(baseUpdated, refCopy);
        Assert.Equal(basePopped, refPopped);
    }

    [Fact]
    public void PopByConditionsInPlace_MatchesBase()
    {
        string input = """[{"A":1,"B":"x"},{"A":2,"B":"y"},{"A":3,"B":"x"}]""";
        string cond = """[{"path":"A","operator":"GreaterThan","value":"1"},{"path":"B","operator":"Equals","value":"x"}]""";

        string refCopy = input;
        _sut.List_PopByConditionsInPlace(ref refCopy, cond, "AND", false, out var refPopped);
        _sut.List_PopByConditions(input, cond, "AND", false, out var baseUpdated, out var basePopped);

        Assert.Equal(baseUpdated, refCopy);
        Assert.Equal(basePopped, refPopped);
    }

    [Fact]
    public void PopMultipleByConditionsInPlace_MatchesBase()
    {
        string input = """[{"A":1},{"A":2},{"A":3},{"A":4}]""";
        string cond = """[{"path":"A","operator":"GreaterOrEqual","value":"3"}]""";

        string refCopy = input;
        _sut.List_PopMultipleByConditionsInPlace(ref refCopy, cond, "OR", out var refPopped);
        _sut.List_PopMultipleByConditions(input, cond, "OR", out var baseUpdated, out var basePopped);

        Assert.Equal(baseUpdated, refCopy);
        Assert.Equal(basePopped, refPopped);
    }

    [Fact]
    public void ZipInPlace_MatchesBase()
    {
        string a = """[1,2,3]""";
        string b = """["x","y","z"]""";

        string refCopy = a;
        _sut.List_ZipInPlace(ref refCopy, b, "N", "L");
        _sut.List_Zip(a, b, "N", "L", out var baseOut);

        Assert.Equal(baseOut, refCopy);
    }

    [Fact]
    public void GroupByInPlace_MatchesBase()
    {
        string input = """[{"K":"a","V":1},{"K":"b","V":2},{"K":"a","V":3},{"K":"c","V":4}]""";

        string refCopy = input;
        _sut.List_GroupByInPlace(ref refCopy, "K");
        _sut.List_GroupBy(input, "K", out var baseOut);

        Assert.Equal(baseOut, refCopy);
    }

    [Fact]
    public void DifferenceInPlace_MatchesBase()
    {
        string a = """[{"Id":1},{"Id":2},{"Id":3},{"Id":4}]""";
        string b = """[{"Id":2},{"Id":4}]""";

        string refCopy = a;
        _sut.List_DifferenceInPlace(ref refCopy, b, "Id", "Equals", false);
        _sut.List_Difference(a, b, "Id", "Equals", false, out var baseOut);

        Assert.Equal(baseOut, refCopy);
    }

    [Fact]
    public void DistinctByInPlace_MatchesBase()
    {
        string input = """[{"K":"a"},{"K":"B"},{"K":"a"},{"K":"b"}]""";

        string refCopy = input;
        _sut.List_DistinctByInPlace(ref refCopy, "K", true);
        _sut.List_DistinctBy(input, "K", true, out var baseOut);

        Assert.Equal(baseOut, refCopy);
    }

    [Fact]
    public void UpdateAtInPlace_MatchesBase()
    {
        string input = """[{"S":"Old"},{"S":"Keep"}]""";

        string refCopy = input;
        _sut.List_UpdateAtInPlace(ref refCopy, 0, "S", "\"New\"", out var refPrev);
        _sut.List_UpdateAt(input, 0, "S", "\"New\"", out var baseUpdated, out var basePrev);

        Assert.Equal(baseUpdated, refCopy);
        Assert.Equal(basePrev, refPrev);
    }

    // ── Ref semantics — chained calls, aliasing, secondary-output isolation ─

    [Fact]
    public void PopInPlace_ChainedCalls_ReduceListSequentially()
    {
        string list = """[10,20,30,40,50]""";

        _sut.List_PopInPlace(ref list, 0, out var pop1);
        _sut.List_PopInPlace(ref list, 0, out var pop2);
        _sut.List_PopInPlace(ref list, 0, out var pop3);

        Assert.Equal("10", pop1);
        Assert.Equal("20", pop2);
        Assert.Equal("30", pop3);
        var arr = JsonNode.Parse(list)!.AsArray();
        Assert.Equal(2, arr.Count);
        Assert.Equal("40", arr[0]!.ToString());
        Assert.Equal("50", arr[1]!.ToString());
    }

    [Fact]
    public void ShuffleInPlace_ChainedSameSeed_SecondCallShufflesTheFirstResult()
    {
        // Second call feeds the first shuffle's output back in, so the result
        // is NOT the original ordering — it is the shuffle-of-the-shuffle.
        string list = """[1,2,3,4,5,6,7,8,9,10]""";
        _sut.List_ShuffleInPlace(ref list, 42);
        string afterFirst = list;
        _sut.List_ShuffleInPlace(ref list, 42);

        _sut.List_Shuffle(afterFirst, 42, out var expected);
        Assert.Equal(expected, list);
    }

    [Fact]
    public void ShuffleInPlace_FreshInputSameSeed_ProducesSamePermutationEveryCall()
    {
        string input = """[1,2,3,4,5,6,7,8,9,10]""";

        string a = input, b = input, c = input;
        _sut.List_ShuffleInPlace(ref a, 42);
        _sut.List_ShuffleInPlace(ref b, 42);
        _sut.List_ShuffleInPlace(ref c, 42);

        Assert.Equal(a, b);
        Assert.Equal(b, c);
    }

    [Fact]
    public void ChunkInPlace_Then_SliceInPlace_Composes()
    {
        string list = """[1,2,3,4,5,6,7,8,9,10]""";
        _sut.List_ChunkInPlace(ref list, 2);
        _sut.List_SliceInPlace(ref list, 1, 4, 1);

        var arr = JsonNode.Parse(list)!.AsArray();
        Assert.Equal(3, arr.Count);
        Assert.Equal("3", arr[0]!.AsArray()[0]!.ToString());
        Assert.Equal("8", arr[2]!.AsArray()[1]!.ToString());
    }

    [Fact]
    public void DistinctByInPlace_Then_GroupByInPlace_Composes()
    {
        string list = """[{"K":"a","V":1},{"K":"b","V":2},{"K":"a","V":3}]""";
        _sut.List_DistinctByInPlace(ref list, "K", false);
        _sut.List_GroupByInPlace(ref list, "K");

        var arr = JsonNode.Parse(list)!.AsArray();
        Assert.Equal(2, arr.Count);
        Assert.Single(arr[0]!["Items"]!.AsArray());
        Assert.Single(arr[1]!["Items"]!.AsArray());
    }

    [Fact]
    public void PopByConditionInPlace_ToggleSearchDirection_InSequence()
    {
        // First pop the first "X", then the last remaining "Y".
        string list = """[{"S":"X"},{"S":"Y"},{"S":"X"},{"S":"Y"}]""";

        _sut.List_PopByConditionInPlace(ref list, "S", "X", "Equals", false, false, out var firstX);
        _sut.List_PopByConditionInPlace(ref list, "S", "Y", "Equals", false, true, out var lastY);

        var arr = JsonNode.Parse(list)!.AsArray();
        Assert.Equal(2, arr.Count);
        Assert.Contains("X", firstX);
        Assert.Contains("Y", lastY);
        // The remaining items are the second X (index 2) and the first Y (index 1).
        Assert.Equal("Y", arr[0]!["S"]!.ToString());
        Assert.Equal("X", arr[1]!["S"]!.ToString());
    }

    [Fact]
    public void PopInPlace_SecondaryOutput_IsIndependentOfRef()
    {
        string list = """[{"K":"unique-value-in-first"},{"K":"remaining"}]""";
        _sut.List_PopInPlace(ref list, 0, out var popped);

        Assert.Contains("unique-value-in-first", popped);
        Assert.DoesNotContain("unique-value-in-first", list);
    }

    [Fact]
    public void UpdateAtInPlace_PreviousValueIsSnapshotBeforeMutation()
    {
        string list = """[{"S":"before"}]""";
        _sut.List_UpdateAtInPlace(ref list, 0, "S", "\"after\"", out var previous);

        Assert.Equal("\"before\"", previous);
        Assert.Contains("after", list);
        Assert.DoesNotContain("before", list);
    }

    [Fact]
    public void UpdateAtInPlace_ChainedWithPopInPlace_Composes()
    {
        string list = """[{"S":"A","V":1},{"S":"B","V":2},{"S":"C","V":3}]""";

        _sut.List_UpdateAtInPlace(ref list, 1, "S", "\"B-mod\"", out _);
        _sut.List_PopInPlace(ref list, 0, out var popped);

        Assert.Contains("A", popped);
        var arr = JsonNode.Parse(list)!.AsArray();
        Assert.Equal(2, arr.Count);
        Assert.Equal("B-mod", arr[0]!["S"]!.ToString());
        Assert.Equal("C", arr[1]!["S"]!.ToString());
    }

    [Fact]
    public void ShuffleInPlace_MalformedJson_ThrowsJsonException()
    {
        string list = "not-json";
        Assert.ThrowsAny<System.Text.Json.JsonException>(() =>
        {
            _sut.List_ShuffleInPlace(ref list, 42);
        });
    }

    [Fact]
    public void SliceInPlace_MalformedJson_ThrowsJsonException()
    {
        string list = "{not-an-array";
        Assert.ThrowsAny<System.Text.Json.JsonException>(() =>
        {
            _sut.List_SliceInPlace(ref list, 0, 3, 1);
        });
    }

    [Fact]
    public void ZipInPlace_ListBUnchanged_OnlyListAIsRef()
    {
        string a = """[1,2,3]""";
        string b = """["x","y","z"]""";
        string bBefore = b;

        _sut.List_ZipInPlace(ref a, b, "N", "L");

        // ListB (value input) must not be touched.
        Assert.Equal(bBefore, b);
    }

    [Fact]
    public void DifferenceInPlace_ListBUnchanged_OnlyListAIsRef()
    {
        string a = """[{"Id":1},{"Id":2},{"Id":3}]""";
        string b = """[{"Id":2}]""";
        string bBefore = b;

        _sut.List_DifferenceInPlace(ref a, b, "Id", "Equals", false);

        Assert.Equal(bBefore, b);
    }

    [Fact]
    public void RefAssignment_ReplacesStringReference_NotContents()
    {
        // Strings are immutable in .NET — InPlace must produce a NEW string
        // and assign it to the ref parameter. Sanity check that the caller's
        // original snapshot is preserved through a separate variable.
        string original = """[1,2,3]""";
        string live = original;
        _sut.List_ShuffleInPlace(ref live, 42);

        Assert.Equal("""[1,2,3]""", original);
        Assert.NotEqual(original, live);
    }
}
