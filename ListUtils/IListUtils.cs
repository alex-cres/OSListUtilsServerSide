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
        [OSParameter(Description = "List of Condition Structures. Each entry has Path, Operator, Value, CaseSensitive. Build with the ListUtils.Operators constants (e.g. Operators.Equals) for compile-time safety.")]
        List<Condition> Conditions,
        [OSParameter(Description = "How to combine conditions: 'AND' (default) - all must match; 'OR' - at least one must match. Empty = AND. See ListUtils.LogicalOperators.")]
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
        [OSParameter(Description = "List of Condition Structures.")]
        List<Condition> Conditions,
        [OSParameter(Description = "How to combine conditions: 'AND' (default) or 'OR'. Empty = AND. See ListUtils.LogicalOperators.")]
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

    [OSAction(Description = "Groups two JSON lists by a shared key value. Each output group contains the Key plus a named array of items from ListA and a named array of items from ListB that share the same key. Union of first-seen keys across both lists preserves ordering. Useful for cogroup / two-list join scenarios (orders + payments per customer, etc.).")]
    void List_ZipGroupBy(
        [OSParameter(Description = "The first JSON list")]
        string ListAJson,
        [OSParameter(Description = "The second JSON list")]
        string ListBJson,
        [OSParameter(Description = "Property path in ListA that supplies the group key. Supports nested paths and array indexing.")]
        string KeyPropertyA,
        [OSParameter(Description = "Property path in ListB that supplies the group key. Supports nested paths and array indexing.")]
        string KeyPropertyB,
        [OSParameter(Description = "Output field name for the ListA items array within each group (e.g. 'Orders').")]
        string KeyNameA,
        [OSParameter(Description = "Output field name for the ListB items array within each group (e.g. 'Payments').")]
        string KeyNameB,
        [OSParameter(Description = "When true, key comparison is case-sensitive. When false (default), case-insensitive.")]
        bool CaseSensitive,
        [OSParameter(Description = "JSON array of groups: [{\"Key\": <keyValue>, <KeyNameA>: [...ListA items...], <KeyNameB>: [...ListB items...]}, ...]. Items with no key value fall into a single 'Unknown' bucket.")]
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

    [OSAction(Description = "Splits a JSON list into a list of JSON-string chunks of a fixed size. Last chunk may be smaller. Essential for batching API payloads and throttled loops. Each output entry is a standalone JSON array ready for JSON Deserialize into the caller's Structure List.")]
    void List_Chunk(
        [OSParameter(Description = "The source list serialized as a JSON array string")]
        string SourceListJson,
        [OSParameter(Description = "Maximum number of elements per chunk. Must be >= 1; values <= 0 return an empty list.")]
        int ChunkSize,
        [OSParameter(Description = "A list of JSON-array strings — one string per chunk (e.g. [\"[..first N..]\", \"[..next N..]\", ...]). The last entry may hold fewer elements than ChunkSize. Empty list when the source is empty or ChunkSize <= 0.")]
        out List<string> ChunksListJson);

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

    // ── Aggregations (3) ─────────────────────────────────────────────────

    [OSAction(Description = "Returns the element with the smallest value at a given property path. Empty list returns MinValue = '' and MinIndex = -1.")]
    void List_MinBy(
        [OSParameter(Description = "The source list serialized as a JSON array string")]
        string SourceListJson,
        [OSParameter(Description = "Property path to compare on. Supports nested paths (e.g. 'Meta.Score') and array indexing (e.g. 'Items[0].Qty').")]
        string PropertyName,
        [OSParameter(Description = "When true, values are parsed as decimals for numeric comparison. When false, compared as ordinal text (case-insensitive).")]
        bool NumericMode,
        [OSParameter(Description = "The full element at the minimum, or 'null' when the source is empty or no item had a value at PropertyName.")]
        out string ElementJson,
        [OSParameter(Description = "The raw value at PropertyName for the returned element, or '' when the list is empty.")]
        out string MinValue,
        [OSParameter(Description = "The 0-based index of the returned element in the source list, or -1 when no element qualified.")]
        out int MinIndex);

    [OSAction(Description = "Returns the element with the largest value at a given property path. Empty list returns MaxValue = '' and MaxIndex = -1.")]
    void List_MaxBy(
        [OSParameter(Description = "The source list serialized as a JSON array string")]
        string SourceListJson,
        [OSParameter(Description = "Property path to compare on. Supports nested paths and array indexing.")]
        string PropertyName,
        [OSParameter(Description = "When true, values are parsed as decimals for numeric comparison. When false, compared as ordinal text.")]
        bool NumericMode,
        [OSParameter(Description = "The full element at the maximum, or 'null' when the source is empty or no item had a value at PropertyName.")]
        out string ElementJson,
        [OSParameter(Description = "The raw value at PropertyName for the returned element, or '' when the list is empty.")]
        out string MaxValue,
        [OSParameter(Description = "The 0-based index of the returned element in the source list, or -1 when no element qualified.")]
        out int MaxIndex);

    [OSAction(Description = "Applies a scalar aggregation over a property. Sum / Avg / Min / Max parse each value as a decimal; Count / CountDistinct work on any value. Missing or non-parseable values are skipped and reported via MatchedCount.")]
    void List_Aggregate(
        [OSParameter(Description = "The source list serialized as a JSON array string")]
        string SourceListJson,
        [OSParameter(Description = "Property path to aggregate on. Supports nested paths and array indexing.")]
        string PropertyName,
        [OSParameter(Description = "One of: Sum, Avg, Min, Max, Count, CountDistinct. Case-insensitive. Empty defaults to Sum.")]
        string Operation,
        [OSParameter(Description = "The aggregation result as text. Sum / Avg / Min / Max are decimals formatted with InvariantCulture; Count and CountDistinct are non-negative integers. Empty when no item contributed (e.g. empty source, or numeric op with all non-numeric values).")]
        out string ResultValue,
        [OSParameter(Description = "Number of items that contributed to the aggregation — items with a non-null value at PropertyName, and (for numeric ops) parseable as decimal.")]
        out int MatchedCount);

    // ── Set operations (2) ───────────────────────────────────────────────

    [OSAction(Description = "Set intersection A ∩ B on a keyed property. Returns the elements from A whose key value also appears in B, preserving A's original order.")]
    void List_Intersect(
        [OSParameter(Description = "The base JSON list")]
        string ListAJson,
        [OSParameter(Description = "The intersection JSON list")]
        string ListBJson,
        [OSParameter(Description = "Property path to match on. Supports nested paths and array indexing.")]
        string MatchKey,
        [OSParameter(Description = "Comparison operator for key matching: Equals (default), Contains, StartsWith, EndsWith, NotEquals, GreaterThan, LessThan, GreaterOrEqual, LessOrEqual. Empty = Equals.")]
        string ComparisonOperator,
        [OSParameter(Description = "When true, key comparison is case-sensitive. When false (default), case-insensitive.")]
        bool CaseSensitive,
        [OSParameter(Description = "The elements in A whose MatchKey has at least one match in B.")]
        out string IntersectionListJson);

    [OSAction(Description = "Set union of two lists: concatenates A + B and deduplicates by a keyed property (first occurrence wins; A's items appear first).")]
    void List_Union(
        [OSParameter(Description = "The first JSON list")]
        string ListAJson,
        [OSParameter(Description = "The second JSON list")]
        string ListBJson,
        [OSParameter(Description = "Property path used as the uniqueness key. Empty = dedupe by the whole element's JSON.")]
        string MatchKey,
        [OSParameter(Description = "When true, key comparison is case-sensitive. When false (default), case-insensitive.")]
        bool CaseSensitive,
        [OSParameter(Description = "A + B with duplicates removed by MatchKey.")]
        out string UnionListJson);

    // ── Split / partition (3) ────────────────────────────────────────────

    [OSAction(Description = "Splits a JSON list at a given index into a Left (elements before Index) and Right (Index and later) pair.")]
    void List_SplitAt(
        [OSParameter(Description = "The source list serialized as a JSON array string")]
        string SourceListJson,
        [OSParameter(Description = "0-based split index. Negative values count from the end. Values <= 0 leave Left empty; values >= list.Length leave Right empty.")]
        int Index,
        [OSParameter(Description = "The elements with position < Index (or the empty list when Index <= 0).")]
        out string LeftListJson,
        [OSParameter(Description = "The elements with position >= Index (or the empty list when Index >= list.Length).")]
        out string RightListJson);

    [OSAction(Description = "Partitions a JSON list into matching and non-matching sublists based on a single property condition. Non-mutating dual of List_PopMultipleByCondition — both sides are returned.")]
    void List_Partition(
        [OSParameter(Description = "The source list serialized as a JSON array string")]
        string SourceListJson,
        [OSParameter(Description = "Property path to check.")]
        string PropertyName,
        [OSParameter(Description = "The value to compare against.")]
        string TargetValue,
        [OSParameter(Description = "Comparison operator. Empty = Equals.")]
        string ComparisonOperator,
        [OSParameter(Description = "When true, string comparison is case-sensitive.")]
        bool CaseSensitive,
        [OSParameter(Description = "Elements that satisfy the condition.")]
        out string MatchingListJson,
        [OSParameter(Description = "Elements that do NOT satisfy the condition.")]
        out string NonMatchingListJson);

    [OSAction(Description = "Partitions a JSON list into matching and non-matching sublists based on multiple conditions combined with AND / OR.")]
    void List_PartitionByConditions(
        [OSParameter(Description = "The source list serialized as a JSON array string")]
        string SourceListJson,
        [OSParameter(Description = "List of Condition Structures.")]
        List<Condition> Conditions,
        [OSParameter(Description = "'AND' (default) or 'OR'. See ListUtils.LogicalOperators.")]
        string LogicalOperator,
        [OSParameter(Description = "Elements that satisfy the combined condition.")]
        out string MatchingListJson,
        [OSParameter(Description = "Elements that do NOT satisfy the combined condition.")]
        out string NonMatchingListJson);

    // ── Shape (3) ────────────────────────────────────────────────────────

    [OSAction(Description = "Reverses a JSON list. Equivalent to List_Slice(-1, 0, -1) but explicit and cheaper.")]
    void List_Reverse(
        [OSParameter(Description = "The source list serialized as a JSON array string")]
        string SourceListJson,
        [OSParameter(Description = "The reversed JSON array.")]
        out string ReversedListJson);

    [OSAction(Description = "Concatenates a list of JSON-array strings into a single flat JSON array. Inverse of List_Chunk — feed List_Chunk's output directly and get back the original list. Non-array entries are skipped.")]
    void List_Flatten(
        [OSParameter(Description = "A list of JSON-array strings (typically List_Chunk's output). Each entry is a self-contained JSON array. Non-array entries are silently skipped.")]
        List<string> ChunksListJson,
        [OSParameter(Description = "The concatenation of every parsed inner array, in order.")]
        out string FlatListJson);

    [OSAction(Description = "Returns a random subset of SampleSize elements from the source list, without replacement. Uses the same RNG semantics as List_Shuffle (Seed = 0 → CSPRNG; Seed != 0 → deterministic).")]
    void List_Sample(
        [OSParameter(Description = "The source list serialized as a JSON array string")]
        string SourceListJson,
        [OSParameter(Description = "Number of elements to sample. Values <= 0 return the empty list; values >= list.Length return the full list in shuffled order.")]
        int SampleSize,
        [OSParameter(Description = "Random seed. 0 = non-deterministic (CSPRNG); non-zero = deterministic (System.Random).")]
        int Seed,
        [OSParameter(Description = "The sampled JSON array of length min(SampleSize, list.Length). Source is not mutated.")]
        out string SampleListJson);

    // ── Mass update (2) ──────────────────────────────────────────────────

    [OSAction(Description = "Applies the same property update to every element that satisfies a multi-condition filter (AND/OR combination of any number of property comparisons). Returns the modified list and the number of items that were updated.")]
    void List_ReplaceWhere(
        [OSParameter(Description = "The source list serialized as a JSON array string")]
        string SourceListJson,
        [OSParameter(Description = "List of Condition Structures. Same shape used by List_PopByConditions.")]
        List<Condition> Conditions,
        [OSParameter(Description = "How to combine conditions: 'AND' (default) or 'OR'. See ListUtils.LogicalOperators.")]
        string LogicalOperator,
        [OSParameter(Description = "Property path on each matched item to overwrite. Missing intermediate objects are created; array indices must already exist.")]
        string UpdateProperty,
        [OSParameter(Description = "The new value as JSON. Falls back to a raw string when the value is not valid JSON.")]
        string NewValueJson,
        [OSParameter(Description = "The updated JSON array with every matching element's UpdateProperty replaced by NewValueJson.")]
        out string UpdatedListJson,
        [OSParameter(Description = "Number of elements that matched the combined condition and were updated.")]
        out int MatchCount);

    [OSAction(Description = "Updates the same PropertyName to the same NewValueJson on multiple elements at specified indices. Out-of-range indices are silently skipped.")]
    void List_UpdateMultipleAt(
        [OSParameter(Description = "The source list serialized as a JSON array string")]
        string SourceListJson,
        [OSParameter(Description = "Comma-separated 0-based indices to update (e.g. '1,3,5'). Negative values count from the end. Whitespace around commas is trimmed.")]
        string IndicesToUpdate,
        [OSParameter(Description = "Property path on each targeted item to overwrite. Missing intermediate objects are created; array indices must already exist.")]
        string PropertyName,
        [OSParameter(Description = "The new value as JSON. Falls back to a raw string when the value is not valid JSON.")]
        string NewValueJson,
        [OSParameter(Description = "The updated JSON array with the property changed at every in-range index.")]
        out string UpdatedListJson,
        [OSParameter(Description = "Number of indices that were in-range and successfully updated.")]
        out int UpdatedCount);

    // ── Multi-list zip (2) ───────────────────────────────────────────────

    [OSAction(Description = "Zips N lists together into paired objects by matching position. Generalisation of List_Zip to any number of inputs. Output length is the minimum of the input lengths.")]
    void List_ZipMany(
        [OSParameter(Description = "A list of JSON-array strings — one entry per input list.")]
        List<string> ListsJson,
        [OSParameter(Description = "A list of property names — one label per input list, used as object keys in the output. If shorter than ListsJson, remaining lists default to 'Items0', 'Items1', ...")]
        List<string> KeyNamesJson,
        [OSParameter(Description = "JSON array of objects, one per position, each holding one entry per input list.")]
        out string ZippedListJson);

    [OSAction(Description = "Cogroups N lists by a shared key. Each output group contains the Key plus one named array per input list of items sharing that key. Generalisation of List_ZipGroupBy to any number of inputs.")]
    void List_ZipManyGroupBy(
        [OSParameter(Description = "A list of JSON-array strings — one entry per input list.")]
        List<string> ListsJson,
        [OSParameter(Description = "A list of property paths — position i is the key path applied to items of ListsJson[i]. Supports nested paths.")]
        List<string> KeyPropertiesJson,
        [OSParameter(Description = "A list of property names — position i is the output field name for ListsJson[i]'s array within each group. Shorter lists default to 'Items0', 'Items1', ...")]
        List<string> KeyNamesJson,
        [OSParameter(Description = "When true, key comparison is case-sensitive. When false (default), case-insensitive.")]
        bool CaseSensitive,
        [OSParameter(Description = "JSON array of groups: [{Key: <keyValue>, <KeyNames[0]>: [...], <KeyNames[1]>: [...], ...}, ...]. Items whose key is null share the 'Unknown' bucket.")]
        out string GroupedListJson);
}
