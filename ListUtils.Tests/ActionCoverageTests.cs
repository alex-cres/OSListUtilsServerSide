using System.Text.Json.Nodes;

namespace ListUtils.Tests;

// Contract tests that close per-action coverage gaps found in a 33-action review.
// One region per Server Action, in IListUtils declaration order. Shared by ODC and O11
// (the O11 copy must stay byte-for-byte identical).
public class ActionCoverageTests
{
    private readonly ListUtils _sut = new();

    private static JsonArray Arr(string json) => JsonNode.Parse(json)!.AsArray();

    #region List_Pop

    [Fact]
    public void List_Pop_JsonNullElement_PoppedIsNullLiteral()
    {
        _sut.List_Pop("[1,null,3]", 1, out var updated, out var popped);

        Assert.Equal("null", popped);
        Assert.Equal("[1,3]", updated);
    }

    #endregion

    #region List_PopMultiple

    [Fact]
    public void List_PopMultiple_UnsortedIndicesWithSpacesAndDuplicates_PopsInSourceOrder()
    {
        _sut.List_PopMultiple("""["a","b","c","d"]""", " 3 , 1,1", out var updated, out var popped);

        Assert.Equal("""["b","d"]""", popped);
        Assert.Equal("""["a","c"]""", updated);
    }

    #endregion

    #region List_SplitAt

    [Fact]
    public void List_SplitAt_NullOrEmptySource_ReturnsEmptyPair()
    {
        _sut.List_SplitAt(null!, 1, out var left, out var right);
        _sut.List_SplitAt("[]", 1, out var left2, out var right2);

        Assert.Equal(("[]", "[]"), (left, right));
        Assert.Equal(("[]", "[]"), (left2, right2));
    }

    #endregion

    #region List_PopByCondition

    [Fact]
    public void List_PopByCondition_EmptyOperator_DefaultsToEquals()
    {
        _sut.List_PopByCondition("""[{"S":"x"},{"S":"y"}]""", "S", "y", "", false, false, out var updated, out var popped);

        Assert.Equal("""{"S":"y"}""", popped);
        Assert.Equal("""[{"S":"x"}]""", updated);
    }

    #endregion

    #region List_PopMultipleByCondition

    [Fact]
    public void List_PopMultipleByCondition_EmptyPropertyName_ReturnsSourceAndEmptyPopped()
    {
        string json = """[{"S":"a"}]""";

        _sut.List_PopMultipleByCondition(json, "", "a", "Equals", false, out var updated, out var popped);

        Assert.Equal(json, updated);
        Assert.Equal("[]", popped);
    }

    [Fact]
    public void List_PopMultipleByCondition_NullSource_ReturnsEmptyArrays()
    {
        _sut.List_PopMultipleByCondition(null!, "S", "a", "Equals", false, out var updated, out var popped);

        Assert.Equal("[]", updated);
        Assert.Equal("[]", popped);
    }

    #endregion

    #region List_PopByConditions

    [Fact]
    public void List_PopByConditions_NoMatch_ReturnsSourceAndEmptyObject()
    {
        var cond = new List<Condition> { new() { Path = "S", Operator = Operators.Equals, Value = "z" } };

        _sut.List_PopByConditions("""[{"S":"a"}]""", cond, LogicalOperators.AND, false, out var updated, out var popped);

        Assert.Equal("{}", popped);
        Assert.Equal("""[{"S":"a"}]""", updated);
    }

    [Fact]
    public void List_PopByConditions_ConditionOnMissingPath_TreatedAsNoMatch()
    {
        var cond = new List<Condition>
        {
            new() { Path = "S", Operator = Operators.Equals, Value = "a" },
            new() { Path = "Missing.Path", Operator = Operators.Equals, Value = "x" }
        };

        _sut.List_PopByConditions("""[{"S":"a"}]""", cond, LogicalOperators.AND, false, out var updated, out var popped);

        Assert.Equal("{}", popped);
        Assert.Single(Arr(updated));
    }

    #endregion

    #region List_PopMultipleByConditions

    [Fact]
    public void List_PopMultipleByConditions_NullSource_ReturnsEmptyArrays()
    {
        var cond = new List<Condition> { new() { Path = "S", Operator = Operators.Equals, Value = "a" } };

        _sut.List_PopMultipleByConditions(null!, cond, LogicalOperators.AND, out var updated, out var popped);

        Assert.Equal("[]", updated);
        Assert.Equal("[]", popped);
    }

    [Fact]
    public void List_PopMultipleByConditions_LowercaseOr_IsAccepted()
    {
        var cond = new List<Condition>
        {
            new() { Path = "S", Operator = Operators.Equals, Value = "a" },
            new() { Path = "S", Operator = Operators.Equals, Value = "b" }
        };

        _sut.List_PopMultipleByConditions("""[{"S":"a"},{"S":"b"},{"S":"c"}]""", cond, "or", out var updated, out var popped);

        Assert.Equal("""[{"S":"a"},{"S":"b"}]""", popped);
        Assert.Equal("""[{"S":"c"}]""", updated);
    }

    #endregion

    #region List_Partition

    [Fact]
    public void List_Partition_EmptyPropertyName_AllElementsNonMatching()
    {
        string json = """[{"S":"a"},{"S":"b"}]""";

        _sut.List_Partition(json, "", "a", "Equals", false, out var matching, out var nonMatching);

        Assert.Equal("[]", matching);
        Assert.Equal(json, nonMatching);
    }

    [Fact]
    public void List_Partition_NullSource_ReturnsEmptyArrays()
    {
        _sut.List_Partition(null!, "S", "a", "Equals", false, out var matching, out var nonMatching);

        Assert.Equal("[]", matching);
        Assert.Equal("[]", nonMatching);
    }

    [Fact]
    public void List_Partition_MissingPath_GoesToNonMatching_AndOrderIsPreserved()
    {
        string json = """[{"S":"a","I":1},{"I":2},{"S":"a","I":3},{"S":"b","I":4}]""";

        _sut.List_Partition(json, "S", "a", "Equals", false, out var matching, out var nonMatching);

        Assert.Equal("""[{"S":"a","I":1},{"S":"a","I":3}]""", matching);
        Assert.Equal("""[{"I":2},{"S":"b","I":4}]""", nonMatching);
    }

    [Fact]
    public void List_Partition_NumericOperator_SplitsByValue()
    {
        _sut.List_Partition("""[{"V":1},{"V":2},{"V":3},{"V":4}]""", "V", "2", "GreaterThan", false, out var matching, out var nonMatching);

        Assert.Equal("""[{"V":3},{"V":4}]""", matching);
        Assert.Equal("""[{"V":1},{"V":2}]""", nonMatching);
    }

    [Fact]
    public void List_Partition_CaseSensitiveFlag_IsRespected()
    {
        string json = """[{"S":"a"},{"S":"A"}]""";

        _sut.List_Partition(json, "S", "A", "Equals", true, out var strict, out _);
        _sut.List_Partition(json, "S", "A", "Equals", false, out var loose, out _);

        Assert.Equal("""[{"S":"A"}]""", strict);
        Assert.Equal(2, Arr(loose).Count);
    }

    #endregion

    #region List_PartitionByConditions

    [Fact]
    public void List_PartitionByConditions_OR_MatchesAnyAndPreservesOrder()
    {
        var cond = new List<Condition>
        {
            new() { Path = "S", Operator = Operators.Equals, Value = "a" },
            new() { Path = "S", Operator = Operators.Equals, Value = "c" }
        };

        _sut.List_PartitionByConditions("""[{"S":"a"},{"S":"b"},{"S":"c"}]""", cond, LogicalOperators.OR, out var matching, out var nonMatching);

        Assert.Equal("""[{"S":"a"},{"S":"c"}]""", matching);
        Assert.Equal("""[{"S":"b"}]""", nonMatching);
    }

    [Fact]
    public void List_PartitionByConditions_NullSource_ReturnsEmptyArrays()
    {
        var cond = new List<Condition> { new() { Path = "S", Operator = Operators.Equals, Value = "a" } };

        _sut.List_PartitionByConditions(null!, cond, LogicalOperators.AND, out var matching, out var nonMatching);

        Assert.Equal("[]", matching);
        Assert.Equal("[]", nonMatching);
    }

    #endregion

    #region List_Zip

    [Fact]
    public void List_Zip_NullObjectAndArrayElements_ArePreservedInPairs()
    {
        _sut.List_Zip("""["x",null]""", """[{"k":1},[1,2]]""", "A", "B", out var zipped);

        Assert.Equal("""[{"A":"x","B":{"k":1}},{"A":null,"B":[1,2]}]""", zipped);
    }

    #endregion

    #region List_GroupBy

    [Fact]
    public void List_GroupBy_NumberAndStringWithSameText_ShareOneTextKey()
    {
        _sut.List_GroupBy("""[{"K":1},{"K":"1"}]""", "K", out var grouped);

        var arr = Arr(grouped);
        Assert.Single(arr);
        Assert.Equal("1", arr[0]!["Key"]!.GetValue<string>());
        Assert.Equal(2, arr[0]!["Items"]!.AsArray().Count);
    }

    #endregion

    #region List_GroupByMultiple

    [Fact]
    public void List_GroupByMultiple_ShortKeyNamesAndBlankItemsField_UseDefaults()
    {
        var paths = new List<string> { "R", "C" };
        var names = new List<string> { "Region" };

        _sut.List_GroupByMultiple("""[{"R":"EU","C":"A"}]""", paths, names, "", false, out var grouped);

        Assert.Equal("""[{"Region":"EU","Key1":"A","Items":[{"R":"EU","C":"A"}]}]""", grouped);
    }

    #endregion

    #region List_ZipGroupByMultiple

    [Fact]
    public void List_ZipGroupByMultiple_BlankNames_DefaultToKeyNAndItemsAB()
    {
        var keys = new List<string> { "K" };

        _sut.List_ZipGroupByMultiple("""[{"K":"1"}]""", """[{"K":"1"}]""", keys, keys, null!, "", null!, false, out var grouped);

        Assert.Equal("""[{"Key0":"1","ItemsA":[{"K":"1"}],"ItemsB":[{"K":"1"}]}]""", grouped);
    }

    [Fact]
    public void List_ZipGroupByMultiple_BOnlyKeys_AppendedAfterAKeys()
    {
        var keys = new List<string> { "K" };

        _sut.List_ZipGroupByMultiple("""[{"K":"1"},{"K":"2"}]""", """[{"K":"3"},{"K":"1"}]""", keys, keys, null!, "A", "B", false, out var grouped);

        var arr = Arr(grouped);
        Assert.Equal(new[] { "1", "2", "3" }, arr.Select(g => g!["Key0"]!.GetValue<string>()).ToArray());
        Assert.Empty(arr[2]!["A"]!.AsArray());
        Assert.Single(arr[2]!["B"]!.AsArray());
    }

    [Fact]
    public void List_ZipGroupByMultiple_CaseSensitiveFlag_IsRespected()
    {
        var keys = new List<string> { "C" };

        _sut.List_ZipGroupByMultiple("""[{"C":"eu"}]""", """[{"C":"EU"}]""", keys, keys, null!, "A", "B", false, out var loose);
        _sut.List_ZipGroupByMultiple("""[{"C":"eu"}]""", """[{"C":"EU"}]""", keys, keys, null!, "A", "B", true, out var strict);

        Assert.Single(Arr(loose));
        Assert.Equal(2, Arr(strict).Count);
    }

    #endregion

    #region List_ZipManyGroupByMultiple

    [Fact]
    public void List_ZipManyGroupByMultiple_NullLabels_DefaultToKeyNAndItemsN()
    {
        var lists = new List<string> { """[{"K":"1"}]""", """[{"K":"1"}]""" };
        var keys = new List<string> { "K", "K" };

        _sut.List_ZipManyGroupByMultiple(lists, 1, keys, null!, null!, false, out var grouped);

        Assert.Equal("""[{"Key0":"1","Items0":[{"K":"1"}],"Items1":[{"K":"1"}]}]""", grouped);
    }

    #endregion

    #region List_Difference

    [Fact]
    public void List_Difference_KeylessAItems_AreKept()
    {
        _sut.List_Difference("""[{"Id":1},{"X":2}]""", """[{"Id":1}]""", "Id", "Equals", false, out var diff);

        Assert.Equal("""[{"X":2}]""", diff);
    }

    #endregion

    #region List_Intersect

    [Fact]
    public void List_Intersect_StartsWithOperator_UsesSlowPath()
    {
        _sut.List_Intersect("""[{"C":"ABC"},{"C":"XYZ"}]""", """[{"C":"AB"}]""", "C", "StartsWith", false, out var result);

        Assert.Equal("""[{"C":"ABC"}]""", result);
    }

    [Fact]
    public void List_Intersect_GreaterThanOperator_ComparesNumerically()
    {
        _sut.List_Intersect("""[{"V":5},{"V":1}]""", """[{"V":3}]""", "V", "GreaterThan", false, out var result);

        Assert.Equal("""[{"V":5}]""", result);
    }

    [Fact]
    public void List_Intersect_CaseSensitiveFlag_IsRespected()
    {
        _sut.List_Intersect("""[{"C":"a"}]""", """[{"C":"A"}]""", "C", "Equals", false, out var loose);
        _sut.List_Intersect("""[{"C":"a"}]""", """[{"C":"A"}]""", "C", "Equals", true, out var strict);

        Assert.Single(Arr(loose));
        Assert.Equal("[]", strict);
    }

    [Fact]
    public void List_Intersect_KeylessAExcluded_DuplicatesInAKept()
    {
        _sut.List_Intersect("""[{"Id":1,"n":1},{"Id":1,"n":2},{"X":0}]""", """[{"Id":1}]""", "Id", "Equals", false, out var result);

        Assert.Equal("""[{"Id":1,"n":1},{"Id":1,"n":2}]""", result);
    }

    #endregion

    #region List_Union

    [Fact]
    public void List_Union_CaseSensitiveFlag_IsRespected()
    {
        _sut.List_Union("""[{"C":"a"}]""", """[{"C":"A"}]""", "C", false, out var loose);
        _sut.List_Union("""[{"C":"a"}]""", """[{"C":"A"}]""", "C", true, out var strict);

        Assert.Equal("""[{"C":"a"}]""", loose);
        Assert.Equal(2, Arr(strict).Count);
    }

    [Fact]
    public void List_Union_KeylessItems_OnlyFirstSurvives()
    {
        _sut.List_Union("""[{"X":1}]""", """[{"Y":2},{"Id":3}]""", "Id", false, out var result);

        Assert.Equal("""[{"X":1},{"Id":3}]""", result);
    }

    [Fact]
    public void List_Union_OneSideEmpty_DedupesTheOther()
    {
        _sut.List_Union("", """[{"Id":1},{"Id":1},{"Id":2}]""", "Id", false, out var result);
        _sut.List_Union(null!, null!, "Id", false, out var bothEmpty);

        Assert.Equal("""[{"Id":1},{"Id":2}]""", result);
        Assert.Equal("[]", bothEmpty);
    }

    [Fact]
    public void List_Union_NestedKeyPath_Works()
    {
        _sut.List_Union("""[{"M":{"Id":1}}]""", """[{"M":{"Id":1}},{"M":{"Id":2}}]""", "M.Id", false, out var result);

        Assert.Equal("""[{"M":{"Id":1}},{"M":{"Id":2}}]""", result);
    }

    #endregion

    #region List_DistinctBy

    [Fact]
    public void List_DistinctBy_CaseSensitive_KeepsDifferentCasings()
    {
        _sut.List_DistinctBy("""[{"K":"a"},{"K":"A"},{"K":"a"}]""", "K", true, out var result);

        Assert.Equal("""[{"K":"a"},{"K":"A"}]""", result);
    }

    #endregion

    #region List_Slice

    [Fact]
    public void List_Slice_NegativeStepWithExplicitBounds_MatchesPython()
    {
        // Python: [0,1,2,3,4,5][4:1:-2] == [4, 2]
        _sut.List_Slice("[0,1,2,3,4,5]", 4, 1, -2, out var result);

        Assert.Equal("[4,2]", result);
    }

    #endregion

    #region List_UpdateAt

    [Fact]
    public void List_UpdateAt_ExistingArrayElement_ReplacesAndReturnsPrevious()
    {
        _sut.List_UpdateAt("""[{"Tags":["a","b"]}]""", 0, "Tags[1]", "\"z\"", out var updated, out var previous);

        Assert.Equal("""[{"Tags":["a","z"]}]""", updated);
        Assert.Equal("\"b\"", previous);
    }

    [Fact]
    public void List_UpdateAt_CamelCaseExistingProperty_UpdatedInPlace()
    {
        _sut.List_UpdateAt("""[{"status":"old"}]""", 0, "Status", "\"new\"", out var updated, out var previous);

        Assert.Equal("""[{"status":"new"}]""", updated);
        Assert.Equal("\"old\"", previous);
    }

    #endregion

    #region List_Reverse

    [Fact]
    public void List_Reverse_ObjectsPreserved_SingleElementUnchanged()
    {
        _sut.List_Reverse("""[{"Id":1,"N":{"x":1}},{"Id":2}]""", out var reversed);
        _sut.List_Reverse("""["a"]""", out var single);

        Assert.Equal("""[{"Id":2},{"Id":1,"N":{"x":1}}]""", reversed);
        Assert.Equal("""["a"]""", single);
    }

    #endregion

    #region List_Flatten

    [Fact]
    public void List_Flatten_OnlyFlattensOneLevel()
    {
        _sut.List_Flatten(new List<string> { "[[1,2],[3]]", "[4]" }, out var flat);

        Assert.Equal("[[1,2],[3],4]", flat);
    }

    #endregion

    #region List_Sample

    [Theory]
    [InlineData(7)]
    [InlineData(0)]
    public void List_Sample_ResultIsSubsetWithoutDuplicates(int seed)
    {
        string source = "[" + string.Join(",", Enumerable.Range(0, 20)) + "]";

        _sut.List_Sample(source, 10, seed, out var sample);

        var values = Arr(sample).Select(n => n!.GetValue<int>()).ToList();
        Assert.Equal(10, values.Count);
        Assert.Equal(10, values.Distinct().Count());
        Assert.All(values, v => Assert.InRange(v, 0, 19));
    }

    #endregion

    #region List_ReplaceWhere

    [Fact]
    public void List_ReplaceWhere_RawStringValue_CreatesNestedObject()
    {
        var cond = new List<Condition> { new() { Path = "S", Operator = Operators.Equals, Value = "a" } };

        _sut.List_ReplaceWhere("""[{"S":"a"},{"S":"b"}]""", cond, LogicalOperators.AND, "Meta.Note", "hello", out var updated, out var count);

        Assert.Equal("""[{"S":"a","Meta":{"Note":"hello"}},{"S":"b"}]""", updated);
        Assert.Equal(1, count);
    }

    [Fact]
    public void List_ReplaceWhere_EmptyUpdatePropertyOrNullSource_NoChange()
    {
        var cond = new List<Condition> { new() { Path = "S", Operator = Operators.Equals, Value = "a" } };
        string json = """[{"S":"a"}]""";

        _sut.List_ReplaceWhere(json, cond, LogicalOperators.AND, "", "1", out var updated, out var count);
        _sut.List_ReplaceWhere(null!, cond, LogicalOperators.AND, "S", "1", out var nullUpdated, out var nullCount);

        Assert.Equal(json, updated);
        Assert.Equal(0, count);
        Assert.Equal("[]", nullUpdated);
        Assert.Equal(0, nullCount);
    }

    #endregion

    #region List_UpdateMultipleAt

    [Fact]
    public void List_UpdateMultipleAt_DuplicateAndAliasIndices_CountedOnce()
    {
        _sut.List_UpdateMultipleAt("""[{"Id":1},{"Id":2}]""", "0,0,-2", "N", "1", out var updated, out var count);

        Assert.Equal(1, count);
        Assert.Equal("""[{"Id":1,"N":1},{"Id":2}]""", updated);
    }

    [Fact]
    public void List_UpdateMultipleAt_NestedPathWithRawString_CreatesObjects()
    {
        _sut.List_UpdateMultipleAt("""[{"Id":1},{"Id":2}]""", "1", "Meta.Note", "hi", out var updated, out var count);

        Assert.Equal(1, count);
        Assert.Equal("""[{"Id":1},{"Id":2,"Meta":{"Note":"hi"}}]""", updated);
    }

    [Fact]
    public void List_UpdateMultipleAt_NonObjectItems_SkippedAndNotCounted()
    {
        _sut.List_UpdateMultipleAt("""["x",{"Id":1}]""", "0,1", "N", "1", out var updated, out var count);

        Assert.Equal(1, count);
        Assert.Equal("""["x",{"Id":1,"N":1}]""", updated);
    }

    [Fact]
    public void List_UpdateMultipleAt_EmptyIndicesOrNullSource_NoChange()
    {
        string json = """[{"Id":1}]""";

        _sut.List_UpdateMultipleAt(json, "", "N", "1", out var updated, out var count);
        _sut.List_UpdateMultipleAt(null!, "0", "N", "1", out var nullUpdated, out var nullCount);

        Assert.Equal(json, updated);
        Assert.Equal(0, count);
        Assert.Equal("[]", nullUpdated);
        Assert.Equal(0, nullCount);
    }

    #endregion

    #region List_MinBy

    [Fact]
    public void List_MinBy_Tie_KeepsFirstOccurrence()
    {
        _sut.List_MinBy("""[{"S":1},{"S":1}]""", "S", true, out _, out var minVal, out var idx);

        Assert.Equal("1", minVal);
        Assert.Equal(0, idx);
    }

    [Fact]
    public void List_MinBy_TextMode_IsCaseInsensitive()
    {
        // Case-sensitive ordinal would pick "B" (66 < 97); case-insensitive picks "a".
        _sut.List_MinBy("""[{"K":"B"},{"K":"a"}]""", "K", false, out _, out var minVal, out var idx);

        Assert.Equal("a", minVal);
        Assert.Equal(1, idx);
    }

    #endregion

    #region List_MaxBy

    [Fact]
    public void List_MaxBy_TextMode_IsCaseInsensitive()
    {
        // Case-sensitive ordinal would pick "a" (97 > 66); case-insensitive picks "B".
        _sut.List_MaxBy("""[{"K":"a"},{"K":"B"}]""", "K", false, out var element, out var maxVal, out var idx);

        Assert.Equal("B", maxVal);
        Assert.Equal(1, idx);
        Assert.Equal("""{"K":"B"}""", element);
    }

    [Fact]
    public void List_MaxBy_EmptySourceOrEmptyProperty_ReturnsSentinels()
    {
        _sut.List_MaxBy("[]", "S", true, out var e1, out var v1, out var i1);
        _sut.List_MaxBy("""[{"S":1}]""", "", true, out var e2, out var v2, out var i2);

        Assert.Equal(("null", "", -1), (e1, v1, i1));
        Assert.Equal(("null", "", -1), (e2, v2, i2));
    }

    [Fact]
    public void List_MaxBy_NumericModeNestedPath_SkipsNonNumericAndMissing()
    {
        _sut.List_MaxBy("""[{"M":{"V":"x"}},{"M":{"V":5}},{"Z":1},{"M":{"V":"9"}}]""", "M.V", true, out var element, out var maxVal, out var idx);

        Assert.Equal("9", maxVal);
        Assert.Equal(3, idx);
        Assert.Equal("""{"M":{"V":"9"}}""", element);
    }

    #endregion

    #region List_Aggregate

    [Theory]
    [InlineData("Median")]
    [InlineData("")]
    public void List_Aggregate_UnknownOrEmptyOperation_TreatedAsSum(string operation)
    {
        _sut.List_Aggregate("""[{"V":1},{"V":2},{"V":3}]""", "V", operation, out var result, out var matched);

        Assert.Equal("6", result);
        Assert.Equal(3, matched);
    }

    [Fact]
    public void List_Aggregate_OperationNameIsCaseInsensitive_AvgUsesInvariantDecimal()
    {
        _sut.List_Aggregate("""[{"V":1},{"V":2}]""", "V", "avg", out var result, out var matched);

        Assert.Equal("1.5", result);
        Assert.Equal(2, matched);
    }

    [Fact]
    public void List_Aggregate_CountDistinct_IsCaseSensitive()
    {
        _sut.List_Aggregate("""[{"V":"a"},{"V":"A"},{"V":"a"}]""", "V", "CountDistinct", out var result, out var matched);

        Assert.Equal("2", result);
        Assert.Equal(3, matched);
    }

    #endregion

    #region List_ZipMany

    [Fact]
    public void List_ZipMany_EmptyEntry_YieldsNoRows()
    {
        _sut.List_ZipMany(new List<string> { "[1,2]", "" }, new List<string> { "A", "B" }, out var zipped);

        Assert.Equal("[]", zipped);
    }

    [Fact]
    public void List_ZipMany_BlankKeyName_DefaultsToItemsN()
    {
        _sut.List_ZipMany(new List<string> { "[1]", "[2]" }, new List<string> { "A", "" }, out var zipped);

        Assert.Equal("""[{"A":1,"Items1":2}]""", zipped);
    }

    #endregion

    #region List_ZipManyGroupBy

    [Fact]
    public void List_ZipManyGroupBy_CaseSensitiveFlag_IsRespected()
    {
        var lists = new List<string> { """[{"K":"a"}]""", """[{"K":"A"}]""" };
        var keys = new List<string> { "K", "K" };

        _sut.List_ZipManyGroupBy(lists, keys, null!, false, out var loose);
        _sut.List_ZipManyGroupBy(lists, keys, null!, true, out var strict);

        Assert.Single(Arr(loose));
        Assert.Equal(2, Arr(strict).Count);
    }

    [Fact]
    public void List_ZipManyGroupBy_DefaultNames_AndFirstSeenOrderAcrossLists()
    {
        var lists = new List<string> { """[{"K":"2"},{"K":"1"}]""", """[{"K":"3"},{"K":"2"}]""" };
        var keys = new List<string> { "K", "K" };

        _sut.List_ZipManyGroupBy(lists, keys, null!, false, out var grouped);

        var arr = Arr(grouped);
        Assert.Equal(new[] { "2", "1", "3" }, arr.Select(g => g!["Key"]!.GetValue<string>()).ToArray());
        Assert.Single(arr[0]!["Items0"]!.AsArray());
        Assert.Single(arr[0]!["Items1"]!.AsArray());
        Assert.Empty(arr[2]!["Items0"]!.AsArray());
    }

    [Fact]
    public void List_ZipManyGroupBy_NullOrEmptyLists_ReturnsEmpty()
    {
        _sut.List_ZipManyGroupBy(null!, new List<string> { "K" }, null!, false, out var nullResult);
        _sut.List_ZipManyGroupBy(new List<string>(), new List<string> { "K" }, null!, false, out var emptyResult);

        Assert.Equal("[]", nullResult);
        Assert.Equal("[]", emptyResult);
    }

    #endregion
}
