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
}
