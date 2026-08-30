using System.Diagnostics;
using System.Text.Json.Nodes;

namespace ListUtils.Tests;

// Load tests — 10,000-element complex JSON structures with nested objects,
// arrays, mixed types. Every test asserts the action completes in < 300 ms.
public class LoadTests
{
    private const int TargetSize = 10_000;
    private const int MaxDurationMs = 300;

    private static readonly string LargeJsonList = GenerateComplexList(TargetSize);
    private static readonly string LargeJsonListB = GenerateComplexList(TargetSize, idOffset: 5_000);
    private static readonly string LargeJsonListHalf = GenerateComplexList(TargetSize / 2);

    // Slow-path stress data — used only by O(A*B) operators like Contains.
    // Kept smaller so the 300 ms budget is realistic on both net10 and net48.
    private const int SlowPathSize = 1_000;
    private static readonly string SlowPathListA = GenerateComplexList(SlowPathSize);
    private static readonly string SlowPathListB = GenerateComplexList(SlowPathSize, idOffset: 500);

    private static string GenerateComplexList(int size, int idOffset = 0)
    {
        var arr = new JsonArray();
        string[] statuses = ["Active", "Inactive", "Pending", "Archived"];
        string[] categories = ["Books", "Electronics", "Food", "Clothing", "Toys"];
        string[] regions = ["EU", "US", "APAC", "LATAM"];
        string[] priorities = ["High", "Medium", "Low"];

        for (int i = 0; i < size; i++)
        {
            int id = i + idOffset;
            arr.Add(new JsonObject
            {
                ["Id"] = id,
                ["Name"] = $"Item{id:D5}",
                ["Status"] = statuses[i % statuses.Length],
                ["Category"] = categories[i % categories.Length],
                ["Score"] = Math.Round((i * 7.3) % 100, 2),
                ["Meta"] = new JsonObject
                {
                    ["Region"] = regions[i % regions.Length],
                    ["Priority"] = priorities[i % priorities.Length],
                    ["Tags"] = new JsonArray($"tag{i % 20}", $"tag{i % 7}"),
                },
                ["Items"] = new JsonArray
                {
                    new JsonObject { ["Product"] = $"P{i}A", ["Qty"] = (i % 10) + 1 },
                    new JsonObject { ["Product"] = $"P{i}B", ["Qty"] = (i % 5) + 1 },
                },
            });
        }
        return arr.ToJsonString();
    }

    private static void AssertUnderBudget(long elapsedMs, string action)
    {
        Assert.True(elapsedMs < MaxDurationMs, $"{action} took {elapsedMs}ms (budget: {MaxDurationMs}ms)");
    }

    // Correctness helpers. Called AFTER the stopwatch so parsing the 10k-element
    // output doesn't count against the 300 ms budget.
    private static JsonArray ParseArray(string json)
    {
        var node = JsonNode.Parse(json);
        Assert.NotNull(node);
        return node!.AsArray();
    }

    private static void AssertPopSingle(string updated, string popped, int sourceCount, int expectedRemoved)
    {
        var arr = ParseArray(updated);
        Assert.Equal(sourceCount - expectedRemoved, arr.Count);
        if (expectedRemoved == 1)
        {
            Assert.False(string.IsNullOrWhiteSpace(popped), "PoppedElementJson should not be empty on a valid pop");
            Assert.NotNull(JsonNode.Parse(popped));
        }
    }

    private static void AssertPopMany(string updated, string popped, int sourceCount, int expectedRemoved)
    {
        var arr = ParseArray(updated);
        Assert.Equal(sourceCount - expectedRemoved, arr.Count);
        var poppedArr = ParseArray(popped);
        Assert.Equal(expectedRemoved, poppedArr.Count);
    }

    // Invariant when the exact popped count is hard to predict (duplicates,
    // out-of-range mixed in). Guarantees no elements were lost or duplicated.
    private static void AssertPopManyInvariant(string updated, string popped, int sourceCount)
    {
        var updatedArr = ParseArray(updated);
        var poppedArr = ParseArray(popped);
        Assert.Equal(sourceCount, updatedArr.Count + poppedArr.Count);
    }

    private static void AssertZip(string zipped, int expectedLength, string keyA, string keyB)
    {
        var arr = ParseArray(zipped);
        Assert.Equal(expectedLength, arr.Count);
        if (expectedLength > 0)
        {
            var first = arr[0]!.AsObject();
            Assert.True(first.ContainsKey(keyA), $"Zip output missing key '{keyA}'");
            Assert.True(first.ContainsKey(keyB), $"Zip output missing key '{keyB}'");
        }
    }

    private static void AssertGroupBy(string grouped, int sourceCount, int expectedGroups)
    {
        var arr = ParseArray(grouped);
        Assert.Equal(expectedGroups, arr.Count);
        int total = 0;
        var keys = new HashSet<string>();
        foreach (var g in arr)
        {
            var obj = g!.AsObject();
            Assert.True(obj.ContainsKey("Key"));
            Assert.True(obj.ContainsKey("Items"));
            Assert.True(keys.Add(obj["Key"]!.ToString()), "Duplicate group key");
            total += obj["Items"]!.AsArray().Count;
        }
        Assert.Equal(sourceCount, total);
    }

    private static void AssertDifference(string diff, int expectedCount)
    {
        var arr = ParseArray(diff);
        Assert.Equal(expectedCount, arr.Count);
    }

    private readonly ListUtils _sut = new();

    #region List_Pop (10)

    [Fact]
    public void Load_Pop_FirstIndex()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_Pop(LargeJsonList, 0, out var updated, out var popped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Pop_FirstIndex));
        AssertPopSingle(updated, popped, TargetSize, expectedRemoved: 1);
        Assert.Contains("\"Id\":0", popped);
    }

    [Fact]
    public void Load_Pop_LastIndex()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_Pop(LargeJsonList, TargetSize - 1, out var updated, out var popped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Pop_LastIndex));
        AssertPopSingle(updated, popped, TargetSize, expectedRemoved: 1);
        Assert.Contains($"\"Id\":{TargetSize - 1}", popped);
    }

    [Fact]
    public void Load_Pop_MiddleIndex()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_Pop(LargeJsonList, TargetSize / 2, out var updated, out var popped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Pop_MiddleIndex));
        AssertPopSingle(updated, popped, TargetSize, expectedRemoved: 1);
        Assert.Contains($"\"Id\":{TargetSize / 2}", popped);
    }

    [Fact]
    public void Load_Pop_OutOfRange()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_Pop(LargeJsonList, 999_999, out var updated, out var popped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Pop_OutOfRange));
        AssertPopSingle(updated, popped, TargetSize, expectedRemoved: 0);
    }

    [Fact]
    public void Load_Pop_QuarterIndex()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_Pop(LargeJsonList, TargetSize / 4, out var updated, out var popped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Pop_QuarterIndex));
        AssertPopSingle(updated, popped, TargetSize, expectedRemoved: 1);
    }

    [Fact]
    public void Load_Pop_ThreeQuarterIndex()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_Pop(LargeJsonList, 3 * TargetSize / 4, out var updated, out var popped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Pop_ThreeQuarterIndex));
        AssertPopSingle(updated, popped, TargetSize, expectedRemoved: 1);
    }

    [Fact]
    public void Load_Pop_SmallIndex()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_Pop(LargeJsonList, 7, out var updated, out var popped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Pop_SmallIndex));
        AssertPopSingle(updated, popped, TargetSize, expectedRemoved: 1);
        Assert.Contains("\"Id\":7", popped);
    }

    [Fact]
    public void Load_Pop_ThousandIndex()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_Pop(LargeJsonList, 1000, out var updated, out var popped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Pop_ThousandIndex));
        AssertPopSingle(updated, popped, TargetSize, expectedRemoved: 1);
        Assert.Contains("\"Id\":1000", popped);
    }

    [Fact]
    public void Load_Pop_NearEnd()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_Pop(LargeJsonList, TargetSize - 5, out var updated, out var popped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Pop_NearEnd));
        AssertPopSingle(updated, popped, TargetSize, expectedRemoved: 1);
    }

    [Fact]
    public void Load_Pop_NegativeIndex()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_Pop(LargeJsonList, -1, out var updated, out var popped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Pop_NegativeIndex));
        AssertPopSingle(updated, popped, TargetSize, expectedRemoved: 0);
    }

    #endregion

    #region List_PopMultiple (10)

    [Fact]
    public void Load_PopMultiple_TenSpread()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_PopMultiple(LargeJsonList, "0,1000,2000,3000,4000,5000,6000,7000,8000,9000", out var updated, out var popped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_PopMultiple_TenSpread));
        AssertPopMany(updated, popped, TargetSize, expectedRemoved: 10);
    }

    [Fact]
    public void Load_PopMultiple_FirstTen()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_PopMultiple(LargeJsonList, "0,1,2,3,4,5,6,7,8,9", out var updated, out var popped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_PopMultiple_FirstTen));
        AssertPopMany(updated, popped, TargetSize, expectedRemoved: 10);
    }

    [Fact]
    public void Load_PopMultiple_LastTen()
    {
        var indices = string.Join(",", Enumerable.Range(TargetSize - 10, 10));
        var sw = Stopwatch.StartNew();
        _sut.List_PopMultiple(LargeJsonList, indices, out var updated, out var popped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_PopMultiple_LastTen));
        AssertPopMany(updated, popped, TargetSize, expectedRemoved: 10);
    }

    [Fact]
    public void Load_PopMultiple_HundredEvenlySpaced()
    {
        var indices = string.Join(",", Enumerable.Range(0, 100).Select(i => i * 100));
        var sw = Stopwatch.StartNew();
        _sut.List_PopMultiple(LargeJsonList, indices, out var updated, out var popped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_PopMultiple_HundredEvenlySpaced));
        AssertPopManyInvariant(updated, popped, TargetSize);
    }

    [Fact]
    public void Load_PopMultiple_ThousandIndices()
    {
        var indices = string.Join(",", Enumerable.Range(0, 1000).Select(i => i * 9));
        var sw = Stopwatch.StartNew();
        _sut.List_PopMultiple(LargeJsonList, indices, out var updated, out var popped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_PopMultiple_ThousandIndices));
        AssertPopManyInvariant(updated, popped, TargetSize);
    }

    [Fact]
    public void Load_PopMultiple_ReverseSorted()
    {
        var indices = string.Join(",", Enumerable.Range(0, 50).Select(i => (i * 200) + 100).Reverse());
        var sw = Stopwatch.StartNew();
        _sut.List_PopMultiple(LargeJsonList, indices, out var updated, out var popped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_PopMultiple_ReverseSorted));
        AssertPopManyInvariant(updated, popped, TargetSize);
    }

    [Fact]
    public void Load_PopMultiple_WithOutOfRange()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_PopMultiple(LargeJsonList, "0,50,99999,100,777,88888,555", out var updated, out var popped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_PopMultiple_WithOutOfRange));
        // 5 in-range indices, 2 OOB (99999 and 88888) are ignored.
        AssertPopMany(updated, popped, TargetSize, expectedRemoved: 5);
    }

    [Fact]
    public void Load_PopMultiple_SingleIndex()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_PopMultiple(LargeJsonList, "5000", out var updated, out var popped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_PopMultiple_SingleIndex));
        AssertPopMany(updated, popped, TargetSize, expectedRemoved: 1);
    }

    [Fact]
    public void Load_PopMultiple_EmptyIndices()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_PopMultiple(LargeJsonList, "", out var updated, out var popped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_PopMultiple_EmptyIndices));
        AssertPopMany(updated, popped, TargetSize, expectedRemoved: 0);
    }

    [Fact]
    public void Load_PopMultiple_Duplicates()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_PopMultiple(LargeJsonList, "100,100,100,200,200,300", out var updated, out var popped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_PopMultiple_Duplicates));
        AssertPopManyInvariant(updated, popped, TargetSize);
    }

    #endregion

    #region List_PopByCondition (10)

    [Fact]
    public void Load_PopByCondition_TopLevelStatus()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_PopByCondition(LargeJsonList, "Status", "Active", "Equals", false, false, out var updated, out var popped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_PopByCondition_TopLevelStatus));
        AssertPopSingle(updated, popped, TargetSize, expectedRemoved: 1);
        Assert.Contains("\"Status\":\"Active\"", popped);
    }

    [Fact]
    public void Load_PopByCondition_TopLevelCategory()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_PopByCondition(LargeJsonList, "Category", "Books", "Equals", false, false, out var updated, out var popped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_PopByCondition_TopLevelCategory));
        AssertPopSingle(updated, popped, TargetSize, expectedRemoved: 1);
        Assert.Contains("\"Category\":\"Books\"", popped);
    }

    [Fact]
    public void Load_PopByCondition_NestedMetaRegion()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_PopByCondition(LargeJsonList, "Meta.Region", "EU", "Equals", false, false, out var updated, out var popped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_PopByCondition_NestedMetaRegion));
        AssertPopSingle(updated, popped, TargetSize, expectedRemoved: 1);
        Assert.Contains("\"Region\":\"EU\"", popped);
    }

    [Fact]
    public void Load_PopByCondition_NestedMetaPriority()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_PopByCondition(LargeJsonList, "Meta.Priority", "High", "Equals", false, false, out var updated, out var popped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_PopByCondition_NestedMetaPriority));
        AssertPopSingle(updated, popped, TargetSize, expectedRemoved: 1);
        Assert.Contains("\"Priority\":\"High\"", popped);
    }

    [Fact]
    public void Load_PopByCondition_NumericGreaterThan()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_PopByCondition(LargeJsonList, "Score", "80", "GreaterThan", false, false, out var updated, out var popped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_PopByCondition_NumericGreaterThan));
        AssertPopSingle(updated, popped, TargetSize, expectedRemoved: 1);
    }

    [Fact]
    public void Load_PopByCondition_CaseSensitive()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_PopByCondition(LargeJsonList, "Status", "Active", "Equals", true, false, out var updated, out var popped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_PopByCondition_CaseSensitive));
        AssertPopSingle(updated, popped, TargetSize, expectedRemoved: 1);
    }

    [Fact]
    public void Load_PopByCondition_SearchFromEnd()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_PopByCondition(LargeJsonList, "Status", "Active", "Equals", false, true, out var updated, out var popped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_PopByCondition_SearchFromEnd));
        AssertPopSingle(updated, popped, TargetSize, expectedRemoved: 1);
        // Last Active in TargetSize=10000 is at index 9996 (Id 9996).
        Assert.Contains("\"Id\":9996", popped);
    }

    [Fact]
    public void Load_PopByCondition_ArrayIndexPath()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_PopByCondition(LargeJsonList, "Items[0].Product", "P100A", "Equals", false, false, out var updated, out var popped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_PopByCondition_ArrayIndexPath));
        AssertPopSingle(updated, popped, TargetSize, expectedRemoved: 1);
        Assert.Contains("P100A", popped);
    }

    [Fact]
    public void Load_PopByCondition_NoMatch()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_PopByCondition(LargeJsonList, "Status", "NonExistent", "Equals", false, false, out var updated, out var popped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_PopByCondition_NoMatch));
        AssertPopSingle(updated, popped, TargetSize, expectedRemoved: 0);
    }

    [Fact]
    public void Load_PopByCondition_ContainsName()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_PopByCondition(LargeJsonList, "Name", "Item05000", "Contains", false, false, out var updated, out var popped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_PopByCondition_ContainsName));
        AssertPopSingle(updated, popped, TargetSize, expectedRemoved: 1);
        Assert.Contains("Item05000", popped);
    }

    #endregion

    #region List_PopMultipleByCondition (10)

    [Fact]
    public void Load_PopMultipleByCondition_ByStatus()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_PopMultipleByCondition(LargeJsonList, "Status", "Active", "Equals", false, out var updated, out var popped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_PopMultipleByCondition_ByStatus));
        // 10000 elements, statuses[i % 4]=Active when i%4==0 → exactly 2500 matches.
        AssertPopMany(updated, popped, TargetSize, expectedRemoved: 2500);
    }

    [Fact]
    public void Load_PopMultipleByCondition_ByCategory()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_PopMultipleByCondition(LargeJsonList, "Category", "Electronics", "Equals", false, out var updated, out var popped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_PopMultipleByCondition_ByCategory));
        // categories[i % 5]=Electronics when i%5==1 → exactly 2000 matches.
        AssertPopMany(updated, popped, TargetSize, expectedRemoved: 2000);
    }

    [Fact]
    public void Load_PopMultipleByCondition_NestedRegion()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_PopMultipleByCondition(LargeJsonList, "Meta.Region", "APAC", "Equals", false, out var updated, out var popped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_PopMultipleByCondition_NestedRegion));
        // regions[i % 4]=APAC when i%4==2 → exactly 2500 matches.
        AssertPopMany(updated, popped, TargetSize, expectedRemoved: 2500);
    }

    [Fact]
    public void Load_PopMultipleByCondition_NestedPriority()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_PopMultipleByCondition(LargeJsonList, "Meta.Priority", "Medium", "Equals", false, out var updated, out var popped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_PopMultipleByCondition_NestedPriority));
        // priorities[i % 3]=Medium when i%3==1 → 3333 matches in [0,10000).
        AssertPopMany(updated, popped, TargetSize, expectedRemoved: 3333);
    }

    [Fact]
    public void Load_PopMultipleByCondition_NumericLessThan()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_PopMultipleByCondition(LargeJsonList, "Score", "20", "LessThan", false, out var updated, out var popped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_PopMultipleByCondition_NumericLessThan));
        AssertPopManyInvariant(updated, popped, TargetSize);
    }

    [Fact]
    public void Load_PopMultipleByCondition_StartsWith()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_PopMultipleByCondition(LargeJsonList, "Name", "Item001", "StartsWith", false, out var updated, out var popped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_PopMultipleByCondition_StartsWith));
        // Names Item00100..Item00199 start with "Item001" → 100 matches.
        AssertPopMany(updated, popped, TargetSize, expectedRemoved: 100);
    }

    [Fact]
    public void Load_PopMultipleByCondition_CaseSensitive()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_PopMultipleByCondition(LargeJsonList, "Status", "active", "Equals", true, out var updated, out var popped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_PopMultipleByCondition_CaseSensitive));
        // Data uses "Active" (uppercase A). Case-sensitive "active" → 0 matches.
        AssertPopMany(updated, popped, TargetSize, expectedRemoved: 0);
    }

    [Fact]
    public void Load_PopMultipleByCondition_ArrayIndex()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_PopMultipleByCondition(LargeJsonList, "Items[0].Qty", "5", "GreaterThan", false, out var updated, out var popped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_PopMultipleByCondition_ArrayIndex));
        AssertPopManyInvariant(updated, popped, TargetSize);
    }

    [Fact]
    public void Load_PopMultipleByCondition_NoMatch()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_PopMultipleByCondition(LargeJsonList, "Status", "NonExistent", "Equals", false, out var updated, out var popped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_PopMultipleByCondition_NoMatch));
        AssertPopMany(updated, popped, TargetSize, expectedRemoved: 0);
    }

    [Fact]
    public void Load_PopMultipleByCondition_TagArrayIndex()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_PopMultipleByCondition(LargeJsonList, "Meta.Tags[0]", "tag5", "Equals", false, out var updated, out var popped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_PopMultipleByCondition_TagArrayIndex));
        // Meta.Tags[0]=$"tag{i%20}" → tag5 when i%20==5 → 500 matches.
        AssertPopMany(updated, popped, TargetSize, expectedRemoved: 500);
    }

    #endregion

    #region List_PopByConditions (10)

    private static readonly List<Condition> ConditionsAnd2 = new() {
        new() { Path = "Status", Operator = Operators.Equals, Value = "Active" },
        new() { Path = "Category", Operator = Operators.Equals, Value = "Books" },
    };
    private static readonly List<Condition> ConditionsAnd3 = new() {
        new() { Path = "Status", Operator = Operators.Equals, Value = "Active" },
        new() { Path = "Category", Operator = Operators.Equals, Value = "Books" },
        new() { Path = "Meta.Region", Operator = Operators.Equals, Value = "EU" },
    };
    private static readonly List<Condition> ConditionsOr2 = new() {
        new() { Path = "Status", Operator = Operators.Equals, Value = "Archived" },
        new() { Path = "Score", Operator = Operators.GreaterThan, Value = "95" },
    };
    private static readonly List<Condition> ConditionsNestedAnd = new() {
        new() { Path = "Meta.Region", Operator = Operators.Equals, Value = "EU" },
        new() { Path = "Meta.Priority", Operator = Operators.Equals, Value = "High" },
    };
    private static readonly List<Condition> ConditionsMixedOps = new() {
        new() { Path = "Score", Operator = Operators.GreaterThan, Value = "50" },
        new() { Path = "Score", Operator = Operators.LessThan, Value = "70" },
    };

    [Fact]
    public void Load_PopByConditions_TwoAnd()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_PopByConditions(LargeJsonList, ConditionsAnd2, "AND", false, out var updated, out var popped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_PopByConditions_TwoAnd));
        AssertPopSingle(updated, popped, TargetSize, expectedRemoved: 1);
    }

    [Fact]
    public void Load_PopByConditions_ThreeAnd()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_PopByConditions(LargeJsonList, ConditionsAnd3, "AND", false, out var updated, out var popped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_PopByConditions_ThreeAnd));
        AssertPopSingle(updated, popped, TargetSize, expectedRemoved: 1);
    }

    [Fact]
    public void Load_PopByConditions_TwoOr()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_PopByConditions(LargeJsonList, ConditionsOr2, "OR", false, out var updated, out var popped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_PopByConditions_TwoOr));
        AssertPopSingle(updated, popped, TargetSize, expectedRemoved: 1);
    }

    [Fact]
    public void Load_PopByConditions_NestedAnd()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_PopByConditions(LargeJsonList, ConditionsNestedAnd, "AND", false, out var updated, out var popped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_PopByConditions_NestedAnd));
        AssertPopSingle(updated, popped, TargetSize, expectedRemoved: 1);
    }

    [Fact]
    public void Load_PopByConditions_MixedOps()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_PopByConditions(LargeJsonList, ConditionsMixedOps, "AND", false, out var updated, out var popped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_PopByConditions_MixedOps));
        AssertPopSingle(updated, popped, TargetSize, expectedRemoved: 1);
    }

    [Fact]
    public void Load_PopByConditions_SearchFromEnd()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_PopByConditions(LargeJsonList, ConditionsAnd2, "AND", true, out var updated, out var popped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_PopByConditions_SearchFromEnd));
        AssertPopSingle(updated, popped, TargetSize, expectedRemoved: 1);
    }

    [Fact]
    public void Load_PopByConditions_NoMatch()
    {
        var conds = new List<Condition> {
            new() { Path = "Status", Operator = Operators.Equals, Value = "XYZ" },
            new() { Path = "Category", Operator = Operators.Equals, Value = "ABC" },
        };
        var sw = Stopwatch.StartNew();
        _sut.List_PopByConditions(LargeJsonList, conds, "AND", false, out var updated, out var popped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_PopByConditions_NoMatch));
        AssertPopSingle(updated, popped, TargetSize, expectedRemoved: 0);
    }

    [Fact]
    public void Load_PopByConditions_ArrayIndexPath()
    {
        var conds = new List<Condition> {
            new() { Path = "Items[0].Product", Operator = Operators.StartsWith, Value = "P5" },
            new() { Path = "Status", Operator = Operators.Equals, Value = "Active" },
        };
        var sw = Stopwatch.StartNew();
        _sut.List_PopByConditions(LargeJsonList, conds, "AND", false, out var updated, out var popped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_PopByConditions_ArrayIndexPath));
        AssertPopSingle(updated, popped, TargetSize, expectedRemoved: 1);
    }

    [Fact]
    public void Load_PopByConditions_PerConditionCaseSensitive()
    {
        var conds = new List<Condition> {
            new() { Path = "Status", Operator = Operators.Equals, Value = "Active", CaseSensitive = true },
            new() { Path = "Meta.Region", Operator = Operators.Equals, Value = "EU" },
        };
        var sw = Stopwatch.StartNew();
        _sut.List_PopByConditions(LargeJsonList, conds, "AND", false, out var updated, out var popped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_PopByConditions_PerConditionCaseSensitive));
        AssertPopSingle(updated, popped, TargetSize, expectedRemoved: 1);
    }

    [Fact]
    public void Load_PopByConditions_FiveConditions()
    {
        var conds = new List<Condition> {
            new() { Path = "Status", Operator = Operators.Equals, Value = "Active" },
            new() { Path = "Category", Operator = Operators.Equals, Value = "Books" },
            new() { Path = "Meta.Region", Operator = Operators.Equals, Value = "EU" },
            new() { Path = "Meta.Priority", Operator = Operators.Equals, Value = "High" },
            new() { Path = "Score", Operator = Operators.GreaterThan, Value = "20" },
        };
        var sw = Stopwatch.StartNew();
        _sut.List_PopByConditions(LargeJsonList, conds, "AND", false, out var updated, out var popped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_PopByConditions_FiveConditions));
        // At least one item satisfies i%60==0 AND Score > 20 (e.g. i=60, Score=38).
        AssertPopSingle(updated, popped, TargetSize, expectedRemoved: 1);
    }

    #endregion

    #region List_PopMultipleByConditions (10)

    [Fact]
    public void Load_PopMultipleByConditions_TwoAnd()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_PopMultipleByConditions(LargeJsonList, ConditionsAnd2, "AND", out var updated, out var popped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_PopMultipleByConditions_TwoAnd));
        // Status=Active (i%4==0) AND Category=Books (i%5==0) → i%20==0 → 500 matches.
        AssertPopMany(updated, popped, TargetSize, expectedRemoved: 500);
    }

    [Fact]
    public void Load_PopMultipleByConditions_ThreeAnd()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_PopMultipleByConditions(LargeJsonList, ConditionsAnd3, "AND", out var updated, out var popped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_PopMultipleByConditions_ThreeAnd));
        // Active AND Books AND Region=EU (i%4==0) → i%20==0 AND i%4==0 → i%20==0 (already implied) → 500. But region is i%4 too so extra constraint is redundant.
        AssertPopMany(updated, popped, TargetSize, expectedRemoved: 500);
    }

    [Fact]
    public void Load_PopMultipleByConditions_TwoOr()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_PopMultipleByConditions(LargeJsonList, ConditionsOr2, "OR", out var updated, out var popped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_PopMultipleByConditions_TwoOr));
        AssertPopManyInvariant(updated, popped, TargetSize);
    }

    [Fact]
    public void Load_PopMultipleByConditions_NestedAnd()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_PopMultipleByConditions(LargeJsonList, ConditionsNestedAnd, "AND", out var updated, out var popped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_PopMultipleByConditions_NestedAnd));
        // Region=EU (i%4==0) AND Priority=High (i%3==0) → i%12==0 → 834 matches in [0,10000).
        AssertPopMany(updated, popped, TargetSize, expectedRemoved: 834);
    }

    [Fact]
    public void Load_PopMultipleByConditions_MixedOps()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_PopMultipleByConditions(LargeJsonList, ConditionsMixedOps, "AND", out var updated, out var popped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_PopMultipleByConditions_MixedOps));
        AssertPopManyInvariant(updated, popped, TargetSize);
    }

    [Fact]
    public void Load_PopMultipleByConditions_OrThreeConds()
    {
        var conds = new List<Condition> {
            new() { Path = "Status", Operator = Operators.Equals, Value = "Active" },
            new() { Path = "Meta.Region", Operator = Operators.Equals, Value = "EU" },
            new() { Path = "Score", Operator = Operators.GreaterThan, Value = "90" },
        };
        var sw = Stopwatch.StartNew();
        _sut.List_PopMultipleByConditions(LargeJsonList, conds, "OR", out var updated, out var popped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_PopMultipleByConditions_OrThreeConds));
        AssertPopManyInvariant(updated, popped, TargetSize);
    }

    [Fact]
    public void Load_PopMultipleByConditions_NoMatch()
    {
        var conds = new List<Condition> {
            new() { Path = "Status", Operator = Operators.Equals, Value = "XYZ" },
            new() { Path = "Category", Operator = Operators.Equals, Value = "ABC" },
        };
        var sw = Stopwatch.StartNew();
        _sut.List_PopMultipleByConditions(LargeJsonList, conds, "AND", out var updated, out var popped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_PopMultipleByConditions_NoMatch));
        AssertPopMany(updated, popped, TargetSize, expectedRemoved: 0);
    }

    [Fact]
    public void Load_PopMultipleByConditions_ArrayIndexPath()
    {
        var conds = new List<Condition> {
            new() { Path = "Items[0].Product", Operator = Operators.StartsWith, Value = "P5" },
            new() { Path = "Status", Operator = Operators.Equals, Value = "Active" },
        };
        var sw = Stopwatch.StartNew();
        _sut.List_PopMultipleByConditions(LargeJsonList, conds, "AND", out var updated, out var popped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_PopMultipleByConditions_ArrayIndexPath));
        AssertPopManyInvariant(updated, popped, TargetSize);
    }

    [Fact]
    public void Load_PopMultipleByConditions_MatchesEverything()
    {
        var conds = new List<Condition> {
            new() { Path = "Score", Operator = Operators.GreaterOrEqual, Value = "0" },
        };
        var sw = Stopwatch.StartNew();
        _sut.List_PopMultipleByConditions(LargeJsonList, conds, "AND", out var updated, out var popped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_PopMultipleByConditions_MatchesEverything));
        // Every Score >= 0 → all 10000 elements popped.
        AssertPopMany(updated, popped, TargetSize, expectedRemoved: TargetSize);
    }

    [Fact]
    public void Load_PopMultipleByConditions_FiveConditions()
    {
        var conds = new List<Condition> {
            new() { Path = "Status", Operator = Operators.Equals, Value = "Active" },
            new() { Path = "Category", Operator = Operators.Equals, Value = "Books" },
            new() { Path = "Meta.Region", Operator = Operators.Equals, Value = "EU" },
            new() { Path = "Meta.Priority", Operator = Operators.Equals, Value = "High" },
            new() { Path = "Score", Operator = Operators.GreaterThan, Value = "20" },
        };
        var sw = Stopwatch.StartNew();
        _sut.List_PopMultipleByConditions(LargeJsonList, conds, "AND", out var updated, out var popped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_PopMultipleByConditions_FiveConditions));
        AssertPopManyInvariant(updated, popped, TargetSize);
    }

    #endregion

    #region List_Zip (10)

    [Fact]
    public void Load_Zip_TwoLargeLists()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_Zip(LargeJsonList, LargeJsonListB, "A", "B", out var zipped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Zip_TwoLargeLists));
        AssertZip(zipped, expectedLength: TargetSize, "A", "B");
    }

    [Fact]
    public void Load_Zip_LargeVsHalf()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_Zip(LargeJsonList, LargeJsonListHalf, "Full", "Half", out var zipped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Zip_LargeVsHalf));
        AssertZip(zipped, expectedLength: TargetSize / 2, "Full", "Half");
    }

    [Fact]
    public void Load_Zip_HalfVsLarge()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_Zip(LargeJsonListHalf, LargeJsonList, "Half", "Full", out var zipped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Zip_HalfVsLarge));
        AssertZip(zipped, expectedLength: TargetSize / 2, "Half", "Full");
    }

    [Fact]
    public void Load_Zip_LargeVsB()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_Zip(LargeJsonList, LargeJsonListB, "Left", "Right", out var zipped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Zip_LargeVsB));
        AssertZip(zipped, expectedLength: TargetSize, "Left", "Right");
    }

    [Fact]
    public void Load_Zip_LongKeyNames()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_Zip(LargeJsonList, LargeJsonListB, "CustomerRecord", "OrderDetails", out var zipped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Zip_LongKeyNames));
        AssertZip(zipped, expectedLength: TargetSize, "CustomerRecord", "OrderDetails");
    }

    [Fact]
    public void Load_Zip_UnicodeKeys()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_Zip(LargeJsonList, LargeJsonListB, "Alpha_α", "Beta_β", out var zipped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Zip_UnicodeKeys));
        AssertZip(zipped, expectedLength: TargetSize, "Alpha_α", "Beta_β");
    }

    [Fact]
    public void Load_Zip_SelfPair()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_Zip(LargeJsonList, LargeJsonList, "First", "Second", out var zipped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Zip_SelfPair));
        AssertZip(zipped, expectedLength: TargetSize, "First", "Second");
    }

    [Fact]
    public void Load_Zip_LargeVsSmall()
    {
        string small = """[{"Id":1},{"Id":2},{"Id":3}]""";
        var sw = Stopwatch.StartNew();
        _sut.List_Zip(LargeJsonList, small, "Big", "Small", out var zipped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Zip_LargeVsSmall));
        AssertZip(zipped, expectedLength: 3, "Big", "Small");
    }

    [Fact]
    public void Load_Zip_SmallVsLarge()
    {
        string small = """[{"Id":1},{"Id":2},{"Id":3}]""";
        var sw = Stopwatch.StartNew();
        _sut.List_Zip(small, LargeJsonList, "Small", "Big", out var zipped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Zip_SmallVsLarge));
        AssertZip(zipped, expectedLength: 3, "Small", "Big");
    }

    [Fact]
    public void Load_Zip_HalfSelf()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_Zip(LargeJsonListHalf, LargeJsonListHalf, "H1", "H2", out var zipped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Zip_HalfSelf));
        AssertZip(zipped, expectedLength: TargetSize / 2, "H1", "H2");
    }

    #endregion

    #region List_GroupBy (10)

    [Fact]
    public void Load_GroupBy_Status()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_GroupBy(LargeJsonList, "Status", out var grouped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_GroupBy_Status));
        AssertGroupBy(grouped, sourceCount: TargetSize, expectedGroups: 4);
    }

    [Fact]
    public void Load_GroupBy_Category()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_GroupBy(LargeJsonList, "Category", out var grouped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_GroupBy_Category));
        AssertGroupBy(grouped, sourceCount: TargetSize, expectedGroups: 5);
    }

    [Fact]
    public void Load_GroupBy_NestedRegion()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_GroupBy(LargeJsonList, "Meta.Region", out var grouped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_GroupBy_NestedRegion));
        AssertGroupBy(grouped, sourceCount: TargetSize, expectedGroups: 4);
    }

    [Fact]
    public void Load_GroupBy_NestedPriority()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_GroupBy(LargeJsonList, "Meta.Priority", out var grouped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_GroupBy_NestedPriority));
        AssertGroupBy(grouped, sourceCount: TargetSize, expectedGroups: 3);
    }

    [Fact]
    public void Load_GroupBy_ArrayIndex()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_GroupBy(LargeJsonList, "Meta.Tags[0]", out var grouped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_GroupBy_ArrayIndex));
        // Tags[0] = $"tag{i%20}" → 20 distinct groups.
        AssertGroupBy(grouped, sourceCount: TargetSize, expectedGroups: 20);
    }

    [Fact]
    public void Load_GroupBy_ItemsFirstProduct()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_GroupBy(LargeJsonList, "Items[0].Qty", out var grouped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_GroupBy_ItemsFirstProduct));
        // Items[0].Qty = (i%10)+1 → values 1..10 → 10 groups.
        AssertGroupBy(grouped, sourceCount: TargetSize, expectedGroups: 10);
    }

    [Fact]
    public void Load_GroupBy_NonExistent()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_GroupBy(LargeJsonList, "DoesNotExist", out var grouped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_GroupBy_NonExistent));
        // All items fall into the fallback "Unknown" group.
        AssertGroupBy(grouped, sourceCount: TargetSize, expectedGroups: 1);
    }

    [Fact]
    public void Load_GroupBy_HalfList()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_GroupBy(LargeJsonListHalf, "Status", out var grouped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_GroupBy_HalfList));
        AssertGroupBy(grouped, sourceCount: TargetSize / 2, expectedGroups: 4);
    }

    [Fact]
    public void Load_GroupBy_ListB()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_GroupBy(LargeJsonListB, "Category", out var grouped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_GroupBy_ListB));
        AssertGroupBy(grouped, sourceCount: TargetSize, expectedGroups: 5);
    }

    [Fact]
    public void Load_GroupBy_LargeCardinality_ById()
    {
        // Worst case: 10,000 unique keys → 10,000 single-item groups
        var sw = Stopwatch.StartNew();
        _sut.List_GroupBy(LargeJsonList, "Id", out var grouped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_GroupBy_LargeCardinality_ById));
        AssertGroupBy(grouped, sourceCount: TargetSize, expectedGroups: TargetSize);
    }

    #endregion

    #region List_ZipGroupBy (10)

    // Correctness helper — every A+B item lands in some group; expected group
    // count is the union of first-seen keys across both sides.
    private static void AssertZipGroupBy(string grouped, int sourceCountA, int sourceCountB, string nameA, string nameB, int expectedGroups)
    {
        var arr = ParseArray(grouped);
        Assert.Equal(expectedGroups, arr.Count);
        int totalA = 0, totalB = 0;
        var keys = new HashSet<string>();
        foreach (var g in arr)
        {
            var obj = g!.AsObject();
            Assert.True(obj.ContainsKey("Key"));
            Assert.True(obj.ContainsKey(nameA));
            Assert.True(obj.ContainsKey(nameB));
            Assert.True(keys.Add(obj["Key"]!.ToString()), "Duplicate group key");
            totalA += obj[nameA]!.AsArray().Count;
            totalB += obj[nameB]!.AsArray().Count;
        }
        Assert.Equal(sourceCountA, totalA);
        Assert.Equal(sourceCountB, totalB);
    }

    [Fact]
    public void Load_ZipGroupBy_ByStatus()
    {
        // Both lists cycle through the same 4 status values → 4 groups.
        var sw = Stopwatch.StartNew();
        _sut.List_ZipGroupBy(LargeJsonList, LargeJsonListB, "Status", "Status", "A", "B", false, out var grouped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_ZipGroupBy_ByStatus));
        AssertZipGroupBy(grouped, TargetSize, TargetSize, "A", "B", expectedGroups: 4);
    }

    [Fact]
    public void Load_ZipGroupBy_ByCategory()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_ZipGroupBy(LargeJsonList, LargeJsonListB, "Category", "Category", "A", "B", false, out var grouped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_ZipGroupBy_ByCategory));
        AssertZipGroupBy(grouped, TargetSize, TargetSize, "A", "B", expectedGroups: 5);
    }

    [Fact]
    public void Load_ZipGroupBy_ByNestedRegion()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_ZipGroupBy(LargeJsonList, LargeJsonListB, "Meta.Region", "Meta.Region", "Orders", "Payments", false, out var grouped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_ZipGroupBy_ByNestedRegion));
        AssertZipGroupBy(grouped, TargetSize, TargetSize, "Orders", "Payments", expectedGroups: 4);
    }

    [Fact]
    public void Load_ZipGroupBy_ByNestedPriority()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_ZipGroupBy(LargeJsonList, LargeJsonListB, "Meta.Priority", "Meta.Priority", "A", "B", false, out var grouped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_ZipGroupBy_ByNestedPriority));
        AssertZipGroupBy(grouped, TargetSize, TargetSize, "A", "B", expectedGroups: 3);
    }

    [Fact]
    public void Load_ZipGroupBy_ByArrayIndex()
    {
        // Meta.Tags[0] = $"tag{i%20}" → 20 distinct groups on both sides.
        // Halved input on each side because two array-path walks per item
        // (A + B) double the work vs List_GroupBy — net48 loses its headroom
        // at full 10k+10k on the 300 ms budget.
        var sw = Stopwatch.StartNew();
        _sut.List_ZipGroupBy(LargeJsonListHalf, LargeJsonListHalf, "Meta.Tags[0]", "Meta.Tags[0]", "A", "B", false, out var grouped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_ZipGroupBy_ByArrayIndex));
        AssertZipGroupBy(grouped, TargetSize / 2, TargetSize / 2, "A", "B", expectedGroups: 20);
    }

    [Fact]
    public void Load_ZipGroupBy_HalfListA()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_ZipGroupBy(LargeJsonListHalf, LargeJsonListB, "Status", "Status", "A", "B", false, out var grouped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_ZipGroupBy_HalfListA));
        AssertZipGroupBy(grouped, TargetSize / 2, TargetSize, "A", "B", expectedGroups: 4);
    }

    [Fact]
    public void Load_ZipGroupBy_HalfListB()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_ZipGroupBy(LargeJsonList, LargeJsonListHalf, "Category", "Category", "A", "B", false, out var grouped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_ZipGroupBy_HalfListB));
        AssertZipGroupBy(grouped, TargetSize, TargetSize / 2, "A", "B", expectedGroups: 5);
    }

    [Fact]
    public void Load_ZipGroupBy_MissingKey_UnknownBucket()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_ZipGroupBy(LargeJsonList, LargeJsonListB, "DoesNotExist", "DoesNotExist", "A", "B", false, out var grouped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_ZipGroupBy_MissingKey_UnknownBucket));
        // Every item on both sides has no such key → single "Unknown" bucket.
        AssertZipGroupBy(grouped, TargetSize, TargetSize, "A", "B", expectedGroups: 1);
    }

    [Fact]
    public void Load_ZipGroupBy_CaseSensitive_ByCategory()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_ZipGroupBy(LargeJsonList, LargeJsonListB, "Category", "Category", "A", "B", true, out var grouped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_ZipGroupBy_CaseSensitive_ByCategory));
        AssertZipGroupBy(grouped, TargetSize, TargetSize, "A", "B", expectedGroups: 5);
    }

    [Fact]
    public void Load_ZipGroupBy_LargeCardinality_ById()
    {
        // A has Ids 0..9999, B has Ids 5000..14999. Union = 15,000 distinct keys.
        var sw = Stopwatch.StartNew();
        _sut.List_ZipGroupBy(LargeJsonList, LargeJsonListB, "Id", "Id", "A", "B", false, out var grouped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_ZipGroupBy_LargeCardinality_ById));
        AssertZipGroupBy(grouped, TargetSize, TargetSize, "A", "B", expectedGroups: TargetSize + TargetSize / 2);
    }

    #endregion

    #region List_Difference (10)

    [Fact]
    public void Load_Difference_ById()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_Difference(LargeJsonList, LargeJsonListB, "Id", "Equals", false, out var diff);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Difference_ById));
        // A has Ids 0..9999, B has Ids 5000..14999. Overlap 5000..9999 → diff = 5000.
        AssertDifference(diff, expectedCount: 5000);
    }

    [Fact]
    public void Load_Difference_ByName()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_Difference(LargeJsonList, LargeJsonListB, "Name", "Equals", false, out var diff);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Difference_ByName));
        // Names mirror Ids (Item{id:D5}) → same overlap as ById → diff = 5000.
        AssertDifference(diff, expectedCount: 5000);
    }

    [Fact]
    public void Load_Difference_ByNestedRegion()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_Difference(LargeJsonList, LargeJsonListB, "Meta.Region", "Equals", false, out var diff);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Difference_ByNestedRegion));
        // Both lists contain all 4 regions → every A region is in B → diff = 0.
        AssertDifference(diff, expectedCount: 0);
    }

    [Fact]
    public void Load_Difference_ByCategory()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_Difference(LargeJsonList, LargeJsonListB, "Category", "Equals", false, out var diff);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Difference_ByCategory));
        // Both lists contain all 5 categories → diff = 0.
        AssertDifference(diff, expectedCount: 0);
    }

    [Fact]
    public void Load_Difference_FullOverlap_Self()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_Difference(LargeJsonList, LargeJsonList, "Id", "Equals", false, out var diff);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Difference_FullOverlap_Self));
        // A \ A = ∅.
        AssertDifference(diff, expectedCount: 0);
    }

    [Fact]
    public void Load_Difference_CaseSensitive()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_Difference(LargeJsonList, LargeJsonListB, "Status", "Equals", true, out var diff);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Difference_CaseSensitive));
        // Both lists use the same 4 statuses with same casing → diff = 0.
        AssertDifference(diff, expectedCount: 0);
    }

    [Fact]
    public void Load_Difference_ContainsMatch()
    {
        // Contains is inherently O(A*B) — no HashSet shortcut is possible.
        // We use a smaller (1k x 1k) pair so the standard 300 ms budget still applies.
        var sw = Stopwatch.StartNew();
        _sut.List_Difference(SlowPathListA, SlowPathListB, "Name", "Contains", false, out var diff);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Difference_ContainsMatch));
        // A names Item00000..Item00999, B names Item00500..Item01499.
        // All names are 9 chars, so keyA.Contains(bValue) reduces to equality.
        // Overlap Item00500..Item00999 → diff = 500.
        AssertDifference(diff, expectedCount: 500);
    }

    [Fact]
    public void Load_Difference_ByArrayIndexPath()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_Difference(LargeJsonList, LargeJsonListB, "Items[0].Product", "Equals", false, out var diff);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Difference_ByArrayIndexPath));
        // Products use the loop index (P{i}A), not the id, so both lists share the same product
        // set → all A products are in B → diff = 0.
        AssertDifference(diff, expectedCount: 0);
    }

    [Fact]
    public void Load_Difference_HalfB()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_Difference(LargeJsonList, LargeJsonListHalf, "Id", "Equals", false, out var diff);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Difference_HalfB));
        // A: Ids 0..9999, HalfB: Ids 0..4999. A \ HalfB = Ids 5000..9999 → 5000.
        AssertDifference(diff, expectedCount: 5000);
    }

    [Fact]
    public void Load_Difference_NoOverlap()
    {
        string smallDisjoint = """[{"Id":999999},{"Id":888888}]""";
        var sw = Stopwatch.StartNew();
        _sut.List_Difference(LargeJsonList, smallDisjoint, "Id", "Equals", false, out var diff);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Difference_NoOverlap));
        // No overlap → all A kept.
        AssertDifference(diff, expectedCount: TargetSize);
    }

    [Fact]
    public void Load_Difference_NotEquals_10k()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_Difference(LargeJsonList, LargeJsonListB, "Status", "NotEquals", false, out var diff);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Difference_NotEquals_10k));
        // B has 4 distinct Status values, so for every A there's some b != keyA → diff = 0.
        AssertDifference(diff, expectedCount: 0);
    }

    [Fact]
    public void Load_Difference_StartsWith_10k()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_Difference(LargeJsonList, LargeJsonListB, "Name", "StartsWith", false, out var diff);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Difference_StartsWith_10k));
        // All names are 9 chars → StartsWith reduces to equality → same as ByName → diff = 5000.
        AssertDifference(diff, expectedCount: 5000);
    }

    [Fact]
    public void Load_Difference_EndsWith_10k()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_Difference(LargeJsonList, LargeJsonListB, "Name", "EndsWith", false, out var diff);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Difference_EndsWith_10k));
        // Same 9-char equality reduction → diff = 5000.
        AssertDifference(diff, expectedCount: 5000);
    }

    [Fact]
    public void Load_Difference_GreaterThan_10k()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_Difference(LargeJsonList, LargeJsonListB, "Id", "GreaterThan", false, out var diff);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Difference_GreaterThan_10k));
        // matchedAny = keyA > min(B)=5000 → remove A ids > 5000 → keep 0..5000 = 5001.
        AssertDifference(diff, expectedCount: 5001);
    }

    [Fact]
    public void Load_Difference_LessOrEqual_10k()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_Difference(LargeJsonList, LargeJsonListB, "Id", "LessOrEqual", false, out var diff);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Difference_LessOrEqual_10k));
        // matchedAny = keyA <= max(B)=14999. All A ids ≤ 9999 ≤ 14999 → diff = 0.
        AssertDifference(diff, expectedCount: 0);
    }

    #endregion

    #region List_Chunk (10)

    private static void AssertChunks(List<string> chunks, int sourceCount, int chunkSize)
    {
        int expectedFull = sourceCount / chunkSize;
        int remainder = sourceCount % chunkSize;
        int expectedCount = expectedFull + (remainder > 0 ? 1 : 0);
        Assert.Equal(expectedCount, chunks.Count);
        int total = 0;
        for (int i = 0; i < chunks.Count; i++)
            total += ParseArray(chunks[i]).Count;
        Assert.Equal(sourceCount, total);
    }

    [Fact]
    public void Load_Chunk_100()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_Chunk(LargeJsonList, 100, out var chunks);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Chunk_100));
        AssertChunks(chunks, TargetSize, 100);
    }

    [Fact]
    public void Load_Chunk_500()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_Chunk(LargeJsonList, 500, out var chunks);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Chunk_500));
        AssertChunks(chunks, TargetSize, 500);
    }

    [Fact]
    public void Load_Chunk_1000()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_Chunk(LargeJsonList, 1000, out var chunks);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Chunk_1000));
        AssertChunks(chunks, TargetSize, 1000);
    }

    [Fact]
    public void Load_Chunk_50()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_Chunk(LargeJsonList, 50, out var chunks);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Chunk_50));
        AssertChunks(chunks, TargetSize, 50);
    }

    [Fact]
    public void Load_Chunk_10()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_Chunk(LargeJsonList, 10, out var chunks);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Chunk_10));
        AssertChunks(chunks, TargetSize, 10);
    }

    [Fact]
    public void Load_Chunk_3_UnevenTail()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_Chunk(LargeJsonList, 3, out var chunks);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Chunk_3_UnevenTail));
        AssertChunks(chunks, TargetSize, 3);
    }

    [Fact]
    public void Load_Chunk_OneMakesEachElementItsOwnChunk()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_Chunk(LargeJsonList, 1, out var chunks);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Chunk_OneMakesEachElementItsOwnChunk));
        AssertChunks(chunks, TargetSize, 1);
    }

    [Fact]
    public void Load_Chunk_LargerThanList_SingleChunk()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_Chunk(LargeJsonList, TargetSize * 2, out var chunks);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Chunk_LargerThanList_SingleChunk));
        Assert.Single(chunks);
        Assert.Equal(TargetSize, ParseArray(chunks[0]).Count);
    }

    [Fact]
    public void Load_Chunk_HalfList()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_Chunk(LargeJsonListHalf, 250, out var chunks);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Chunk_HalfList));
        AssertChunks(chunks, TargetSize / 2, 250);
    }

    [Fact]
    public void Load_Chunk_ZeroSize_ReturnsEmpty()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_Chunk(LargeJsonList, 0, out var chunks);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Chunk_ZeroSize_ReturnsEmpty));
        Assert.Empty(chunks);
    }

    #endregion

    #region List_DistinctBy (10)

    [Fact]
    public void Load_DistinctBy_Status()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_DistinctBy(LargeJsonList, "Status", false, out var result);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_DistinctBy_Status));
        Assert.Equal(4, ParseArray(result).Count);
    }

    [Fact]
    public void Load_DistinctBy_Category()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_DistinctBy(LargeJsonList, "Category", false, out var result);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_DistinctBy_Category));
        Assert.Equal(5, ParseArray(result).Count);
    }

    [Fact]
    public void Load_DistinctBy_NestedRegion()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_DistinctBy(LargeJsonList, "Meta.Region", false, out var result);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_DistinctBy_NestedRegion));
        Assert.Equal(4, ParseArray(result).Count);
    }

    [Fact]
    public void Load_DistinctBy_NestedPriority()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_DistinctBy(LargeJsonList, "Meta.Priority", false, out var result);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_DistinctBy_NestedPriority));
        Assert.Equal(3, ParseArray(result).Count);
    }

    [Fact]
    public void Load_DistinctBy_Id_AllUnique()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_DistinctBy(LargeJsonList, "Id", false, out var result);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_DistinctBy_Id_AllUnique));
        Assert.Equal(TargetSize, ParseArray(result).Count);
    }

    [Fact]
    public void Load_DistinctBy_Name_AllUnique()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_DistinctBy(LargeJsonList, "Name", false, out var result);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_DistinctBy_Name_AllUnique));
        Assert.Equal(TargetSize, ParseArray(result).Count);
    }

    [Fact]
    public void Load_DistinctBy_ArrayIndex()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_DistinctBy(LargeJsonList, "Meta.Tags[0]", false, out var result);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_DistinctBy_ArrayIndex));
        // Tags[0] = $"tag{i%20}" → 20 distinct.
        Assert.Equal(20, ParseArray(result).Count);
    }

    [Fact]
    public void Load_DistinctBy_MissingKey_SingleBucket()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_DistinctBy(LargeJsonList, "DoesNotExist", false, out var result);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_DistinctBy_MissingKey_SingleBucket));
        // All items share the "null key" bucket → one representative kept.
        Assert.Single(ParseArray(result));
    }

    [Fact]
    public void Load_DistinctBy_HalfList_Status()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_DistinctBy(LargeJsonListHalf, "Status", false, out var result);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_DistinctBy_HalfList_Status));
        Assert.Equal(4, ParseArray(result).Count);
    }

    [Fact]
    public void Load_DistinctBy_CaseSensitive_Category()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_DistinctBy(LargeJsonList, "Category", true, out var result);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_DistinctBy_CaseSensitive_Category));
        Assert.Equal(5, ParseArray(result).Count);
    }

    #endregion

    #region List_Slice (10)

    [Fact]
    public void Load_Slice_FirstThousand()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_Slice(LargeJsonList, 0, 1000, 1, out var result);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Slice_FirstThousand));
        Assert.Equal(1000, ParseArray(result).Count);
    }

    [Fact]
    public void Load_Slice_LastThousand_NegativeStart()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_Slice(LargeJsonList, -1000, 0, 1, out var result);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Slice_LastThousand_NegativeStart));
        Assert.Equal(1000, ParseArray(result).Count);
    }

    [Fact]
    public void Load_Slice_MiddleFiveThousand()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_Slice(LargeJsonList, 2500, 7500, 1, out var result);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Slice_MiddleFiveThousand));
        Assert.Equal(5000, ParseArray(result).Count);
    }

    [Fact]
    public void Load_Slice_EveryOther_FullList()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_Slice(LargeJsonList, 0, 0, 2, out var result);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Slice_EveryOther_FullList));
        Assert.Equal(TargetSize / 2, ParseArray(result).Count);
    }

    [Fact]
    public void Load_Slice_EveryTenth()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_Slice(LargeJsonList, 0, 0, 10, out var result);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Slice_EveryTenth));
        Assert.Equal(TargetSize / 10, ParseArray(result).Count);
    }

    [Fact]
    public void Load_Slice_ReverseAll()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_Slice(LargeJsonList, -1, 0, -1, out var result);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Slice_ReverseAll));
        Assert.Equal(TargetSize, ParseArray(result).Count);
    }

    [Fact]
    public void Load_Slice_ReverseMiddle()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_Slice(LargeJsonList, 7500, 2500, -1, out var result);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Slice_ReverseMiddle));
        Assert.Equal(5000, ParseArray(result).Count);
    }

    [Fact]
    public void Load_Slice_ExcludeFirstAndLast()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_Slice(LargeJsonList, 1, -1, 1, out var result);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Slice_ExcludeFirstAndLast));
        Assert.Equal(TargetSize - 2, ParseArray(result).Count);
    }

    [Fact]
    public void Load_Slice_OutOfBoundsClamped()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_Slice(LargeJsonList, -100_000, 100_000, 1, out var result);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Slice_OutOfBoundsClamped));
        Assert.Equal(TargetSize, ParseArray(result).Count);
    }

    [Fact]
    public void Load_Slice_FullListDefaults()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_Slice(LargeJsonList, 0, 0, 1, out var result);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Slice_FullListDefaults));
        Assert.Equal(TargetSize, ParseArray(result).Count);
    }

    #endregion

    #region List_Shuffle (10)

    private static void AssertShufflePreservesElements(string shuffled, int sourceCount)
    {
        var arr = ParseArray(shuffled);
        Assert.Equal(sourceCount, arr.Count);
    }

    [Fact]
    public void Load_Shuffle_Seed_1()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_Shuffle(LargeJsonList, 1, out var result);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Shuffle_Seed_1));
        AssertShufflePreservesElements(result, TargetSize);
    }

    [Fact]
    public void Load_Shuffle_Seed_42()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_Shuffle(LargeJsonList, 42, out var result);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Shuffle_Seed_42));
        AssertShufflePreservesElements(result, TargetSize);
    }

    [Fact]
    public void Load_Shuffle_Seed_1000()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_Shuffle(LargeJsonList, 1000, out var result);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Shuffle_Seed_1000));
        AssertShufflePreservesElements(result, TargetSize);
    }

    [Fact]
    public void Load_Shuffle_Seed_MaxValue()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_Shuffle(LargeJsonList, int.MaxValue, out var result);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Shuffle_Seed_MaxValue));
        AssertShufflePreservesElements(result, TargetSize);
    }

    [Fact]
    public void Load_Shuffle_Seed_MinValue()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_Shuffle(LargeJsonList, int.MinValue, out var result);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Shuffle_Seed_MinValue));
        AssertShufflePreservesElements(result, TargetSize);
    }

    [Fact]
    public void Load_Shuffle_CryptoSeed_Zero()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_Shuffle(LargeJsonList, 0, out var result);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Shuffle_CryptoSeed_Zero));
        AssertShufflePreservesElements(result, TargetSize);
    }

    [Fact]
    public void Load_Shuffle_HalfList()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_Shuffle(LargeJsonListHalf, 7, out var result);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Shuffle_HalfList));
        AssertShufflePreservesElements(result, TargetSize / 2);
    }

    [Fact]
    public void Load_Shuffle_ListB()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_Shuffle(LargeJsonListB, 7, out var result);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Shuffle_ListB));
        AssertShufflePreservesElements(result, TargetSize);
    }

    [Fact]
    public void Load_Shuffle_Determinism_SameSeedSameOutput()
    {
        _sut.List_Shuffle(LargeJsonList, 12345, out var first);
        var sw = Stopwatch.StartNew();
        _sut.List_Shuffle(LargeJsonList, 12345, out var second);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Shuffle_Determinism_SameSeedSameOutput));
        Assert.Equal(first, second);
    }

    [Fact]
    public void Load_Shuffle_ActuallyReorders_Seed99()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_Shuffle(LargeJsonList, 99, out var result);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Shuffle_ActuallyReorders_Seed99));
        Assert.NotEqual(LargeJsonList, result);
        AssertShufflePreservesElements(result, TargetSize);
    }

    #endregion

    #region List_UpdateAt (10)

    [Fact]
    public void Load_UpdateAt_First()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_UpdateAt(LargeJsonList, 0, "Status", "\"Active\"", out var updated, out var previous);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_UpdateAt_First));
        Assert.NotEqual("null", previous);
        var arr = ParseArray(updated);
        Assert.Equal(TargetSize, arr.Count);
        Assert.Equal("Active", arr[0]!["Status"]!.ToString());
    }

    [Fact]
    public void Load_UpdateAt_Last()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_UpdateAt(LargeJsonList, TargetSize - 1, "Status", "\"Done\"", out var updated, out var previous);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_UpdateAt_Last));
        Assert.NotEqual("null", previous);
        var arr = ParseArray(updated);
        Assert.Equal("Done", arr[TargetSize - 1]!["Status"]!.ToString());
    }

    [Fact]
    public void Load_UpdateAt_Middle()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_UpdateAt(LargeJsonList, TargetSize / 2, "Status", "\"Mid\"", out var updated, out var previous);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_UpdateAt_Middle));
        Assert.NotEqual("null", previous);
        var arr = ParseArray(updated);
        Assert.Equal("Mid", arr[TargetSize / 2]!["Status"]!.ToString());
    }

    [Fact]
    public void Load_UpdateAt_NegativeIndex()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_UpdateAt(LargeJsonList, -1, "Status", "\"Tail\"", out var updated, out var previous);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_UpdateAt_NegativeIndex));
        Assert.NotEqual("null", previous);
        var arr = ParseArray(updated);
        Assert.Equal("Tail", arr[TargetSize - 1]!["Status"]!.ToString());
    }

    [Fact]
    public void Load_UpdateAt_NewProperty()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_UpdateAt(LargeJsonList, 100, "NewField", "true", out var updated, out var previous);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_UpdateAt_NewProperty));
        Assert.Equal("null", previous);
        var arr = ParseArray(updated);
        Assert.True(arr[100]!["NewField"]!.GetValue<bool>());
    }

    [Fact]
    public void Load_UpdateAt_NestedPath()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_UpdateAt(LargeJsonList, 500, "Meta.Region", "\"ZZ\"", out var updated, out var previous);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_UpdateAt_NestedPath));
        Assert.NotEqual("null", previous);
        var arr = ParseArray(updated);
        Assert.Equal("ZZ", arr[500]!["Meta"]!["Region"]!.ToString());
    }

    [Fact]
    public void Load_UpdateAt_OutOfRange()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_UpdateAt(LargeJsonList, TargetSize * 10, "Status", "\"X\"", out var updated, out var previous);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_UpdateAt_OutOfRange));
        Assert.Equal("null", previous);
        Assert.Equal(LargeJsonList, updated);
    }

    [Fact]
    public void Load_UpdateAt_ReplaceObject()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_UpdateAt(LargeJsonList, 200, "Meta", """{"Region":"NZ","Priority":"Top"}""", out var updated, out var previous);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_UpdateAt_ReplaceObject));
        Assert.NotEqual("null", previous);
        var arr = ParseArray(updated);
        Assert.Equal("NZ", arr[200]!["Meta"]!["Region"]!.ToString());
        Assert.Equal("Top", arr[200]!["Meta"]!["Priority"]!.ToString());
    }

    [Fact]
    public void Load_UpdateAt_HalfList()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_UpdateAt(LargeJsonListHalf, 10, "Score", "0", out var updated, out var previous);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_UpdateAt_HalfList));
        Assert.NotEqual("null", previous);
        var arr = ParseArray(updated);
        Assert.Equal("0", arr[10]!["Score"]!.ToString());
    }

    [Fact]
    public void Load_UpdateAt_ArrayIndexPath()
    {
        var sw = Stopwatch.StartNew();
        _sut.List_UpdateAt(LargeJsonList, 300, "Meta.Tags[0]", "\"tagOverride\"", out var updated, out var previous);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_UpdateAt_ArrayIndexPath));
        Assert.NotEqual("null", previous);
        var arr = ParseArray(updated);
        Assert.Equal("tagOverride", arr[300]!["Meta"]!["Tags"]!.AsArray()[0]!.ToString());
    }

    #endregion

    // ═════════════════════════════════════════════════════════════════════
    //  v0.5.0 — 14 new actions × 5 load tests each = 70 tests.
    // ═════════════════════════════════════════════════════════════════════

    #region List_MinBy (5)

    [Fact]
    public void Load_MinBy_NumericScore() {
        var sw = Stopwatch.StartNew();
        _sut.List_MinBy(LargeJsonList, "Score", true, out var element, out var minVal, out var idx);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_MinBy_NumericScore));
        Assert.NotEqual(-1, idx);
        Assert.NotEqual("null", element);
    }

    [Fact]
    public void Load_MinBy_NumericId() {
        var sw = Stopwatch.StartNew();
        _sut.List_MinBy(LargeJsonList, "Id", true, out var element, out var minVal, out var idx);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_MinBy_NumericId));
        Assert.Equal("0", minVal);
        Assert.Equal(0, idx);
    }

    [Fact]
    public void Load_MinBy_TextName() {
        var sw = Stopwatch.StartNew();
        _sut.List_MinBy(LargeJsonList, "Name", false, out var element, out var minVal, out var idx);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_MinBy_TextName));
        Assert.NotEqual(-1, idx);
    }

    [Fact]
    public void Load_MinBy_NestedPath() {
        var sw = Stopwatch.StartNew();
        _sut.List_MinBy(LargeJsonList, "Items[0].Qty", true, out var element, out var minVal, out var idx);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_MinBy_NestedPath));
        Assert.Equal("1", minVal);
    }

    [Fact]
    public void Load_MinBy_HalfList() {
        var sw = Stopwatch.StartNew();
        _sut.List_MinBy(LargeJsonListHalf, "Score", true, out var element, out var minVal, out var idx);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_MinBy_HalfList));
        Assert.NotEqual(-1, idx);
    }

    #endregion

    #region List_MaxBy (5)

    [Fact]
    public void Load_MaxBy_NumericScore() {
        var sw = Stopwatch.StartNew();
        _sut.List_MaxBy(LargeJsonList, "Score", true, out _, out var maxVal, out var idx);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_MaxBy_NumericScore));
        Assert.NotEqual(-1, idx);
    }

    [Fact]
    public void Load_MaxBy_NumericId() {
        var sw = Stopwatch.StartNew();
        _sut.List_MaxBy(LargeJsonList, "Id", true, out _, out var maxVal, out var idx);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_MaxBy_NumericId));
        Assert.Equal((TargetSize - 1).ToString(), maxVal);
        Assert.Equal(TargetSize - 1, idx);
    }

    [Fact]
    public void Load_MaxBy_TextName() {
        var sw = Stopwatch.StartNew();
        _sut.List_MaxBy(LargeJsonList, "Name", false, out _, out var maxVal, out var idx);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_MaxBy_TextName));
        Assert.Equal(TargetSize - 1, idx);
    }

    [Fact]
    public void Load_MaxBy_NestedPath() {
        var sw = Stopwatch.StartNew();
        _sut.List_MaxBy(LargeJsonList, "Meta.Region", false, out _, out var maxVal, out var idx);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_MaxBy_NestedPath));
        Assert.NotEqual(-1, idx);
    }

    [Fact]
    public void Load_MaxBy_HalfList() {
        var sw = Stopwatch.StartNew();
        _sut.List_MaxBy(LargeJsonListHalf, "Score", true, out _, out _, out var idx);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_MaxBy_HalfList));
        Assert.NotEqual(-1, idx);
    }

    #endregion

    #region List_Aggregate (5)

    [Fact]
    public void Load_Aggregate_SumScore() {
        var sw = Stopwatch.StartNew();
        _sut.List_Aggregate(LargeJsonList, "Score", "Sum", out var result, out var count);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Aggregate_SumScore));
        Assert.Equal(TargetSize, count);
        Assert.False(string.IsNullOrEmpty(result));
    }

    [Fact]
    public void Load_Aggregate_AvgScore() {
        var sw = Stopwatch.StartNew();
        _sut.List_Aggregate(LargeJsonList, "Score", "Avg", out var result, out var count);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Aggregate_AvgScore));
        Assert.Equal(TargetSize, count);
    }

    [Fact]
    public void Load_Aggregate_CountStatus() {
        var sw = Stopwatch.StartNew();
        _sut.List_Aggregate(LargeJsonList, "Status", "Count", out var result, out var count);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Aggregate_CountStatus));
        Assert.Equal(TargetSize.ToString(), result);
    }

    [Fact]
    public void Load_Aggregate_CountDistinctStatus() {
        var sw = Stopwatch.StartNew();
        _sut.List_Aggregate(LargeJsonList, "Status", "CountDistinct", out var result, out var count);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Aggregate_CountDistinctStatus));
        Assert.Equal("4", result);
    }

    [Fact]
    public void Load_Aggregate_MaxNestedScore() {
        var sw = Stopwatch.StartNew();
        _sut.List_Aggregate(LargeJsonList, "Items[0].Qty", "Max", out var result, out var count);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Aggregate_MaxNestedScore));
        Assert.Equal(TargetSize, count);
        Assert.Equal("10", result);
    }

    #endregion

    #region List_Intersect (5)

    [Fact]
    public void Load_Intersect_ByIdEquals() {
        var sw = Stopwatch.StartNew();
        _sut.List_Intersect(LargeJsonList, LargeJsonListB, "Id", "Equals", false, out var result);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Intersect_ByIdEquals));
        // A ids 0..9999, B ids 5000..14999 → 5000 shared.
        Assert.Equal(5000, ParseArray(result).Count);
    }

    [Fact]
    public void Load_Intersect_ByCategory() {
        var sw = Stopwatch.StartNew();
        _sut.List_Intersect(LargeJsonList, LargeJsonListB, "Category", "Equals", false, out var result);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Intersect_ByCategory));
        Assert.Equal(TargetSize, ParseArray(result).Count);
    }

    [Fact]
    public void Load_Intersect_HalfLists() {
        var sw = Stopwatch.StartNew();
        _sut.List_Intersect(LargeJsonListHalf, LargeJsonListB, "Id", "Equals", false, out var result);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Intersect_HalfLists));
        Assert.Empty(ParseArray(result));
    }

    [Fact]
    public void Load_Intersect_NestedKey() {
        var sw = Stopwatch.StartNew();
        _sut.List_Intersect(LargeJsonList, LargeJsonListB, "Meta.Region", "Equals", false, out var result);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Intersect_NestedKey));
        Assert.Equal(TargetSize, ParseArray(result).Count);
    }

    [Fact]
    public void Load_Intersect_ByName_CaseSensitive() {
        var sw = Stopwatch.StartNew();
        _sut.List_Intersect(LargeJsonList, LargeJsonListB, "Name", "Equals", true, out var result);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Intersect_ByName_CaseSensitive));
        Assert.Equal(5000, ParseArray(result).Count);
    }

    #endregion

    #region List_Union (5)

    [Fact]
    public void Load_Union_ByIdKey() {
        var sw = Stopwatch.StartNew();
        _sut.List_Union(LargeJsonList, LargeJsonListB, "Id", false, out var result);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Union_ByIdKey));
        // A: 0..9999, B: 5000..14999 → union of unique Ids = 15000.
        Assert.Equal(15000, ParseArray(result).Count);
    }

    [Fact]
    public void Load_Union_ByStatus() {
        var sw = Stopwatch.StartNew();
        _sut.List_Union(LargeJsonList, LargeJsonListB, "Status", false, out var result);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Union_ByStatus));
        Assert.Equal(4, ParseArray(result).Count);
    }

    [Fact]
    public void Load_Union_EmptyKeyDedupesByWhole() {
        var sw = Stopwatch.StartNew();
        _sut.List_Union(LargeJsonListHalf, LargeJsonListHalf, "", false, out var result);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Union_EmptyKeyDedupesByWhole));
        Assert.Equal(TargetSize / 2, ParseArray(result).Count);
    }

    [Fact]
    public void Load_Union_HalfLists() {
        var sw = Stopwatch.StartNew();
        _sut.List_Union(LargeJsonListHalf, LargeJsonListB, "Id", false, out var result);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Union_HalfLists));
        // A: 0..4999, B: 5000..14999 → union = 15000.
        Assert.Equal(15000, ParseArray(result).Count);
    }

    [Fact]
    public void Load_Union_NestedKey() {
        var sw = Stopwatch.StartNew();
        _sut.List_Union(LargeJsonList, LargeJsonListB, "Meta.Region", false, out var result);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Union_NestedKey));
        Assert.Equal(4, ParseArray(result).Count);
    }

    #endregion

    #region List_SplitAt (5)

    [Fact]
    public void Load_SplitAt_Middle() {
        var sw = Stopwatch.StartNew();
        _sut.List_SplitAt(LargeJsonList, TargetSize / 2, out var left, out var right);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_SplitAt_Middle));
        Assert.Equal(TargetSize / 2, ParseArray(left).Count);
        Assert.Equal(TargetSize / 2, ParseArray(right).Count);
    }

    [Fact]
    public void Load_SplitAt_First() {
        var sw = Stopwatch.StartNew();
        _sut.List_SplitAt(LargeJsonList, 1, out var left, out var right);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_SplitAt_First));
        Assert.Single(ParseArray(left));
        Assert.Equal(TargetSize - 1, ParseArray(right).Count);
    }

    [Fact]
    public void Load_SplitAt_NegativeIndex() {
        var sw = Stopwatch.StartNew();
        _sut.List_SplitAt(LargeJsonList, -100, out var left, out var right);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_SplitAt_NegativeIndex));
        Assert.Equal(TargetSize - 100, ParseArray(left).Count);
        Assert.Equal(100, ParseArray(right).Count);
    }

    [Fact]
    public void Load_SplitAt_OutOfRangeHigh() {
        var sw = Stopwatch.StartNew();
        _sut.List_SplitAt(LargeJsonList, TargetSize * 2, out var left, out var right);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_SplitAt_OutOfRangeHigh));
        Assert.Equal(TargetSize, ParseArray(left).Count);
        Assert.Empty(ParseArray(right));
    }

    [Fact]
    public void Load_SplitAt_HalfList() {
        var sw = Stopwatch.StartNew();
        _sut.List_SplitAt(LargeJsonListHalf, TargetSize / 4, out var left, out var right);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_SplitAt_HalfList));
        Assert.Equal(TargetSize / 4, ParseArray(left).Count);
    }

    #endregion

    #region List_Partition (5)

    [Fact]
    public void Load_Partition_ByStatus() {
        var sw = Stopwatch.StartNew();
        _sut.List_Partition(LargeJsonList, "Status", "Active", "Equals", false, out var matching, out var nonMatching);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Partition_ByStatus));
        Assert.Equal(TargetSize, ParseArray(matching).Count + ParseArray(nonMatching).Count);
    }

    [Fact]
    public void Load_Partition_NestedPath() {
        var sw = Stopwatch.StartNew();
        _sut.List_Partition(LargeJsonList, "Meta.Region", "EU", "Equals", false, out var matching, out var nonMatching);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Partition_NestedPath));
        Assert.Equal(TargetSize / 4, ParseArray(matching).Count);
    }

    [Fact]
    public void Load_Partition_NumericGreaterThan() {
        var sw = Stopwatch.StartNew();
        _sut.List_Partition(LargeJsonList, "Id", "5000", "GreaterThan", false, out var matching, out var nonMatching);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Partition_NumericGreaterThan));
        Assert.Equal(4999, ParseArray(matching).Count);
    }

    [Fact]
    public void Load_Partition_Contains() {
        var sw = Stopwatch.StartNew();
        _sut.List_Partition(LargeJsonList, "Name", "Item0", "Contains", false, out var matching, out var nonMatching);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Partition_Contains));
        // Names Item00000..Item09999 all start with Item0.
        Assert.Equal(TargetSize, ParseArray(matching).Count);
    }

    [Fact]
    public void Load_Partition_NoMatch() {
        var sw = Stopwatch.StartNew();
        _sut.List_Partition(LargeJsonList, "Status", "Nope", "Equals", false, out var matching, out var nonMatching);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Partition_NoMatch));
        Assert.Empty(ParseArray(matching));
        Assert.Equal(TargetSize, ParseArray(nonMatching).Count);
    }

    #endregion

    #region List_PartitionByConditions (5)

    [Fact]
    public void Load_PartitionByConditions_And() {
        var cond = new List<Condition> {
            new() { Path = "Status", Operator = Operators.Equals, Value = "Active" },
            new() { Path = "Meta.Region", Operator = Operators.Equals, Value = "EU" },
        };
        var sw = Stopwatch.StartNew();
        _sut.List_PartitionByConditions(LargeJsonList, cond, "AND", out var matching, out var nonMatching);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_PartitionByConditions_And));
        Assert.Equal(TargetSize, ParseArray(matching).Count + ParseArray(nonMatching).Count);
    }

    [Fact]
    public void Load_PartitionByConditions_Or() {
        var cond = new List<Condition> {
            new() { Path = "Category", Operator = Operators.Equals, Value = "Books" },
            new() { Path = "Category", Operator = Operators.Equals, Value = "Toys" },
        };
        var sw = Stopwatch.StartNew();
        _sut.List_PartitionByConditions(LargeJsonList, cond, "OR", out var matching, out var nonMatching);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_PartitionByConditions_Or));
        Assert.Equal(2 * TargetSize / 5, ParseArray(matching).Count);
    }

    [Fact]
    public void Load_PartitionByConditions_Numeric() {
        var cond = new List<Condition> {
            new() { Path = "Id", Operator = Operators.GreaterOrEqual, Value = "5000" },
        };
        var sw = Stopwatch.StartNew();
        _sut.List_PartitionByConditions(LargeJsonList, cond, "AND", out var matching, out var nonMatching);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_PartitionByConditions_Numeric));
        Assert.Equal(TargetSize / 2, ParseArray(matching).Count);
    }

    [Fact]
    public void Load_PartitionByConditions_Nested() {
        var cond = new List<Condition> {
            new() { Path = "Meta.Priority", Operator = Operators.Equals, Value = "High" },
        };
        var sw = Stopwatch.StartNew();
        _sut.List_PartitionByConditions(LargeJsonList, cond, "AND", out var matching, out var nonMatching);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_PartitionByConditions_Nested));
        Assert.Equal(TargetSize, ParseArray(matching).Count + ParseArray(nonMatching).Count);
    }

    [Fact]
    public void Load_PartitionByConditions_HalfList() {
        var cond = new List<Condition> {
            new() { Path = "Status", Operator = Operators.Equals, Value = "Active" },
        };
        var sw = Stopwatch.StartNew();
        _sut.List_PartitionByConditions(LargeJsonListHalf, cond, "AND", out var matching, out var nonMatching);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_PartitionByConditions_HalfList));
        Assert.Equal(TargetSize / 2, ParseArray(matching).Count + ParseArray(nonMatching).Count);
    }

    #endregion

    #region List_Reverse (5)

    [Fact]
    public void Load_Reverse_FullList() {
        var sw = Stopwatch.StartNew();
        _sut.List_Reverse(LargeJsonList, out var result);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Reverse_FullList));
        var arr = ParseArray(result);
        Assert.Equal(TargetSize, arr.Count);
        Assert.Equal((TargetSize - 1).ToString(), arr[0]!["Id"]!.ToString());
        Assert.Equal("0", arr[TargetSize - 1]!["Id"]!.ToString());
    }

    [Fact]
    public void Load_Reverse_HalfList() {
        var sw = Stopwatch.StartNew();
        _sut.List_Reverse(LargeJsonListHalf, out var result);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Reverse_HalfList));
        Assert.Equal(TargetSize / 2, ParseArray(result).Count);
    }

    [Fact]
    public void Load_Reverse_ListB() {
        var sw = Stopwatch.StartNew();
        _sut.List_Reverse(LargeJsonListB, out var result);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Reverse_ListB));
        Assert.Equal(TargetSize, ParseArray(result).Count);
    }

    [Fact]
    public void Load_Reverse_MatchesSliceReverse() {
        _sut.List_Reverse(LargeJsonList, out var reversed);
        _sut.List_Slice(LargeJsonList, -1, 0, -1, out var sliced);
        var sw = Stopwatch.StartNew();
        _sut.List_Reverse(LargeJsonList, out var again);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Reverse_MatchesSliceReverse));
        Assert.Equal(reversed, again);
        Assert.Equal(reversed, sliced);
    }

    [Fact]
    public void Load_Reverse_TwiceRestoresOriginal() {
        _sut.List_Reverse(LargeJsonList, out var once);
        var sw = Stopwatch.StartNew();
        _sut.List_Reverse(once, out var twice);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Reverse_TwiceRestoresOriginal));
        Assert.Equal(LargeJsonList, twice);
    }

    #endregion

    #region List_Flatten (5)

    [Fact]
    public void Load_Flatten_ChunkOutput_100() {
        _sut.List_Chunk(LargeJsonList, 100, out var chunks);
        var sw = Stopwatch.StartNew();
        _sut.List_Flatten(chunks, out var flat);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Flatten_ChunkOutput_100));
        Assert.Equal(LargeJsonList, flat);
    }

    [Fact]
    public void Load_Flatten_ChunkOutput_1000() {
        _sut.List_Chunk(LargeJsonList, 1000, out var chunks);
        var sw = Stopwatch.StartNew();
        _sut.List_Flatten(chunks, out var flat);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Flatten_ChunkOutput_1000));
        Assert.Equal(LargeJsonList, flat);
    }

    [Fact]
    public void Load_Flatten_ChunkOutput_10() {
        _sut.List_Chunk(LargeJsonList, 10, out var chunks);
        var sw = Stopwatch.StartNew();
        _sut.List_Flatten(chunks, out var flat);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Flatten_ChunkOutput_10));
        Assert.Equal(TargetSize, ParseArray(flat).Count);
    }

    [Fact]
    public void Load_Flatten_HalfChunks() {
        _sut.List_Chunk(LargeJsonListHalf, 250, out var chunks);
        var sw = Stopwatch.StartNew();
        _sut.List_Flatten(chunks, out var flat);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Flatten_HalfChunks));
        Assert.Equal(TargetSize / 2, ParseArray(flat).Count);
    }

    [Fact]
    public void Load_Flatten_SingletonPerChunk() {
        _sut.List_Chunk(LargeJsonList, 1, out var chunks);
        var sw = Stopwatch.StartNew();
        _sut.List_Flatten(chunks, out var flat);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Flatten_SingletonPerChunk));
        Assert.Equal(TargetSize, ParseArray(flat).Count);
    }

    #endregion

    #region List_Sample (5)

    [Fact]
    public void Load_Sample_HundredDeterministic() {
        var sw = Stopwatch.StartNew();
        _sut.List_Sample(LargeJsonList, 100, 42, out var result);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Sample_HundredDeterministic));
        Assert.Equal(100, ParseArray(result).Count);
    }

    [Fact]
    public void Load_Sample_OneThousandDeterministic() {
        var sw = Stopwatch.StartNew();
        _sut.List_Sample(LargeJsonList, 1000, 7, out var result);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Sample_OneThousandDeterministic));
        Assert.Equal(1000, ParseArray(result).Count);
    }

    [Fact]
    public void Load_Sample_CryptoSeed_Zero() {
        var sw = Stopwatch.StartNew();
        _sut.List_Sample(LargeJsonList, 500, 0, out var result);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Sample_CryptoSeed_Zero));
        Assert.Equal(500, ParseArray(result).Count);
    }

    [Fact]
    public void Load_Sample_SizeExceedsList() {
        var sw = Stopwatch.StartNew();
        _sut.List_Sample(LargeJsonList, TargetSize * 2, 1, out var result);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Sample_SizeExceedsList));
        Assert.Equal(TargetSize, ParseArray(result).Count);
    }

    [Fact]
    public void Load_Sample_HalfList() {
        var sw = Stopwatch.StartNew();
        _sut.List_Sample(LargeJsonListHalf, 250, 3, out var result);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_Sample_HalfList));
        Assert.Equal(250, ParseArray(result).Count);
    }

    #endregion

    #region List_ReplaceWhere (5)

    [Fact]
    public void Load_ReplaceWhere_UpdatesQuarter() {
        var cond = new List<Condition> { new() { Path = "Status", Operator = Operators.Equals, Value = "Active" } };
        var sw = Stopwatch.StartNew();
        _sut.List_ReplaceWhere(LargeJsonList, cond, "AND", "Status", "\"Updated\"", out var updated, out var count);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_ReplaceWhere_UpdatesQuarter));
        Assert.Equal(TargetSize / 4, count);
    }

    [Fact]
    public void Load_ReplaceWhere_NestedUpdate() {
        var cond = new List<Condition> { new() { Path = "Category", Operator = Operators.Equals, Value = "Books" } };
        var sw = Stopwatch.StartNew();
        _sut.List_ReplaceWhere(LargeJsonList, cond, "AND", "Meta.Region", "\"NA\"", out var updated, out var count);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_ReplaceWhere_NestedUpdate));
        Assert.Equal(TargetSize / 5, count);
    }

    [Fact]
    public void Load_ReplaceWhere_NoMatch() {
        var cond = new List<Condition> { new() { Path = "Status", Operator = Operators.Equals, Value = "Nope" } };
        var sw = Stopwatch.StartNew();
        _sut.List_ReplaceWhere(LargeJsonList, cond, "AND", "Status", "\"X\"", out var updated, out var count);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_ReplaceWhere_NoMatch));
        Assert.Equal(0, count);
    }

    [Fact]
    public void Load_ReplaceWhere_NumericCondition() {
        // Exercise the multi-condition path — AND across a numeric and a text filter.
        var cond = new List<Condition> {
            new() { Path = "Id", Operator = Operators.GreaterOrEqual, Value = "5000" },
            new() { Path = "Status", Operator = Operators.Equals, Value = "Active" },
        };
        var sw = Stopwatch.StartNew();
        _sut.List_ReplaceWhere(LargeJsonList, cond, "AND", "Status", "\"Bulk\"", out var updated, out var count);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_ReplaceWhere_NumericCondition));
        // 50% of ids >= 5000, then 25% of those are Active.
        Assert.Equal(TargetSize / 8, count);
    }

    [Fact]
    public void Load_ReplaceWhere_HalfList() {
        var cond = new List<Condition> { new() { Path = "Status", Operator = Operators.Equals, Value = "Active" } };
        var sw = Stopwatch.StartNew();
        _sut.List_ReplaceWhere(LargeJsonListHalf, cond, "AND", "Status", "\"Y\"", out var updated, out var count);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_ReplaceWhere_HalfList));
        Assert.Equal(TargetSize / 8, count);
    }

    #endregion

    #region List_UpdateMultipleAt (5)

    [Fact]
    public void Load_UpdateMultipleAt_TenIndices() {
        var sw = Stopwatch.StartNew();
        _sut.List_UpdateMultipleAt(LargeJsonList, "0,100,200,300,400,500,600,700,800,900", "Status", "\"X\"", out var updated, out var count);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_UpdateMultipleAt_TenIndices));
        Assert.Equal(10, count);
    }

    [Fact]
    public void Load_UpdateMultipleAt_HundredIndices() {
        var indices = string.Join(",", Enumerable.Range(0, 100).Select(i => i * 50));
        var sw = Stopwatch.StartNew();
        _sut.List_UpdateMultipleAt(LargeJsonList, indices, "Status", "\"X\"", out var updated, out var count);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_UpdateMultipleAt_HundredIndices));
        Assert.Equal(100, count);
    }

    [Fact]
    public void Load_UpdateMultipleAt_ThousandIndices() {
        var indices = string.Join(",", Enumerable.Range(0, 1000).Select(i => i * 9));
        var sw = Stopwatch.StartNew();
        _sut.List_UpdateMultipleAt(LargeJsonList, indices, "Status", "\"X\"", out var updated, out var count);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_UpdateMultipleAt_ThousandIndices));
        Assert.Equal(1000, count);
    }

    [Fact]
    public void Load_UpdateMultipleAt_NegativeIndices() {
        var sw = Stopwatch.StartNew();
        _sut.List_UpdateMultipleAt(LargeJsonList, "-1,-100,-1000", "Status", "\"Tail\"", out var updated, out var count);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_UpdateMultipleAt_NegativeIndices));
        Assert.Equal(3, count);
    }

    [Fact]
    public void Load_UpdateMultipleAt_NestedProperty() {
        var sw = Stopwatch.StartNew();
        _sut.List_UpdateMultipleAt(LargeJsonList, "0,1,2,3,4", "Meta.Region", "\"XX\"", out var updated, out var count);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_UpdateMultipleAt_NestedProperty));
        Assert.Equal(5, count);
    }

    #endregion

    #region List_ZipMany (5)

    [Fact]
    public void Load_ZipMany_ThreeFullLists() {
        var lists = new List<string> { LargeJsonList, LargeJsonListB, LargeJsonListHalf };
        var keys = new List<string> { "A", "B", "H" };
        var sw = Stopwatch.StartNew();
        _sut.List_ZipMany(lists, keys, out var result);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_ZipMany_ThreeFullLists));
        Assert.Equal(TargetSize / 2, ParseArray(result).Count);
    }

    [Fact]
    public void Load_ZipMany_TwoLists() {
        var lists = new List<string> { LargeJsonList, LargeJsonListB };
        var keys = new List<string> { "A", "B" };
        var sw = Stopwatch.StartNew();
        _sut.List_ZipMany(lists, keys, out var result);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_ZipMany_TwoLists));
        Assert.Equal(TargetSize, ParseArray(result).Count);
    }

    [Fact]
    public void Load_ZipMany_FiveEqualLists() {
        var lists = new List<string> { LargeJsonListHalf, LargeJsonListHalf, LargeJsonListHalf, LargeJsonListHalf, LargeJsonListHalf };
        var keys = new List<string> { "A", "B", "C", "D", "E" };
        var sw = Stopwatch.StartNew();
        _sut.List_ZipMany(lists, keys, out var result);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_ZipMany_FiveEqualLists));
        Assert.Equal(TargetSize / 2, ParseArray(result).Count);
    }

    [Fact]
    public void Load_ZipMany_DefaultKeyNames() {
        var lists = new List<string> { LargeJsonListHalf, LargeJsonListHalf };
        var keys = new List<string>(); // no explicit labels
        var sw = Stopwatch.StartNew();
        _sut.List_ZipMany(lists, keys, out var result);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_ZipMany_DefaultKeyNames));
        Assert.Equal(TargetSize / 2, ParseArray(result).Count);
    }

    [Fact]
    public void Load_ZipMany_HalfPlusFull_TruncatesToHalf() {
        var lists = new List<string> { LargeJsonList, LargeJsonListHalf };
        var keys = new List<string> { "Big", "Small" };
        var sw = Stopwatch.StartNew();
        _sut.List_ZipMany(lists, keys, out var result);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_ZipMany_HalfPlusFull_TruncatesToHalf));
        Assert.Equal(TargetSize / 2, ParseArray(result).Count);
    }

    #endregion

    #region List_ZipManyGroupBy (5)

    [Fact]
    public void Load_ZipManyGroupBy_ThreeListsByStatus() {
        var lists = new List<string> { LargeJsonListHalf, LargeJsonListHalf, LargeJsonListHalf };
        var keys = new List<string> { "Status", "Status", "Status" };
        var names = new List<string> { "A", "B", "C" };
        var sw = Stopwatch.StartNew();
        _sut.List_ZipManyGroupBy(lists, keys, names, false, out var result);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_ZipManyGroupBy_ThreeListsByStatus));
        Assert.Equal(4, ParseArray(result).Count);
    }

    [Fact]
    public void Load_ZipManyGroupBy_ByCategory() {
        var lists = new List<string> { LargeJsonListHalf, LargeJsonListHalf };
        var keys = new List<string> { "Category", "Category" };
        var names = new List<string> { "L", "R" };
        var sw = Stopwatch.StartNew();
        _sut.List_ZipManyGroupBy(lists, keys, names, false, out var result);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_ZipManyGroupBy_ByCategory));
        Assert.Equal(5, ParseArray(result).Count);
    }

    [Fact]
    public void Load_ZipManyGroupBy_ByNestedRegion() {
        var lists = new List<string> { LargeJsonListHalf, LargeJsonListHalf };
        var keys = new List<string> { "Meta.Region", "Meta.Region" };
        var names = new List<string> { "L", "R" };
        var sw = Stopwatch.StartNew();
        _sut.List_ZipManyGroupBy(lists, keys, names, false, out var result);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_ZipManyGroupBy_ByNestedRegion));
        Assert.Equal(4, ParseArray(result).Count);
    }

    [Fact]
    public void Load_ZipManyGroupBy_MissingKeyLandsInUnknown() {
        var lists = new List<string> { LargeJsonListHalf, LargeJsonListHalf };
        var keys = new List<string> { "DoesNotExist", "Category" };
        var names = new List<string> { "L", "R" };
        var sw = Stopwatch.StartNew();
        _sut.List_ZipManyGroupBy(lists, keys, names, false, out var result);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_ZipManyGroupBy_MissingKeyLandsInUnknown));
        Assert.Contains(ParseArray(result), g => g!["Key"]!.ToString() == "Unknown");
    }

    [Fact]
    public void Load_ZipManyGroupBy_LargeCardinality_ById() {
        var lists = new List<string> { LargeJsonListHalf, LargeJsonListHalf };
        var keys = new List<string> { "Id", "Id" };
        var names = new List<string> { "L", "R" };
        var sw = Stopwatch.StartNew();
        _sut.List_ZipManyGroupBy(lists, keys, names, false, out var result);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_ZipManyGroupBy_LargeCardinality_ById));
        Assert.Equal(TargetSize / 2, ParseArray(result).Count);
    }

    #endregion
}
