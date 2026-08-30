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
        var conditions = new List<Condition> {
            new() { Path = "Age", Operator = Operators.Equals, Value = "25" },
            new() { Path = "Status", Operator = Operators.Equals, Value = "Active" },
        };

        _sut.List_PopByConditions(json, conditions, LogicalOperators.AND, false, out _, out var popped);

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
        var conditions = new List<Condition> {
            new() { Path = "Status", Operator = Operators.Equals, Value = "Active" },
            new() { Path = "Score", Operator = Operators.GreaterThan, Value = "80" },
        };

        _sut.List_PopMultipleByConditions(json, conditions, LogicalOperators.OR, out _, out var popped);

        var arr = JsonNode.Parse(popped)!.AsArray();
        Assert.Equal(2, arr.Count);
    }

    [Fact]
    public void PopByConditions_AND_DefaultMode()
    {
        string json = """[{"A":"x","B":"y"},{"A":"x","B":"z"}]""";
        var conditions = new List<Condition> {
            new() { Path = "A", Operator = Operators.Equals, Value = "x" },
            new() { Path = "B", Operator = Operators.Equals, Value = "z" },
        };

        // Empty LogicalOperator defaults to AND.
        _sut.List_PopByConditions(json, conditions, "", false, out _, out var popped);

        var poppedObj = JsonNode.Parse(popped)!.AsObject();
        Assert.Equal("z", poppedObj["B"]!.ToString());
    }

    [Fact]
    public void PopByConditions_EmptyConditions_ReturnsOriginal()
    {
        string json = """[{"Id":1}]""";

        _sut.List_PopByConditions(json, new List<Condition>(), LogicalOperators.AND, false, out var updated, out var popped);

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
        var conditions = new List<Condition> {
            new() { Path = "Category", Operator = Operators.Equals, Value = "Books" },
            new() { Path = "Price", Operator = Operators.GreaterOrEqual, Value = "20" },
            new() { Path = "InStock", Operator = Operators.Equals, Value = "true" },
        };

        _sut.List_PopMultipleByConditions(json, conditions, LogicalOperators.AND, out _, out var popped);

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
        var conditions = new List<Condition> {
            new() { Path = "Meta.Region", Operator = Operators.Equals, Value = "EU" },
            new() { Path = "Meta.Priority", Operator = Operators.Equals, Value = "High" },
        };

        _sut.List_PopByConditions(json, conditions, LogicalOperators.AND, false, out _, out var popped);

        var poppedObj = JsonNode.Parse(popped)!.AsObject();
        Assert.Equal("1", poppedObj["Id"]!.ToString());
    }

    [Fact]
    public void PopMultipleByConditions_CaseSensitivePerCondition()
    {
        string json = """[{"Tag":"URGENT"},{"Tag":"urgent"},{"Tag":"low"}]""";
        var conditions = new List<Condition> {
            new() { Path = "Tag", Operator = Operators.Equals, Value = "URGENT", CaseSensitive = true },
        };

        _sut.List_PopMultipleByConditions(json, conditions, LogicalOperators.AND, out _, out var popped);

        var arr = JsonNode.Parse(popped)!.AsArray();
        Assert.Single(arr);
        Assert.Equal("URGENT", arr[0]!["Tag"]!.ToString());
    }

    [Fact]
    public void PopByConditions_NullSource_ReturnsDefaults()
    {
        var conditions = new List<Condition> { new() { Path = "X", Operator = Operators.Equals, Value = "y" } };

        _sut.List_PopByConditions(null!, conditions, LogicalOperators.AND, false, out var updated, out var popped);

        Assert.Equal("[]", updated);
        Assert.Equal("{}", popped);
    }

    [Fact]
    public void PopMultipleByConditions_NoMatch_ReturnsEmpty()
    {
        string json = """[{"X":"a"},{"X":"b"}]""";
        var conditions = new List<Condition> { new() { Path = "X", Operator = Operators.Equals, Value = "z" } };

        _sut.List_PopMultipleByConditions(json, conditions, LogicalOperators.AND, out var updated, out var popped);

        Assert.Equal("[]", popped);
        var arr = JsonNode.Parse(updated)!.AsArray();
        Assert.Equal(2, arr.Count);
    }
}
