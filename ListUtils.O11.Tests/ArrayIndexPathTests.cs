using System.Text.Json.Nodes;

namespace ListUtils.Tests;

public class ArrayIndexPathTests
{
    private readonly ListUtils _sut = new();

    [Fact]
    public void PopByCondition_ArrayIndex_MatchesFirstElement()
    {
        string json = """
            [
                {"Id":1,"Tags":["red","urgent"]},
                {"Id":2,"Tags":["blue","normal"]},
                {"Id":3,"Tags":["red","normal"]}
            ]
            """;

        _sut.List_PopByCondition(json, "Tags[0]", "blue", "Equals", false, false, out _, out var popped);

        var poppedObj = JsonNode.Parse(popped)!.AsObject();
        Assert.Equal("2", poppedObj["Id"]!.ToString());
    }

    [Fact]
    public void PopByCondition_ArrayIndex_LastElementNegative()
    {
        string json = """
            [
                {"Id":1,"Path":["root","folder","file.txt"]},
                {"Id":2,"Path":["root","other","data.bin"]}
            ]
            """;

        _sut.List_PopByCondition(json, "Path[-1]", "data.bin", "Equals", false, false, out _, out var popped);

        var poppedObj = JsonNode.Parse(popped)!.AsObject();
        Assert.Equal("2", poppedObj["Id"]!.ToString());
    }

    [Fact]
    public void PopByCondition_ArrayIndexWithNestedObject()
    {
        string json = """
            [
                {"Id":1,"Items":[{"Name":"first","Qty":5}]},
                {"Id":2,"Items":[{"Name":"other","Qty":3}]}
            ]
            """;

        _sut.List_PopByCondition(json, "Items[0].Name", "other", "Equals", false, false, out _, out var popped);

        var poppedObj = JsonNode.Parse(popped)!.AsObject();
        Assert.Equal("2", poppedObj["Id"]!.ToString());
    }

    [Fact]
    public void PopMultipleByCondition_ArrayIndex_MultipleLevels()
    {
        string json = """
            [
                {"Id":1,"Data":{"Values":[100,200,300]}},
                {"Id":2,"Data":{"Values":[500,600,700]}},
                {"Id":3,"Data":{"Values":[100,999,300]}}
            ]
            """;

        _sut.List_PopMultipleByCondition(json, "Data.Values[0]", "100", "Equals", false, out _, out var popped);

        var arr = JsonNode.Parse(popped)!.AsArray();
        Assert.Equal(2, arr.Count);
    }

    [Fact]
    public void PopByCondition_ArrayIndexOutOfRange_NoMatch()
    {
        string json = """[{"Id":1,"Tags":["a"]}]""";

        _sut.List_PopByCondition(json, "Tags[5]", "a", "Equals", false, false, out var updated, out var popped);

        Assert.Equal("{}", popped);
        Assert.Equal(json, updated);
    }

    [Fact]
    public void GroupBy_ArrayIndex_GroupsByFirstElement()
    {
        string json = """
            [
                {"Id":1,"Roles":["admin","viewer"]},
                {"Id":2,"Roles":["user","editor"]},
                {"Id":3,"Roles":["admin","user"]}
            ]
            """;

        _sut.List_GroupBy(json, "Roles[0]", out var grouped);

        var arr = JsonNode.Parse(grouped)!.AsArray();
        Assert.Equal(2, arr.Count);
        Assert.Equal("admin", arr[0]!["Key"]!.ToString());
        Assert.Equal(2, arr[0]!["Items"]!.AsArray().Count);
    }

    [Fact]
    public void Difference_ArrayIndexPath_MatchesOnFirstElement()
    {
        string listA = """
            [
                {"Id":1,"Codes":["A1","B1"]},
                {"Id":2,"Codes":["A2","B2"]},
                {"Id":3,"Codes":["A3","B3"]}
            ]
            """;
        string listB = """[{"Codes":["A2"]}]""";

        _sut.List_Difference(listA, listB, "Codes[0]", "Equals", false, out var diff);

        var arr = JsonNode.Parse(diff)!.AsArray();
        Assert.Equal(2, arr.Count);
    }

    [Fact]
    public void PopByCondition_ArrayInsideNestedArray()
    {
        string json = """
            [
                {"Id":1,"Groups":[{"Members":["Alice","Bob"]}]},
                {"Id":2,"Groups":[{"Members":["Carol"]}]}
            ]
            """;

        _sut.List_PopByCondition(json, "Groups[0].Members[0]", "Carol", "Equals", false, false, out _, out var popped);

        var poppedObj = JsonNode.Parse(popped)!.AsObject();
        Assert.Equal("2", poppedObj["Id"]!.ToString());
    }

    [Fact]
    public void PopByCondition_NegativeIndex_SecondFromLast()
    {
        string json = """[{"Id":1,"Arr":["x","y","z"]}]""";

        _sut.List_PopByCondition(json, "Arr[-2]", "y", "Equals", false, false, out _, out var popped);

        var poppedObj = JsonNode.Parse(popped)!.AsObject();
        Assert.Equal("1", poppedObj["Id"]!.ToString());
    }
}
