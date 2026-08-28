using System.Collections.Generic;

namespace OutSystems.NssListUtils;

public interface IssListUtils
{
    void MssList_Pop(
        List<string> ssSourceList,
        int ssIndex,
        out List<string> ssUpdatedList,
        out string ssPoppedElement);

    void MssList_PopMultiple(
        List<string> ssSourceList,
        List<int> ssIndicesToPop,
        out List<string> ssUpdatedList,
        out List<string> ssPoppedElements);

    void MssList_PopByCondition(
        string ssSourceListJson,
        string ssPropertyName,
        string ssTargetValue,
        out string ssUpdatedListJson,
        out string ssPoppedElementJson);

    void MssList_PopMultipleByCondition(
        string ssSourceListJson,
        string ssPropertyName,
        string ssTargetValue,
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
        out string ssDifferenceListJson);
}
