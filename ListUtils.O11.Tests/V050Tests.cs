using System.Text.Json.Nodes;

namespace ListUtils.Tests;

public class V050Tests
{
    private readonly ListUtils _sut = new();

    // ── List_MinBy / List_MaxBy ─────────────────────────────────────────────

    [Fact]
    public void MinBy_NumericMode_ReturnsSmallestByValue()
    {
        string json = """[{"S":10},{"S":3},{"S":7},{"S":20}]""";

        _sut.List_MinBy(json, "S", true, out var element, out var minVal, out var idx);

        Assert.Equal("3", minVal);
        Assert.Equal(1, idx);
        Assert.Equal("3", JsonNode.Parse(element)!["S"]!.ToString());
    }

    [Fact]
    public void MaxBy_NumericMode_ReturnsLargestByValue()
    {
        string json = """[{"S":10},{"S":30},{"S":7},{"S":30}]""";

        _sut.List_MaxBy(json, "S", true, out var element, out var maxVal, out var idx);

        Assert.Equal("30", maxVal);
        Assert.Equal(1, idx); // first-occurrence wins on ties
    }

    [Fact]
    public void MinBy_TextMode_UsesOrdinalCompare()
    {
        string json = """[{"K":"beta"},{"K":"alpha"},{"K":"gamma"}]""";

        _sut.List_MinBy(json, "K", false, out var element, out var minVal, out var idx);

        Assert.Equal("alpha", minVal);
        Assert.Equal(1, idx);
    }

    [Fact]
    public void MinBy_NestedPath_Works()
    {
        string json = """[{"Meta":{"Score":50}},{"Meta":{"Score":10}}]""";

        _sut.List_MinBy(json, "Meta.Score", true, out var element, out var minVal, out var idx);

        Assert.Equal("10", minVal);
        Assert.Equal(1, idx);
    }

    [Fact]
    public void MinBy_EmptyList_ReturnsSentinels()
    {
        _sut.List_MinBy("[]", "S", true, out var element, out var minVal, out var idx);

        Assert.Equal("null", element);
        Assert.Equal("", minVal);
        Assert.Equal(-1, idx);
    }

    [Fact]
    public void MinBy_MissingProperty_SkipsItem()
    {
        string json = """[{"NotS":1},{"S":5},{"NotS":2}]""";

        _sut.List_MinBy(json, "S", true, out var element, out var minVal, out var idx);

        Assert.Equal("5", minVal);
        Assert.Equal(1, idx);
    }

    [Fact]
    public void MinBy_NumericMode_SkipsNonNumericValues()
    {
        string json = """[{"S":"n/a"},{"S":42},{"S":"foo"}]""";

        _sut.List_MinBy(json, "S", true, out var element, out var minVal, out var idx);

        Assert.Equal("42", minVal);
        Assert.Equal(1, idx);
    }

    // ── List_Aggregate ──────────────────────────────────────────────────────

    [Fact]
    public void Aggregate_Sum_AddsAllValues()
    {
        string json = """[{"S":1},{"S":2},{"S":3},{"S":4}]""";

        _sut.List_Aggregate(json, "S", "Sum", out var result, out var count);

        Assert.Equal("10", result);
        Assert.Equal(4, count);
    }

    [Fact]
    public void Aggregate_Avg_ComputesArithmeticMean()
    {
        string json = """[{"S":10},{"S":20},{"S":30}]""";

        _sut.List_Aggregate(json, "S", "Avg", out var result, out var count);

        Assert.Equal("20", result);
        Assert.Equal(3, count);
    }

    [Fact]
    public void Aggregate_MinMax_ReportSmallestAndLargest()
    {
        string json = """[{"S":5},{"S":1},{"S":9},{"S":3}]""";

        _sut.List_Aggregate(json, "S", "Min", out var min, out _);
        _sut.List_Aggregate(json, "S", "Max", out var max, out _);

        Assert.Equal("1", min);
        Assert.Equal("9", max);
    }

    [Fact]
    public void Aggregate_Count_CountsNonNullValues()
    {
        string json = """[{"S":"a"},{"NotS":"x"},{"S":"b"},{"NotS":"y"}]""";

        _sut.List_Aggregate(json, "S", "Count", out var result, out var count);

        Assert.Equal("2", result);
        Assert.Equal(2, count);
    }

    [Fact]
    public void Aggregate_CountDistinct_CountsUniqueValues()
    {
        string json = """[{"S":"a"},{"S":"b"},{"S":"a"},{"S":"c"},{"S":"b"}]""";

        _sut.List_Aggregate(json, "S", "CountDistinct", out var result, out var count);

        Assert.Equal("3", result);
        Assert.Equal(5, count);
    }

    [Fact]
    public void Aggregate_Sum_SkipsNonNumeric()
    {
        string json = """[{"S":10},{"S":"n/a"},{"S":20}]""";

        _sut.List_Aggregate(json, "S", "Sum", out var result, out var count);

        Assert.Equal("30", result);
        Assert.Equal(2, count);
    }

    [Fact]
    public void Aggregate_EmptyList_ReturnsEmptyResultAndZeroCount()
    {
        _sut.List_Aggregate("[]", "S", "Sum", out var result, out var count);

        Assert.Equal("", result);
        Assert.Equal(0, count);
    }

    // ── List_Intersect ──────────────────────────────────────────────────────

    [Fact]
    public void Intersect_ByKey_KeepsAElementsWhoseKeyIsInB()
    {
        string a = """[{"Id":1},{"Id":2},{"Id":3},{"Id":4}]""";
        string b = """[{"Id":2},{"Id":4},{"Id":9}]""";

        _sut.List_Intersect(a, b, "Id", "Equals", false, out var result);

        var arr = JsonNode.Parse(result)!.AsArray();
        Assert.Equal(2, arr.Count);
        Assert.Equal("2", arr[0]!["Id"]!.ToString());
        Assert.Equal("4", arr[1]!["Id"]!.ToString());
    }

    [Fact]
    public void Intersect_PreservesAOrder()
    {
        string a = """[{"K":"z"},{"K":"a"},{"K":"m"}]""";
        string b = """[{"K":"a"},{"K":"m"},{"K":"z"}]""";

        _sut.List_Intersect(a, b, "K", "Equals", false, out var result);

        var arr = JsonNode.Parse(result)!.AsArray();
        Assert.Equal("z", arr[0]!["K"]!.ToString());
        Assert.Equal("a", arr[1]!["K"]!.ToString());
    }

    [Fact]
    public void Intersect_EmptyEitherSide_ReturnsEmpty()
    {
        _sut.List_Intersect("[]", """[{"Id":1}]""", "Id", "Equals", false, out var lhsEmpty);
        _sut.List_Intersect("""[{"Id":1}]""", "[]", "Id", "Equals", false, out var rhsEmpty);

        Assert.Equal("[]", lhsEmpty);
        Assert.Equal("[]", rhsEmpty);
    }

    // ── List_Union ──────────────────────────────────────────────────────────

    [Fact]
    public void Union_ByKey_DedupesFirstOccurrenceWins()
    {
        string a = """[{"Id":1,"From":"A"},{"Id":2,"From":"A"}]""";
        string b = """[{"Id":2,"From":"B"},{"Id":3,"From":"B"}]""";

        _sut.List_Union(a, b, "Id", false, out var result);

        var arr = JsonNode.Parse(result)!.AsArray();
        Assert.Equal(3, arr.Count);
        Assert.Equal("A", arr[1]!["From"]!.ToString()); // Id=2 kept from A
        Assert.Equal("B", arr[2]!["From"]!.ToString()); // Id=3 came from B only
    }

    [Fact]
    public void Union_EmptyMatchKey_DedupesByWholeItem()
    {
        string a = """[1,2,3]""";
        string b = """[2,3,4]""";

        _sut.List_Union(a, b, "", false, out var result);

        var arr = JsonNode.Parse(result)!.AsArray();
        Assert.Equal(4, arr.Count);
    }

    // ── List_SplitAt ────────────────────────────────────────────────────────

    [Fact]
    public void SplitAt_PositiveIndex_SplitsAtBoundary()
    {
        string json = """[1,2,3,4,5]""";

        _sut.List_SplitAt(json, 2, out var left, out var right);

        Assert.Equal(2, JsonNode.Parse(left)!.AsArray().Count);
        Assert.Equal(3, JsonNode.Parse(right)!.AsArray().Count);
        Assert.Equal("3", JsonNode.Parse(right)!.AsArray()[0]!.ToString());
    }

    [Fact]
    public void SplitAt_NegativeIndex_CountsFromEnd()
    {
        string json = """[1,2,3,4,5]""";

        _sut.List_SplitAt(json, -2, out var left, out var right);

        Assert.Equal(3, JsonNode.Parse(left)!.AsArray().Count);
        Assert.Equal(2, JsonNode.Parse(right)!.AsArray().Count);
    }

    [Fact]
    public void SplitAt_OutOfRange_ClampsToBounds()
    {
        string json = """[1,2,3]""";

        _sut.List_SplitAt(json, 100, out var leftHigh, out var rightHigh);
        _sut.List_SplitAt(json, -100, out var leftLow, out var rightLow);

        Assert.Equal(3, JsonNode.Parse(leftHigh)!.AsArray().Count);
        Assert.Equal("[]", rightHigh);
        Assert.Equal("[]", leftLow);
        Assert.Equal(3, JsonNode.Parse(rightLow)!.AsArray().Count);
    }

    // ── List_Partition / PartitionByConditions ─────────────────────────────

    [Fact]
    public void Partition_ByCondition_ReturnsBothSides()
    {
        string json = """[{"S":"X"},{"S":"Y"},{"S":"X"},{"S":"Z"}]""";

        _sut.List_Partition(json, "S", "X", "Equals", false, out var matching, out var nonMatching);

        Assert.Equal(2, JsonNode.Parse(matching)!.AsArray().Count);
        Assert.Equal(2, JsonNode.Parse(nonMatching)!.AsArray().Count);
    }

    [Fact]
    public void PartitionByConditions_AND_UsesAllConditions()
    {
        string json = """[{"A":1,"B":"x"},{"A":2,"B":"x"},{"A":2,"B":"y"}]""";
        var cond = new List<Condition> {
            new() { Path = "A", Operator = Operators.Equals, Value = "2" },
            new() { Path = "B", Operator = Operators.Equals, Value = "x" },
        };

        _sut.List_PartitionByConditions(json, cond, "AND", out var matching, out var nonMatching);

        Assert.Single(JsonNode.Parse(matching)!.AsArray());
        Assert.Equal(2, JsonNode.Parse(nonMatching)!.AsArray().Count);
    }

    // ── List_Reverse ────────────────────────────────────────────────────────

    [Fact]
    public void Reverse_FlipsOrder()
    {
        _sut.List_Reverse("""[1,2,3,4,5]""", out var result);

        var arr = JsonNode.Parse(result)!.AsArray();
        Assert.Equal("5", arr[0]!.ToString());
        Assert.Equal("1", arr[4]!.ToString());
    }

    [Fact]
    public void Reverse_EmptyOrNullSource_ReturnsEmpty()
    {
        _sut.List_Reverse("", out var empty);
        _sut.List_Reverse(null!, out var nullSrc);
        _sut.List_Reverse("[]", out var emptyArr);

        Assert.Equal("[]", empty);
        Assert.Equal("[]", nullSrc);
        Assert.Equal("[]", emptyArr);
    }

    // ── List_Flatten ────────────────────────────────────────────────────────

    [Fact]
    public void Flatten_InverseOfChunk_RestoresOriginal()
    {
        string source = """[1,2,3,4,5,6,7]""";
        _sut.List_Chunk(source, 3, out var chunks);

        _sut.List_Flatten(chunks, out var flat);

        Assert.Equal(source, flat);
    }

    [Fact]
    public void Flatten_SkipsMalformedAndEmptyEntries()
    {
        var chunks = new List<string> { "[1,2]", "", null!, "not-json", "[3,4]" };

        _sut.List_Flatten(chunks, out var flat);

        Assert.Equal("[1,2,3,4]", flat);
    }

    [Fact]
    public void Flatten_NullOrEmptyInput_ReturnsEmpty()
    {
        _sut.List_Flatten(new List<string>(), out var empty);
        _sut.List_Flatten(null!, out var nullList);

        Assert.Equal("[]", empty);
        Assert.Equal("[]", nullList);
    }

    // ── List_Sample ─────────────────────────────────────────────────────────

    [Fact]
    public void Sample_DeterministicSeed_IsReproducible()
    {
        string json = """[1,2,3,4,5,6,7,8,9,10]""";

        _sut.List_Sample(json, 4, 42, out var first);
        _sut.List_Sample(json, 4, 42, out var second);

        Assert.Equal(first, second);
    }

    [Fact]
    public void Sample_ReturnsRequestedSize()
    {
        string json = """[1,2,3,4,5,6,7,8,9,10]""";

        _sut.List_Sample(json, 3, 7, out var result);

        Assert.Equal(3, JsonNode.Parse(result)!.AsArray().Count);
    }

    [Fact]
    public void Sample_SizeExceedsList_ReturnsFullShuffledList()
    {
        string json = """[1,2,3]""";

        _sut.List_Sample(json, 10, 1, out var result);

        Assert.Equal(3, JsonNode.Parse(result)!.AsArray().Count);
    }

    [Fact]
    public void Sample_ZeroOrNegativeSize_ReturnsEmpty()
    {
        _sut.List_Sample("""[1,2,3]""", 0, 1, out var zero);
        _sut.List_Sample("""[1,2,3]""", -5, 1, out var negative);

        Assert.Equal("[]", zero);
        Assert.Equal("[]", negative);
    }

    // ── List_ReplaceWhere ───────────────────────────────────────────────────

    [Fact]
    public void ReplaceWhere_SingleCondition_UpdatesAllMatches()
    {
        string json = """[{"S":"Old","V":1},{"S":"Keep","V":2},{"S":"Old","V":3}]""";
        var cond = new List<Condition> { new() { Path = "S", Operator = Operators.Equals, Value = "Old" } };

        _sut.List_ReplaceWhere(json, cond, "AND", "V", "999", out var updated, out var count);

        Assert.Equal(2, count);
        var arr = JsonNode.Parse(updated)!.AsArray();
        Assert.Equal("999", arr[0]!["V"]!.ToString());
        Assert.Equal("2", arr[1]!["V"]!.ToString());
        Assert.Equal("999", arr[2]!["V"]!.ToString());
    }

    [Fact]
    public void ReplaceWhere_NoMatches_LeavesSourceUnchanged()
    {
        string json = """[{"S":"A"}]""";
        var cond = new List<Condition> { new() { Path = "S", Operator = Operators.Equals, Value = "Z" } };

        _sut.List_ReplaceWhere(json, cond, "AND", "S", "\"X\"", out var updated, out var count);

        Assert.Equal(0, count);
        Assert.Equal("A", JsonNode.Parse(updated)![0]!["S"]!.ToString());
    }

    [Fact]
    public void ReplaceWhere_AndAcrossFields_MatchesRowsSatisfyingAllConditions()
    {
        string json = """[{"S":"Active","Score":10,"V":1},{"S":"Active","Score":90,"V":2},{"S":"Inactive","Score":90,"V":3},{"S":"Active","Score":50,"V":4}]""";
        var cond = new List<Condition> {
            new() { Path = "S", Operator = Operators.Equals, Value = "Active" },
            new() { Path = "Score", Operator = Operators.GreaterOrEqual, Value = "50" },
        };

        _sut.List_ReplaceWhere(json, cond, "AND", "V", "99", out var updated, out var count);

        Assert.Equal(2, count); // rows 2 and 4 only
        var arr = JsonNode.Parse(updated)!.AsArray();
        Assert.Equal("1", arr[0]!["V"]!.ToString());
        Assert.Equal("99", arr[1]!["V"]!.ToString());
        Assert.Equal("3", arr[2]!["V"]!.ToString());
        Assert.Equal("99", arr[3]!["V"]!.ToString());
    }

    [Fact]
    public void ReplaceWhere_OrLogic_MatchesAnyOfTheConditions()
    {
        string json = """[{"S":"A","V":1},{"S":"B","V":2},{"S":"C","V":3}]""";
        var cond = new List<Condition> {
            new() { Path = "S", Operator = Operators.Equals, Value = "A" },
            new() { Path = "S", Operator = Operators.Equals, Value = "C" },
        };

        _sut.List_ReplaceWhere(json, cond, "OR", "V", "0", out var updated, out var count);

        Assert.Equal(2, count);
        var arr = JsonNode.Parse(updated)!.AsArray();
        Assert.Equal("0", arr[0]!["V"]!.ToString());
        Assert.Equal("2", arr[1]!["V"]!.ToString());
        Assert.Equal("0", arr[2]!["V"]!.ToString());
    }

    [Fact]
    public void ReplaceWhere_EmptyConditions_DoesNothing()
    {
        string json = """[{"S":"A","V":1},{"S":"B","V":2}]""";

        _sut.List_ReplaceWhere(json, new List<Condition>(), "AND", "V", "0", out var updated, out var count);

        Assert.Equal(0, count);
        Assert.Equal("1", JsonNode.Parse(updated)![0]!["V"]!.ToString());
        Assert.Equal("2", JsonNode.Parse(updated)![1]!["V"]!.ToString());
    }

    [Fact]
    public void ReplaceWhere_NestedPathAndNumericOperator_WorkTogether()
    {
        string json = """[{"Meta":{"Score":10},"V":1},{"Meta":{"Score":80},"V":2},{"Meta":{"Score":90},"V":3}]""";
        var cond = new List<Condition> {
            new() { Path = "Meta.Score", Operator = Operators.GreaterThan, Value = "75" },
        };

        _sut.List_ReplaceWhere(json, cond, "AND", "V", "9", out var updated, out var count);

        Assert.Equal(2, count);
    }

    // ── List_UpdateMultipleAt ───────────────────────────────────────────────

    [Fact]
    public void UpdateMultipleAt_UpdatesRequestedIndices()
    {
        string json = """[{"V":1},{"V":2},{"V":3},{"V":4}]""";

        _sut.List_UpdateMultipleAt(json, "0,2", "V", "99", out var updated, out var count);

        Assert.Equal(2, count);
        var arr = JsonNode.Parse(updated)!.AsArray();
        Assert.Equal("99", arr[0]!["V"]!.ToString());
        Assert.Equal("2", arr[1]!["V"]!.ToString());
        Assert.Equal("99", arr[2]!["V"]!.ToString());
    }

    [Fact]
    public void UpdateMultipleAt_NegativeAndOutOfRangeIndices_HandledSafely()
    {
        string json = """[{"V":1},{"V":2},{"V":3}]""";

        _sut.List_UpdateMultipleAt(json, "-1, 99, 0, 0", "V", "0", out var updated, out var count);

        // -1 → index 2 ; 99 → skipped ; 0 → index 0 ; duplicate 0 → dedupe.
        Assert.Equal(2, count);
        var arr = JsonNode.Parse(updated)!.AsArray();
        Assert.Equal("0", arr[0]!["V"]!.ToString());
        Assert.Equal("2", arr[1]!["V"]!.ToString());
        Assert.Equal("0", arr[2]!["V"]!.ToString());
    }

    // ── List_ZipMany ────────────────────────────────────────────────────────

    [Fact]
    public void ZipMany_ThreeLists_PairsByPosition()
    {
        var lists = new List<string> { """[1,2,3]""", """["a","b","c"]""", """[true,false,true]""" };
        var keys = new List<string> { "N", "L", "F" };

        _sut.List_ZipMany(lists, keys, out var result);

        var arr = JsonNode.Parse(result)!.AsArray();
        Assert.Equal(3, arr.Count);
        Assert.Equal("1", arr[0]!["N"]!.ToString());
        Assert.Equal("a", arr[0]!["L"]!.ToString());
        Assert.True(arr[0]!["F"]!.GetValue<bool>());
    }

    [Fact]
    public void ZipMany_UnequalLengths_TruncatesToShortest()
    {
        var lists = new List<string> { """[1,2,3,4]""", """["a","b"]""", """[true,false,true]""" };
        var keys = new List<string> { "N", "L", "F" };

        _sut.List_ZipMany(lists, keys, out var result);

        Assert.Equal(2, JsonNode.Parse(result)!.AsArray().Count);
    }

    [Fact]
    public void ZipMany_MissingKeyNames_DefaultsToItemsN()
    {
        var lists = new List<string> { """[1,2]""", """["a","b"]""" };
        var keys = new List<string> { "First" }; // second list has no explicit label

        _sut.List_ZipMany(lists, keys, out var result);

        var arr = JsonNode.Parse(result)!.AsArray();
        Assert.Equal("1", arr[0]!["First"]!.ToString());
        Assert.Equal("a", arr[0]!["Items1"]!.ToString());
    }

    [Fact]
    public void ZipMany_EmptyLists_ReturnsEmpty()
    {
        _sut.List_ZipMany(new List<string>(), new List<string>(), out var result);

        Assert.Equal("[]", result);
    }

    // ── List_ZipManyGroupBy ─────────────────────────────────────────────────

    [Fact]
    public void ZipManyGroupBy_CogroupsThreeListsByKey()
    {
        var lists = new List<string> {
            """[{"CustomerId":1,"OrderId":"A"},{"CustomerId":2,"OrderId":"B"}]""",
            """[{"CustomerId":1,"PayId":"P1"},{"CustomerId":1,"PayId":"P2"}]""",
            """[{"CustomerId":3,"Ret":"R1"}]"""
        };
        var keyPaths = new List<string> { "CustomerId", "CustomerId", "CustomerId" };
        var names = new List<string> { "Orders", "Payments", "Returns" };

        _sut.List_ZipManyGroupBy(lists, keyPaths, names, false, out var result);

        var arr = JsonNode.Parse(result)!.AsArray();
        Assert.Equal(3, arr.Count);

        var one = arr[0]!.AsObject();
        Assert.Equal("1", one["Key"]!.ToString());
        Assert.Single(one["Orders"]!.AsArray());
        Assert.Equal(2, one["Payments"]!.AsArray().Count);
        Assert.Empty(one["Returns"]!.AsArray());

        var three = arr[2]!.AsObject();
        Assert.Equal("3", three["Key"]!.ToString());
        Assert.Empty(three["Orders"]!.AsArray());
        Assert.Empty(three["Payments"]!.AsArray());
        Assert.Single(three["Returns"]!.AsArray());
    }

    [Fact]
    public void ZipManyGroupBy_MissingKeyPath_LandsInUnknownBucket()
    {
        var lists = new List<string> {
            """[{"CustomerId":1},{"NoKey":true}]"""
        };
        var keyPaths = new List<string> { "CustomerId" };
        var names = new List<string> { "Items" };

        _sut.List_ZipManyGroupBy(lists, keyPaths, names, false, out var result);

        var arr = JsonNode.Parse(result)!.AsArray();
        Assert.Equal(2, arr.Count);
        Assert.Contains("Unknown", arr.Select(g => g!["Key"]!.ToString()));
    }

    // ── v0.5.0 refactor — regression guards for the typed Condition API ──────

    [Fact]
    public void PopMultipleByConditions_EmptyConditions_ReturnsSourceByteForByte()
    {
        // R2: byte-identity guard — updated must equal the exact SourceListJson string,
        // not a re-serialised round-trip.
        string json = """[ {"Id":1} , {"Id":2}  ]""";

        _sut.List_PopMultipleByConditions(json, new List<Condition>(), LogicalOperators.AND, out var updated, out var popped);

        Assert.Equal(json, updated);
        Assert.Equal("[]", popped);
    }

    [Fact]
    public void PartitionByConditions_EmptyConditions_ReturnsSourceByteForByte()
    {
        // R2: byte-identity guard on the non-matching output.
        string json = """[ {"Id":1} , {"Id":2}  ]""";

        _sut.List_PartitionByConditions(json, new List<Condition>(), LogicalOperators.AND, out var matching, out var nonMatching);

        Assert.Equal("[]", matching);
        Assert.Equal(json, nonMatching);
    }

    [Fact]
    public void ReplaceWhere_EmptyConditions_ReturnsSourceByteForByte()
    {
        // R2: byte-identity guard — the existing EmptyConditions_DoesNothing test only
        // checks per-item values via JsonNode round-trip; this one asserts UpdatedListJson
        // equals SourceListJson character-for-character.
        string json = """[ {"S":"A","V":1} , {"S":"B","V":2}  ]""";

        _sut.List_ReplaceWhere(json, new List<Condition>(), LogicalOperators.AND, "V", "0", out var updated, out var count);

        Assert.Equal(0, count);
        Assert.Equal(json, updated);
    }

    [Theory]
    [InlineData("!=", "20")]  // NotEquals alias
    [InlineData(">", "10")]   // GreaterThan alias
    [InlineData("<", "30")]   // LessThan alias
    [InlineData(">=", "20")]  // GreaterOrEqual alias
    [InlineData("<=", "20")]  // LessOrEqual alias
    public void PopByConditions_SymbolOperatorAliases_MatchSameAsNamedOperators(string symbol, string value)
    {
        // R3: exercise every legacy symbol alias through the typed Condition API.
        // Row with Score=20 must be selected by !=/>=/<= but not by > or < around 20.
        string json = """[{"Id":1,"Score":10},{"Id":2,"Score":20},{"Id":3,"Score":30}]""";
        var cond = new List<Condition> {
            new() { Path = "Score", Operator = symbol, Value = value },
        };

        _sut.List_PopByConditions(json, cond, LogicalOperators.AND, false, out _, out var popped);

        var poppedObj = JsonNode.Parse(popped)!.AsObject();
        // For symbol > 10  → Id 2 (first match).
        // For symbol < 30  → Id 1 (first match).
        // For symbol != 20 → Id 1 (first non-20).
        // For symbol >= 20 → Id 2 (first match).
        // For symbol <= 20 → Id 1 (first match).
        string expectedId = symbol switch {
            ">"  => "2",
            "<"  => "1",
            "!=" => "1",
            ">=" => "2",
            "<=" => "1",
            _    => throw new System.InvalidOperationException(symbol),
        };
        Assert.Equal(expectedId, poppedObj["Id"]!.ToString());
    }
}
