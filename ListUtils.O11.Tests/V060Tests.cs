using System.Text.Json.Nodes;

namespace ListUtils.Tests;

public class V060Tests
{
    private readonly ListUtils _sut = new();

    // ── List_GroupByMultiple ─────────────────────────────────────────────────

    [Fact]
    public void GroupByMultiple_TwoKeys_BucketsByCompositeKey()
    {
        string json = """
            [
                {"Region":"EU","Cat":"Books","Id":1},
                {"Region":"EU","Cat":"Toys","Id":2},
                {"Region":"EU","Cat":"Books","Id":3},
                {"Region":"US","Cat":"Books","Id":4}
            ]
            """;
        var paths = new List<string> { "Region", "Cat" };
        var names = new List<string> { "Region", "Category" };

        _sut.List_GroupByMultiple(json, paths, names, "Items", false, out var result);

        var arr = JsonNode.Parse(result)!.AsArray();
        Assert.Equal(3, arr.Count);

        var euBooks = arr[0]!.AsObject();
        Assert.Equal("EU", euBooks["Region"]!.ToString());
        Assert.Equal("Books", euBooks["Category"]!.ToString());
        Assert.Equal(2, euBooks["Items"]!.AsArray().Count);

        var euToys = arr[1]!.AsObject();
        Assert.Equal("EU", euToys["Region"]!.ToString());
        Assert.Equal("Toys", euToys["Category"]!.ToString());
        Assert.Single(euToys["Items"]!.AsArray());
    }

    [Fact]
    public void GroupByMultiple_ThreeKeys_WithNestedPaths()
    {
        string json = """
            [
                {"Meta":{"Region":"EU"},"Cat":"A","Priority":"High"},
                {"Meta":{"Region":"EU"},"Cat":"A","Priority":"Low"},
                {"Meta":{"Region":"EU"},"Cat":"A","Priority":"High"}
            ]
            """;
        var paths = new List<string> { "Meta.Region", "Cat", "Priority" };
        var names = new List<string> { "R", "C", "P" };

        _sut.List_GroupByMultiple(json, paths, names, "Rows", false, out var result);

        var arr = JsonNode.Parse(result)!.AsArray();
        Assert.Equal(2, arr.Count);
        Assert.Equal("High", arr[0]!["P"]!.ToString());
        Assert.Equal(2, arr[0]!["Rows"]!.AsArray().Count);
        Assert.Equal("Low", arr[1]!["P"]!.ToString());
        Assert.Single(arr[1]!["Rows"]!.AsArray());
    }

    [Fact]
    public void GroupByMultiple_MissingKey_UsesUnknownBucket()
    {
        string json = """[{"A":"x"},{"B":"y"},{"A":"x","B":"z"}]""";
        var paths = new List<string> { "A", "B" };

        _sut.List_GroupByMultiple(json, paths, null!, "", false, out var result);

        var arr = JsonNode.Parse(result)!.AsArray();
        Assert.Equal(3, arr.Count);
        // Default field names Key0, Key1 when KeyNames is null; default Items field.
        Assert.Contains("Key0", (string)arr[0]!.ToJsonString()!);
        Assert.Contains("Items", (string)arr[0]!.ToJsonString()!);
        // Missing-key rows land under "Unknown".
        Assert.Contains(arr.Select(g => g!["Key0"]!.ToString() + "|" + g["Key1"]!.ToString()),
            k => k == "Unknown|y");
    }

    [Fact]
    public void GroupByMultiple_EmptySource_ReturnsEmptyArray()
    {
        _sut.List_GroupByMultiple("", new List<string> { "A" }, null!, "", false, out var result);
        Assert.Equal("[]", result);
    }

    [Fact]
    public void GroupByMultiple_EmptyPaths_ReturnsEmptyArray()
    {
        _sut.List_GroupByMultiple("""[{"A":1}]""", new List<string>(), null!, "", false, out var result);
        Assert.Equal("[]", result);
    }

    [Fact]
    public void GroupByMultiple_CaseSensitive_FlagRespected()
    {
        string json = """[{"K":"a"},{"K":"A"},{"K":"a"}]""";
        var paths = new List<string> { "K" };

        _sut.List_GroupByMultiple(json, paths, null!, "Items", true, out var strict);
        _sut.List_GroupByMultiple(json, paths, null!, "Items", false, out var loose);

        Assert.Equal(2, JsonNode.Parse(strict)!.AsArray().Count);
        Assert.Single(JsonNode.Parse(loose)!.AsArray());
    }

    // ── List_ZipGroupByMultiple ──────────────────────────────────────────────

    [Fact]
    public void ZipGroupByMultiple_TwoKeys_CogroupsAcrossTwoLists()
    {
        string listA = """
            [
                {"Region":"EU","Cat":"Books","OrderId":1},
                {"Region":"EU","Cat":"Toys","OrderId":2},
                {"Region":"US","Cat":"Books","OrderId":3}
            ]
            """;
        string listB = """
            [
                {"Region":"EU","Cat":"Books","PayId":"P1"},
                {"Region":"EU","Cat":"Books","PayId":"P2"},
                {"Region":"US","Cat":"Books","PayId":"P3"}
            ]
            """;
        var keysA = new List<string> { "Region", "Cat" };
        var keysB = new List<string> { "Region", "Cat" };
        var names = new List<string> { "Region", "Category" };

        _sut.List_ZipGroupByMultiple(listA, listB, keysA, keysB, names, "Orders", "Payments", false, out var result);

        var arr = JsonNode.Parse(result)!.AsArray();
        Assert.Equal(3, arr.Count);

        var euBooks = arr[0]!.AsObject();
        Assert.Equal("EU", euBooks["Region"]!.ToString());
        Assert.Equal("Books", euBooks["Category"]!.ToString());
        Assert.Single(euBooks["Orders"]!.AsArray());
        Assert.Equal(2, euBooks["Payments"]!.AsArray().Count);

        var euToys = arr[1]!.AsObject();
        Assert.Single(euToys["Orders"]!.AsArray());
        Assert.Empty(euToys["Payments"]!.AsArray());
    }

    [Fact]
    public void ZipGroupByMultiple_DifferentPathsPerList_StillPair()
    {
        // A stores city under "City", B stores it under "Loc.Town" — same conceptual key.
        string listA = """[{"City":"NYC","OrderId":1},{"City":"LON","OrderId":2}]""";
        string listB = """[{"Loc":{"Town":"NYC"},"PayId":"P1"},{"Loc":{"Town":"NYC"},"PayId":"P2"}]""";
        var keysA = new List<string> { "City" };
        var keysB = new List<string> { "Loc.Town" };

        _sut.List_ZipGroupByMultiple(listA, listB, keysA, keysB, new List<string> { "City" }, "A", "B", false, out var result);

        var arr = JsonNode.Parse(result)!.AsArray();
        Assert.Equal(2, arr.Count);
        var nyc = arr[0]!.AsObject();
        Assert.Equal("NYC", nyc["City"]!.ToString());
        Assert.Single(nyc["A"]!.AsArray());
        Assert.Equal(2, nyc["B"]!.AsArray().Count);
    }

    [Fact]
    public void ZipGroupByMultiple_BothListsEmpty_ReturnsEmptyArray()
    {
        _sut.List_ZipGroupByMultiple("", "", new List<string> { "K" }, new List<string> { "K" }, null!, "A", "B", false, out var result);
        Assert.Equal("[]", result);
    }

    [Fact]
    public void ZipGroupByMultiple_MissingKeyInList_FallsIntoUnknown()
    {
        string listA = """[{"K":"a"},{"NoKey":true}]""";
        string listB = """[{"K":"a"}]""";
        var keys = new List<string> { "K" };

        _sut.List_ZipGroupByMultiple(listA, listB, keys, keys, null!, "A", "B", false, out var result);

        var arr = JsonNode.Parse(result)!.AsArray();
        Assert.Equal(2, arr.Count);
        Assert.Contains(arr.Select(g => g!["Key0"]!.ToString()), k => k == "Unknown");
    }

    // ── List_ZipManyGroupByMultiple ──────────────────────────────────────────

    [Fact]
    public void ZipManyGroupByMultiple_ThreeListsTwoKeys_CogroupsCorrectly()
    {
        var lists = new List<string> {
            """[{"Region":"EU","Cat":"Books","OrderId":1},{"Region":"US","Cat":"Books","OrderId":2}]""",
            """[{"Region":"EU","Cat":"Books","PayId":"P1"},{"Region":"EU","Cat":"Books","PayId":"P2"}]""",
            """[{"Region":"US","Cat":"Toys","RetId":"R1"}]"""
        };
        // 3 lists × 2 keys, list-major: [L0K0, L0K1, L1K0, L1K1, L2K0, L2K1]
        var paths = new List<string> { "Region", "Cat", "Region", "Cat", "Region", "Cat" };
        var keyNames = new List<string> { "Region", "Category" };
        var itemNames = new List<string> { "Orders", "Payments", "Returns" };

        _sut.List_ZipManyGroupByMultiple(lists, 2, paths, keyNames, itemNames, false, out var result);

        var arr = JsonNode.Parse(result)!.AsArray();
        Assert.Equal(3, arr.Count);

        var euBooks = arr[0]!.AsObject();
        Assert.Equal("EU", euBooks["Region"]!.ToString());
        Assert.Equal("Books", euBooks["Category"]!.ToString());
        Assert.Single(euBooks["Orders"]!.AsArray());
        Assert.Equal(2, euBooks["Payments"]!.AsArray().Count);
        Assert.Empty(euBooks["Returns"]!.AsArray());

        var usToys = arr[2]!.AsObject();
        Assert.Equal("US", usToys["Region"]!.ToString());
        Assert.Equal("Toys", usToys["Category"]!.ToString());
        Assert.Single(usToys["Returns"]!.AsArray());
    }

    [Fact]
    public void ZipManyGroupByMultiple_EmptyLists_ReturnsEmptyArray()
    {
        _sut.List_ZipManyGroupByMultiple(new List<string>(), 2, new List<string> { "A", "B" }, null!, null!, false, out var result);
        Assert.Equal("[]", result);
    }

    [Fact]
    public void ZipManyGroupByMultiple_KeyCountZero_ReturnsEmptyArray()
    {
        var lists = new List<string> { """[{"A":1}]""" };
        _sut.List_ZipManyGroupByMultiple(lists, 0, new List<string>(), null!, null!, false, out var result);
        Assert.Equal("[]", result);
    }

    [Fact]
    public void ZipManyGroupByMultiple_MissingKeyPaths_FillsWithUnknown()
    {
        var lists = new List<string> {
            """[{"A":"x","B":"y"}]""",
            """[{"A":"x"}]"""    // missing B path — falls back to Unknown for second key
        };
        // Only 3 paths provided for 2 lists × 2 keys — trailing gap fills Unknown.
        var paths = new List<string> { "A", "B", "A" };

        _sut.List_ZipManyGroupByMultiple(lists, 2, paths, new List<string> { "A", "B" }, new List<string> { "L0", "L1" }, false, out var result);

        var arr = JsonNode.Parse(result)!.AsArray();
        Assert.Equal(2, arr.Count);
        Assert.Contains(arr.Select(g => g!["B"]!.ToString()), b => b == "Unknown");
    }

    [Fact]
    public void ZipManyGroupByMultiple_CaseInsensitiveByDefault()
    {
        var lists = new List<string> {
            """[{"K":"abc"}]""",
            """[{"K":"ABC"}]"""
        };
        var paths = new List<string> { "K", "K" };

        _sut.List_ZipManyGroupByMultiple(lists, 1, paths, null!, null!, false, out var loose);
        _sut.List_ZipManyGroupByMultiple(lists, 1, paths, null!, null!, true, out var strict);

        Assert.Single(JsonNode.Parse(loose)!.AsArray());
        Assert.Equal(2, JsonNode.Parse(strict)!.AsArray().Count);
    }

    // ── Cross-cutting: composite key correctness ─────────────────────────────

    [Fact]
    public void GroupByMultiple_KeysWithSeparatorInValues_DoNotCollide()
    {
        // Ensure the internal composite key separator can't be forged from user values.
        // "a|" + "b" and "a" + "|b" would collide under naive '|' joining but not under \u001F.
        string json = """[{"X":"a|","Y":"b"},{"X":"a","Y":"|b"}]""";
        var paths = new List<string> { "X", "Y" };

        _sut.List_GroupByMultiple(json, paths, null!, "Items", false, out var result);

        var arr = JsonNode.Parse(result)!.AsArray();
        Assert.Equal(2, arr.Count);
    }
}
