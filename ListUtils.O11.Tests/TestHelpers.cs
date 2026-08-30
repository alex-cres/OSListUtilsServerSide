using System.Collections.Generic;
using System.Linq;

namespace ListUtils.Tests;

// ── O11 adapter types ─────────────────────────────────────────────────────────
// Mirror the ODC IListUtils surface so that all test files are byte-for-byte
// identical to the ODC test project.

// Shadow of the ODC ListUtils.Condition [OSStructure], defined here in the
// ListUtils.Tests namespace so the test files can construct `new Condition
// { Path = ..., Operator = ..., Value = ..., CaseSensitive = ... }` and have
// it resolve identically on both platforms. The wrapper below translates
// to OutSystems.NssListUtils.Condition when calling the O11 side.
internal struct Condition
{
    public string Path;
    public string Operator;
    public string Value;
    public bool CaseSensitive;
}

// Shadows of the ODC ListUtils.Operators / LogicalOperators / AggregateOperations
// static classes. Defined in ListUtils.Tests so test files can reference
// `Operators.Equals`, `LogicalOperators.AND`, etc. identically on both platforms.
internal static class Operators
{
    // 'new' suppresses CS0108: name clash with object.Equals(object).
    public new const string Equals = "Equals";
    public const string NotEquals = "NotEquals";
    public const string Contains = "Contains";
    public const string StartsWith = "StartsWith";
    public const string EndsWith = "EndsWith";
    public const string GreaterThan = "GreaterThan";
    public const string LessThan = "LessThan";
    public const string GreaterOrEqual = "GreaterOrEqual";
    public const string LessOrEqual = "LessOrEqual";
}

internal static class LogicalOperators
{
    public const string AND = "AND";
    public const string OR = "OR";
}

internal static class AggregateOperations
{
    public const string Sum = "Sum";
    public const string Avg = "Avg";
    public const string Min = "Min";
    public const string Max = "Max";
    public const string Count = "Count";
    public const string CountDistinct = "CountDistinct";
}

internal interface IListUtils
{
    void List_Pop(string SourceListJson, int Index, out string UpdatedListJson, out string PoppedElementJson);
    void List_PopMultiple(string SourceListJson, string IndicesToPop, out string UpdatedListJson, out string PoppedElementsJson);
    void List_PopByCondition(string SourceListJson, string PropertyName, string TargetValue, string ComparisonOperator, bool CaseSensitive, bool SearchFromEnd, out string UpdatedListJson, out string PoppedElementJson);
    void List_PopMultipleByCondition(string SourceListJson, string PropertyName, string TargetValue, string ComparisonOperator, bool CaseSensitive, out string UpdatedListJson, out string PoppedElementsJson);
    void List_PopByConditions(string SourceListJson, List<Condition> Conditions, string LogicalOperator, bool SearchFromEnd, out string UpdatedListJson, out string PoppedElementJson);
    void List_PopMultipleByConditions(string SourceListJson, List<Condition> Conditions, string LogicalOperator, out string UpdatedListJson, out string PoppedElementsJson);
    void List_Zip(string ListAJson, string ListBJson, string KeyNameA, string KeyNameB, out string ZippedListJson);
    void List_GroupBy(string SourceListJson, string PropertyName, out string GroupedListJson);
    void List_ZipGroupBy(string ListAJson, string ListBJson, string KeyPropertyA, string KeyPropertyB, string KeyNameA, string KeyNameB, bool CaseSensitive, out string GroupedListJson);
    void List_Difference(string ListAJson, string ListBJson, string MatchKey, string ComparisonOperator, bool CaseSensitive, out string DifferenceListJson);
    void List_Chunk(string SourceListJson, int ChunkSize, out List<string> ChunksListJson);
    void List_DistinctBy(string SourceListJson, string PropertyName, bool CaseSensitive, out string DistinctListJson);
    void List_Slice(string SourceListJson, int Start, int End, int Step, out string SliceListJson);
    void List_Shuffle(string SourceListJson, int Seed, out string ShuffledListJson);
    void List_UpdateAt(string SourceListJson, int Index, string PropertyName, string NewValueJson, out string UpdatedListJson, out string PreviousValueJson);
    void List_MinBy(string SourceListJson, string PropertyName, bool NumericMode, out string ElementJson, out string MinValue, out int MinIndex);
    void List_MaxBy(string SourceListJson, string PropertyName, bool NumericMode, out string ElementJson, out string MaxValue, out int MaxIndex);
    void List_Aggregate(string SourceListJson, string PropertyName, string Operation, out string ResultValue, out int MatchedCount);
    void List_Intersect(string ListAJson, string ListBJson, string MatchKey, string ComparisonOperator, bool CaseSensitive, out string IntersectionListJson);
    void List_Union(string ListAJson, string ListBJson, string MatchKey, bool CaseSensitive, out string UnionListJson);
    void List_SplitAt(string SourceListJson, int Index, out string LeftListJson, out string RightListJson);
    void List_Partition(string SourceListJson, string PropertyName, string TargetValue, string ComparisonOperator, bool CaseSensitive, out string MatchingListJson, out string NonMatchingListJson);
    void List_PartitionByConditions(string SourceListJson, List<Condition> Conditions, string LogicalOperator, out string MatchingListJson, out string NonMatchingListJson);
    void List_Reverse(string SourceListJson, out string ReversedListJson);
    void List_Flatten(List<string> ChunksListJson, out string FlatListJson);
    void List_Sample(string SourceListJson, int SampleSize, int Seed, out string SampleListJson);
    void List_ReplaceWhere(string SourceListJson, List<Condition> Conditions, string LogicalOperator, string UpdateProperty, string NewValueJson, out string UpdatedListJson, out int MatchCount);
    void List_UpdateMultipleAt(string SourceListJson, string IndicesToUpdate, string PropertyName, string NewValueJson, out string UpdatedListJson, out int UpdatedCount);
    void List_ZipMany(List<string> ListsJson, List<string> KeyNamesJson, out string ZippedListJson);
    void List_ZipManyGroupBy(List<string> ListsJson, List<string> KeyPropertiesJson, List<string> KeyNamesJson, bool CaseSensitive, out string GroupedListJson);
}

internal sealed class ListUtils : IListUtils
{
    private readonly OutSystems.NssListUtils.CssListUtils _inner = new();

    public void List_Pop(string SourceListJson, int Index, out string UpdatedListJson, out string PoppedElementJson)
        => _inner.MssList_Pop(SourceListJson, Index, out UpdatedListJson, out PoppedElementJson);

    public void List_PopMultiple(string SourceListJson, string IndicesToPop, out string UpdatedListJson, out string PoppedElementsJson)
        => _inner.MssList_PopMultiple(SourceListJson, IndicesToPop, out UpdatedListJson, out PoppedElementsJson);

    public void List_PopByCondition(string SourceListJson, string PropertyName, string TargetValue, string ComparisonOperator, bool CaseSensitive, bool SearchFromEnd, out string UpdatedListJson, out string PoppedElementJson)
        => _inner.MssList_PopByCondition(SourceListJson, PropertyName, TargetValue, ComparisonOperator, CaseSensitive, SearchFromEnd, out UpdatedListJson, out PoppedElementJson);

    public void List_PopMultipleByCondition(string SourceListJson, string PropertyName, string TargetValue, string ComparisonOperator, bool CaseSensitive, out string UpdatedListJson, out string PoppedElementsJson)
        => _inner.MssList_PopMultipleByCondition(SourceListJson, PropertyName, TargetValue, ComparisonOperator, CaseSensitive, out UpdatedListJson, out PoppedElementsJson);

    public void List_PopByConditions(string SourceListJson, List<Condition> Conditions, string LogicalOperator, bool SearchFromEnd, out string UpdatedListJson, out string PoppedElementJson)
        => _inner.MssList_PopByConditions(SourceListJson, ToO11(Conditions), LogicalOperator, SearchFromEnd, out UpdatedListJson, out PoppedElementJson);

    public void List_PopMultipleByConditions(string SourceListJson, List<Condition> Conditions, string LogicalOperator, out string UpdatedListJson, out string PoppedElementsJson)
        => _inner.MssList_PopMultipleByConditions(SourceListJson, ToO11(Conditions), LogicalOperator, out UpdatedListJson, out PoppedElementsJson);

    public void List_Zip(string ListAJson, string ListBJson, string KeyNameA, string KeyNameB, out string ZippedListJson)
        => _inner.MssList_Zip(ListAJson, ListBJson, KeyNameA, KeyNameB, out ZippedListJson);

    public void List_GroupBy(string SourceListJson, string PropertyName, out string GroupedListJson)
        => _inner.MssList_GroupBy(SourceListJson, PropertyName, out GroupedListJson);

    public void List_ZipGroupBy(string ListAJson, string ListBJson, string KeyPropertyA, string KeyPropertyB, string KeyNameA, string KeyNameB, bool CaseSensitive, out string GroupedListJson)
        => _inner.MssList_ZipGroupBy(ListAJson, ListBJson, KeyPropertyA, KeyPropertyB, KeyNameA, KeyNameB, CaseSensitive, out GroupedListJson);

    public void List_Difference(string ListAJson, string ListBJson, string MatchKey, string ComparisonOperator, bool CaseSensitive, out string DifferenceListJson)
        => _inner.MssList_Difference(ListAJson, ListBJson, MatchKey, ComparisonOperator, CaseSensitive, out DifferenceListJson);

    public void List_Chunk(string SourceListJson, int ChunkSize, out List<string> ChunksListJson)
        => _inner.MssList_Chunk(SourceListJson, ChunkSize, out ChunksListJson);

    public void List_DistinctBy(string SourceListJson, string PropertyName, bool CaseSensitive, out string DistinctListJson)
        => _inner.MssList_DistinctBy(SourceListJson, PropertyName, CaseSensitive, out DistinctListJson);

    public void List_Slice(string SourceListJson, int Start, int End, int Step, out string SliceListJson)
        => _inner.MssList_Slice(SourceListJson, Start, End, Step, out SliceListJson);

    public void List_Shuffle(string SourceListJson, int Seed, out string ShuffledListJson)
        => _inner.MssList_Shuffle(SourceListJson, Seed, out ShuffledListJson);

    public void List_UpdateAt(string SourceListJson, int Index, string PropertyName, string NewValueJson, out string UpdatedListJson, out string PreviousValueJson)
        => _inner.MssList_UpdateAt(SourceListJson, Index, PropertyName, NewValueJson, out UpdatedListJson, out PreviousValueJson);

    public void List_MinBy(string SourceListJson, string PropertyName, bool NumericMode, out string ElementJson, out string MinValue, out int MinIndex)
        => _inner.MssList_MinBy(SourceListJson, PropertyName, NumericMode, out ElementJson, out MinValue, out MinIndex);
    public void List_MaxBy(string SourceListJson, string PropertyName, bool NumericMode, out string ElementJson, out string MaxValue, out int MaxIndex)
        => _inner.MssList_MaxBy(SourceListJson, PropertyName, NumericMode, out ElementJson, out MaxValue, out MaxIndex);
    public void List_Aggregate(string SourceListJson, string PropertyName, string Operation, out string ResultValue, out int MatchedCount)
        => _inner.MssList_Aggregate(SourceListJson, PropertyName, Operation, out ResultValue, out MatchedCount);
    public void List_Intersect(string ListAJson, string ListBJson, string MatchKey, string ComparisonOperator, bool CaseSensitive, out string IntersectionListJson)
        => _inner.MssList_Intersect(ListAJson, ListBJson, MatchKey, ComparisonOperator, CaseSensitive, out IntersectionListJson);
    public void List_Union(string ListAJson, string ListBJson, string MatchKey, bool CaseSensitive, out string UnionListJson)
        => _inner.MssList_Union(ListAJson, ListBJson, MatchKey, CaseSensitive, out UnionListJson);
    public void List_SplitAt(string SourceListJson, int Index, out string LeftListJson, out string RightListJson)
        => _inner.MssList_SplitAt(SourceListJson, Index, out LeftListJson, out RightListJson);
    public void List_Partition(string SourceListJson, string PropertyName, string TargetValue, string ComparisonOperator, bool CaseSensitive, out string MatchingListJson, out string NonMatchingListJson)
        => _inner.MssList_Partition(SourceListJson, PropertyName, TargetValue, ComparisonOperator, CaseSensitive, out MatchingListJson, out NonMatchingListJson);
    public void List_PartitionByConditions(string SourceListJson, List<Condition> Conditions, string LogicalOperator, out string MatchingListJson, out string NonMatchingListJson)
        => _inner.MssList_PartitionByConditions(SourceListJson, ToO11(Conditions), LogicalOperator, out MatchingListJson, out NonMatchingListJson);
    public void List_Reverse(string SourceListJson, out string ReversedListJson)
        => _inner.MssList_Reverse(SourceListJson, out ReversedListJson);
    public void List_Flatten(List<string> ChunksListJson, out string FlatListJson)
        => _inner.MssList_Flatten(ChunksListJson, out FlatListJson);
    public void List_Sample(string SourceListJson, int SampleSize, int Seed, out string SampleListJson)
        => _inner.MssList_Sample(SourceListJson, SampleSize, Seed, out SampleListJson);
    public void List_ReplaceWhere(string SourceListJson, List<Condition> Conditions, string LogicalOperator, string UpdateProperty, string NewValueJson, out string UpdatedListJson, out int MatchCount)
        => _inner.MssList_ReplaceWhere(SourceListJson, ToO11(Conditions), LogicalOperator, UpdateProperty, NewValueJson, out UpdatedListJson, out MatchCount);

    // Shallow copy from the shadow struct to the O11 project's struct.
    private static List<OutSystems.NssListUtils.Condition> ToO11(List<Condition> src)
        => src == null ? null : src.Select(c => new OutSystems.NssListUtils.Condition {
            Path = c.Path, Operator = c.Operator, Value = c.Value, CaseSensitive = c.CaseSensitive
        }).ToList();
    public void List_UpdateMultipleAt(string SourceListJson, string IndicesToUpdate, string PropertyName, string NewValueJson, out string UpdatedListJson, out int UpdatedCount)
        => _inner.MssList_UpdateMultipleAt(SourceListJson, IndicesToUpdate, PropertyName, NewValueJson, out UpdatedListJson, out UpdatedCount);
    public void List_ZipMany(List<string> ListsJson, List<string> KeyNamesJson, out string ZippedListJson)
        => _inner.MssList_ZipMany(ListsJson, KeyNamesJson, out ZippedListJson);
    public void List_ZipManyGroupBy(List<string> ListsJson, List<string> KeyPropertiesJson, List<string> KeyNamesJson, bool CaseSensitive, out string GroupedListJson)
        => _inner.MssList_ZipManyGroupBy(ListsJson, KeyPropertiesJson, KeyNamesJson, CaseSensitive, out GroupedListJson);
}
