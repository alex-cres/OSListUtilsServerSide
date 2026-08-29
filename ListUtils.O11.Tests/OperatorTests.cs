using System.Text.Json.Nodes;

namespace ListUtils.Tests;

public class OperatorTests
{
    private readonly ListUtils _sut = new();

    #region Contains

    [Fact]
    public void PopByCondition_Contains_MatchesSubstring()
    {
        string json = """[{"Name":"Alexander"},{"Name":"Alex"},{"Name":"Bob"}]""";

        _sut.List_PopByCondition(json, "Name", "lex", "Contains", false, out var updated, out var popped);

        var poppedObj = JsonNode.Parse(popped)!.AsObject();
        Assert.Equal("Alexander", poppedObj["Name"]!.ToString());
    }

    [Fact]
    public void PopMultipleByCondition_Contains_MatchesAll()
    {
        string json = """[{"Tag":"urgent-review"},{"Tag":"low"},{"Tag":"urgent-fix"}]""";

        _sut.List_PopMultipleByCondition(json, "Tag", "urgent", "Contains", false, out var updated, out var popped);

        var poppedArr = JsonNode.Parse(popped)!.AsArray();
        Assert.Equal(2, poppedArr.Count);
        var updatedArr = JsonNode.Parse(updated)!.AsArray();
        Assert.Single(updatedArr);
    }

    #endregion

    #region StartsWith / EndsWith

    [Fact]
    public void PopByCondition_StartsWith_MatchesPrefix()
    {
        string json = """[{"Email":"admin@co.com"},{"Email":"user@co.com"},{"Email":"admin@other.com"}]""";

        _sut.List_PopByCondition(json, "Email", "admin", "StartsWith", false, out _, out var popped);

        var poppedObj = JsonNode.Parse(popped)!.AsObject();
        Assert.Equal("admin@co.com", poppedObj["Email"]!.ToString());
    }

    [Fact]
    public void PopMultipleByCondition_EndsWith_MatchesSuffix()
    {
        string json = """[{"File":"report.pdf"},{"File":"image.png"},{"File":"doc.pdf"}]""";

        _sut.List_PopMultipleByCondition(json, "File", ".pdf", "EndsWith", false, out _, out var popped);

        var poppedArr = JsonNode.Parse(popped)!.AsArray();
        Assert.Equal(2, poppedArr.Count);
    }

    #endregion

    #region NotEquals

    [Fact]
    public void PopMultipleByCondition_NotEquals_RemovesNonMatching()
    {
        string json = """[{"Status":"Active"},{"Status":"Inactive"},{"Status":"Active"}]""";

        _sut.List_PopMultipleByCondition(json, "Status", "Active", "NotEquals", false, out var updated, out var popped);

        var poppedArr = JsonNode.Parse(popped)!.AsArray();
        Assert.Single(poppedArr);
        Assert.Equal("Inactive", poppedArr[0]!["Status"]!.ToString());

        var updatedArr = JsonNode.Parse(updated)!.AsArray();
        Assert.Equal(2, updatedArr.Count);
    }

    #endregion

    #region GreaterThan / LessThan

    [Fact]
    public void PopMultipleByCondition_GreaterThan_NumericComparison()
    {
        string json = """[{"Name":"A","Score":95},{"Name":"B","Score":40},{"Name":"C","Score":80},{"Name":"D","Score":60}]""";

        _sut.List_PopMultipleByCondition(json, "Score", "70", "GreaterThan", false, out var updated, out var popped);

        var poppedArr = JsonNode.Parse(popped)!.AsArray();
        Assert.Equal(2, poppedArr.Count);
        Assert.Equal("A", poppedArr[0]!["Name"]!.ToString());
        Assert.Equal("C", poppedArr[1]!["Name"]!.ToString());
    }

    [Fact]
    public void PopByCondition_LessThan_PopsFirst()
    {
        string json = """[{"Price":99.99},{"Price":5.50},{"Price":25.00}]""";

        _sut.List_PopByCondition(json, "Price", "10", "LessThan", false, out _, out var popped);

        var poppedObj = JsonNode.Parse(popped)!.AsObject();
        Assert.True(decimal.Parse(poppedObj["Price"]!.ToString(), System.Globalization.CultureInfo.InvariantCulture) < 10);
    }

    [Fact]
    public void PopMultipleByCondition_GreaterOrEqual_IncludesBoundary()
    {
        string json = """[{"Val":10},{"Val":20},{"Val":30}]""";

        _sut.List_PopMultipleByCondition(json, "Val", "20", "GreaterOrEqual", false, out _, out var popped);

        var poppedArr = JsonNode.Parse(popped)!.AsArray();
        Assert.Equal(2, poppedArr.Count);
    }

    [Fact]
    public void PopMultipleByCondition_LessOrEqual_IncludesBoundary()
    {
        string json = """[{"Val":10},{"Val":20},{"Val":30}]""";

        _sut.List_PopMultipleByCondition(json, "Val", "20", "LessOrEqual", false, out _, out var popped);

        var poppedArr = JsonNode.Parse(popped)!.AsArray();
        Assert.Equal(2, poppedArr.Count);
    }

    #endregion

    #region Numeric with non-numeric values

    [Fact]
    public void PopByCondition_GreaterThan_NonNumericValue_NoMatch()
    {
        string json = """[{"Name":"Alice","Score":"high"},{"Name":"Bob","Score":"low"}]""";

        _sut.List_PopByCondition(json, "Score", "50", "GreaterThan", false, out var updated, out var popped);

        Assert.Equal("{}", popped);
        var updatedArr = JsonNode.Parse(updated)!.AsArray();
        Assert.Equal(2, updatedArr.Count);
    }

    #endregion

    #region Operator symbol aliases

    [Fact]
    public void PopMultipleByCondition_SymbolAlias_GreaterThan()
    {
        string json = """[{"Age":25},{"Age":17},{"Age":30}]""";

        _sut.List_PopMultipleByCondition(json, "Age", "18", ">", false, out _, out var popped);

        var poppedArr = JsonNode.Parse(popped)!.AsArray();
        Assert.Equal(2, poppedArr.Count);
    }

    [Fact]
    public void PopByCondition_SymbolAlias_NotEquals()
    {
        string json = """[{"Type":"A"},{"Type":"B"}]""";

        _sut.List_PopByCondition(json, "Type", "A", "!=", false, out _, out var popped);

        var poppedObj = JsonNode.Parse(popped)!.AsObject();
        Assert.Equal("B", poppedObj["Type"]!.ToString());
    }

    #endregion

    #region Difference with operators

    [Fact]
    public void Difference_Contains_RemovesPartialKeyMatches()
    {
        string listA = """[{"Code":"US-NY"},{"Code":"US-CA"},{"Code":"UK-LDN"}]""";
        string listB = """[{"Code":"US"}]""";

        _sut.List_Difference(listA, listB, "Code", "Contains", false, out var diff);

        var arr = JsonNode.Parse(diff)!.AsArray();
        Assert.Single(arr);
        Assert.Equal("UK-LDN", arr[0]!["Code"]!.ToString());
    }

    [Fact]
    public void Difference_DefaultEquals_BackwardCompatible()
    {
        string listA = """[{"Id":"1"},{"Id":"2"},{"Id":"3"}]""";
        string listB = """[{"Id":"2"}]""";

        _sut.List_Difference(listA, listB, "Id", "", false, out var diff);

        var arr = JsonNode.Parse(diff)!.AsArray();
        Assert.Equal(2, arr.Count);
    }

    #endregion
}
