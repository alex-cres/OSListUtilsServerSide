using System.Collections.Generic;
using System.Linq;

namespace ListUtils.Tests;

// ── O11 adapter types ─────────────────────────────────────────────────────────
// Mirror the ODC IListUtils surface so that all test files are byte-for-byte
// identical to the ODC test project.

internal interface IListUtils
{
    void List_Pop(string SourceListJson, int Index, out string UpdatedListJson, out string PoppedElementJson);
    void List_PopMultiple(string SourceListJson, string IndicesToPop, out string UpdatedListJson, out string PoppedElementsJson);
    void List_PopByCondition(string SourceListJson, string PropertyName, string TargetValue, string ComparisonOperator, bool CaseSensitive, bool SearchFromEnd, out string UpdatedListJson, out string PoppedElementJson);
    void List_PopMultipleByCondition(string SourceListJson, string PropertyName, string TargetValue, string ComparisonOperator, bool CaseSensitive, out string UpdatedListJson, out string PoppedElementsJson);
    void List_PopByConditions(string SourceListJson, string ConditionsJson, string LogicalOperator, bool SearchFromEnd, out string UpdatedListJson, out string PoppedElementJson);
    void List_PopMultipleByConditions(string SourceListJson, string ConditionsJson, string LogicalOperator, out string UpdatedListJson, out string PoppedElementsJson);
    void List_Zip(string ListAJson, string ListBJson, string KeyNameA, string KeyNameB, out string ZippedListJson);
    void List_GroupBy(string SourceListJson, string PropertyName, out string GroupedListJson);
    void List_Difference(string ListAJson, string ListBJson, string MatchKey, string ComparisonOperator, bool CaseSensitive, out string DifferenceListJson);
    void List_Chunk(string SourceListJson, int ChunkSize, out string ChunksListJson);
    void List_DistinctBy(string SourceListJson, string PropertyName, bool CaseSensitive, out string DistinctListJson);
    void List_Slice(string SourceListJson, int Start, int End, int Step, out string SliceListJson);
    void List_Shuffle(string SourceListJson, int Seed, out string ShuffledListJson);
    void List_UpdateAt(string SourceListJson, int Index, string PropertyName, string NewValueJson, out string UpdatedListJson, out string PreviousValueJson);

    // In-place variants
    void List_PopInPlace(ref string SourceListJson, int Index, out string PoppedElementJson);
    void List_PopMultipleInPlace(ref string SourceListJson, string IndicesToPop, out string PoppedElementsJson);
    void List_PopByConditionInPlace(ref string SourceListJson, string PropertyName, string TargetValue, string ComparisonOperator, bool CaseSensitive, bool SearchFromEnd, out string PoppedElementJson);
    void List_PopMultipleByConditionInPlace(ref string SourceListJson, string PropertyName, string TargetValue, string ComparisonOperator, bool CaseSensitive, out string PoppedElementsJson);
    void List_PopByConditionsInPlace(ref string SourceListJson, string ConditionsJson, string LogicalOperator, bool SearchFromEnd, out string PoppedElementJson);
    void List_PopMultipleByConditionsInPlace(ref string SourceListJson, string ConditionsJson, string LogicalOperator, out string PoppedElementsJson);
    void List_ZipInPlace(ref string ListAJson, string ListBJson, string KeyNameA, string KeyNameB);
    void List_GroupByInPlace(ref string SourceListJson, string PropertyName);
    void List_DifferenceInPlace(ref string ListAJson, string ListBJson, string MatchKey, string ComparisonOperator, bool CaseSensitive);
    void List_ChunkInPlace(ref string SourceListJson, int ChunkSize);
    void List_DistinctByInPlace(ref string SourceListJson, string PropertyName, bool CaseSensitive);
    void List_SliceInPlace(ref string SourceListJson, int Start, int End, int Step);
    void List_ShuffleInPlace(ref string SourceListJson, int Seed);
    void List_UpdateAtInPlace(ref string SourceListJson, int Index, string PropertyName, string NewValueJson, out string PreviousValueJson);
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

    public void List_PopByConditions(string SourceListJson, string ConditionsJson, string LogicalOperator, bool SearchFromEnd, out string UpdatedListJson, out string PoppedElementJson)
        => _inner.MssList_PopByConditions(SourceListJson, ConditionsJson, LogicalOperator, SearchFromEnd, out UpdatedListJson, out PoppedElementJson);

    public void List_PopMultipleByConditions(string SourceListJson, string ConditionsJson, string LogicalOperator, out string UpdatedListJson, out string PoppedElementsJson)
        => _inner.MssList_PopMultipleByConditions(SourceListJson, ConditionsJson, LogicalOperator, out UpdatedListJson, out PoppedElementsJson);

    public void List_Zip(string ListAJson, string ListBJson, string KeyNameA, string KeyNameB, out string ZippedListJson)
        => _inner.MssList_Zip(ListAJson, ListBJson, KeyNameA, KeyNameB, out ZippedListJson);

    public void List_GroupBy(string SourceListJson, string PropertyName, out string GroupedListJson)
        => _inner.MssList_GroupBy(SourceListJson, PropertyName, out GroupedListJson);

    public void List_Difference(string ListAJson, string ListBJson, string MatchKey, string ComparisonOperator, bool CaseSensitive, out string DifferenceListJson)
        => _inner.MssList_Difference(ListAJson, ListBJson, MatchKey, ComparisonOperator, CaseSensitive, out DifferenceListJson);

    public void List_Chunk(string SourceListJson, int ChunkSize, out string ChunksListJson)
        => _inner.MssList_Chunk(SourceListJson, ChunkSize, out ChunksListJson);

    public void List_DistinctBy(string SourceListJson, string PropertyName, bool CaseSensitive, out string DistinctListJson)
        => _inner.MssList_DistinctBy(SourceListJson, PropertyName, CaseSensitive, out DistinctListJson);

    public void List_Slice(string SourceListJson, int Start, int End, int Step, out string SliceListJson)
        => _inner.MssList_Slice(SourceListJson, Start, End, Step, out SliceListJson);

    public void List_Shuffle(string SourceListJson, int Seed, out string ShuffledListJson)
        => _inner.MssList_Shuffle(SourceListJson, Seed, out ShuffledListJson);

    public void List_UpdateAt(string SourceListJson, int Index, string PropertyName, string NewValueJson, out string UpdatedListJson, out string PreviousValueJson)
        => _inner.MssList_UpdateAt(SourceListJson, Index, PropertyName, NewValueJson, out UpdatedListJson, out PreviousValueJson);

    // In-place adapters — forward `ref` through to the O11 in-place methods.
    public void List_PopInPlace(ref string SourceListJson, int Index, out string PoppedElementJson)
        => _inner.MssList_PopInPlace(ref SourceListJson, Index, out PoppedElementJson);

    public void List_PopMultipleInPlace(ref string SourceListJson, string IndicesToPop, out string PoppedElementsJson)
        => _inner.MssList_PopMultipleInPlace(ref SourceListJson, IndicesToPop, out PoppedElementsJson);

    public void List_PopByConditionInPlace(ref string SourceListJson, string PropertyName, string TargetValue, string ComparisonOperator, bool CaseSensitive, bool SearchFromEnd, out string PoppedElementJson)
        => _inner.MssList_PopByConditionInPlace(ref SourceListJson, PropertyName, TargetValue, ComparisonOperator, CaseSensitive, SearchFromEnd, out PoppedElementJson);

    public void List_PopMultipleByConditionInPlace(ref string SourceListJson, string PropertyName, string TargetValue, string ComparisonOperator, bool CaseSensitive, out string PoppedElementsJson)
        => _inner.MssList_PopMultipleByConditionInPlace(ref SourceListJson, PropertyName, TargetValue, ComparisonOperator, CaseSensitive, out PoppedElementsJson);

    public void List_PopByConditionsInPlace(ref string SourceListJson, string ConditionsJson, string LogicalOperator, bool SearchFromEnd, out string PoppedElementJson)
        => _inner.MssList_PopByConditionsInPlace(ref SourceListJson, ConditionsJson, LogicalOperator, SearchFromEnd, out PoppedElementJson);

    public void List_PopMultipleByConditionsInPlace(ref string SourceListJson, string ConditionsJson, string LogicalOperator, out string PoppedElementsJson)
        => _inner.MssList_PopMultipleByConditionsInPlace(ref SourceListJson, ConditionsJson, LogicalOperator, out PoppedElementsJson);

    public void List_ZipInPlace(ref string ListAJson, string ListBJson, string KeyNameA, string KeyNameB)
        => _inner.MssList_ZipInPlace(ref ListAJson, ListBJson, KeyNameA, KeyNameB);

    public void List_GroupByInPlace(ref string SourceListJson, string PropertyName)
        => _inner.MssList_GroupByInPlace(ref SourceListJson, PropertyName);

    public void List_DifferenceInPlace(ref string ListAJson, string ListBJson, string MatchKey, string ComparisonOperator, bool CaseSensitive)
        => _inner.MssList_DifferenceInPlace(ref ListAJson, ListBJson, MatchKey, ComparisonOperator, CaseSensitive);

    public void List_ChunkInPlace(ref string SourceListJson, int ChunkSize)
        => _inner.MssList_ChunkInPlace(ref SourceListJson, ChunkSize);

    public void List_DistinctByInPlace(ref string SourceListJson, string PropertyName, bool CaseSensitive)
        => _inner.MssList_DistinctByInPlace(ref SourceListJson, PropertyName, CaseSensitive);

    public void List_SliceInPlace(ref string SourceListJson, int Start, int End, int Step)
        => _inner.MssList_SliceInPlace(ref SourceListJson, Start, End, Step);

    public void List_ShuffleInPlace(ref string SourceListJson, int Seed)
        => _inner.MssList_ShuffleInPlace(ref SourceListJson, Seed);

    public void List_UpdateAtInPlace(ref string SourceListJson, int Index, string PropertyName, string NewValueJson, out string PreviousValueJson)
        => _inner.MssList_UpdateAtInPlace(ref SourceListJson, Index, PropertyName, NewValueJson, out PreviousValueJson);
}
