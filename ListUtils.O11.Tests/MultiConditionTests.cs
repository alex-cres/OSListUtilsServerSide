using System.Text.Json.Nodes;

namespace ListUtils.Tests;

public class MultiConditionTests
{
    private readonly ListUtils _sut = new();

    [Fact]
    public void PopByConditions_AND_AllMatch()
    {
        string json = """
            [
                {"Name":"Alice","Age":25,"Status":"Active"},
                {"Name":"Bob","Age":35,"Status":"Active"},
                {"Name":"Carol","Age":25,"Status":"Inactive"}
            ]
            """;
        string conditions = """
            [
                {"path":"Age","operator":"Equals","value":"25"},
                {"path":"Status","operator":"Equals","value":"Active"}
            ]
            """;

        _sut.List_PopByConditions(json, conditions, "AND", false, out _, out var popped);

        var poppedObj = JsonNode.Parse(popped)!.AsObject();
        Assert.Equal("Alice", poppedObj["Name"]!.ToString());
    }

    [Fact]
    public void PopMultipleByConditions_OR_AnyMatch()
    {
        string json = """
            [
                {"Id":1,"Status":"Active","Score":50},
                {"Id":2,"Status":"Inactive","Score":90},
                {"Id":3,"Status":"Inactive","Score":30}
            ]
            """;
        string conditions = """
            [
                {"path":"Status","operator":"Equals","value":"Active"},
                {"path":"Score","operator":"GreaterThan","value":"80"}
            ]
            """;

        _sut.List_PopMultipleByConditions(json, conditions, "OR", out _, out var popped);

        var arr = JsonNode.Parse(popped)!.AsArray();
        Assert.Equal(2, arr.Count);
    }

    [Fact]
    public void PopByConditions_AND_DefaultMode()
    {
        string json = """[{"A":"x","B":"y"},{"A":"x","B":"z"}]""";
        string conditions = """[{"path":"A","operator":"Equals","value":"x"},{"path":"B","operator":"Equals","value":"z"}]""";

        _sut.List_PopByConditions(json, conditions, "", false, out _, out var popped);

        var poppedObj = JsonNode.Parse(popped)!.AsObject();
        Assert.Equal("z", poppedObj["B"]!.ToString());
    }

    [Fact]
    public void PopByConditions_EmptyConditions_ReturnsOriginal()
    {
        string json = """[{"Id":1}]""";

        _sut.List_PopByConditions(json, "[]", "AND", false, out var updated, out var popped);

        Assert.Equal(json, updated);
        Assert.Equal("{}", popped);
    }

    [Fact]
    public void PopMultipleByConditions_MixedOperators_ANDMatching()
    {
        string json = """
            [
                {"Category":"Books","Price":25.00,"InStock":true},
                {"Category":"Books","Price":50.00,"InStock":true},
                {"Category":"Books","Price":15.00,"InStock":false},
                {"Category":"Electronics","Price":30.00,"InStock":true}
            ]
            """;
        string conditions = """
            [
                {"path":"Category","operator":"Equals","value":"Books"},
                {"path":"Price","operator":"GreaterOrEqual","value":"20"},
                {"path":"InStock","operator":"Equals","value":"true"}
            ]
            """;

        _sut.List_PopMultipleByConditions(json, conditions, "AND", out _, out var popped);

        var arr = JsonNode.Parse(popped)!.AsArray();
        Assert.Equal(2, arr.Count);
    }

    [Fact]
    public void PopByConditions_NestedPathInCondition()
    {
        string json = """
            [
                {"Id":1,"Meta":{"Region":"EU","Priority":"High"}},
                {"Id":2,"Meta":{"Region":"US","Priority":"High"}},
                {"Id":3,"Meta":{"Region":"EU","Priority":"Low"}}
            ]
            """;
        string conditions = """
            [
                {"path":"Meta.Region","operator":"Equals","value":"EU"},
                {"path":"Meta.Priority","operator":"Equals","value":"High"}
            ]
            """;

        _sut.List_PopByConditions(json, conditions, "AND", false, out _, out var popped);

        var poppedObj = JsonNode.Parse(popped)!.AsObject();
        Assert.Equal("1", poppedObj["Id"]!.ToString());
    }

    [Fact]
    public void PopMultipleByConditions_CaseSensitivePerCondition()
    {
        string json = """[{"Tag":"URGENT"},{"Tag":"urgent"},{"Tag":"low"}]""";
        string conditions = """[{"path":"Tag","operator":"Equals","value":"URGENT","caseSensitive":true}]""";

        _sut.List_PopMultipleByConditions(json, conditions, "AND", out _, out var popped);

        var arr = JsonNode.Parse(popped)!.AsArray();
        Assert.Single(arr);
        Assert.Equal("URGENT", arr[0]!["Tag"]!.ToString());
    }

    [Fact]
    public void PopByConditions_NullSource_ReturnsDefaults()
    {
        _sut.List_PopByConditions(null!, """[{"path":"X","operator":"Equals","value":"y"}]""", "AND", false, out var updated, out var popped);

        Assert.Equal("[]", updated);
        Assert.Equal("{}", popped);
    }

    [Fact]
    public void PopMultipleByConditions_NoMatch_ReturnsEmpty()
    {
        string json = """[{"X":"a"},{"X":"b"}]""";
        string conditions = """[{"path":"X","operator":"Equals","value":"z"}]""";

        _sut.List_PopMultipleByConditions(json, conditions, "AND", out var updated, out var popped);

        Assert.Equal("[]", popped);
        var arr = JsonNode.Parse(updated)!.AsArray();
        Assert.Equal(2, arr.Count);
    }
}
