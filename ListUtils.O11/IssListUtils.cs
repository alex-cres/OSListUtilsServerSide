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
}
