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

        _sut.List_PopByCondition(json, "Id", "1", "", false, false, out var updated, out var popped);

        var poppedObj = JsonNode.Parse(popped)!.AsObject();
        Assert.Equal("Alice", poppedObj["Name"]!.ToString());

        var updatedArr = JsonNode.Parse(updated)!.AsArray();
        Assert.Equal(2, updatedArr.Count);
    }

    [Fact]
    public void List_PopByCondition_NoMatch_ReturnsOriginal()
    {
        string json = """[{"Id":"1"},{"Id":"2"}]""";

        _sut.List_PopByCondition(json, "Id", "99", "", false, false, out var updated, out var popped);

        Assert.Equal("{}", popped);
        var updatedArr = JsonNode.Parse(updated)!.AsArray();
        Assert.Equal(2, updatedArr.Count);
    }

    [Fact]
    public void List_PopByCondition_NullInput_ReturnsDefaults()
    {
        _sut.List_PopByCondition(null!, "Id", "1", "", false, false, out var updated, out var popped);

        Assert.Equal("[]", updated);
        Assert.Equal("{}", popped);
    }

    [Fact]
    public void List_PopByCondition_CamelCaseFallback_MatchesProperty()
    {
        string json = """[{"isActive":"true","name":"Item1"}]""";

        _sut.List_PopByCondition(json, "IsActive", "true", "", false, false, out var updated, out var popped);

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

    #region ZipGroupBy

    [Fact]
    public void List_ZipGroupBy_TwoListsSharedKey_GroupsBothSides()
    {
        string orders = """[{"CustomerId":"1","OrderId":101},{"CustomerId":"2","OrderId":102},{"CustomerId":"1","OrderId":103}]""";
        string payments = """[{"CustomerId":"1","PaymentId":201},{"CustomerId":"2","PaymentId":202},{"CustomerId":"2","PaymentId":203}]""";

        _sut.List_ZipGroupBy(orders, payments, "CustomerId", "CustomerId", "Orders", "Payments", false, out var grouped);

        var arr = JsonNode.Parse(grouped)!.AsArray();
        Assert.Equal(2, arr.Count);

        var g1 = arr[0]!.AsObject();
        Assert.Equal("1", g1["Key"]!.ToString());
        Assert.Equal(2, g1["Orders"]!.AsArray().Count);
        Assert.Single(g1["Payments"]!.AsArray());

        var g2 = arr[1]!.AsObject();
        Assert.Equal("2", g2["Key"]!.ToString());
        Assert.Single(g2["Orders"]!.AsArray());
        Assert.Equal(2, g2["Payments"]!.AsArray().Count);
    }

    [Fact]
    public void List_ZipGroupBy_KeyOnlyInListA_ListBArrayIsEmpty()
    {
        string a = """[{"K":"x","V":1}]""";
        string b = """[{"K":"y","V":2}]""";

        _sut.List_ZipGroupBy(a, b, "K", "K", "A", "B", false, out var grouped);

        var arr = JsonNode.Parse(grouped)!.AsArray();
        Assert.Equal(2, arr.Count);
        // Key "x" comes from A → its B array is empty.
        Assert.Equal("x", arr[0]!["Key"]!.ToString());
        Assert.Single(arr[0]!["A"]!.AsArray());
        Assert.Empty(arr[0]!["B"]!.AsArray());
        // Key "y" only exists in B → its A array is empty.
        Assert.Equal("y", arr[1]!["Key"]!.ToString());
        Assert.Empty(arr[1]!["A"]!.AsArray());
        Assert.Single(arr[1]!["B"]!.AsArray());
    }

    [Fact]
    public void List_ZipGroupBy_DifferentKeyPropertiesPerSide()
    {
        // Common scenario: ListA uses "CustomerId", ListB uses "customer_id".
        string a = """[{"CustomerId":"42","OrderId":1}]""";
        string b = """[{"customer_id":"42","Method":"Card"}]""";

        _sut.List_ZipGroupBy(a, b, "CustomerId", "customer_id", "Orders", "Payments", false, out var grouped);

        var arr = JsonNode.Parse(grouped)!.AsArray();
        Assert.Single(arr);
        Assert.Equal("42", arr[0]!["Key"]!.ToString());
        Assert.Single(arr[0]!["Orders"]!.AsArray());
        Assert.Single(arr[0]!["Payments"]!.AsArray());
    }

    [Fact]
    public void List_ZipGroupBy_NestedKeyPath_Works()
    {
        string a = """[{"Meta":{"CustomerId":"1"},"V":"a"}]""";
        string b = """[{"Meta":{"CustomerId":"1"},"V":"b"}]""";

        _sut.List_ZipGroupBy(a, b, "Meta.CustomerId", "Meta.CustomerId", "L1", "L2", false, out var grouped);

        var arr = JsonNode.Parse(grouped)!.AsArray();
        Assert.Single(arr);
        Assert.Equal("1", arr[0]!["Key"]!.ToString());
    }

    [Fact]
    public void List_ZipGroupBy_CaseInsensitiveByDefault()
    {
        string a = """[{"K":"ABC"},{"K":"abc"}]""";
        string b = """[{"K":"AbC"}]""";

        _sut.List_ZipGroupBy(a, b, "K", "K", "A", "B", false, out var grouped);

        var arr = JsonNode.Parse(grouped)!.AsArray();
        // "ABC", "abc", "AbC" all collapse into one bucket keyed by the first-seen "ABC".
        Assert.Single(arr);
        Assert.Equal("ABC", arr[0]!["Key"]!.ToString());
        Assert.Equal(2, arr[0]!["A"]!.AsArray().Count);
        Assert.Single(arr[0]!["B"]!.AsArray());
    }

    [Fact]
    public void List_ZipGroupBy_CaseSensitiveKeepsSeparate()
    {
        string a = """[{"K":"X"}]""";
        string b = """[{"K":"x"}]""";

        _sut.List_ZipGroupBy(a, b, "K", "K", "A", "B", true, out var grouped);

        var arr = JsonNode.Parse(grouped)!.AsArray();
        Assert.Equal(2, arr.Count);
    }

    [Fact]
    public void List_ZipGroupBy_MissingKeyGoesToUnknownBucket()
    {
        string a = """[{"NoK":"1"}]""";
        string b = """[{"K":"real"},{"NotK":"x"}]""";

        _sut.List_ZipGroupBy(a, b, "K", "K", "A", "B", false, out var grouped);

        var arr = JsonNode.Parse(grouped)!.AsArray();
        // Union order: "Unknown" from A → then "real" from B.
        Assert.Equal(2, arr.Count);
        Assert.Equal("Unknown", arr[0]!["Key"]!.ToString());
        // Unknown bucket contains the A item and the B item without the key.
        Assert.Single(arr[0]!["A"]!.AsArray());
        Assert.Single(arr[0]!["B"]!.AsArray());
        Assert.Equal("real", arr[1]!["Key"]!.ToString());
    }

    [Fact]
    public void List_ZipGroupBy_EmptyLists_ReturnsEmpty()
    {
        _sut.List_ZipGroupBy("", "", "K", "K", "A", "B", false, out var bothEmpty);
        _sut.List_ZipGroupBy("[]", "[]", "K", "K", "A", "B", false, out var bothEmptyArr);

        Assert.Equal("[]", bothEmpty);
        Assert.Equal("[]", bothEmptyArr);
    }

    [Fact]
    public void List_ZipGroupBy_EmptyKeyNames_FallBackToDefaults()
    {
        string a = """[{"K":"1"}]""";
        string b = """[{"K":"1"}]""";

        _sut.List_ZipGroupBy(a, b, "K", "K", "", "", false, out var grouped);

        var arr = JsonNode.Parse(grouped)!.AsArray();
        Assert.Single(arr);
        Assert.NotNull(arr[0]!["ItemsA"]);
        Assert.NotNull(arr[0]!["ItemsB"]);
    }

    [Fact]
    public void List_ZipGroupBy_PreservesFirstSeenOrder()
    {
        // A order: "b", "c". B order: "a", "b", "d". Expected union order:
        // b (A), c (A), a (B, new), d (B, new).
        string a = """[{"K":"b"},{"K":"c"}]""";
        string b = """[{"K":"a"},{"K":"b"},{"K":"d"}]""";

        _sut.List_ZipGroupBy(a, b, "K", "K", "A", "B", false, out var grouped);

        var arr = JsonNode.Parse(grouped)!.AsArray();
        Assert.Equal(4, arr.Count);
        Assert.Equal("b", arr[0]!["Key"]!.ToString());
        Assert.Equal("c", arr[1]!["Key"]!.ToString());
        Assert.Equal("a", arr[2]!["Key"]!.ToString());
        Assert.Equal("d", arr[3]!["Key"]!.ToString());
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
