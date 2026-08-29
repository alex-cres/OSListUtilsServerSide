using System.Text.Json.Nodes;

namespace ListUtils.Tests;

public class ComplexStructureTests
{
    private readonly ListUtils _sut = new();

    #region Nested objects

    [Fact]
    public void List_Pop_NestedObjects_PreservesInnerStructure()
    {
        string json = """
            [
                {"Id":1,"Address":{"Street":"Main St","City":"NYC","Zip":"10001"}},
                {"Id":2,"Address":{"Street":"Oak Ave","City":"LA","Zip":"90001"}},
                {"Id":3,"Address":{"Street":"Elm Rd","City":"CHI","Zip":"60601"}}
            ]
            """;

        _sut.List_Pop(json, 1, out var updated, out var popped);

        var poppedObj = JsonNode.Parse(popped)!.AsObject();
        Assert.Equal("Oak Ave", poppedObj["Address"]!["Street"]!.ToString());
        Assert.Equal("LA", poppedObj["Address"]!["City"]!.ToString());

        var arr = JsonNode.Parse(updated)!.AsArray();
        Assert.Equal(2, arr.Count);
        Assert.Equal("Main St", arr[0]!["Address"]!["Street"]!.ToString());
    }

    [Fact]
    public void List_PopByCondition_NestedPropertyNotSearched_MatchesTopLevelOnly()
    {
        string json = """
            [
                {"Id":"1","Meta":{"Status":"Active","Priority":"High"}},
                {"Id":"2","Meta":{"Status":"Inactive","Priority":"Low"}}
            ]
            """;

        _sut.List_PopByCondition(json, "Id", "2", "", false, out var updated, out var popped);

        var poppedObj = JsonNode.Parse(popped)!.AsObject();
        Assert.Equal("Inactive", poppedObj["Meta"]!["Status"]!.ToString());
        Assert.Equal("Low", poppedObj["Meta"]!["Priority"]!.ToString());
    }

    [Fact]
    public void List_GroupBy_ObjectsWithNestedArrays_PreservesArrays()
    {
        string json = """
            [
                {"Category":"A","Tags":["urgent","review"],"Score":95},
                {"Category":"B","Tags":["low"],"Score":40},
                {"Category":"A","Tags":["done"],"Score":80}
            ]
            """;

        _sut.List_GroupBy(json, "Category", out var grouped);

        var arr = JsonNode.Parse(grouped)!.AsArray();
        Assert.Equal(2, arr.Count);

        var groupA = arr[0]!["Items"]!.AsArray();
        Assert.Equal(2, groupA.Count);
        var tags = groupA[0]!["Tags"]!.AsArray();
        Assert.Equal(2, tags.Count);
        Assert.Equal("urgent", tags[0]!.ToString());
    }

    #endregion

    #region Deeply nested structures

    [Fact]
    public void List_Zip_DeepNesting_PreservesAllLevels()
    {
        string listA = """
            [
                {"User":{"Name":"Alice","Roles":["Admin","Editor"],"Prefs":{"Theme":"Dark","Lang":"EN"}}},
                {"User":{"Name":"Bob","Roles":["Viewer"],"Prefs":{"Theme":"Light","Lang":"PT"}}}
            ]
            """;
        string listB = """
            [
                {"Metrics":{"Logins":42,"LastSeen":"2024-01-15","Devices":["Desktop","Mobile"]}},
                {"Metrics":{"Logins":7,"LastSeen":"2024-03-01","Devices":["Tablet"]}}
            ]
            """;

        _sut.List_Zip(listA, listB, "Profile", "Activity", out var zipped);

        var arr = JsonNode.Parse(zipped)!.AsArray();
        Assert.Equal(2, arr.Count);

        var first = arr[0]!;
        Assert.Equal("Alice", first["Profile"]!["User"]!["Name"]!.ToString());
        Assert.Equal("Admin", first["Profile"]!["User"]!["Roles"]!.AsArray()[0]!.ToString());
        Assert.Equal("Dark", first["Profile"]!["User"]!["Prefs"]!["Theme"]!.ToString());
        Assert.Equal("42", first["Activity"]!["Metrics"]!["Logins"]!.ToString());
        Assert.Equal("Desktop", first["Activity"]!["Metrics"]!["Devices"]!.AsArray()[0]!.ToString());
    }

    [Fact]
    public void List_Difference_ComplexObjects_MatchesOnTopLevelKey()
    {
        string listA = """
            [
                {"OrderId":"O1","Items":[{"Sku":"A1","Qty":2},{"Sku":"A2","Qty":1}],"Total":150.50},
                {"OrderId":"O2","Items":[{"Sku":"B1","Qty":5}],"Total":300.00},
                {"OrderId":"O3","Items":[{"Sku":"C1","Qty":1}],"Total":25.99}
            ]
            """;
        string listB = """
            [
                {"OrderId":"O2","Cancelled":true}
            ]
            """;

        _sut.List_Difference(listA, listB, "OrderId", "", false, out var diff);

        var arr = JsonNode.Parse(diff)!.AsArray();
        Assert.Equal(2, arr.Count);
        Assert.Equal("O1", arr[0]!["OrderId"]!.ToString());
        Assert.Equal(2, arr[0]!["Items"]!.AsArray().Count);
        Assert.Equal("O3", arr[1]!["OrderId"]!.ToString());
    }

    #endregion

    #region Mixed types in arrays

    [Fact]
    public void List_Pop_MixedValueTypes_HandlesNumbersBoolsNulls()
    {
        string json = """[1, "hello", true, null, 3.14, {"Key":"Val"}]""";

        _sut.List_Pop(json, 2, out var updated, out var popped);

        Assert.Equal("true", popped);
        var arr = JsonNode.Parse(updated)!.AsArray();
        Assert.Equal(5, arr.Count);
    }

    [Fact]
    public void List_PopMultiple_MixedTypes_PopsCorrectElements()
    {
        string json = """[100, "text", false, [1,2,3], {"nested":true}]""";

        _sut.List_PopMultiple(json, "1,3", out var updated, out var popped);

        var poppedArr = JsonNode.Parse(popped)!.AsArray();
        Assert.Equal(2, poppedArr.Count);
        Assert.Equal("text", poppedArr[0]!.ToString());
        Assert.Equal(3, poppedArr[1]!.AsArray().Count);

        var updatedArr = JsonNode.Parse(updated)!.AsArray();
        Assert.Equal(3, updatedArr.Count);
    }

    #endregion

    #region Large structures

    [Fact]
    public void List_PopMultipleByCondition_ManyFields_AllPreserved()
    {
        string json = """
            [
                {"Id":"1","Name":"Widget","Price":9.99,"InStock":true,"Tags":["sale","new"],"Supplier":{"Name":"Acme","Country":"US"},"Ratings":[5,4,5,3]},
                {"Id":"2","Name":"Gadget","Price":19.99,"InStock":false,"Tags":["clearance"],"Supplier":{"Name":"Beta","Country":"UK"},"Ratings":[3,2]},
                {"Id":"3","Name":"Doohickey","Price":4.99,"InStock":true,"Tags":["new"],"Supplier":{"Name":"Acme","Country":"US"},"Ratings":[4,4,4]}
            ]
            """;

        _sut.List_PopMultipleByCondition(json, "InStock", "true", "", false, out var updated, out var popped);

        var poppedArr = JsonNode.Parse(popped)!.AsArray();
        Assert.Equal(2, poppedArr.Count);

        // Verify all fields preserved on complex popped item
        var widget = poppedArr[0]!;
        Assert.Equal("Widget", widget["Name"]!.ToString());
        Assert.Equal("9.99", widget["Price"]!.ToString());
        Assert.Equal(2, widget["Tags"]!.AsArray().Count);
        Assert.Equal("Acme", widget["Supplier"]!["Name"]!.ToString());
        Assert.Equal(4, widget["Ratings"]!.AsArray().Count);

        var updatedArr = JsonNode.Parse(updated)!.AsArray();
        Assert.Single(updatedArr);
        Assert.Equal("Gadget", updatedArr[0]!["Name"]!.ToString());
    }

    [Fact]
    public void List_GroupBy_StructuresWithInnerLists_GroupsCorrectly()
    {
        string json = """
            [
                {"Region":"EU","Customer":{"Id":1,"Name":"Alpha"},"LineItems":[{"Product":"X","Qty":10},{"Product":"Y","Qty":5}]},
                {"Region":"US","Customer":{"Id":2,"Name":"Beta"},"LineItems":[{"Product":"Z","Qty":1}]},
                {"Region":"EU","Customer":{"Id":3,"Name":"Gamma"},"LineItems":[{"Product":"X","Qty":100}]},
                {"Region":"APAC","Customer":{"Id":4,"Name":"Delta"},"LineItems":[]}
            ]
            """;

        _sut.List_GroupBy(json, "Region", out var grouped);

        var groups = JsonNode.Parse(grouped)!.AsArray();
        Assert.Equal(3, groups.Count);

        var euGroup = groups[0]!;
        Assert.Equal("EU", euGroup["Key"]!.ToString());
        var euItems = euGroup["Items"]!.AsArray();
        Assert.Equal(2, euItems.Count);
        Assert.Equal("Alpha", euItems[0]!["Customer"]!["Name"]!.ToString());
        Assert.Equal(2, euItems[0]!["LineItems"]!.AsArray().Count);
    }

    #endregion

    #region Unicode and special characters

    [Fact]
    public void List_PopByCondition_UnicodeValues_MatchesCorrectly()
    {
        string json = """
            [
                {"Lang":"日本語","Text":"こんにちは"},
                {"Lang":"中文","Text":"你好"},
                {"Lang":"العربية","Text":"مرحبا"}
            ]
            """;

        _sut.List_PopByCondition(json, "Lang", "中文", "", false, out var updated, out var popped);

        var poppedObj = JsonNode.Parse(popped)!.AsObject();
        Assert.Equal("你好", poppedObj["Text"]!.ToString());

        var arr = JsonNode.Parse(updated)!.AsArray();
        Assert.Equal(2, arr.Count);
    }

    [Fact]
    public void List_Difference_SpecialCharactersInKeys_WorksCorrectly()
    {
        string listA = """
            [
                {"Email":"alice@example.com","Data":"A"},
                {"Email":"bob+test@domain.co.uk","Data":"B"},
                {"Email":"carol (work)@corp.net","Data":"C"}
            ]
            """;
        string listB = """
            [
                {"Email":"bob+test@domain.co.uk"}
            ]
            """;

        _sut.List_Difference(listA, listB, "Email", "", false, out var diff);

        var arr = JsonNode.Parse(diff)!.AsArray();
        Assert.Equal(2, arr.Count);
        Assert.Equal("alice@example.com", arr[0]!["Email"]!.ToString());
        Assert.Equal("carol (work)@corp.net", arr[1]!["Email"]!.ToString());
    }

    #endregion
}
