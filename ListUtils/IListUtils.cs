using OutSystems.ExternalLibraries.SDK;

namespace ListUtils;

[OSInterface(Description = "Advanced list manipulation utilities — index-based pops, condition-based pops, zip, group-by, and set difference. Uses JSON serialization for generic structure support.", IconResourceName = "ListUtils.resources.icon.png")]
public interface IListUtils
{
    [OSAction(Description = "Removes an element at a specific index from a JSON list. Returns the removed element and the updated list as JSON.")]
    void List_Pop(
        [OSParameter(Description = "The source list serialized as a JSON array string")]
        string SourceListJson,
        [OSParameter(Description = "The 0-based index of the element to remove")]
        int Index,
        [OSParameter(Description = "The JSON array without the popped element")]
        out string UpdatedListJson,
        [OSParameter(Description = "The JSON element that was removed")]
        out string PoppedElementJson);

    [OSAction(Description = "Removes multiple elements at specified indices from a JSON list. Returns the removed elements and the updated list as JSON.")]
    void List_PopMultiple(
        [OSParameter(Description = "The source list serialized as a JSON array string")]
        string SourceListJson,
        [OSParameter(Description = "Comma-separated 0-based indices to remove (e.g. '1,3,5')")]
        string IndicesToPop,
        [OSParameter(Description = "The JSON array without the popped elements")]
        out string UpdatedListJson,
        [OSParameter(Description = "The JSON array of elements that were removed")]
        out string PoppedElementsJson);

    [OSAction(Description = "Pops the first element matching a property condition. Returns the popped element and modified list as JSON.")]
    void List_PopByCondition(
        [OSParameter(Description = "The source list serialized as a JSON string")]
        string SourceListJson,
        [OSParameter(Description = "Property path to check. Supports nested paths with dots (e.g. 'Address.City') and array indexing (e.g. 'Items[0]', 'Tags[-1]'). CamelCase fallback applied at each segment.")]
        string PropertyName,
        [OSParameter(Description = "The value to filter by (as a string, e.g. 'true' or '5')")]
        string TargetValue,
        [OSParameter(Description = "Comparison operator: Equals (default), NotEquals, Contains, StartsWith, EndsWith, GreaterThan, LessThan, GreaterOrEqual, LessOrEqual. Empty = Equals.")]
        string ComparisonOperator,
        [OSParameter(Description = "When true, string comparison is case-sensitive. When false (default), case-insensitive. Ignored by numeric operators.")]
        bool CaseSensitive,
        [OSParameter(Description = "When true, searches from the end of the list backwards (pops the LAST match). When false (default), searches from the beginning (pops the FIRST match).")]
        bool SearchFromEnd,
        [OSParameter(Description = "The updated JSON list without the matched element")]
        out string UpdatedListJson,
        [OSParameter(Description = "The single JSON object that was matched and removed")]
        out string PoppedElementJson);

    [OSAction(Description = "Pops all elements matching a property condition. Returns the popped elements and modified list as JSON.")]
    void List_PopMultipleByCondition(
        [OSParameter(Description = "The source list serialized as a JSON string")]
        string SourceListJson,
        [OSParameter(Description = "Property path to check. Supports nested paths with dots and array indexing (e.g. 'Items[0].Name').")]
        string PropertyName,
        [OSParameter(Description = "The value to filter by")]
        string TargetValue,
        [OSParameter(Description = "Comparison operator: Equals (default), NotEquals, Contains, StartsWith, EndsWith, GreaterThan, LessThan, GreaterOrEqual, LessOrEqual. Empty = Equals.")]
        string ComparisonOperator,
        [OSParameter(Description = "When true, string comparison is case-sensitive. When false (default), case-insensitive.")]
        bool CaseSensitive,
        [OSParameter(Description = "The updated JSON list without any matched elements")]
        out string UpdatedListJson,
        [OSParameter(Description = "The JSON array of all items that were matched and removed")]
        out string PoppedElementsJson);

    [OSAction(Description = "Pops the first element matching multiple conditions combined with AND/OR. Returns the popped element and modified list as JSON.")]
    void List_PopByConditions(
        [OSParameter(Description = "The source list serialized as a JSON string")]
        string SourceListJson,
        [OSParameter(Description = "JSON array of conditions: [{\"path\":\"Status\",\"operator\":\"Equals\",\"value\":\"Active\",\"caseSensitive\":false},{\"path\":\"Score\",\"operator\":\"GreaterThan\",\"value\":\"50\"}]")]
        string ConditionsJson,
        [OSParameter(Description = "How to combine conditions: 'AND' (default) - all must match; 'OR' - at least one must match. Empty = AND.")]
        string LogicalOperator,
        [OSParameter(Description = "When true, searches from the end of the list backwards (pops the LAST match). When false (default), searches from the beginning (pops the FIRST match).")]
        bool SearchFromEnd,
        [OSParameter(Description = "The updated JSON list without the matched element")]
        out string UpdatedListJson,
        [OSParameter(Description = "The single JSON object that was matched and removed")]
        out string PoppedElementJson);

    [OSAction(Description = "Pops all elements matching multiple conditions combined with AND/OR. Returns the popped elements and modified list as JSON.")]
    void List_PopMultipleByConditions(
        [OSParameter(Description = "The source list serialized as a JSON string")]
        string SourceListJson,
        [OSParameter(Description = "JSON array of conditions: [{\"path\":\"Status\",\"operator\":\"Equals\",\"value\":\"Active\",\"caseSensitive\":false},{\"path\":\"Score\",\"operator\":\"GreaterThan\",\"value\":\"50\"}]")]
        string ConditionsJson,
        [OSParameter(Description = "How to combine conditions: 'AND' (default) - all must match; 'OR' - at least one must match. Empty = AND.")]
        string LogicalOperator,
        [OSParameter(Description = "The updated JSON list without any matched elements")]
        out string UpdatedListJson,
        [OSParameter(Description = "The JSON array of all items that were matched and removed")]
        out string PoppedElementsJson);

    [OSAction(Description = "Combines two lists into a single list of paired objects based on matching indexes.")]
    void List_Zip(
        [OSParameter(Description = "The first JSON list source")]
        string ListAJson,
        [OSParameter(Description = "The second JSON list source")]
        string ListBJson,
        [OSParameter(Description = "Key property label for List A entries in the output")]
        string KeyNameA,
        [OSParameter(Description = "Key property label for List B entries in the output")]
        string KeyNameB,
        [OSParameter(Description = "The combined JSON array of paired objects")]
        out string ZippedListJson);

    [OSAction(Description = "Groups a JSON list by a specific property name.")]
    void List_GroupBy(
        [OSParameter(Description = "The source JSON list")]
        string SourceListJson,
        [OSParameter(Description = "Property path to group by. Supports nested paths with dots (e.g. 'Address.City'). CamelCase fallback applied at each segment.")]
        string PropertyName,
        [OSParameter(Description = "Grouped JSON array with Key and Items per group")]
        out string GroupedListJson);

    [OSAction(Description = "Finds elements that exist in List A but not in List B (set difference). Matches on a specified key.")]
    void List_Difference(
        [OSParameter(Description = "The base JSON list")]
        string ListAJson,
        [OSParameter(Description = "The subtraction JSON list")]
        string ListBJson,
        [OSParameter(Description = "Property path to match on. Supports nested paths and array indexing (e.g. 'Meta.Id', 'Refs[0].Code').")]
        string MatchKey,
        [OSParameter(Description = "Comparison operator for key matching: Equals (default), Contains, StartsWith. Empty = Equals.")]
        string ComparisonOperator,
        [OSParameter(Description = "When true, key comparison is case-sensitive. When false (default), case-insensitive.")]
        bool CaseSensitive,
        [OSParameter(Description = "The elements in A that have no match in B")]
        out string DifferenceListJson);
}
