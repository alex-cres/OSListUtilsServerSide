using System.Collections.Generic;
using System.Linq;

namespace ListUtils.Tests;

// ── O11 adapter types ─────────────────────────────────────────────────────────
// Mirror the ODC IListUtils surface so that all test files are byte-for-byte
// identical to the ODC test project.

internal interface IListUtils
{
    void List_Pop(string sourceListJson, int index, out string updatedListJson, out string poppedElementJson);
    void List_PopMultiple(string sourceListJson, string indicesToPop, out string updatedListJson, out string poppedElementsJson);
    void List_PopByCondition(string sourceListJson, string propertyName, string targetValue, string comparisonOperator, bool caseSensitive, out string updatedListJson, out string poppedElementJson);
    void List_PopMultipleByCondition(string sourceListJson, string propertyName, string targetValue, string comparisonOperator, bool caseSensitive, out string updatedListJson, out string poppedElementsJson);
    void List_PopByConditions(string sourceListJson, string conditionsJson, string logicalOperator, out string updatedListJson, out string poppedElementJson);
    void List_PopMultipleByConditions(string sourceListJson, string conditionsJson, string logicalOperator, out string updatedListJson, out string poppedElementsJson);
    void List_Zip(string listAJson, string listBJson, string keyNameA, string keyNameB, out string zippedListJson);
    void List_GroupBy(string sourceListJson, string propertyName, out string groupedListJson);
    void List_Difference(string listAJson, string listBJson, string matchKey, string comparisonOperator, bool caseSensitive, out string differenceListJson);
}

internal sealed class ListUtils : IListUtils
{
    private readonly OutSystems.NssListUtils.CssListUtils _inner = new();

    public void List_Pop(string sourceListJson, int index, out string updatedListJson, out string poppedElementJson)
        => _inner.MssList_Pop(sourceListJson, index, out updatedListJson, out poppedElementJson);

    public void List_PopMultiple(string sourceListJson, string indicesToPop, out string updatedListJson, out string poppedElementsJson)
        => _inner.MssList_PopMultiple(sourceListJson, indicesToPop, out updatedListJson, out poppedElementsJson);

    public void List_PopByCondition(string sourceListJson, string propertyName, string targetValue, string comparisonOperator, bool caseSensitive, out string updatedListJson, out string poppedElementJson)
        => _inner.MssList_PopByCondition(sourceListJson, propertyName, targetValue, comparisonOperator, caseSensitive, out updatedListJson, out poppedElementJson);

    public void List_PopMultipleByCondition(string sourceListJson, string propertyName, string targetValue, string comparisonOperator, bool caseSensitive, out string updatedListJson, out string poppedElementsJson)
        => _inner.MssList_PopMultipleByCondition(sourceListJson, propertyName, targetValue, comparisonOperator, caseSensitive, out updatedListJson, out poppedElementsJson);

    public void List_PopByConditions(string sourceListJson, string conditionsJson, string logicalOperator, out string updatedListJson, out string poppedElementJson)
        => _inner.MssList_PopByConditions(sourceListJson, conditionsJson, logicalOperator, out updatedListJson, out poppedElementJson);

    public void List_PopMultipleByConditions(string sourceListJson, string conditionsJson, string logicalOperator, out string updatedListJson, out string poppedElementsJson)
        => _inner.MssList_PopMultipleByConditions(sourceListJson, conditionsJson, logicalOperator, out updatedListJson, out poppedElementsJson);

    public void List_Zip(string listAJson, string listBJson, string keyNameA, string keyNameB, out string zippedListJson)
        => _inner.MssList_Zip(listAJson, listBJson, keyNameA, keyNameB, out zippedListJson);

    public void List_GroupBy(string sourceListJson, string propertyName, out string groupedListJson)
        => _inner.MssList_GroupBy(sourceListJson, propertyName, out groupedListJson);

    public void List_Difference(string listAJson, string listBJson, string matchKey, string comparisonOperator, bool caseSensitive, out string differenceListJson)
        => _inner.MssList_Difference(listAJson, listBJson, matchKey, comparisonOperator, caseSensitive, out differenceListJson);
}
