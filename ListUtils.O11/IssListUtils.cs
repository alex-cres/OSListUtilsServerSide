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
        List<Condition> ssConditions,
        string ssLogicalOperator,
        bool ssSearchFromEnd,
        out string ssUpdatedListJson,
        out string ssPoppedElementJson);

    void MssList_PopMultipleByConditions(
        string ssSourceListJson,
        List<Condition> ssConditions,
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

    void MssList_ZipGroupBy(
        string ssListAJson,
        string ssListBJson,
        string ssKeyPropertyA,
        string ssKeyPropertyB,
        string ssKeyNameA,
        string ssKeyNameB,
        bool ssCaseSensitive,
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
        out List<string> ssChunksListJson);

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

    void MssList_MinBy(
        string ssSourceListJson,
        string ssPropertyName,
        bool ssNumericMode,
        out string ssElementJson,
        out string ssMinValue,
        out int ssMinIndex);

    void MssList_MaxBy(
        string ssSourceListJson,
        string ssPropertyName,
        bool ssNumericMode,
        out string ssElementJson,
        out string ssMaxValue,
        out int ssMaxIndex);

    void MssList_Aggregate(
        string ssSourceListJson,
        string ssPropertyName,
        string ssOperation,
        out string ssResultValue,
        out int ssMatchedCount);

    void MssList_Intersect(
        string ssListAJson,
        string ssListBJson,
        string ssMatchKey,
        string ssComparisonOperator,
        bool ssCaseSensitive,
        out string ssIntersectionListJson);

    void MssList_Union(
        string ssListAJson,
        string ssListBJson,
        string ssMatchKey,
        bool ssCaseSensitive,
        out string ssUnionListJson);

    void MssList_SplitAt(
        string ssSourceListJson,
        int ssIndex,
        out string ssLeftListJson,
        out string ssRightListJson);

    void MssList_Partition(
        string ssSourceListJson,
        string ssPropertyName,
        string ssTargetValue,
        string ssComparisonOperator,
        bool ssCaseSensitive,
        out string ssMatchingListJson,
        out string ssNonMatchingListJson);

    void MssList_PartitionByConditions(
        string ssSourceListJson,
        List<Condition> ssConditions,
        string ssLogicalOperator,
        out string ssMatchingListJson,
        out string ssNonMatchingListJson);

    void MssList_Reverse(
        string ssSourceListJson,
        out string ssReversedListJson);

    void MssList_Flatten(
        List<string> ssChunksListJson,
        out string ssFlatListJson);

    void MssList_Sample(
        string ssSourceListJson,
        int ssSampleSize,
        int ssSeed,
        out string ssSampleListJson);

    void MssList_ReplaceWhere(
        string ssSourceListJson,
        List<Condition> ssConditions,
        string ssLogicalOperator,
        string ssUpdateProperty,
        string ssNewValueJson,
        out string ssUpdatedListJson,
        out int ssMatchCount);

    void MssList_UpdateMultipleAt(
        string ssSourceListJson,
        string ssIndicesToUpdate,
        string ssPropertyName,
        string ssNewValueJson,
        out string ssUpdatedListJson,
        out int ssUpdatedCount);

    void MssList_ZipMany(
        List<string> ssListsJson,
        List<string> ssKeyNamesJson,
        out string ssZippedListJson);

    void MssList_ZipManyGroupBy(
        List<string> ssListsJson,
        List<string> ssKeyPropertiesJson,
        List<string> ssKeyNamesJson,
        bool ssCaseSensitive,
        out string ssGroupedListJson);
}
