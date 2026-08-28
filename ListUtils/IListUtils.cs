using OutSystems.ExternalLibraries.SDK;

namespace ListUtils;

[OSInterface(Description = "Advanced list manipulation utilities — index-based pops, condition-based pops, zip, group-by, and set difference. Uses JSON serialization for generic structure support.", IconResourceName = "ListUtils.resources.icon.png")]
public interface IListUtils
{
    [OSAction(Description = "Removes an element at a specific index. Returns the removed element and the updated list.")]
    void List_Pop(
        [OSParameter(Description = "The source list to manipulate")]
        List<string> sourceList,
        [OSParameter(Description = "The 0-based index of the element to remove")]
        int index,
        [OSParameter(Description = "The list without the popped element")]
        out List<string> updatedList,
        [OSParameter(Description = "The element that was removed")]
        out string poppedElement);

    [OSAction(Description = "Removes multiple elements at specified indices. Returns the removed elements and the updated list.")]
    void List_PopMultiple(
        [OSParameter(Description = "The source list to manipulate")]
        List<string> sourceList,
        [OSParameter(Description = "The list of 0-based indices to remove")]
        List<int> indicesToPop,
        [OSParameter(Description = "The list without the popped elements")]
        out List<string> updatedList,
        [OSParameter(Description = "The elements that were removed")]
        out List<string> poppedElements);

    [OSAction(Description = "Pops the first element matching a property condition. Returns the popped element and modified list as JSON.")]
    void List_PopByCondition(
        [OSParameter(Description = "The source list serialized as a JSON string")]
        string sourceListJson,
        [OSParameter(Description = "The exact name of the structure attribute to check (e.g. 'IsActive' or 'Id')")]
        string propertyName,
        [OSParameter(Description = "The value to filter by (as a string, e.g. 'true' or '5')")]
        string targetValue,
        [OSParameter(Description = "The updated JSON list without the matched element")]
        out string updatedListJson,
        [OSParameter(Description = "The single JSON object that was matched and removed")]
        out string poppedElementJson);

    [OSAction(Description = "Pops all elements matching a property condition. Returns the popped elements and modified list as JSON.")]
    void List_PopMultipleByCondition(
        [OSParameter(Description = "The source list serialized as a JSON string")]
        string sourceListJson,
        [OSParameter(Description = "The exact name of the structure attribute to check")]
        string propertyName,
        [OSParameter(Description = "The value to filter by")]
        string targetValue,
        [OSParameter(Description = "The updated JSON list without any matched elements")]
        out string updatedListJson,
        [OSParameter(Description = "The JSON array of all items that were matched and removed")]
        out string poppedElementsJson);

    [OSAction(Description = "Combines two lists into a single list of paired objects based on matching indexes.")]
    void List_Zip(
        [OSParameter(Description = "The first JSON list source")]
        string listAJson,
        [OSParameter(Description = "The second JSON list source")]
        string listBJson,
        [OSParameter(Description = "Key property label for List A entries in the output")]
        string keyNameA,
        [OSParameter(Description = "Key property label for List B entries in the output")]
        string keyNameB,
        [OSParameter(Description = "The combined JSON array of paired objects")]
        out string zippedListJson);

    [OSAction(Description = "Groups a JSON list by a specific property name.")]
    void List_GroupBy(
        [OSParameter(Description = "The source JSON list")]
        string sourceListJson,
        [OSParameter(Description = "The property to group by")]
        string propertyName,
        [OSParameter(Description = "Grouped JSON array with Key and Items per group")]
        out string groupedListJson);

    [OSAction(Description = "Finds elements that exist in List A but not in List B (set difference). Matches on a specified key.")]
    void List_Difference(
        [OSParameter(Description = "The base JSON list")]
        string listAJson,
        [OSParameter(Description = "The subtraction JSON list")]
        string listBJson,
        [OSParameter(Description = "The property key to match on (e.g. 'Id')")]
        string matchKey,
        [OSParameter(Description = "The elements in A that have no match in B")]
        out string differenceListJson);
}
