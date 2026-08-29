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

        _sut.List_PopByCondition(json, "Name", "lex", "Contains", false, false, out var updated, out var popped);

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

        _sut.List_PopByCondition(json, "Email", "admin", "StartsWith", false, false, out _, out var popped);

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

        _sut.List_PopByCondition(json, "Price", "10", "LessThan", false, false, out _, out var popped);

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

        _sut.List_PopByCondition(json, "Score", "50", "GreaterThan", false, false, out var updated, out var popped);

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

        _sut.List_PopByCondition(json, "Type", "A", "!=", false, false, out _, out var popped);

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

    [Fact]
    public void Difference_NotEquals_KeepsOnlyItemsThatMatchAllB()
    {
        // ∃b: keyA != b — true unless every b equals keyA (bSet == {keyA}).
        string listA = """[{"Id":"1"},{"Id":"2"},{"Id":"3"}]""";
        string listB = """[{"Id":"2"},{"Id":"2"}]""";

        _sut.List_Difference(listA, listB, "Id", "NotEquals", false, out var diff);

        var arr = JsonNode.Parse(diff)!.AsArray();
        Assert.Single(arr);
        Assert.Equal("2", arr[0]!["Id"]!.ToString());
    }

    [Fact]
    public void Difference_NotEquals_RemovesAll_WhenBHasMultipleDistinctValues()
    {
        string listA = """[{"Id":"1"},{"Id":"2"},{"Id":"3"}]""";
        string listB = """[{"Id":"9"},{"Id":"8"}]""";

        _sut.List_Difference(listA, listB, "Id", "NotEquals", false, out var diff);

        var arr = JsonNode.Parse(diff)!.AsArray();
        Assert.Empty(arr);
    }

    [Fact]
    public void Difference_StartsWith_RemovesItemsThatStartWithAnyBValue()
    {
        string listA = """[{"Code":"US-NY"},{"Code":"US-CA"},{"Code":"UK-LDN"}]""";
        string listB = """[{"Code":"US"}]""";

        _sut.List_Difference(listA, listB, "Code", "StartsWith", false, out var diff);

        var arr = JsonNode.Parse(diff)!.AsArray();
        Assert.Single(arr);
        Assert.Equal("UK-LDN", arr[0]!["Code"]!.ToString());
    }

    [Fact]
    public void Difference_EndsWith_RemovesItemsThatEndWithAnyBValue()
    {
        string listA = """[{"Code":"NY-US"},{"Code":"CA-US"},{"Code":"LDN-UK"}]""";
        string listB = """[{"Code":"-US"}]""";

        _sut.List_Difference(listA, listB, "Code", "EndsWith", false, out var diff);

        var arr = JsonNode.Parse(diff)!.AsArray();
        Assert.Single(arr);
        Assert.Equal("LDN-UK", arr[0]!["Code"]!.ToString());
    }

    [Fact]
    public void Difference_GreaterThan_RemovesItemsAboveMinB()
    {
        // ∃b: keyA > b iff keyA > min(B) = 10. So remove all A > 10.
        string listA = """[{"Score":"5"},{"Score":"10"},{"Score":"15"},{"Score":"20"}]""";
        string listB = """[{"Score":"10"},{"Score":"50"}]""";

        _sut.List_Difference(listA, listB, "Score", "GreaterThan", false, out var diff);

        var arr = JsonNode.Parse(diff)!.AsArray();
        Assert.Equal(2, arr.Count);
        Assert.Equal("5", arr[0]!["Score"]!.ToString());
        Assert.Equal("10", arr[1]!["Score"]!.ToString());
    }

    [Fact]
    public void Difference_LessThan_RemovesItemsBelowMaxB()
    {
        // ∃b: keyA < b iff keyA < max(B) = 50. So remove all A < 50.
        string listA = """[{"Score":"5"},{"Score":"10"},{"Score":"50"},{"Score":"100"}]""";
        string listB = """[{"Score":"10"},{"Score":"50"}]""";

        _sut.List_Difference(listA, listB, "Score", "LessThan", false, out var diff);

        var arr = JsonNode.Parse(diff)!.AsArray();
        Assert.Equal(2, arr.Count);
        Assert.Equal("50", arr[0]!["Score"]!.ToString());
        Assert.Equal("100", arr[1]!["Score"]!.ToString());
    }

    [Fact]
    public void Difference_GreaterOrEqual_KeepsItemsBelowMinB()
    {
        // ∃b: keyA >= b iff keyA >= min(B) = 10. Keep A < 10.
        string listA = """[{"Score":"5"},{"Score":"10"},{"Score":"15"}]""";
        string listB = """[{"Score":"10"},{"Score":"50"}]""";

        _sut.List_Difference(listA, listB, "Score", "GreaterOrEqual", false, out var diff);

        var arr = JsonNode.Parse(diff)!.AsArray();
        Assert.Single(arr);
        Assert.Equal("5", arr[0]!["Score"]!.ToString());
    }

    [Fact]
    public void Difference_LessOrEqual_KeepsItemsAboveMaxB()
    {
        // ∃b: keyA <= b iff keyA <= max(B) = 50. Keep A > 50.
        string listA = """[{"Score":"5"},{"Score":"50"},{"Score":"100"}]""";
        string listB = """[{"Score":"10"},{"Score":"50"}]""";

        _sut.List_Difference(listA, listB, "Score", "LessOrEqual", false, out var diff);

        var arr = JsonNode.Parse(diff)!.AsArray();
        Assert.Single(arr);
        Assert.Equal("100", arr[0]!["Score"]!.ToString());
    }

    [Fact]
    public void Difference_Numeric_NonNumericKeyA_IsKept()
    {
        // Non-numeric keys can't satisfy any numeric comparison → matchedAny=false → kept.
        string listA = """[{"Score":"abc"},{"Score":"5"}]""";
        string listB = """[{"Score":"10"}]""";

        _sut.List_Difference(listA, listB, "Score", "GreaterThan", false, out var diff);

        var arr = JsonNode.Parse(diff)!.AsArray();
        Assert.Equal(2, arr.Count);
    }

    [Fact]
    public void Difference_Numeric_NoNumericB_KeepsEverything()
    {
        // No parseable B → matchedAny=false → keep everything.
        string listA = """[{"Score":"5"},{"Score":"10"}]""";
        string listB = """[{"Score":"foo"},{"Score":"bar"}]""";

        _sut.List_Difference(listA, listB, "Score", "GreaterThan", false, out var diff);

        var arr = JsonNode.Parse(diff)!.AsArray();
        Assert.Equal(2, arr.Count);
    }

    #endregion
}
