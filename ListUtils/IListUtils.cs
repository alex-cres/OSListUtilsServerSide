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

    [OSAction(Description = "Splits a JSON list into an array of sublists of a fixed size. Last chunk may be smaller. Essential for batching API payloads and throttled loops.")]
    void List_Chunk(
        [OSParameter(Description = "The source list serialized as a JSON array string")]
        string SourceListJson,
        [OSParameter(Description = "Maximum number of elements per chunk. Must be >= 1; values <= 0 return an empty array.")]
        int ChunkSize,
        [OSParameter(Description = "A JSON array of arrays: [[..first N..], [..next N..], ...]. The last inner array may hold fewer elements than ChunkSize.")]
        out string ChunksListJson);

    [OSAction(Description = "Filters a JSON list to unique elements based on a property value (first occurrence wins). Works on structures — native ODC Distinct only supports basic types.")]
    void List_DistinctBy(
        [OSParameter(Description = "The source list serialized as a JSON array string")]
        string SourceListJson,
        [OSParameter(Description = "Property path used as the uniqueness key. Supports nested paths (e.g. 'Address.City') and array indexing (e.g. 'Tags[0]'). CamelCase fallback applied at each segment.")]
        string PropertyName,
        [OSParameter(Description = "When true, key comparison is case-sensitive. When false (default), case-insensitive.")]
        bool CaseSensitive,
        [OSParameter(Description = "The JSON array containing only the first occurrence of each unique key, preserving source order.")]
        out string DistinctListJson);

    [OSAction(Description = "Extracts a subset of a JSON list using Python/JavaScript-style Start/End/Step. Supports negative indices, negative step (reverse), and End <= 0 meaning 'to end of list'.")]
    void List_Slice(
        [OSParameter(Description = "The source list serialized as a JSON array string")]
        string SourceListJson,
        [OSParameter(Description = "0-based inclusive start index. Negative values count from the end (e.g. -2 = second-to-last). Clamped to the list bounds.")]
        int Start,
        [OSParameter(Description = "0-based exclusive end index. Negative values count from the end. Special case: End == 0 is treated as 'unspecified' — for positive Step it means 'to end of list', for negative Step it means 'past the beginning' (Python default).")]
        int End,
        [OSParameter(Description = "Step between selected elements. 0 is treated as 1. Positive walks forward; negative walks backward (reverse slice, matching Python semantics).")]
        int Step,
        [OSParameter(Description = "The sliced JSON array")]
        out string SliceListJson);

    [OSAction(Description = "Randomizes the order of a JSON list using the Fisher-Yates algorithm. Deterministic when Seed is non-zero; otherwise uses a cryptographically-seeded RNG.")]
    void List_Shuffle(
        [OSParameter(Description = "The source list serialized as a JSON array string")]
        string SourceListJson,
        [OSParameter(Description = "Random seed. Use 0 for a non-deterministic shuffle. Any non-zero value produces the same permutation every call (useful for reproducible tests).")]
        int Seed,
        [OSParameter(Description = "The shuffled JSON array. Original source list is not mutated.")]
        out string ShuffledListJson);

    [OSAction(Description = "Sets a single property of the item at a given index. Returns the modified list and the previous value of that property.")]
    void List_UpdateAt(
        [OSParameter(Description = "The source list serialized as a JSON array string")]
        string SourceListJson,
        [OSParameter(Description = "0-based index of the item to modify. Negative values count from the end. Out-of-range indices return the source unchanged and PreviousValueJson = 'null'.")]
        int Index,
        [OSParameter(Description = "Property path to update on the target item. Supports nested paths with dots (e.g. 'Address.City') and array indexing (e.g. 'Tags[0]'). Missing intermediate objects are created (arrays are NOT auto-created; if an array indexing step encounters a missing or non-array node, the action returns the source unchanged).")]
        string PropertyName,
        [OSParameter(Description = "The new value as JSON (e.g. '\"Active\"', '42', 'true', '{\"City\":\"Lisbon\"}', 'null'). Falls back to a raw string when the value is not valid JSON.")]
        string NewValueJson,
        [OSParameter(Description = "The updated JSON array with the property changed")]
        out string UpdatedListJson,
        [OSParameter(Description = "The previous JSON value of the modified property, or 'null' when the property did not exist, when the property existed with a JSON null value (these two cases are indistinguishable), or when the index was out of range")]
        out string PreviousValueJson);

    // ─── In-place variants ────────────────────────────────────────────────────
    // Each pairs with an action above. The primary list parameter uses `ref` so
    // it maps to a single Input/Output variable in OutSystems — the caller's
    // variable is mutated directly, no separate output list is produced.
    // Secondary outputs (popped element, previous value) stay as `out`.

    [OSAction(Description = "In-place variant of List_Pop. Removes an element at Index and mutates SourceListJson directly.")]
    void List_PopInPlace(
        [OSParameter(Description = "The source list (Input/Output). On return, contains the list without the popped element.")]
        ref string SourceListJson,
        [OSParameter(Description = "The 0-based index of the element to remove")]
        int Index,
        [OSParameter(Description = "The JSON element that was removed, or 'null' when the index was out of range")]
        out string PoppedElementJson);

    [OSAction(Description = "In-place variant of List_PopMultiple. Removes elements at IndicesToPop and mutates SourceListJson directly.")]
    void List_PopMultipleInPlace(
        [OSParameter(Description = "The source list (Input/Output). On return, contains the list without the popped elements.")]
        ref string SourceListJson,
        [OSParameter(Description = "Comma-separated 0-based indices to remove (e.g. '1,3,5')")]
        string IndicesToPop,
        [OSParameter(Description = "The JSON array of elements that were removed, in their original order")]
        out string PoppedElementsJson);

    [OSAction(Description = "In-place variant of List_PopByCondition. Pops the first matching element and mutates SourceListJson directly.")]
    void List_PopByConditionInPlace(
        [OSParameter(Description = "The source list (Input/Output).")]
        ref string SourceListJson,
        [OSParameter(Description = "Property path to check. Supports nested paths and array indexing.")]
        string PropertyName,
        [OSParameter(Description = "The value to filter by")]
        string TargetValue,
        [OSParameter(Description = "Comparison operator: Equals (default), NotEquals, Contains, StartsWith, EndsWith, GreaterThan, LessThan, GreaterOrEqual, LessOrEqual. Empty = Equals.")]
        string ComparisonOperator,
        [OSParameter(Description = "When true, string comparison is case-sensitive. When false (default), case-insensitive.")]
        bool CaseSensitive,
        [OSParameter(Description = "When true, searches from the end and pops the LAST match.")]
        bool SearchFromEnd,
        [OSParameter(Description = "The single JSON object that was matched and removed, or '{}' when no match was found")]
        out string PoppedElementJson);

    [OSAction(Description = "In-place variant of List_PopMultipleByCondition. Pops every matching element and mutates SourceListJson directly.")]
    void List_PopMultipleByConditionInPlace(
        [OSParameter(Description = "The source list (Input/Output).")]
        ref string SourceListJson,
        [OSParameter(Description = "Property path to check.")]
        string PropertyName,
        [OSParameter(Description = "The value to filter by")]
        string TargetValue,
        [OSParameter(Description = "Comparison operator. Empty = Equals.")]
        string ComparisonOperator,
        [OSParameter(Description = "Case-sensitivity flag (default false).")]
        bool CaseSensitive,
        [OSParameter(Description = "The JSON array of all items that were matched and removed")]
        out string PoppedElementsJson);

    [OSAction(Description = "In-place variant of List_PopByConditions. Pops the first element matching multiple AND/OR conditions and mutates SourceListJson directly.")]
    void List_PopByConditionsInPlace(
        [OSParameter(Description = "The source list (Input/Output).")]
        ref string SourceListJson,
        [OSParameter(Description = "JSON array of conditions.")]
        string ConditionsJson,
        [OSParameter(Description = "How to combine conditions: 'AND' (default) or 'OR'.")]
        string LogicalOperator,
        [OSParameter(Description = "When true, pops the LAST match instead of the first.")]
        bool SearchFromEnd,
        [OSParameter(Description = "The single JSON object that was matched and removed, or '{}' when no match was found")]
        out string PoppedElementJson);

    [OSAction(Description = "In-place variant of List_PopMultipleByConditions. Pops every element matching AND/OR conditions and mutates SourceListJson directly.")]
    void List_PopMultipleByConditionsInPlace(
        [OSParameter(Description = "The source list (Input/Output).")]
        ref string SourceListJson,
        [OSParameter(Description = "JSON array of conditions.")]
        string ConditionsJson,
        [OSParameter(Description = "'AND' or 'OR'.")]
        string LogicalOperator,
        [OSParameter(Description = "The JSON array of all items that were matched and removed")]
        out string PoppedElementsJson);

    [OSAction(Description = "In-place variant of List_Zip. Replaces ListAJson with the zipped list of paired objects; ListBJson is read-only.")]
    void List_ZipInPlace(
        [OSParameter(Description = "The first JSON list (Input/Output). On return, contains the zipped paired objects.")]
        ref string ListAJson,
        [OSParameter(Description = "The second JSON list source")]
        string ListBJson,
        [OSParameter(Description = "Key property label for List A entries in the output")]
        string KeyNameA,
        [OSParameter(Description = "Key property label for List B entries in the output")]
        string KeyNameB);

    [OSAction(Description = "In-place variant of List_GroupBy. Replaces SourceListJson with the grouped {Key, Items} array.")]
    void List_GroupByInPlace(
        [OSParameter(Description = "The source JSON list (Input/Output). On return, contains the grouped {Key, Items} array.")]
        ref string SourceListJson,
        [OSParameter(Description = "Property path to group by.")]
        string PropertyName);

    [OSAction(Description = "In-place variant of List_Difference. Replaces ListAJson with the set difference A − B; ListBJson is read-only.")]
    void List_DifferenceInPlace(
        [OSParameter(Description = "The base JSON list (Input/Output). On return, contains only the elements from A with no match in B.")]
        ref string ListAJson,
        [OSParameter(Description = "The subtraction JSON list")]
        string ListBJson,
        [OSParameter(Description = "Property path to match on.")]
        string MatchKey,
        [OSParameter(Description = "Comparison operator for key matching. Empty = Equals.")]
        string ComparisonOperator,
        [OSParameter(Description = "Case-sensitivity flag.")]
        bool CaseSensitive);

    [OSAction(Description = "In-place variant of List_Chunk. Replaces SourceListJson with the array of sublists.")]
    void List_ChunkInPlace(
        [OSParameter(Description = "The source list (Input/Output). On return, contains an array of sublists (JSON array of arrays).")]
        ref string SourceListJson,
        [OSParameter(Description = "Maximum number of elements per chunk.")]
        int ChunkSize);

    [OSAction(Description = "In-place variant of List_DistinctBy. Replaces SourceListJson with only the first occurrence of each unique key.")]
    void List_DistinctByInPlace(
        [OSParameter(Description = "The source list (Input/Output).")]
        ref string SourceListJson,
        [OSParameter(Description = "Property path used as the uniqueness key. Empty = dedupe by whole item.")]
        string PropertyName,
        [OSParameter(Description = "Case-sensitivity flag.")]
        bool CaseSensitive);

    [OSAction(Description = "In-place variant of List_Slice. Replaces SourceListJson with the selected Start/End/Step slice.")]
    void List_SliceInPlace(
        [OSParameter(Description = "The source list (Input/Output).")]
        ref string SourceListJson,
        [OSParameter(Description = "0-based inclusive start index. Negative counts from the end. Clamped.")]
        int Start,
        [OSParameter(Description = "0-based exclusive end index. End == 0 is treated as 'unspecified' (to end for positive Step; past beginning for negative Step).")]
        int End,
        [OSParameter(Description = "Step between selected elements. 0 = 1. Negative walks backward.")]
        int Step);

    [OSAction(Description = "In-place variant of List_Shuffle. Replaces SourceListJson with the shuffled list.")]
    void List_ShuffleInPlace(
        [OSParameter(Description = "The source list (Input/Output).")]
        ref string SourceListJson,
        [OSParameter(Description = "Random seed. 0 = non-deterministic (CSPRNG); non-zero = deterministic (System.Random).")]
        int Seed);

    [OSAction(Description = "In-place variant of List_UpdateAt. Mutates the property of the item at Index directly on SourceListJson and returns the previous value.")]
    void List_UpdateAtInPlace(
        [OSParameter(Description = "The source list (Input/Output).")]
        ref string SourceListJson,
        [OSParameter(Description = "0-based index of the item to modify. Negative counts from the end.")]
        int Index,
        [OSParameter(Description = "Property path to update on the target item. Missing intermediate objects are created; array indices must already exist.")]
        string PropertyName,
        [OSParameter(Description = "The new value as JSON. Falls back to a raw string when invalid JSON.")]
        string NewValueJson,
        [OSParameter(Description = "The previous JSON value of the modified property, or 'null' (see List_UpdateAt for full semantics).")]
        out string PreviousValueJson);
}
