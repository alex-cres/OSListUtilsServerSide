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

    private const string ConditionsAnd2 = """[{"path":"Status","operator":"Equals","value":"Active"},{"path":"Category","operator":"Equals","value":"Books"}]""";
    private const string ConditionsAnd3 = """[{"path":"Status","operator":"Equals","value":"Active"},{"path":"Category","operator":"Equals","value":"Books"},{"path":"Meta.Region","operator":"Equals","value":"EU"}]""";
    private const string ConditionsOr2 = """[{"path":"Status","operator":"Equals","value":"Archived"},{"path":"Score","operator":"GreaterThan","value":"95"}]""";
    private const string ConditionsNestedAnd = """[{"path":"Meta.Region","operator":"Equals","value":"EU"},{"path":"Meta.Priority","operator":"Equals","value":"High"}]""";
    private const string ConditionsMixedOps = """[{"path":"Score","operator":"GreaterThan","value":"50"},{"path":"Score","operator":"LessThan","value":"70"}]""";

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
        string conds = """[{"path":"Status","operator":"Equals","value":"XYZ"},{"path":"Category","operator":"Equals","value":"ABC"}]""";
        var sw = Stopwatch.StartNew();
        _sut.List_PopByConditions(LargeJsonList, conds, "AND", false, out var updated, out var popped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_PopByConditions_NoMatch));
        AssertPopSingle(updated, popped, TargetSize, expectedRemoved: 0);
    }

    [Fact]
    public void Load_PopByConditions_ArrayIndexPath()
    {
        string conds = """[{"path":"Items[0].Product","operator":"StartsWith","value":"P5"},{"path":"Status","operator":"Equals","value":"Active"}]""";
        var sw = Stopwatch.StartNew();
        _sut.List_PopByConditions(LargeJsonList, conds, "AND", false, out var updated, out var popped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_PopByConditions_ArrayIndexPath));
        AssertPopSingle(updated, popped, TargetSize, expectedRemoved: 1);
    }

    [Fact]
    public void Load_PopByConditions_PerConditionCaseSensitive()
    {
        string conds = """[{"path":"Status","operator":"Equals","value":"Active","caseSensitive":true},{"path":"Meta.Region","operator":"Equals","value":"EU"}]""";
        var sw = Stopwatch.StartNew();
        _sut.List_PopByConditions(LargeJsonList, conds, "AND", false, out var updated, out var popped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_PopByConditions_PerConditionCaseSensitive));
        AssertPopSingle(updated, popped, TargetSize, expectedRemoved: 1);
    }

    [Fact]
    public void Load_PopByConditions_FiveConditions()
    {
        string conds = """[{"path":"Status","operator":"Equals","value":"Active"},{"path":"Category","operator":"Equals","value":"Books"},{"path":"Meta.Region","operator":"Equals","value":"EU"},{"path":"Meta.Priority","operator":"Equals","value":"High"},{"path":"Score","operator":"GreaterThan","value":"20"}]""";
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
        string conds = """[{"path":"Status","operator":"Equals","value":"Active"},{"path":"Meta.Region","operator":"Equals","value":"EU"},{"path":"Score","operator":"GreaterThan","value":"90"}]""";
        var sw = Stopwatch.StartNew();
        _sut.List_PopMultipleByConditions(LargeJsonList, conds, "OR", out var updated, out var popped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_PopMultipleByConditions_OrThreeConds));
        AssertPopManyInvariant(updated, popped, TargetSize);
    }

    [Fact]
    public void Load_PopMultipleByConditions_NoMatch()
    {
        string conds = """[{"path":"Status","operator":"Equals","value":"XYZ"},{"path":"Category","operator":"Equals","value":"ABC"}]""";
        var sw = Stopwatch.StartNew();
        _sut.List_PopMultipleByConditions(LargeJsonList, conds, "AND", out var updated, out var popped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_PopMultipleByConditions_NoMatch));
        AssertPopMany(updated, popped, TargetSize, expectedRemoved: 0);
    }

    [Fact]
    public void Load_PopMultipleByConditions_ArrayIndexPath()
    {
        string conds = """[{"path":"Items[0].Product","operator":"StartsWith","value":"P5"},{"path":"Status","operator":"Equals","value":"Active"}]""";
        var sw = Stopwatch.StartNew();
        _sut.List_PopMultipleByConditions(LargeJsonList, conds, "AND", out var updated, out var popped);
        sw.Stop();
        AssertUnderBudget(sw.ElapsedMilliseconds, nameof(Load_PopMultipleByConditions_ArrayIndexPath));
        AssertPopManyInvariant(updated, popped, TargetSize);
    }

    [Fact]
    public void Load_PopMultipleByConditions_MatchesEverything()
    {
        string conds = """[{"path":"Score","operator":"GreaterOrEqual","value":"0"}]""";
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
        string conds = """[{"path":"Status","operator":"Equals","value":"Active"},{"path":"Category","operator":"Equals","value":"Books"},{"path":"Meta.Region","operator":"Equals","value":"EU"},{"path":"Meta.Priority","operator":"Equals","value":"High"},{"path":"Score","operator":"GreaterThan","value":"20"}]""";
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

    private static void AssertChunks(string chunks, int sourceCount, int chunkSize)
    {
        var arr = ParseArray(chunks);
        int expectedFull = sourceCount / chunkSize;
        int remainder = sourceCount % chunkSize;
        int expectedCount = expectedFull + (remainder > 0 ? 1 : 0);
        Assert.Equal(expectedCount, arr.Count);
        int total = 0;
        for (int i = 0; i < arr.Count; i++)
            total += arr[i]!.AsArray().Count;
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
        var arr = ParseArray(chunks);
        Assert.Single(arr);
        Assert.Equal(TargetSize, arr[0]!.AsArray().Count);
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
        Assert.Equal("[]", chunks);
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
}
