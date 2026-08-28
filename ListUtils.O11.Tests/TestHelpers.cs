using System.Collections.Generic;
using System.Linq;

namespace ListUtils.Tests;

// ── O11 adapter types ─────────────────────────────────────────────────────────
// Mirror the ODC IListUtils surface so that all test files are byte-for-byte
// identical to the ODC test project.

internal interface IListUtils
{
    void List_Pop(List<string> sourceList, int index, out List<string> updatedList, out string poppedElement);
    void List_PopMultiple(List<string> sourceList, List<int> indicesToPop, out List<string> updatedList, out List<string> poppedElements);
    void List_PopByCondition(string sourceListJson, string propertyName, string targetValue, out string updatedListJson, out string poppedElementJson);
    void List_PopMultipleByCondition(string sourceListJson, string propertyName, string targetValue, out string updatedListJson, out string poppedElementsJson);
    void List_Zip(string listAJson, string listBJson, string keyNameA, string keyNameB, out string zippedListJson);
    void List_GroupBy(string sourceListJson, string propertyName, out string groupedListJson);
    void List_Difference(string listAJson, string listBJson, string matchKey, out string differenceListJson);
}

internal sealed class ListUtils : IListUtils
{
    private readonly OutSystems.NssListUtils.CssListUtils _inner = new();

    public void List_Pop(List<string> sourceList, int index, out List<string> updatedList, out string poppedElement)
        => _inner.MssList_Pop(sourceList, index, out updatedList, out poppedElement);

    public void List_PopMultiple(List<string> sourceList, List<int> indicesToPop, out List<string> updatedList, out List<string> poppedElements)
        => _inner.MssList_PopMultiple(sourceList, indicesToPop, out updatedList, out poppedElements);

    public void List_PopByCondition(string sourceListJson, string propertyName, string targetValue, out string updatedListJson, out string poppedElementJson)
        => _inner.MssList_PopByCondition(sourceListJson, propertyName, targetValue, out updatedListJson, out poppedElementJson);

    public void List_PopMultipleByCondition(string sourceListJson, string propertyName, string targetValue, out string updatedListJson, out string poppedElementsJson)
        => _inner.MssList_PopMultipleByCondition(sourceListJson, propertyName, targetValue, out updatedListJson, out poppedElementsJson);

    public void List_Zip(string listAJson, string listBJson, string keyNameA, string keyNameB, out string zippedListJson)
        => _inner.MssList_Zip(listAJson, listBJson, keyNameA, keyNameB, out zippedListJson);

    public void List_GroupBy(string sourceListJson, string propertyName, out string groupedListJson)
        => _inner.MssList_GroupBy(sourceListJson, propertyName, out groupedListJson);

    public void List_Difference(string listAJson, string listBJson, string matchKey, out string differenceListJson)
        => _inner.MssList_Difference(listAJson, listBJson, matchKey, out differenceListJson);
}
