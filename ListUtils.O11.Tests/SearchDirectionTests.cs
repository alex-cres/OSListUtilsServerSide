using System.Text.Json.Nodes;

namespace ListUtils.Tests;

public class SearchDirectionTests
{
    private readonly ListUtils _sut = new();

    [Fact]
    public void PopByCondition_SearchFromBeginning_PopsFirstMatch()
    {
        string json = """[{"Id":1,"Status":"Active"},{"Id":2,"Status":"Active"},{"Id":3,"Status":"Active"}]""";

        _sut.List_PopByCondition(json, "Status", "Active", "Equals", false, false, out _, out var popped);

        var poppedObj = JsonNode.Parse(popped)!.AsObject();
        Assert.Equal("1", poppedObj["Id"]!.ToString());
    }

    [Fact]
    public void PopByCondition_SearchFromEnd_PopsLastMatch()
    {
        string json = """[{"Id":1,"Status":"Active"},{"Id":2,"Status":"Active"},{"Id":3,"Status":"Active"}]""";

        _sut.List_PopByCondition(json, "Status", "Active", "Equals", false, true, out _, out var popped);

        var poppedObj = JsonNode.Parse(popped)!.AsObject();
        Assert.Equal("3", poppedObj["Id"]!.ToString());
    }

    [Fact]
    public void PopByCondition_SearchFromEnd_UpdatedListPreservesOrder()
    {
        string json = """[{"Id":1,"Kind":"X"},{"Id":2,"Kind":"Y"},{"Id":3,"Kind":"X"}]""";

        _sut.List_PopByCondition(json, "Kind", "X", "Equals", false, true, out var updated, out _);

        var arr = JsonNode.Parse(updated)!.AsArray();
        Assert.Equal(2, arr.Count);
        Assert.Equal("1", arr[0]!["Id"]!.ToString());
        Assert.Equal("2", arr[1]!["Id"]!.ToString());
    }

    [Fact]
    public void PopByCondition_SearchFromEnd_NoMatch_ReturnsOriginal()
    {
        string json = """[{"Id":1},{"Id":2}]""";

        _sut.List_PopByCondition(json, "Id", "99", "Equals", false, true, out var updated, out var popped);

        Assert.Equal("{}", popped);
        var arr = JsonNode.Parse(updated)!.AsArray();
        Assert.Equal(2, arr.Count);
    }

    [Fact]
    public void PopByCondition_SearchFromEnd_WithOperator_PopsLastGreaterThan()
    {
        string json = """[{"Id":1,"Score":40},{"Id":2,"Score":80},{"Id":3,"Score":90},{"Id":4,"Score":30}]""";

        _sut.List_PopByCondition(json, "Score", "50", "GreaterThan", false, true, out _, out var popped);

        var poppedObj = JsonNode.Parse(popped)!.AsObject();
        Assert.Equal("3", poppedObj["Id"]!.ToString());
    }

    [Fact]
    public void PopByConditions_SearchFromBeginning_PopsFirstMatch()
    {
        string json = """[{"Id":1,"A":"x","B":"y"},{"Id":2,"A":"x","B":"y"},{"Id":3,"A":"x","B":"y"}]""";
        var conditions = new List<Condition> {
            new() { Path = "A", Operator = Operators.Equals, Value = "x" },
            new() { Path = "B", Operator = Operators.Equals, Value = "y" },
        };

        _sut.List_PopByConditions(json, conditions, LogicalOperators.AND, false, out _, out var popped);

        var poppedObj = JsonNode.Parse(popped)!.AsObject();
        Assert.Equal("1", poppedObj["Id"]!.ToString());
    }

    [Fact]
    public void PopByConditions_SearchFromEnd_PopsLastMatch()
    {
        string json = """[{"Id":1,"A":"x","B":"y"},{"Id":2,"A":"x","B":"y"},{"Id":3,"A":"x","B":"y"}]""";
        var conditions = new List<Condition> {
            new() { Path = "A", Operator = Operators.Equals, Value = "x" },
            new() { Path = "B", Operator = Operators.Equals, Value = "y" },
        };

        _sut.List_PopByConditions(json, conditions, LogicalOperators.AND, true, out _, out var popped);

        var poppedObj = JsonNode.Parse(popped)!.AsObject();
        Assert.Equal("3", poppedObj["Id"]!.ToString());
    }

    [Fact]
    public void PopByConditions_SearchFromEnd_OR_PopsLastMatchingEither()
    {
        string json = """
            [
                {"Id":1,"Status":"Active","Score":50},
                {"Id":2,"Status":"Inactive","Score":90},
                {"Id":3,"Status":"Active","Score":30},
                {"Id":4,"Status":"Deleted","Score":10}
            ]
            """;
        var conditions = new List<Condition> {
            new() { Path = "Status", Operator = Operators.Equals, Value = "Active" },
            new() { Path = "Score", Operator = Operators.GreaterThan, Value = "80" },
        };

        _sut.List_PopByConditions(json, conditions, LogicalOperators.OR, true, out _, out var popped);

        var poppedObj = JsonNode.Parse(popped)!.AsObject();
        Assert.Equal("3", poppedObj["Id"]!.ToString());
    }

    [Fact]
    public void PopByCondition_SearchFromEnd_SingleMatch_SameAsFromBeginning()
    {
        string json = """[{"Id":1,"K":"a"},{"Id":2,"K":"b"},{"Id":3,"K":"c"}]""";

        _sut.List_PopByCondition(json, "K", "b", "Equals", false, false, out var updatedStart, out var poppedStart);
        _sut.List_PopByCondition(json, "K", "b", "Equals", false, true, out var updatedEnd, out var poppedEnd);

        Assert.Equal(poppedStart, poppedEnd);
        Assert.Equal(updatedStart, updatedEnd);
    }
}
