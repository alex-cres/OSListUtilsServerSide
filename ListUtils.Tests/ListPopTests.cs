namespace ListUtils.Tests;

public class ListPopTests
{
    private readonly ListUtils _sut = new();

    [Fact]
    public void List_Pop_ValidIndex_ReturnsElementAndUpdatedList()
    {
        var source = new List<string> { "a", "b", "c" };

        _sut.List_Pop(source, 1, out var updated, out var popped);

        Assert.Equal("b", popped);
        Assert.Equal(new List<string> { "a", "c" }, updated);
    }

    [Fact]
    public void List_Pop_IndexOutOfRange_ReturnsOriginalList()
    {
        var source = new List<string> { "a", "b" };

        _sut.List_Pop(source, 5, out var updated, out var popped);

        Assert.Equal("", popped);
        Assert.Equal(source, updated);
    }

    [Fact]
    public void List_Pop_NegativeIndex_ReturnsOriginalList()
    {
        var source = new List<string> { "a", "b" };

        _sut.List_Pop(source, -1, out var updated, out var popped);

        Assert.Equal("", popped);
        Assert.Equal(source, updated);
    }

    [Fact]
    public void List_Pop_NullList_ReturnsEmptyList()
    {
        _sut.List_Pop(null!, 0, out var updated, out var popped);

        Assert.Equal("", popped);
        Assert.Empty(updated);
    }

    [Fact]
    public void List_Pop_FirstElement_RemovesFirst()
    {
        var source = new List<string> { "x", "y", "z" };

        _sut.List_Pop(source, 0, out var updated, out var popped);

        Assert.Equal("x", popped);
        Assert.Equal(new List<string> { "y", "z" }, updated);
    }

    [Fact]
    public void List_Pop_LastElement_RemovesLast()
    {
        var source = new List<string> { "x", "y", "z" };

        _sut.List_Pop(source, 2, out var updated, out var popped);

        Assert.Equal("z", popped);
        Assert.Equal(new List<string> { "x", "y" }, updated);
    }

    [Fact]
    public void List_PopMultiple_ValidIndices_ReturnsElementsInOrder()
    {
        var source = new List<string> { "a", "b", "c", "d", "e" };

        _sut.List_PopMultiple(source, new List<int> { 1, 3 }, out var updated, out var popped);

        Assert.Equal(new List<string> { "b", "d" }, popped);
        Assert.Equal(new List<string> { "a", "c", "e" }, updated);
    }

    [Fact]
    public void List_PopMultiple_NullIndices_ReturnsOriginalList()
    {
        var source = new List<string> { "a", "b" };

        _sut.List_PopMultiple(source, null!, out var updated, out var popped);

        Assert.Equal(source, updated);
        Assert.Empty(popped);
    }

    [Fact]
    public void List_PopMultiple_EmptyIndices_ReturnsOriginalList()
    {
        var source = new List<string> { "a", "b" };

        _sut.List_PopMultiple(source, new List<int>(), out var updated, out var popped);

        Assert.Equal(source, updated);
        Assert.Empty(popped);
    }

    [Fact]
    public void List_PopMultiple_OutOfRangeIndicesIgnored()
    {
        var source = new List<string> { "a", "b", "c" };

        _sut.List_PopMultiple(source, new List<int> { 0, 99 }, out var updated, out var popped);

        Assert.Equal(new List<string> { "a" }, popped);
        Assert.Equal(new List<string> { "b", "c" }, updated);
    }

    [Fact]
    public void List_PopMultiple_NullSource_ReturnsEmpty()
    {
        _sut.List_PopMultiple(null!, new List<int> { 0 }, out var updated, out var popped);

        Assert.Empty(updated);
        Assert.Empty(popped);
    }
}
