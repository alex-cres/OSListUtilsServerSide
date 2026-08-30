using System.Collections.Generic;

namespace OutSystems.NssListUtils;

public interface IssListUtils
{
    void MssList_Pop(
        string ssSourceListJson,
        int ssIndex,
        out string ssUpdatedListJson,
        out string ssPoppedElementJson);

    void MssList_PopMultiple(
        string ssSourceListJson,
        string ssIndicesToPop,
        out string ssUpdatedListJson,
        out string ssPoppedElementsJson);

    void MssList_PopByCondition(
        string ssSourceListJson,
        string ssPropertyName,
        string ssTargetValue,
        string ssComparisonOperator,
        bool ssCaseSensitive,
        bool ssSearchFromEnd,
        out string ssUpdatedListJson,
        out string ssPoppedElementJson);

    void MssList_PopMultipleByCondition(
        string ssSourceListJson,
        string ssPropertyName,
        string ssTargetValue,
        string ssComparisonOperator,
        bool ssCaseSensitive,
        out string ssUpdatedListJson,
        out string ssPoppedElementsJson);

    void MssList_PopByConditions(
        string ssSourceListJson,
        string ssConditionsJson,
        string ssLogicalOperator,
        bool ssSearchFromEnd,
        out string ssUpdatedListJson,
        out string ssPoppedElementJson);

    void MssList_PopMultipleByConditions(
        string ssSourceListJson,
        string ssConditionsJson,
        string ssLogicalOperator,
        out string ssUpdatedListJson,
        out string ssPoppedElementsJson);

    void MssList_Zip(
        string ssListAJson,
        string ssListBJson,
        string ssKeyNameA,
        string ssKeyNameB,
        out string ssZippedListJson);

    void MssList_GroupBy(
        string ssSourceListJson,
        string ssPropertyName,
        out string ssGroupedListJson);

    void MssList_Difference(
        string ssListAJson,
        string ssListBJson,
        string ssMatchKey,
        string ssComparisonOperator,
        bool ssCaseSensitive,
        out string ssDifferenceListJson);

    void MssList_Chunk(
        string ssSourceListJson,
        int ssChunkSize,
        out string ssChunksListJson);

    void MssList_DistinctBy(
        string ssSourceListJson,
        string ssPropertyName,
        bool ssCaseSensitive,
        out string ssDistinctListJson);

    void MssList_Slice(
        string ssSourceListJson,
        int ssStart,
        int ssEnd,
        int ssStep,
        out string ssSliceListJson);

    void MssList_Shuffle(
        string ssSourceListJson,
        int ssSeed,
        out string ssShuffledListJson);

    void MssList_UpdateAt(
        string ssSourceListJson,
        int ssIndex,
        string ssPropertyName,
        string ssNewValueJson,
        out string ssUpdatedListJson,
        out string ssPreviousValueJson);

    // In-place variants — primary list parameter uses `ref` (Input/Output).
    // Secondary outputs stay as `out`.

    void MssList_PopInPlace(
        ref string ssSourceListJson,
        int ssIndex,
        out string ssPoppedElementJson);

    void MssList_PopMultipleInPlace(
        ref string ssSourceListJson,
        string ssIndicesToPop,
        out string ssPoppedElementsJson);

    void MssList_PopByConditionInPlace(
        ref string ssSourceListJson,
        string ssPropertyName,
        string ssTargetValue,
        string ssComparisonOperator,
        bool ssCaseSensitive,
        bool ssSearchFromEnd,
        out string ssPoppedElementJson);

    void MssList_PopMultipleByConditionInPlace(
        ref string ssSourceListJson,
        string ssPropertyName,
        string ssTargetValue,
        string ssComparisonOperator,
        bool ssCaseSensitive,
        out string ssPoppedElementsJson);

    void MssList_PopByConditionsInPlace(
        ref string ssSourceListJson,
        string ssConditionsJson,
        string ssLogicalOperator,
        bool ssSearchFromEnd,
        out string ssPoppedElementJson);

    void MssList_PopMultipleByConditionsInPlace(
        ref string ssSourceListJson,
        string ssConditionsJson,
        string ssLogicalOperator,
        out string ssPoppedElementsJson);

    void MssList_ZipInPlace(
        ref string ssListAJson,
        string ssListBJson,
        string ssKeyNameA,
        string ssKeyNameB);

    void MssList_GroupByInPlace(
        ref string ssSourceListJson,
        string ssPropertyName);

    void MssList_DifferenceInPlace(
        ref string ssListAJson,
        string ssListBJson,
        string ssMatchKey,
        string ssComparisonOperator,
        bool ssCaseSensitive);

    void MssList_ChunkInPlace(
        ref string ssSourceListJson,
        int ssChunkSize);

    void MssList_DistinctByInPlace(
        ref string ssSourceListJson,
        string ssPropertyName,
        bool ssCaseSensitive);

    void MssList_SliceInPlace(
        ref string ssSourceListJson,
        int ssStart,
        int ssEnd,
        int ssStep);

    void MssList_ShuffleInPlace(
        ref string ssSourceListJson,
        int ssSeed);

    void MssList_UpdateAtInPlace(
        ref string ssSourceListJson,
        int ssIndex,
        string ssPropertyName,
        string ssNewValueJson,
        out string ssPreviousValueJson);
}
