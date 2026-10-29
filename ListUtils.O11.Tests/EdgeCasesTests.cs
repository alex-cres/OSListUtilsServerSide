using System.Text.Json.Nodes;

namespace ListUtils.Tests;

// Edge-case tests for boundaries not covered by the other test files:
// empty JSON arrays, whitespace-only input, single-element lists, ordering
// invariants, null/empty parameter names, and contract behaviour for the
// PoppedElement output when no element matches.
public class EdgeCasesTests
{
    private readonly ListUtils _sut = new();

    // ─── Empty & whitespace input ────────────────────────────────────────

    [Fact]
    public void List_Pop_EmptyArray_ReturnsEmpty()
    {
        _sut.List_Pop("[]", 0, out var updated, out var popped);
        Assert.Equal("[]", updated);
        // Contract: empty source → popped = "null".
        Assert.Equal("null", popped);
    }

    [Fact]
    public void List_PopMultiple_EmptyArray_ReturnsEmpty()
    {
        _sut.List_PopMultiple("[]", "0,1,2", out var updated, out var popped);
        Assert.Equal("[]", updated);
        var arr = JsonNode.Parse(popped)!.AsArray();
        Assert.Empty(arr);
    }

    [Fact]
    public void List_PopByCondition_EmptyArray_ReturnsEmpty()
    {
        _sut.List_PopByCondition("[]", "Id", "1", "Equals", false, false, out var updated, out var popped);
        Assert.Equal("[]", updated);
        // Contract: no match → popped = "{}".
        Assert.Equal("{}", popped);
    }

    [Fact]
    public void List_PopMultipleByCondition_EmptyArray_ReturnsEmpty()
    {
        _sut.List_PopMultipleByCondition("[]", "Id", "1", "Equals", false, out var updated, out var popped);
        Assert.Equal("[]", updated);
        var arr = JsonNode.Parse(popped)!.AsArray();
        Assert.Empty(arr);
    }

    [Fact]
    public void List_Zip_BothEmpty_ReturnsEmpty()
    {
        _sut.List_Zip("[]", "[]", "A", "B", out var zipped);
        var arr = JsonNode.Parse(zipped)!.AsArray();
        Assert.Empty(arr);
    }

    [Fact]
    public void List_GroupBy_EmptyArray_ReturnsEmpty()
    {
        _sut.List_GroupBy("[]", "Id", out var grouped);
        var arr = JsonNode.Parse(grouped)!.AsArray();
        Assert.Empty(arr);
    }

    [Fact]
    public void List_Difference_BothEmpty_ReturnsEmpty()
    {
        _sut.List_Difference("[]", "[]", "Id", "Equals", false, out var diff);
        var arr = JsonNode.Parse(diff)!.AsArray();
        Assert.Empty(arr);
    }

    // ─── Single-element list ─────────────────────────────────────────────

    [Fact]
    public void List_Pop_SingleElement_ReturnsEmptyAndElement()
    {
        _sut.List_Pop("""[{"Id":42}]""", 0, out var updated, out var popped);
        var arr = JsonNode.Parse(updated)!.AsArray();
        Assert.Empty(arr);
        Assert.Contains("\"Id\":42", popped);
    }

    [Fact]
    public void List_PopByCondition_SingleElementMatch_ReturnsEmpty()
    {
        _sut.List_PopByCondition("""[{"Id":42}]""", "Id", "42", "Equals", false, false, out var updated, out var popped);
        var arr = JsonNode.Parse(updated)!.AsArray();
        Assert.Empty(arr);
        Assert.Contains("\"Id\":42", popped);
    }

    [Fact]
    public void List_GroupBy_SingleElement_ReturnsOneGroup()
    {
        _sut.List_GroupBy("""[{"Id":1,"Cat":"A"}]""", "Cat", out var grouped);
        var arr = JsonNode.Parse(grouped)!.AsArray();
        Assert.Single(arr);
        Assert.Equal("A", arr[0]!["Key"]!.ToString());
        Assert.Single(arr[0]!["Items"]!.AsArray());
    }

    // ─── Ordering preservation ──────────────────────────────────────────

    [Fact]
    public void List_PopMultipleByCondition_PreservesRemainingOrder()
    {
        string listJson = """[{"Id":1,"K":"a"},{"Id":2,"K":"b"},{"Id":3,"K":"a"},{"Id":4,"K":"c"},{"Id":5,"K":"a"}]""";
        _sut.List_PopMultipleByCondition(listJson, "K", "a", "Equals", false, out var updated, out _);
        var arr = JsonNode.Parse(updated)!.AsArray();
        Assert.Equal(2, arr.Count);
        Assert.Equal(2, (int)arr[0]!["Id"]!);
        Assert.Equal(4, (int)arr[1]!["Id"]!);
    }

    [Fact]
    public void List_PopMultiple_PreservesOrderAfterMultipleRemovals()
    {
        string listJson = """[{"Id":0},{"Id":1},{"Id":2},{"Id":3},{"Id":4}]""";
        _sut.List_PopMultiple(listJson, "1,3", out var updated, out _);
        var arr = JsonNode.Parse(updated)!.AsArray();
        Assert.Equal(3, arr.Count);
        Assert.Equal(0, (int)arr[0]!["Id"]!);
        Assert.Equal(2, (int)arr[1]!["Id"]!);
        Assert.Equal(4, (int)arr[2]!["Id"]!);
    }

    [Fact]
    public void List_GroupBy_PreservesFirstSeenGroupOrder()
    {
        string listJson = """[{"K":"B"},{"K":"A"},{"K":"C"},{"K":"A"},{"K":"B"}]""";
        _sut.List_GroupBy(listJson, "K", out var grouped);
        var arr = JsonNode.Parse(grouped)!.AsArray();
        Assert.Equal(3, arr.Count);
        Assert.Equal("B", arr[0]!["Key"]!.ToString());
        Assert.Equal("A", arr[1]!["Key"]!.ToString());
        Assert.Equal("C", arr[2]!["Key"]!.ToString());
    }

    // ─── Null / empty parameter names ────────────────────────────────────

    [Fact]
    public void List_PopByCondition_EmptyPropertyName_NoMatch()
    {
        string listJson = """[{"Id":1},{"Id":2}]""";
        _sut.List_PopByCondition(listJson, "", "1", "Equals", false, false, out var updated, out _);
        var arr = JsonNode.Parse(updated)!.AsArray();
        Assert.Equal(2, arr.Count);
    }

    [Fact]
    public void List_GroupBy_EmptyPropertyName_AllInUnknownGroup()
    {
        string listJson = """[{"Id":1},{"Id":2},{"Id":3}]""";
        _sut.List_GroupBy(listJson, "", out var grouped);
        var arr = JsonNode.Parse(grouped)!.AsArray();
        Assert.Single(arr);
        Assert.Equal("Unknown", arr[0]!["Key"]!.ToString());
        Assert.Equal(3, arr[0]!["Items"]!.AsArray().Count);
    }

    // ─── PopByConditions edge cases ──────────────────────────────────────

    [Fact]
    public void List_PopByConditions_EmptyLogicalOperator_DefaultsToAnd()
    {
        string listJson = """[{"S":"A","C":"X"},{"S":"A","C":"Y"},{"S":"B","C":"X"}]""";
        var conds = new List<Condition> {
            new() { Path = "S", Operator = Operators.Equals, Value = "A" },
            new() { Path = "C", Operator = Operators.Equals, Value = "X" },
        };
        _sut.List_PopByConditions(listJson, conds, "", false, out var updated, out var popped);
        var arr = JsonNode.Parse(updated)!.AsArray();
        Assert.Equal(2, arr.Count);
        Assert.Contains("\"S\":\"A\"", popped);
        Assert.Contains("\"C\":\"X\"", popped);
    }

    [Fact]
    public void List_PopMultipleByConditions_SingleCondition_MatchesAsExpected()
    {
        string listJson = """[{"S":"A"},{"S":"B"},{"S":"A"}]""";
        var conds = new List<Condition> { new() { Path = "S", Operator = Operators.Equals, Value = "A" } };
        _sut.List_PopMultipleByConditions(listJson, conds, LogicalOperators.AND, out var updated, out var popped);
        var upArr = JsonNode.Parse(updated)!.AsArray();
        var poArr = JsonNode.Parse(popped)!.AsArray();
        Assert.Single(upArr);
        Assert.Equal(2, poArr.Count);
        Assert.Equal("B", upArr[0]!["S"]!.ToString());
    }

    // ─── Zip null keys / uneven ─────────────────────────────────────────

    [Fact]
    public void List_Zip_EmptyKeyNames_UsesEmptyStringKeys()
    {
        string listA = """[{"Id":1}]""";
        string listB = """[{"Id":2}]""";
        _sut.List_Zip(listA, listB, "", "", out var zipped);
        var arr = JsonNode.Parse(zipped)!.AsArray();
        Assert.Single(arr);
        Assert.True(arr[0]!.AsObject().ContainsKey(""));
    }

    [Fact]
    public void List_Zip_OneEmpty_ReturnsEmpty()
    {
        _sut.List_Zip("[]", """[{"Id":1}]""", "A", "B", out var zipped);
        var arr = JsonNode.Parse(zipped)!.AsArray();
        Assert.Empty(arr);
    }

    // ─── Numeric operator on non-numeric value ──────────────────────────

    [Fact]
    public void List_Difference_Numeric_NonNumericTargetValue_KeepsAll()
    {
        string listA = """[{"V":"1"},{"V":"2"}]""";
        string listB = """[{"V":"foo"}]""";
        _sut.List_Difference(listA, listB, "V", "GreaterThan", false, out var diff);
        var arr = JsonNode.Parse(diff)!.AsArray();
        Assert.Equal(2, arr.Count);
    }

    // ─── Difference with NotEquals ──────────────────────────────────────

    [Fact]
    public void List_Difference_NotEqualsWithEmptyB_KeepsAllOfA()
    {
        string listA = """[{"V":"1"},{"V":"2"}]""";
        _sut.List_Difference(listA, "[]", "V", "NotEquals", false, out var diff);
        var arr = JsonNode.Parse(diff)!.AsArray();
        Assert.Equal(2, arr.Count);
    }

    [Fact]
    public void List_Difference_NotEqualsWithKeylessB_KeepsAllOfA()
    {
        string listA = """[{"V":"1"},{"V":"2"}]""";
        string listB = """[{"X":"1"}]""";
        _sut.List_Difference(listA, listB, "V", "NotEquals", false, out var diff);
        var arr = JsonNode.Parse(diff)!.AsArray();
        Assert.Equal(2, arr.Count);
    }

    [Fact]
    public void List_Difference_NotEqualsWithSingleKeyInB_KeepsOnlyThatKey()
    {
        string listA = """[{"V":"1"},{"V":"2"}]""";
        string listB = """[{"V":"1"}]""";
        _sut.List_Difference(listA, listB, "V", "NotEquals", false, out var diff);
        var arr = JsonNode.Parse(diff)!.AsArray();
        Assert.Single(arr);
        Assert.Equal("1", arr[0]!["V"]!.GetValue<string>());
    }

    // ─── Writes through a missing array leave the item untouched ────────

    [Fact]
    public void List_UpdateAt_MissingArrayInPath_LeavesItemUnchanged()
    {
        _sut.List_UpdateAt("""[{"Id":1}]""", 0, "Items[0].Name", "\"X\"", out var updated, out var previous);
        Assert.Equal("""[{"Id":1}]""", updated);
        Assert.Equal("null", previous);
    }

    [Fact]
    public void List_UpdateAt_MissingObjectThenArrayInPath_LeavesItemUnchanged()
    {
        _sut.List_UpdateAt("""[{"Id":1}]""", 0, "Meta.Tags[0]", "\"X\"", out var updated, out var previous);
        Assert.Equal("""[{"Id":1}]""", updated);
        Assert.Equal("null", previous);
    }

    [Fact]
    public void List_UpdateMultipleAt_MissingArrayInPath_LeavesItemsUnchanged()
    {
        _sut.List_UpdateMultipleAt("""[{"Id":1},{"Id":2}]""", "0,1", "Items[0].Name", "\"X\"", out var updated, out var count);
        Assert.Equal("""[{"Id":1},{"Id":2}]""", updated);
        // Count reports targeted object items, including those whose write was skipped.
        Assert.Equal(2, count);
    }

    [Fact]
    public void List_ReplaceWhere_MissingArrayInPath_LeavesItemsUnchanged()
    {
        var cond = new List<Condition> { new() { Path = "Id", Operator = Operators.Equals, Value = "1" } };
        _sut.List_ReplaceWhere("""[{"Id":1}]""", cond, LogicalOperators.AND, "Items[0].Name", "\"X\"", out var updated, out var matchCount);
        Assert.Equal("""[{"Id":1}]""", updated);
        Assert.Equal(1, matchCount);
    }

    [Theory]
    [InlineData("Meta.")]
    [InlineData("Meta..Name")]
    [InlineData("Meta.Info.")]
    public void List_UpdateAt_EmptySegmentAfterMissingObject_LeavesItemUnchanged(string path)
    {
        _sut.List_UpdateAt("""[{"Id":1}]""", 0, path, "\"X\"", out var updated, out var previous);
        Assert.Equal("""[{"Id":1}]""", updated);
        Assert.Equal("null", previous);
    }

    [Fact]
    public void List_UpdateAt_MissingObjectPath_StillCreatesIntermediateObjects()
    {
        _sut.List_UpdateAt("""[{"Id":1}]""", 0, "Meta.Info.Name", "\"X\"", out var updated, out _);
        Assert.Equal("""[{"Id":1,"Meta":{"Info":{"Name":"X"}}}]""", updated);
    }

    // ─── Case-sensitive Zip / GroupBy ───────────────────────────────────

    [Fact]
    public void List_GroupBy_KeysAreCaseSensitiveByDefault()
    {
        string listJson = """[{"K":"a"},{"K":"A"},{"K":"a"}]""";
        _sut.List_GroupBy(listJson, "K", out var grouped);
        var arr = JsonNode.Parse(grouped)!.AsArray();
        // "a" and "A" are distinct groups.
        Assert.Equal(2, arr.Count);
    }
}
