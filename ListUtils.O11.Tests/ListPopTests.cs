using System.Text.Json.Nodes;

namespace ListUtils.Tests;

public class ListPopTests
{
    private readonly ListUtils _sut = new();

    [Fact]
    public void List_Pop_ValidIndex_ReturnsElementAndUpdatedList()
    {
        string json = """["a","b","c"]""";

        _sut.List_Pop(json, 1, out var updated, out var popped);

        Assert.Equal("\"b\"", popped);
        var arr = JsonNode.Parse(updated)!.AsArray();
        Assert.Equal(2, arr.Count);
        Assert.Equal("a", arr[0]!.ToString());
        Assert.Equal("c", arr[1]!.ToString());
    }

    [Fact]
    public void List_Pop_IndexOutOfRange_ReturnsOriginalList()
    {
        string json = """["a","b"]""";

        _sut.List_Pop(json, 5, out var updated, out var popped);

        Assert.Equal("null", popped);
        Assert.Equal(json, updated);
    }

    [Fact]
    public void List_Pop_NegativeIndex_ReturnsOriginalList()
    {
        string json = """["a","b"]""";

        _sut.List_Pop(json, -1, out var updated, out var popped);

        Assert.Equal("null", popped);
        Assert.Equal(json, updated);
    }

    [Fact]
    public void List_Pop_NullInput_ReturnsEmpty()
    {
        _sut.List_Pop(null!, 0, out var updated, out var popped);

        Assert.Equal("null", popped);
        Assert.Equal("[]", updated);
    }

    [Fact]
    public void List_Pop_FirstElement_RemovesFirst()
    {
        string json = """["x","y","z"]""";

        _sut.List_Pop(json, 0, out var updated, out var popped);

        Assert.Equal("\"x\"", popped);
        var arr = JsonNode.Parse(updated)!.AsArray();
        Assert.Equal(2, arr.Count);
    }

    [Fact]
    public void List_Pop_LastElement_RemovesLast()
    {
        string json = """["x","y","z"]""";

        _sut.List_Pop(json, 2, out var updated, out var popped);

        Assert.Equal("\"z\"", popped);
        var arr = JsonNode.Parse(updated)!.AsArray();
        Assert.Equal(2, arr.Count);
    }

    [Fact]
    public void List_Pop_ObjectElement_ReturnsJsonObject()
    {
        string json = """[{"Id":1,"Name":"Alice"},{"Id":2,"Name":"Bob"}]""";

        _sut.List_Pop(json, 0, out var updated, out var popped);

        var poppedObj = JsonNode.Parse(popped)!.AsObject();
        Assert.Equal("Alice", poppedObj["Name"]!.ToString());
        var arr = JsonNode.Parse(updated)!.AsArray();
        Assert.Single(arr);
    }

    [Fact]
    public void List_PopMultiple_ValidIndices_ReturnsElementsInOrder()
    {
        string json = """["a","b","c","d","e"]""";

        _sut.List_PopMultiple(json, "1,3", out var updated, out var popped);

        var poppedArr = JsonNode.Parse(popped)!.AsArray();
        Assert.Equal(2, poppedArr.Count);
        Assert.Equal("b", poppedArr[0]!.ToString());
        Assert.Equal("d", poppedArr[1]!.ToString());

        var updatedArr = JsonNode.Parse(updated)!.AsArray();
        Assert.Equal(3, updatedArr.Count);
    }

    [Fact]
    public void List_PopMultiple_EmptyIndices_ReturnsOriginalList()
    {
        string json = """["a","b"]""";

        _sut.List_PopMultiple(json, "", out var updated, out var popped);

        Assert.Equal(json, updated);
        Assert.Equal("[]", popped);
    }

    [Fact]
    public void List_PopMultiple_OutOfRangeIndicesIgnored()
    {
        string json = """["a","b","c"]""";

        _sut.List_PopMultiple(json, "0,99", out var updated, out var popped);

        var poppedArr = JsonNode.Parse(popped)!.AsArray();
        Assert.Single(poppedArr);
        Assert.Equal("a", poppedArr[0]!.ToString());
    }

    [Fact]
    public void List_PopMultiple_NullSource_ReturnsEmpty()
    {
        _sut.List_PopMultiple(null!, "0", out var updated, out var popped);

        Assert.Equal("[]", updated);
        Assert.Equal("[]", popped);
    }
}
