namespace ListUtils;

// In-place variants — each delegates to the corresponding non-InPlace action
// and assigns its output list back to the ref parameter. Secondary outputs are
// passed through unchanged. Keeps behaviour identical to the base action while
// exposing an Input/Output parameter to OutSystems callers.
public partial class ListUtils
{
    public void List_PopInPlace(ref string SourceListJson, int Index, out string PoppedElementJson)
    {
        List_Pop(SourceListJson, Index, out var updated, out PoppedElementJson);
        SourceListJson = updated;
    }

    public void List_PopMultipleInPlace(ref string SourceListJson, string IndicesToPop, out string PoppedElementsJson)
    {
        List_PopMultiple(SourceListJson, IndicesToPop, out var updated, out PoppedElementsJson);
        SourceListJson = updated;
    }

    public void List_PopByConditionInPlace(
        ref string SourceListJson,
        string PropertyName,
        string TargetValue,
        string ComparisonOperator,
        bool CaseSensitive,
        bool SearchFromEnd,
        out string PoppedElementJson)
    {
        List_PopByCondition(SourceListJson, PropertyName, TargetValue, ComparisonOperator, CaseSensitive, SearchFromEnd, out var updated, out PoppedElementJson);
        SourceListJson = updated;
    }

    public void List_PopMultipleByConditionInPlace(
        ref string SourceListJson,
        string PropertyName,
        string TargetValue,
        string ComparisonOperator,
        bool CaseSensitive,
        out string PoppedElementsJson)
    {
        List_PopMultipleByCondition(SourceListJson, PropertyName, TargetValue, ComparisonOperator, CaseSensitive, out var updated, out PoppedElementsJson);
        SourceListJson = updated;
    }

    public void List_PopByConditionsInPlace(
        ref string SourceListJson,
        string ConditionsJson,
        string LogicalOperator,
        bool SearchFromEnd,
        out string PoppedElementJson)
    {
        List_PopByConditions(SourceListJson, ConditionsJson, LogicalOperator, SearchFromEnd, out var updated, out PoppedElementJson);
        SourceListJson = updated;
    }

    public void List_PopMultipleByConditionsInPlace(
        ref string SourceListJson,
        string ConditionsJson,
        string LogicalOperator,
        out string PoppedElementsJson)
    {
        List_PopMultipleByConditions(SourceListJson, ConditionsJson, LogicalOperator, out var updated, out PoppedElementsJson);
        SourceListJson = updated;
    }

    public void List_ZipInPlace(ref string ListAJson, string ListBJson, string KeyNameA, string KeyNameB)
    {
        List_Zip(ListAJson, ListBJson, KeyNameA, KeyNameB, out var zipped);
        ListAJson = zipped;
    }

    public void List_GroupByInPlace(ref string SourceListJson, string PropertyName)
    {
        List_GroupBy(SourceListJson, PropertyName, out var grouped);
        SourceListJson = grouped;
    }

    public void List_DifferenceInPlace(
        ref string ListAJson,
        string ListBJson,
        string MatchKey,
        string ComparisonOperator,
        bool CaseSensitive)
    {
        List_Difference(ListAJson, ListBJson, MatchKey, ComparisonOperator, CaseSensitive, out var diff);
        ListAJson = diff;
    }

    public void List_ChunkInPlace(ref string SourceListJson, int ChunkSize)
    {
        List_Chunk(SourceListJson, ChunkSize, out var chunks);
        SourceListJson = chunks;
    }

    public void List_DistinctByInPlace(ref string SourceListJson, string PropertyName, bool CaseSensitive)
    {
        List_DistinctBy(SourceListJson, PropertyName, CaseSensitive, out var distinct);
        SourceListJson = distinct;
    }

    public void List_SliceInPlace(ref string SourceListJson, int Start, int End, int Step)
    {
        List_Slice(SourceListJson, Start, End, Step, out var slice);
        SourceListJson = slice;
    }

    public void List_ShuffleInPlace(ref string SourceListJson, int Seed)
    {
        List_Shuffle(SourceListJson, Seed, out var shuffled);
        SourceListJson = shuffled;
    }

    public void List_UpdateAtInPlace(
        ref string SourceListJson,
        int Index,
        string PropertyName,
        string NewValueJson,
        out string PreviousValueJson)
    {
        List_UpdateAt(SourceListJson, Index, PropertyName, NewValueJson, out var updated, out PreviousValueJson);
        SourceListJson = updated;
    }
}
