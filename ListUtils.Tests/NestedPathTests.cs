using System.Text.Json.Nodes;

namespace ListUtils.Tests;

public class NestedPathTests
{
    private readonly ListUtils _sut = new();

    [Fact]
    public void PopByCondition_NestedPath_MatchesInnerProperty()
    {
        string json = """
            [
                {"Id":1,"Address":{"City":"NYC","Country":"US"}},
                {"Id":2,"Address":{"City":"LA","Country":"US"}},
                {"Id":3,"Address":{"City":"LDN","Country":"UK"}}
            ]
            """;

        _sut.List_PopByCondition(json, "Address.City", "LA", "", false, out var updated, out var popped);

        var poppedObj = JsonNode.Parse(popped)!.AsObject();
        Assert.Equal("2", poppedObj["Id"]!.ToString());
        var updatedArr = JsonNode.Parse(updated)!.AsArray();
        Assert.Equal(2, updatedArr.Count);
    }

    [Fact]
    public void PopMultipleByCondition_NestedPath_MatchesAll()
    {
        string json = """
            [
                {"Order":1,"Meta":{"Status":"Paid","Priority":"High"}},
                {"Order":2,"Meta":{"Status":"Pending","Priority":"Low"}},
                {"Order":3,"Meta":{"Status":"Paid","Priority":"Medium"}}
            ]
            """;

        _sut.List_PopMultipleByCondition(json, "Meta.Status", "Paid", "", false, out _, out var popped);

        var poppedArr = JsonNode.Parse(popped)!.AsArray();
        Assert.Equal(2, poppedArr.Count);
    }

    [Fact]
    public void GroupBy_NestedPath_GroupsByInnerProperty()
    {
        string json = """
            [
                {"Id":1,"Customer":{"Country":"US","Name":"Alice"}},
                {"Id":2,"Customer":{"Country":"UK","Name":"Bob"}},
                {"Id":3,"Customer":{"Country":"US","Name":"Carol"}}
            ]
            """;

        _sut.List_GroupBy(json, "Customer.Country", out var grouped);

        var arr = JsonNode.Parse(grouped)!.AsArray();
        Assert.Equal(2, arr.Count);
        Assert.Equal("US", arr[0]!["Key"]!.ToString());
        Assert.Equal(2, arr[0]!["Items"]!.AsArray().Count);
    }

    [Fact]
    public void Difference_NestedPath_MatchesOnDeepKey()
    {
        string listA = """
            [
                {"Id":1,"Ref":{"Code":"A"}},
                {"Id":2,"Ref":{"Code":"B"}},
                {"Id":3,"Ref":{"Code":"C"}}
            ]
            """;
        string listB = """
            [
                {"Id":99,"Ref":{"Code":"B"}}
            ]
            """;

        _sut.List_Difference(listA, listB, "Ref.Code", "", false, out var diff);

        var arr = JsonNode.Parse(diff)!.AsArray();
        Assert.Equal(2, arr.Count);
        Assert.Equal("A", arr[0]!["Ref"]!["Code"]!.ToString());
        Assert.Equal("C", arr[1]!["Ref"]!["Code"]!.ToString());
    }

    [Fact]
    public void PopByCondition_ThreeLevelNestedPath_Matches()
    {
        string json = """
            [
                {"Id":1,"Wrapper":{"Data":{"Value":"target"}}},
                {"Id":2,"Wrapper":{"Data":{"Value":"other"}}}
            ]
            """;

        _sut.List_PopByCondition(json, "Wrapper.Data.Value", "target", "", false, out _, out var popped);

        var poppedObj = JsonNode.Parse(popped)!.AsObject();
        Assert.Equal("1", poppedObj["Id"]!.ToString());
    }

    [Fact]
    public void PopByCondition_NestedPath_MissingSegment_NoMatch()
    {
        string json = """[{"Id":1,"Address":{"City":"NYC"}}]""";

        _sut.List_PopByCondition(json, "Address.State", "NY", "", false, out var updated, out var popped);

        Assert.Equal("{}", popped);
        Assert.Equal(json, updated);
    }

    [Fact]
    public void PopByCondition_NestedPath_WithOperator_ContainsMatch()
    {
        string json = """
            [
                {"Item":1,"Details":{"Description":"Red widget"}},
                {"Item":2,"Details":{"Description":"Blue gadget"}}
            ]
            """;

        _sut.List_PopByCondition(json, "Details.Description", "widget", "Contains", false, out _, out var popped);

        var poppedObj = JsonNode.Parse(popped)!.AsObject();
        Assert.Equal("1", poppedObj["Item"]!.ToString());
    }

    [Fact]
    public void PopByCondition_NestedPath_CamelCaseFallback()
    {
        string json = """[{"Id":1,"address":{"city":"NYC"}}]""";

        _sut.List_PopByCondition(json, "Address.City", "NYC", "", false, out _, out var popped);

        var poppedObj = JsonNode.Parse(popped)!.AsObject();
        Assert.Equal("1", poppedObj["Id"]!.ToString());
    }
}
