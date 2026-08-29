using System.Text.Json.Nodes;

namespace ListUtils.Tests;

public class CaseSensitiveTests
{
    private readonly ListUtils _sut = new();

    [Fact]
    public void PopByCondition_CaseSensitiveEquals_ExactMatchOnly()
    {
        string json = """[{"Name":"Alice"},{"Name":"ALICE"},{"Name":"alice"}]""";

        _sut.List_PopByCondition(json, "Name", "Alice", "Equals", true, false, out _, out var popped);

        var poppedObj = JsonNode.Parse(popped)!.AsObject();
        Assert.Equal("Alice", poppedObj["Name"]!.ToString());
    }

    [Fact]
    public void PopMultipleByCondition_CaseSensitiveEquals_NoBroadMatch()
    {
        string json = """[{"Tag":"URGENT"},{"Tag":"urgent"},{"Tag":"Urgent"}]""";

        _sut.List_PopMultipleByCondition(json, "Tag", "URGENT", "Equals", true, out _, out var popped);

        var arr = JsonNode.Parse(popped)!.AsArray();
        Assert.Single(arr);
    }

    [Fact]
    public void PopByCondition_CaseInsensitive_MatchesRegardlessOfCase()
    {
        string json = """[{"Name":"ALICE"},{"Name":"bob"}]""";

        _sut.List_PopByCondition(json, "Name", "alice", "Equals", false, false, out _, out var popped);

        var poppedObj = JsonNode.Parse(popped)!.AsObject();
        Assert.Equal("ALICE", poppedObj["Name"]!.ToString());
    }

    [Fact]
    public void PopMultipleByCondition_CaseSensitiveContains_ExactCase()
    {
        string json = """[{"Text":"HelloWorld"},{"Text":"helloworld"},{"Text":"Hello There"}]""";

        _sut.List_PopMultipleByCondition(json, "Text", "Hello", "Contains", true, out _, out var popped);

        var arr = JsonNode.Parse(popped)!.AsArray();
        Assert.Equal(2, arr.Count);
    }

    [Fact]
    public void PopByCondition_CaseSensitiveStartsWith()
    {
        string json = """[{"File":"README.md"},{"File":"readme.md"}]""";

        _sut.List_PopByCondition(json, "File", "README", "StartsWith", true, false, out _, out var popped);

        var poppedObj = JsonNode.Parse(popped)!.AsObject();
        Assert.Equal("README.md", poppedObj["File"]!.ToString());
    }

    [Fact]
    public void Difference_CaseSensitive_ExactMatchOnly()
    {
        string listA = """[{"Code":"ABC"},{"Code":"abc"},{"Code":"XYZ"}]""";
        string listB = """[{"Code":"abc"}]""";

        _sut.List_Difference(listA, listB, "Code", "Equals", true, out var diff);

        var arr = JsonNode.Parse(diff)!.AsArray();
        Assert.Equal(2, arr.Count);
    }

    [Fact]
    public void Difference_CaseInsensitive_RemovesBothCasings()
    {
        string listA = """[{"Code":"ABC"},{"Code":"abc"},{"Code":"XYZ"}]""";
        string listB = """[{"Code":"abc"}]""";

        _sut.List_Difference(listA, listB, "Code", "Equals", false, out var diff);

        var arr = JsonNode.Parse(diff)!.AsArray();
        Assert.Single(arr);
        Assert.Equal("XYZ", arr[0]!["Code"]!.ToString());
    }

    [Fact]
    public void PopByCondition_CaseSensitiveNotEquals()
    {
        string json = """[{"Status":"ACTIVE"},{"Status":"active"}]""";

        _sut.List_PopByCondition(json, "Status", "ACTIVE", "NotEquals", true, false, out _, out var popped);

        var poppedObj = JsonNode.Parse(popped)!.AsObject();
        Assert.Equal("active", poppedObj["Status"]!.ToString());
    }
}
