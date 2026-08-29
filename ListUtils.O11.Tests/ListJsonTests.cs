using System.Text.Json.Nodes;

namespace ListUtils.Tests;

public class ListJsonTests
{
    private readonly ListUtils _sut = new();

    #region PopByCondition

    [Fact]
    public void List_PopByCondition_MatchExists_RemovesFirst()
    {
        string json = """[{"Id":"1","Name":"Alice"},{"Id":"2","Name":"Bob"},{"Id":"1","Name":"Charlie"}]""";

        _sut.List_PopByCondition(json, "Id", "1", "", false, out var updated, out var popped);

        var poppedObj = JsonNode.Parse(popped)!.AsObject();
        Assert.Equal("Alice", poppedObj["Name"]!.ToString());

        var updatedArr = JsonNode.Parse(updated)!.AsArray();
        Assert.Equal(2, updatedArr.Count);
    }

    [Fact]
    public void List_PopByCondition_NoMatch_ReturnsOriginal()
    {
        string json = """[{"Id":"1"},{"Id":"2"}]""";

        _sut.List_PopByCondition(json, "Id", "99", "", false, out var updated, out var popped);

        Assert.Equal("{}", popped);
        var updatedArr = JsonNode.Parse(updated)!.AsArray();
        Assert.Equal(2, updatedArr.Count);
    }

    [Fact]
    public void List_PopByCondition_NullInput_ReturnsDefaults()
    {
        _sut.List_PopByCondition(null!, "Id", "1", "", false, out var updated, out var popped);

        Assert.Equal("[]", updated);
        Assert.Equal("{}", popped);
    }

    [Fact]
    public void List_PopByCondition_CamelCaseFallback_MatchesProperty()
    {
        string json = """[{"isActive":"true","name":"Item1"}]""";

        _sut.List_PopByCondition(json, "IsActive", "true", "", false, out var updated, out var popped);

        var poppedObj = JsonNode.Parse(popped)!.AsObject();
        Assert.Equal("Item1", poppedObj["name"]!.ToString());
        Assert.Equal("[]", updated);
    }

    #endregion

    #region PopMultipleByCondition

    [Fact]
    public void List_PopMultipleByCondition_MatchesAll()
    {
        string json = """[{"Status":"Active"},{"Status":"Inactive"},{"Status":"Active"}]""";

        _sut.List_PopMultipleByCondition(json, "Status", "Active", "", false, out var updated, out var popped);

        var poppedArr = JsonNode.Parse(popped)!.AsArray();
        Assert.Equal(2, poppedArr.Count);

        var updatedArr = JsonNode.Parse(updated)!.AsArray();
        Assert.Single(updatedArr);
    }

    [Fact]
    public void List_PopMultipleByCondition_NoMatch_ReturnsOriginal()
    {
        string json = """[{"Status":"Active"}]""";

        _sut.List_PopMultipleByCondition(json, "Status", "Deleted", "", false, out var updated, out var popped);

        Assert.Equal("[]", popped);
        var updatedArr = JsonNode.Parse(updated)!.AsArray();
        Assert.Single(updatedArr);
    }

    #endregion

    #region Zip

    [Fact]
    public void List_Zip_EqualLengthLists_PairsCorrectly()
    {
        string listA = """[{"Name":"Alice"},{"Name":"Bob"}]""";
        string listB = """[{"Score":90},{"Score":85}]""";

        _sut.List_Zip(listA, listB, "Person", "Result", out var zipped);

        var arr = JsonNode.Parse(zipped)!.AsArray();
        Assert.Equal(2, arr.Count);
        Assert.Equal("Alice", arr[0]!["Person"]!["Name"]!.ToString());
        Assert.Equal("85", arr[1]!["Result"]!["Score"]!.ToString());
    }

    [Fact]
    public void List_Zip_UnequalLists_UsesMinLength()
    {
        string listA = """[{"X":1},{"X":2},{"X":3}]""";
        string listB = """[{"Y":10}]""";

        _sut.List_Zip(listA, listB, "A", "B", out var zipped);

        var arr = JsonNode.Parse(zipped)!.AsArray();
        Assert.Single(arr);
    }

    [Fact]
    public void List_Zip_EmptyInput_ReturnsEmpty()
    {
        _sut.List_Zip("", """[{"X":1}]""", "A", "B", out var zipped);
        Assert.Equal("[]", zipped);
    }

    #endregion

    #region GroupBy

    [Fact]
    public void List_GroupBy_GroupsCorrectly()
    {
        string json = """[{"Dept":"Eng","Name":"A"},{"Dept":"Sales","Name":"B"},{"Dept":"Eng","Name":"C"}]""";

        _sut.List_GroupBy(json, "Dept", out var grouped);

        var arr = JsonNode.Parse(grouped)!.AsArray();
        Assert.Equal(2, arr.Count);
        Assert.Equal("Eng", arr[0]!["Key"]!.ToString());
        Assert.Equal(2, arr[0]!["Items"]!.AsArray().Count);
        Assert.Equal("Sales", arr[1]!["Key"]!.ToString());
    }

    [Fact]
    public void List_GroupBy_EmptyInput_ReturnsEmpty()
    {
        _sut.List_GroupBy("", "Dept", out var grouped);
        Assert.Equal("[]", grouped);
    }

    #endregion

    #region Difference

    [Fact]
    public void List_Difference_RemovesMatchingItems()
    {
        string listA = """[{"Id":"1"},{"Id":"2"},{"Id":"3"}]""";
        string listB = """[{"Id":"2"}]""";

        _sut.List_Difference(listA, listB, "Id", "", false, out var diff);

        var arr = JsonNode.Parse(diff)!.AsArray();
        Assert.Equal(2, arr.Count);
        Assert.Equal("1", arr[0]!["Id"]!.ToString());
        Assert.Equal("3", arr[1]!["Id"]!.ToString());
    }

    [Fact]
    public void List_Difference_NullListB_ReturnsListA()
    {
        string listA = """[{"Id":"1"}]""";

        _sut.List_Difference(listA, null!, "Id", "", false, out var diff);

        Assert.Equal(listA, diff);
    }

    [Fact]
    public void List_Difference_NullListA_ReturnsEmpty()
    {
        _sut.List_Difference(null!, """[{"Id":"1"}]""", "Id", "", false, out var diff);

        Assert.Equal("[]", diff);
    }

    [Fact]
    public void List_Difference_CaseInsensitiveMatch()
    {
        string listA = """[{"Id":"ABC"},{"Id":"def"}]""";
        string listB = """[{"Id":"abc"}]""";

        _sut.List_Difference(listA, listB, "Id", "", false, out var diff);

        var arr = JsonNode.Parse(diff)!.AsArray();
        Assert.Single(arr);
        Assert.Equal("def", arr[0]!["Id"]!.ToString());
    }

    #endregion
}
