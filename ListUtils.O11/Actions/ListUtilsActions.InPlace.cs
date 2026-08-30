namespace OutSystems.NssListUtils;

// In-place variants — each delegates to the corresponding Mss action and
// assigns its output list back to the ref parameter. Behaviour is identical
// to the base action; only the parameter direction changes.
public partial class CssListUtils
{
    public void MssList_PopInPlace(ref string ssSourceListJson, int ssIndex, out string ssPoppedElementJson)
    {
        string updated;
        MssList_Pop(ssSourceListJson, ssIndex, out updated, out ssPoppedElementJson);
        ssSourceListJson = updated;
    }

    public void MssList_PopMultipleInPlace(ref string ssSourceListJson, string ssIndicesToPop, out string ssPoppedElementsJson)
    {
        string updated;
        MssList_PopMultiple(ssSourceListJson, ssIndicesToPop, out updated, out ssPoppedElementsJson);
        ssSourceListJson = updated;
    }

    public void MssList_PopByConditionInPlace(
        ref string ssSourceListJson,
        string ssPropertyName,
        string ssTargetValue,
        string ssComparisonOperator,
        bool ssCaseSensitive,
        bool ssSearchFromEnd,
        out string ssPoppedElementJson)
    {
        string updated;
        MssList_PopByCondition(ssSourceListJson, ssPropertyName, ssTargetValue, ssComparisonOperator, ssCaseSensitive, ssSearchFromEnd, out updated, out ssPoppedElementJson);
        ssSourceListJson = updated;
    }

    public void MssList_PopMultipleByConditionInPlace(
        ref string ssSourceListJson,
        string ssPropertyName,
        string ssTargetValue,
        string ssComparisonOperator,
        bool ssCaseSensitive,
        out string ssPoppedElementsJson)
    {
        string updated;
        MssList_PopMultipleByCondition(ssSourceListJson, ssPropertyName, ssTargetValue, ssComparisonOperator, ssCaseSensitive, out updated, out ssPoppedElementsJson);
        ssSourceListJson = updated;
    }

    public void MssList_PopByConditionsInPlace(
        ref string ssSourceListJson,
        string ssConditionsJson,
        string ssLogicalOperator,
        bool ssSearchFromEnd,
        out string ssPoppedElementJson)
    {
        string updated;
        MssList_PopByConditions(ssSourceListJson, ssConditionsJson, ssLogicalOperator, ssSearchFromEnd, out updated, out ssPoppedElementJson);
        ssSourceListJson = updated;
    }

    public void MssList_PopMultipleByConditionsInPlace(
        ref string ssSourceListJson,
        string ssConditionsJson,
        string ssLogicalOperator,
        out string ssPoppedElementsJson)
    {
        string updated;
        MssList_PopMultipleByConditions(ssSourceListJson, ssConditionsJson, ssLogicalOperator, out updated, out ssPoppedElementsJson);
        ssSourceListJson = updated;
    }

    public void MssList_ZipInPlace(ref string ssListAJson, string ssListBJson, string ssKeyNameA, string ssKeyNameB)
    {
        string zipped;
        MssList_Zip(ssListAJson, ssListBJson, ssKeyNameA, ssKeyNameB, out zipped);
        ssListAJson = zipped;
    }

    public void MssList_GroupByInPlace(ref string ssSourceListJson, string ssPropertyName)
    {
        string grouped;
        MssList_GroupBy(ssSourceListJson, ssPropertyName, out grouped);
        ssSourceListJson = grouped;
    }

    public void MssList_DifferenceInPlace(
        ref string ssListAJson,
        string ssListBJson,
        string ssMatchKey,
        string ssComparisonOperator,
        bool ssCaseSensitive)
    {
        string diff;
        MssList_Difference(ssListAJson, ssListBJson, ssMatchKey, ssComparisonOperator, ssCaseSensitive, out diff);
        ssListAJson = diff;
    }

    public void MssList_ChunkInPlace(ref string ssSourceListJson, int ssChunkSize)
    {
        string chunks;
        MssList_Chunk(ssSourceListJson, ssChunkSize, out chunks);
        ssSourceListJson = chunks;
    }

    public void MssList_DistinctByInPlace(ref string ssSourceListJson, string ssPropertyName, bool ssCaseSensitive)
    {
        string distinct;
        MssList_DistinctBy(ssSourceListJson, ssPropertyName, ssCaseSensitive, out distinct);
        ssSourceListJson = distinct;
    }

    public void MssList_SliceInPlace(ref string ssSourceListJson, int ssStart, int ssEnd, int ssStep)
    {
        string slice;
        MssList_Slice(ssSourceListJson, ssStart, ssEnd, ssStep, out slice);
        ssSourceListJson = slice;
    }

    public void MssList_ShuffleInPlace(ref string ssSourceListJson, int ssSeed)
    {
        string shuffled;
        MssList_Shuffle(ssSourceListJson, ssSeed, out shuffled);
        ssSourceListJson = shuffled;
    }

    public void MssList_UpdateAtInPlace(
        ref string ssSourceListJson,
        int ssIndex,
        string ssPropertyName,
        string ssNewValueJson,
        out string ssPreviousValueJson)
    {
        string updated;
        MssList_UpdateAt(ssSourceListJson, ssIndex, ssPropertyName, ssNewValueJson, out updated, out ssPreviousValueJson);
        ssSourceListJson = updated;
    }
}
