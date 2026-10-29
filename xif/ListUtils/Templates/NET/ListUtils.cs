using System;
using System.Collections;
using System.Data;
using OutSystems.HubEdition.RuntimePlatform;
using OutSystems.RuntimePublic.Db;

namespace OutSystems.NssListUtils {

	public class CssListUtils: IssListUtils {

		/// <summary>
		/// Removes an element at a specific index from a JSON list. Returns the removed element and the updated list as JSON.
		/// </summary>
		/// <param name="ssSourceListJson">The source list serialized as a JSON array string</param>
		/// <param name="ssIndex">The 0-based index of the element to remove. Negative values count from the end (-1 = last element).</param>
		/// <param name="ssUpdatedListJson">The JSON array without the popped element</param>
		/// <param name="ssPoppedElementJson">The JSON element that was removed</param>
		public void MssList_Pop(string ssSourceListJson, int ssIndex, out string ssUpdatedListJson, out string ssPoppedElementJson) {
			ssUpdatedListJson = "";
			ssPoppedElementJson = "";
			// TODO: Write implementation for action
		} // MssList_Pop

		/// <summary>
		/// Removes multiple elements at specified indices from a JSON list. Returns the removed elements and the updated list as JSON.
		/// </summary>
		/// <param name="ssSourceListJson">The source list serialized as a JSON array string</param>
		/// <param name="ssIndicesToPop">Comma-separated 0-based indices to remove (e.g. &apos;1,3,-1&apos;). Negative values count from the end. Duplicates, non-numeric and out-of-range entries are ignored.</param>
		/// <param name="ssUpdatedListJson">The JSON array without the popped elements</param>
		/// <param name="ssPoppedElementsJson">The JSON array of elements that were removed</param>
		public void MssList_PopMultiple(string ssSourceListJson, string ssIndicesToPop, out string ssUpdatedListJson, out string ssPoppedElementsJson) {
			ssUpdatedListJson = "";
			ssPoppedElementsJson = "";
			// TODO: Write implementation for action
		} // MssList_PopMultiple

		/// <summary>
		/// Pops the first element matching a property condition. Returns the popped element and modified list as JSON.
		/// </summary>
		/// <param name="ssSourceListJson">The source list serialized as a JSON string</param>
		/// <param name="ssPropertyName">Property path to check. Supports nested paths with dots (e.g. &apos;Address.City&apos;) and array indexing (e.g. &apos;Items[0]&apos;, &apos;Tags[-1]&apos;). CamelCase fallback applied at each segment.</param>
		/// <param name="ssTargetValue">The value to filter by (as a string, e.g. &apos;true&apos; or &apos;5&apos;)</param>
		/// <param name="ssComparisonOperator">Comparison operator: Equals (default), NotEquals, Contains, StartsWith, EndsWith, GreaterThan, LessThan, GreaterOrEqual, LessOrEqual. Empty = Equals.</param>
		/// <param name="ssCaseSensitive">When true, string comparison is case-sensitive. When false (default), case-insensitive. Ignored by numeric operators.</param>
		/// <param name="ssSearchFromEnd">When true, searches from the end of the list backwards (pops the LAST match). When false (default), searches from the beginning (pops the FIRST match).</param>
		/// <param name="ssUpdatedListJson">The updated JSON list without the matched element</param>
		/// <param name="ssPoppedElementJson">The single JSON object that was matched and removed</param>
		public void MssList_PopByCondition(string ssSourceListJson, string ssPropertyName, string ssTargetValue, string ssComparisonOperator, bool ssCaseSensitive, bool ssSearchFromEnd, out string ssUpdatedListJson, out string ssPoppedElementJson) {
			ssUpdatedListJson = "";
			ssPoppedElementJson = "";
			// TODO: Write implementation for action
		} // MssList_PopByCondition

		/// <summary>
		/// Pops all elements matching a property condition. Returns the popped elements and modified list as JSON.
		/// </summary>
		/// <param name="ssSourceListJson">The source list serialized as a JSON string</param>
		/// <param name="ssPropertyName">Property path to check. Supports nested paths with dots and array indexing (e.g. &apos;Items[0].Name&apos;).</param>
		/// <param name="ssTargetValue">The value to filter by</param>
		/// <param name="ssComparisonOperator">Comparison operator: Equals (default), NotEquals, Contains, StartsWith, EndsWith, GreaterThan, LessThan, GreaterOrEqual, LessOrEqual. Empty = Equals.</param>
		/// <param name="ssCaseSensitive">When true, string comparison is case-sensitive. When false (default), case-insensitive.</param>
		/// <param name="ssUpdatedListJson">The updated JSON list without any matched elements</param>
		/// <param name="ssPoppedElementsJson">The JSON array of all items that were matched and removed</param>
		public void MssList_PopMultipleByCondition(string ssSourceListJson, string ssPropertyName, string ssTargetValue, string ssComparisonOperator, bool ssCaseSensitive, out string ssUpdatedListJson, out string ssPoppedElementsJson) {
			ssUpdatedListJson = "";
			ssPoppedElementsJson = "";
			// TODO: Write implementation for action
		} // MssList_PopMultipleByCondition

		/// <summary>
		/// Pops the first element matching multiple conditions combined with AND/OR. Returns the popped element and modified list as JSON.
		/// </summary>
		/// <param name="ssSourceListJson">The source list serialized as a JSON string</param>
		/// <param name="ssConditions">List of Condition Structures. Each entry has Path, Operator, Value, CaseSensitive. Build with the ListUtils.Operators constants (e.g. Operators.Equals) for compile-time safety.</param>
		/// <param name="ssLogicalOperator">How to combine conditions: &apos;AND&apos; (default) - all must match; &apos;OR&apos; - at least one must match. Empty = AND. See ListUtils.LogicalOperators.</param>
		/// <param name="ssSearchFromEnd">When true, searches from the end of the list backwards (pops the LAST match). When false (default), searches from the beginning (pops the FIRST match).</param>
		/// <param name="ssUpdatedListJson">The updated JSON list without the matched element</param>
		/// <param name="ssPoppedElementJson">The single JSON object that was matched and removed</param>
		public void MssList_PopByConditions(string ssSourceListJson, RLConditionRecordList ssConditions, string ssLogicalOperator, bool ssSearchFromEnd, out string ssUpdatedListJson, out string ssPoppedElementJson) {
			ssUpdatedListJson = "";
			ssPoppedElementJson = "";
			// TODO: Write implementation for action
		} // MssList_PopByConditions

		/// <summary>
		/// Pops all elements matching multiple conditions combined with AND/OR. Returns the popped elements and modified list as JSON.
		/// </summary>
		/// <param name="ssSourceListJson">The source list serialized as a JSON string</param>
		/// <param name="ssConditions">List of Condition Structures.</param>
		/// <param name="ssLogicalOperator">How to combine conditions: &apos;AND&apos; (default) or &apos;OR&apos;. Empty = AND. See ListUtils.LogicalOperators.</param>
		/// <param name="ssUpdatedListJson">The updated JSON list without any matched elements</param>
		/// <param name="ssPoppedElementsJson">The JSON array of all items that were matched and removed</param>
		public void MssList_PopMultipleByConditions(string ssSourceListJson, RLConditionRecordList ssConditions, string ssLogicalOperator, out string ssUpdatedListJson, out string ssPoppedElementsJson) {
			ssUpdatedListJson = "";
			ssPoppedElementsJson = "";
			// TODO: Write implementation for action
		} // MssList_PopMultipleByConditions

		/// <summary>
		/// Combines two lists into a single list of paired objects based on matching indexes.
		/// </summary>
		/// <param name="ssListAJson">The first JSON list source</param>
		/// <param name="ssListBJson">The second JSON list source</param>
		/// <param name="ssKeyNameA">Key property label for List A entries in the output</param>
		/// <param name="ssKeyNameB">Key property label for List B entries in the output</param>
		/// <param name="ssZippedListJson">The combined JSON array of paired objects</param>
		public void MssList_Zip(string ssListAJson, string ssListBJson, string ssKeyNameA, string ssKeyNameB, out string ssZippedListJson) {
			ssZippedListJson = "";
			// TODO: Write implementation for action
		} // MssList_Zip

		/// <summary>
		/// Groups a JSON list by a specific property name.
		/// </summary>
		/// <param name="ssSourceListJson">The source JSON list</param>
		/// <param name="ssPropertyName">Property path to group by. Supports nested paths with dots (e.g. &apos;Address.City&apos;). CamelCase fallback applied at each segment.</param>
		/// <param name="ssGroupedListJson">Grouped JSON array with Key and Items per group</param>
		public void MssList_GroupBy(string ssSourceListJson, string ssPropertyName, out string ssGroupedListJson) {
			ssGroupedListJson = "";
			// TODO: Write implementation for action
		} // MssList_GroupBy

		/// <summary>
		/// Groups two JSON lists by a shared key value. Each output group contains the Key plus a named array of items from ListA and a named array of items from ListB that share the same key. Union of first-seen keys across both lists preserves ordering. Useful for cogroup / two-list join scenarios (orders + payments per customer, etc.).
		/// </summary>
		/// <param name="ssListAJson">The first JSON list</param>
		/// <param name="ssListBJson">The second JSON list</param>
		/// <param name="ssKeyPropertyA">Property path in ListA that supplies the group key. Supports nested paths and array indexing.</param>
		/// <param name="ssKeyPropertyB">Property path in ListB that supplies the group key. Supports nested paths and array indexing.</param>
		/// <param name="ssKeyNameA">Output field name for the ListA items array within each group (e.g. &apos;Orders&apos;).</param>
		/// <param name="ssKeyNameB">Output field name for the ListB items array within each group (e.g. &apos;Payments&apos;).</param>
		/// <param name="ssCaseSensitive">When true, key comparison is case-sensitive. When false (default), case-insensitive.</param>
		/// <param name="ssGroupedListJson">JSON array of groups: [{&quot;Key&quot;: &lt;keyValue&gt;, &lt;KeyNameA&gt;: [...ListA items...], &lt;KeyNameB&gt;: [...ListB items...]}, ...]. Items with no key value fall into a single &apos;Unknown&apos; bucket.</param>
		public void MssList_ZipGroupBy(string ssListAJson, string ssListBJson, string ssKeyPropertyA, string ssKeyPropertyB, string ssKeyNameA, string ssKeyNameB, bool ssCaseSensitive, out string ssGroupedListJson) {
			ssGroupedListJson = "";
			// TODO: Write implementation for action
		} // MssList_ZipGroupBy

		/// <summary>
		/// Finds elements that exist in List A but not in List B (set difference). Matches on a specified key.
		/// </summary>
		/// <param name="ssListAJson">The base JSON list</param>
		/// <param name="ssListBJson">The subtraction JSON list</param>
		/// <param name="ssMatchKey">Property path to match on. Supports nested paths and array indexing (e.g. &apos;Meta.Id&apos;, &apos;Refs[0].Code&apos;).</param>
		/// <param name="ssComparisonOperator">Comparison operator for key matching: Equals (default), Contains, StartsWith. Empty = Equals.</param>
		/// <param name="ssCaseSensitive">When true, key comparison is case-sensitive. When false (default), case-insensitive.</param>
		/// <param name="ssDifferenceListJson">The elements in A that have no match in B</param>
		public void MssList_Difference(string ssListAJson, string ssListBJson, string ssMatchKey, string ssComparisonOperator, bool ssCaseSensitive, out string ssDifferenceListJson) {
			ssDifferenceListJson = "";
			// TODO: Write implementation for action
		} // MssList_Difference

		/// <summary>
		/// Splits a JSON list into a list of JSON-string chunks of a fixed size. Last chunk may be smaller. Essential for batching API payloads and throttled loops. Each output entry is a standalone JSON array ready for JSON Deserialize into the caller&apos;s Structure List.
		/// </summary>
		/// <param name="ssSourceListJson">The source list serialized as a JSON array string</param>
		/// <param name="ssChunkSize">Maximum number of elements per chunk. Must be &gt;= 1; values &lt;= 0 return an empty list.</param>
		/// <param name="ssChunksListJson">A list of JSON-array strings — one string per chunk (e.g. [&quot;[..first N..]&quot;, &quot;[..next N..]&quot;, ...]). The last entry may hold fewer elements than ChunkSize. Empty list when the source is empty or ChunkSize &lt;= 0.</param>
		public void MssList_Chunk(string ssSourceListJson, int ssChunkSize, out RLTextItemRecordList ssChunksListJson) {
			ssChunksListJson = new RLTextItemRecordList();
			// TODO: Write implementation for action
		} // MssList_Chunk

		/// <summary>
		/// Filters a JSON list to unique elements based on a property value (first occurrence wins). Works on structures — native ODC Distinct only supports basic types.
		/// </summary>
		/// <param name="ssSourceListJson">The source list serialized as a JSON array string</param>
		/// <param name="ssPropertyName">Property path used as the uniqueness key. Supports nested paths (e.g. &apos;Address.City&apos;) and array indexing (e.g. &apos;Tags[0]&apos;). CamelCase fallback applied at each segment.</param>
		/// <param name="ssCaseSensitive">When true, key comparison is case-sensitive. When false (default), case-insensitive.</param>
		/// <param name="ssDistinctListJson">The JSON array containing only the first occurrence of each unique key, preserving source order.</param>
		public void MssList_DistinctBy(string ssSourceListJson, string ssPropertyName, bool ssCaseSensitive, out string ssDistinctListJson) {
			ssDistinctListJson = "";
			// TODO: Write implementation for action
		} // MssList_DistinctBy

		/// <summary>
		/// Extracts a subset of a JSON list using Python/JavaScript-style Start/End/Step. Supports negative indices, negative step (reverse), and End &lt;= 0 meaning &apos;to end of list&apos;.
		/// </summary>
		/// <param name="ssSourceListJson">The source list serialized as a JSON array string</param>
		/// <param name="ssStart">0-based inclusive start index. Negative values count from the end (e.g. -2 = second-to-last). Clamped to the list bounds.</param>
		/// <param name="ssEnd">0-based exclusive end index. Negative values count from the end. Special case: End == 0 is treated as &apos;unspecified&apos; — for positive Step it means &apos;to end of list&apos;, for negative Step it means &apos;past the beginning&apos; (Python default).</param>
		/// <param name="ssStep">Step between selected elements. 0 is treated as 1. Positive walks forward; negative walks backward (reverse slice, matching Python semantics).</param>
		/// <param name="ssSliceListJson">The sliced JSON array</param>
		public void MssList_Slice(string ssSourceListJson, int ssStart, int ssEnd, int ssStep, out string ssSliceListJson) {
			ssSliceListJson = "";
			// TODO: Write implementation for action
		} // MssList_Slice

		/// <summary>
		/// Randomizes the order of a JSON list using the Fisher-Yates algorithm. Deterministic when Seed is non-zero; otherwise uses a cryptographically-seeded RNG.
		/// </summary>
		/// <param name="ssSourceListJson">The source list serialized as a JSON array string</param>
		/// <param name="ssSeed">Random seed. Use 0 for a non-deterministic shuffle. Any non-zero value produces the same permutation every call (useful for reproducible tests).</param>
		/// <param name="ssShuffledListJson">The shuffled JSON array. Original source list is not mutated.</param>
		public void MssList_Shuffle(string ssSourceListJson, int ssSeed, out string ssShuffledListJson) {
			ssShuffledListJson = "";
			// TODO: Write implementation for action
		} // MssList_Shuffle

		/// <summary>
		/// Sets a single property of the item at a given index. Returns the modified list and the previous value of that property.
		/// </summary>
		/// <param name="ssSourceListJson">The source list serialized as a JSON array string</param>
		/// <param name="ssIndex">0-based index of the item to modify. Negative values count from the end. Out-of-range indices return the source unchanged and PreviousValueJson = &apos;null&apos;.</param>
		/// <param name="ssPropertyName">Property path to update on the target item. Supports nested paths with dots (e.g. &apos;Address.City&apos;) and array indexing (e.g. &apos;Tags[0]&apos;). Missing intermediate objects are created (arrays are NOT auto-created; if an array indexing step encounters a missing or non-array node, the action returns the source unchanged).</param>
		/// <param name="ssNewValueJson">The new value as JSON (e.g. &apos;&quot;Active&quot;&apos;, &apos;42&apos;, &apos;true&apos;, &apos;{&quot;City&quot;:&quot;Lisbon&quot;}&apos;, &apos;null&apos;). Falls back to a raw string when the value is not valid JSON.</param>
		/// <param name="ssUpdatedListJson">The updated JSON array with the property changed</param>
		/// <param name="ssPreviousValueJson">The previous JSON value of the modified property, or &apos;null&apos; when the property did not exist, when the property existed with a JSON null value (these two cases are indistinguishable), or when the index was out of range</param>
		public void MssList_UpdateAt(string ssSourceListJson, int ssIndex, string ssPropertyName, string ssNewValueJson, out string ssUpdatedListJson, out string ssPreviousValueJson) {
			ssUpdatedListJson = "";
			ssPreviousValueJson = "";
			// TODO: Write implementation for action
		} // MssList_UpdateAt

		/// <summary>
		/// Returns the element with the smallest value at a given property path. Empty list returns MinValue = &apos;&apos; and MinIndex = -1.
		/// </summary>
		/// <param name="ssSourceListJson">The source list serialized as a JSON array string</param>
		/// <param name="ssPropertyName">Property path to compare on. Supports nested paths (e.g. &apos;Meta.Score&apos;) and array indexing (e.g. &apos;Items[0].Qty&apos;).</param>
		/// <param name="ssNumericMode">When true, values are parsed as decimals for numeric comparison. When false, compared as ordinal text (case-insensitive).</param>
		/// <param name="ssElementJson">The full element at the minimum, or &apos;null&apos; when the source is empty or no item had a value at PropertyName.</param>
		/// <param name="ssMinValue">The raw value at PropertyName for the returned element, or &apos;&apos; when the list is empty.</param>
		/// <param name="ssMinIndex">The 0-based index of the returned element in the source list, or -1 when no element qualified.</param>
		public void MssList_MinBy(string ssSourceListJson, string ssPropertyName, bool ssNumericMode, out string ssElementJson, out string ssMinValue, out int ssMinIndex) {
			ssElementJson = "";
			ssMinValue = "";
			ssMinIndex = 0;
			// TODO: Write implementation for action
		} // MssList_MinBy

		/// <summary>
		/// Returns the element with the largest value at a given property path. Empty list returns MaxValue = &apos;&apos; and MaxIndex = -1.
		/// </summary>
		/// <param name="ssSourceListJson">The source list serialized as a JSON array string</param>
		/// <param name="ssPropertyName">Property path to compare on. Supports nested paths and array indexing.</param>
		/// <param name="ssNumericMode">When true, values are parsed as decimals for numeric comparison. When false, compared as ordinal text.</param>
		/// <param name="ssElementJson">The full element at the maximum, or &apos;null&apos; when the source is empty or no item had a value at PropertyName.</param>
		/// <param name="ssMaxValue">The raw value at PropertyName for the returned element, or &apos;&apos; when the list is empty.</param>
		/// <param name="ssMaxIndex">The 0-based index of the returned element in the source list, or -1 when no element qualified.</param>
		public void MssList_MaxBy(string ssSourceListJson, string ssPropertyName, bool ssNumericMode, out string ssElementJson, out string ssMaxValue, out int ssMaxIndex) {
			ssElementJson = "";
			ssMaxValue = "";
			ssMaxIndex = 0;
			// TODO: Write implementation for action
		} // MssList_MaxBy

		/// <summary>
		/// Applies a scalar aggregation over a property. Sum / Avg / Min / Max parse each value as a decimal; Count / CountDistinct work on any value. Missing or non-parseable values are skipped and reported via MatchedCount.
		/// </summary>
		/// <param name="ssSourceListJson">The source list serialized as a JSON array string</param>
		/// <param name="ssPropertyName">Property path to aggregate on. Supports nested paths and array indexing.</param>
		/// <param name="ssOperation">One of: Sum, Avg, Min, Max, Count, CountDistinct. Case-insensitive. Empty defaults to Sum.</param>
		/// <param name="ssResultValue">The aggregation result as text. Sum / Avg / Min / Max are decimals formatted with InvariantCulture; Count and CountDistinct are non-negative integers. Empty when no item contributed (e.g. empty source, or numeric op with all non-numeric values).</param>
		/// <param name="ssMatchedCount">Number of items that contributed to the aggregation — items with a non-null value at PropertyName, and (for numeric ops) parseable as decimal.</param>
		public void MssList_Aggregate(string ssSourceListJson, string ssPropertyName, string ssOperation, out string ssResultValue, out int ssMatchedCount) {
			ssResultValue = "";
			ssMatchedCount = 0;
			// TODO: Write implementation for action
		} // MssList_Aggregate

		/// <summary>
		/// Set intersection A ∩ B on a keyed property. Returns the elements from A whose key value also appears in B, preserving A&apos;s original order.
		/// </summary>
		/// <param name="ssListAJson">The base JSON list</param>
		/// <param name="ssListBJson">The intersection JSON list</param>
		/// <param name="ssMatchKey">Property path to match on. Supports nested paths and array indexing.</param>
		/// <param name="ssComparisonOperator">Comparison operator for key matching: Equals (default), Contains, StartsWith, EndsWith, NotEquals, GreaterThan, LessThan, GreaterOrEqual, LessOrEqual. Empty = Equals.</param>
		/// <param name="ssCaseSensitive">When true, key comparison is case-sensitive. When false (default), case-insensitive.</param>
		/// <param name="ssIntersectionListJson">The elements in A whose MatchKey has at least one match in B.</param>
		public void MssList_Intersect(string ssListAJson, string ssListBJson, string ssMatchKey, string ssComparisonOperator, bool ssCaseSensitive, out string ssIntersectionListJson) {
			ssIntersectionListJson = "";
			// TODO: Write implementation for action
		} // MssList_Intersect

		/// <summary>
		/// Set union of two lists: concatenates A + B and deduplicates by a keyed property (first occurrence wins; A&apos;s items appear first).
		/// </summary>
		/// <param name="ssListAJson">The first JSON list</param>
		/// <param name="ssListBJson">The second JSON list</param>
		/// <param name="ssMatchKey">Property path used as the uniqueness key. Empty = dedupe by the whole element&apos;s JSON.</param>
		/// <param name="ssCaseSensitive">When true, key comparison is case-sensitive. When false (default), case-insensitive.</param>
		/// <param name="ssUnionListJson">A + B with duplicates removed by MatchKey.</param>
		public void MssList_Union(string ssListAJson, string ssListBJson, string ssMatchKey, bool ssCaseSensitive, out string ssUnionListJson) {
			ssUnionListJson = "";
			// TODO: Write implementation for action
		} // MssList_Union

		/// <summary>
		/// Splits a JSON list at a given index into a Left (elements before Index) and Right (Index and later) pair.
		/// </summary>
		/// <param name="ssSourceListJson">The source list serialized as a JSON array string</param>
		/// <param name="ssIndex">0-based split index. Negative values count from the end. Values &lt;= 0 leave Left empty; values &gt;= list.Length leave Right empty.</param>
		/// <param name="ssLeftListJson">The elements with position &lt; Index (or the empty list when Index &lt;= 0).</param>
		/// <param name="ssRightListJson">The elements with position &gt;= Index (or the empty list when Index &gt;= list.Length).</param>
		public void MssList_SplitAt(string ssSourceListJson, int ssIndex, out string ssLeftListJson, out string ssRightListJson) {
			ssLeftListJson = "";
			ssRightListJson = "";
			// TODO: Write implementation for action
		} // MssList_SplitAt

		/// <summary>
		/// Partitions a JSON list into matching and non-matching sublists based on a single property condition. Non-mutating dual of List_PopMultipleByCondition — both sides are returned.
		/// </summary>
		/// <param name="ssSourceListJson">The source list serialized as a JSON array string</param>
		/// <param name="ssPropertyName">Property path to check.</param>
		/// <param name="ssTargetValue">The value to compare against.</param>
		/// <param name="ssComparisonOperator">Comparison operator. Empty = Equals.</param>
		/// <param name="ssCaseSensitive">When true, string comparison is case-sensitive.</param>
		/// <param name="ssMatchingListJson">Elements that satisfy the condition.</param>
		/// <param name="ssNonMatchingListJson">Elements that do NOT satisfy the condition.</param>
		public void MssList_Partition(string ssSourceListJson, string ssPropertyName, string ssTargetValue, string ssComparisonOperator, bool ssCaseSensitive, out string ssMatchingListJson, out string ssNonMatchingListJson) {
			ssMatchingListJson = "";
			ssNonMatchingListJson = "";
			// TODO: Write implementation for action
		} // MssList_Partition

		/// <summary>
		/// Partitions a JSON list into matching and non-matching sublists based on multiple conditions combined with AND / OR.
		/// </summary>
		/// <param name="ssSourceListJson">The source list serialized as a JSON array string</param>
		/// <param name="ssConditions">List of Condition Structures.</param>
		/// <param name="ssLogicalOperator">&apos;AND&apos; (default) or &apos;OR&apos;. See ListUtils.LogicalOperators.</param>
		/// <param name="ssMatchingListJson">Elements that satisfy the combined condition.</param>
		/// <param name="ssNonMatchingListJson">Elements that do NOT satisfy the combined condition.</param>
		public void MssList_PartitionByConditions(string ssSourceListJson, RLConditionRecordList ssConditions, string ssLogicalOperator, out string ssMatchingListJson, out string ssNonMatchingListJson) {
			ssMatchingListJson = "";
			ssNonMatchingListJson = "";
			// TODO: Write implementation for action
		} // MssList_PartitionByConditions

		/// <summary>
		/// Reverses a JSON list. Equivalent to List_Slice(-1, 0, -1) but explicit and cheaper.
		/// </summary>
		/// <param name="ssSourceListJson">The source list serialized as a JSON array string</param>
		/// <param name="ssReversedListJson">The reversed JSON array.</param>
		public void MssList_Reverse(string ssSourceListJson, out string ssReversedListJson) {
			ssReversedListJson = "";
			// TODO: Write implementation for action
		} // MssList_Reverse

		/// <summary>
		/// Concatenates a list of JSON-array strings into a single flat JSON array. Inverse of List_Chunk — feed List_Chunk&apos;s output directly and get back the original list. Non-array entries are skipped.
		/// </summary>
		/// <param name="ssChunksListJson">A list of JSON-array strings (typically List_Chunk&apos;s output). Each entry is a self-contained JSON array. Non-array entries are silently skipped.</param>
		/// <param name="ssFlatListJson">The concatenation of every parsed inner array, in order.</param>
		public void MssList_Flatten(RLTextItemRecordList ssChunksListJson, out string ssFlatListJson) {
			ssFlatListJson = "";
			// TODO: Write implementation for action
		} // MssList_Flatten

		/// <summary>
		/// Returns a random subset of SampleSize elements from the source list, without replacement. Uses the same RNG semantics as List_Shuffle (Seed = 0 → CSPRNG; Seed != 0 → deterministic).
		/// </summary>
		/// <param name="ssSourceListJson">The source list serialized as a JSON array string</param>
		/// <param name="ssSampleSize">Number of elements to sample. Values &lt;= 0 return the empty list; values &gt;= list.Length return the full list in shuffled order.</param>
		/// <param name="ssSeed">Random seed. 0 = non-deterministic (CSPRNG); non-zero = deterministic (System.Random).</param>
		/// <param name="ssSampleListJson">The sampled JSON array of length min(SampleSize, list.Length). Source is not mutated.</param>
		public void MssList_Sample(string ssSourceListJson, int ssSampleSize, int ssSeed, out string ssSampleListJson) {
			ssSampleListJson = "";
			// TODO: Write implementation for action
		} // MssList_Sample

		/// <summary>
		/// Applies the same property update to every element that satisfies a multi-condition filter (AND/OR combination of any number of property comparisons). Returns the modified list and the number of items that were updated.
		/// </summary>
		/// <param name="ssSourceListJson">The source list serialized as a JSON array string</param>
		/// <param name="ssConditions">List of Condition Structures. Same shape used by List_PopByConditions.</param>
		/// <param name="ssLogicalOperator">How to combine conditions: &apos;AND&apos; (default) or &apos;OR&apos;. See ListUtils.LogicalOperators.</param>
		/// <param name="ssUpdateProperty">Property path on each matched item to overwrite. Missing intermediate objects are created; array indices must already exist.</param>
		/// <param name="ssNewValueJson">The new value as JSON. Falls back to a raw string when the value is not valid JSON.</param>
		/// <param name="ssUpdatedListJson">The updated JSON array with every matching element&apos;s UpdateProperty replaced by NewValueJson.</param>
		/// <param name="ssMatchCount">Number of elements that matched the combined condition and were updated.</param>
		public void MssList_ReplaceWhere(string ssSourceListJson, RLConditionRecordList ssConditions, string ssLogicalOperator, string ssUpdateProperty, string ssNewValueJson, out string ssUpdatedListJson, out int ssMatchCount) {
			ssUpdatedListJson = "";
			ssMatchCount = 0;
			// TODO: Write implementation for action
		} // MssList_ReplaceWhere

		/// <summary>
		/// Updates the same PropertyName to the same NewValueJson on multiple elements at specified indices. Out-of-range indices are silently skipped.
		/// </summary>
		/// <param name="ssSourceListJson">The source list serialized as a JSON array string</param>
		/// <param name="ssIndicesToUpdate">Comma-separated 0-based indices to update (e.g. &apos;1,3,5&apos;). Negative values count from the end. Whitespace around commas is trimmed.</param>
		/// <param name="ssPropertyName">Property path on each targeted item to overwrite. Missing intermediate objects are created; array indices must already exist.</param>
		/// <param name="ssNewValueJson">The new value as JSON. Falls back to a raw string when the value is not valid JSON.</param>
		/// <param name="ssUpdatedListJson">The updated JSON array with the property changed at every in-range index.</param>
		/// <param name="ssUpdatedCount">Number of indices that were in-range and successfully updated.</param>
		public void MssList_UpdateMultipleAt(string ssSourceListJson, string ssIndicesToUpdate, string ssPropertyName, string ssNewValueJson, out string ssUpdatedListJson, out int ssUpdatedCount) {
			ssUpdatedListJson = "";
			ssUpdatedCount = 0;
			// TODO: Write implementation for action
		} // MssList_UpdateMultipleAt

		/// <summary>
		/// Zips N lists together into paired objects by matching position. Generalisation of List_Zip to any number of inputs. Output length is the minimum of the input lengths.
		/// </summary>
		/// <param name="ssListsJson">A list of JSON-array strings — one entry per input list.</param>
		/// <param name="ssKeyNamesJson">A list of property names — one label per input list, used as object keys in the output. If shorter than ListsJson, remaining lists default to &apos;Items0&apos;, &apos;Items1&apos;, ...</param>
		/// <param name="ssZippedListJson">JSON array of objects, one per position, each holding one entry per input list.</param>
		public void MssList_ZipMany(RLTextItemRecordList ssListsJson, RLTextItemRecordList ssKeyNamesJson, out string ssZippedListJson) {
			ssZippedListJson = "";
			// TODO: Write implementation for action
		} // MssList_ZipMany

		/// <summary>
		/// Cogroups N lists by a shared key. Each output group contains the Key plus one named array per input list of items sharing that key. Generalisation of List_ZipGroupBy to any number of inputs.
		/// </summary>
		/// <param name="ssListsJson">A list of JSON-array strings — one entry per input list.</param>
		/// <param name="ssKeyPropertiesJson">A list of property paths — position i is the key path applied to items of ListsJson[i]. Supports nested paths.</param>
		/// <param name="ssKeyNamesJson">A list of property names — position i is the output field name for ListsJson[i]&apos;s array within each group. Shorter lists default to &apos;Items0&apos;, &apos;Items1&apos;, ...</param>
		/// <param name="ssCaseSensitive">When true, key comparison is case-sensitive. When false (default), case-insensitive.</param>
		/// <param name="ssGroupedListJson">JSON array of groups: [{Key: &lt;keyValue&gt;, &lt;KeyNames[0]&gt;: [...], &lt;KeyNames[1]&gt;: [...], ...}, ...]. Items whose key is null share the &apos;Unknown&apos; bucket.</param>
		public void MssList_ZipManyGroupBy(RLTextItemRecordList ssListsJson, RLTextItemRecordList ssKeyPropertiesJson, RLTextItemRecordList ssKeyNamesJson, bool ssCaseSensitive, out string ssGroupedListJson) {
			ssGroupedListJson = "";
			// TODO: Write implementation for action
		} // MssList_ZipManyGroupBy

		/// <summary>
		/// Groups a JSON list by N property paths (composite key). Each output group holds one field per key (labelled by KeyNames) plus the matching items. Generalisation of List_GroupBy to any number of key columns.
		/// </summary>
		/// <param name="ssSourceListJson">The source JSON list</param>
		/// <param name="ssPropertyPaths">Ordered list of N property paths that together form the composite group key. Supports nested paths and array indexing. Missing values fall back to the string &quot;Unknown&quot;.</param>
		/// <param name="ssKeyNames">Ordered list of N labels used as the field names for the group keys in the output. Shorter lists default to &apos;Key0&apos;, &apos;Key1&apos;, ...</param>
		/// <param name="ssItemsFieldName">Field name for the items array within each group. Empty defaults to &apos;Items&apos;.</param>
		/// <param name="ssCaseSensitive">When true, key comparison is case-sensitive. When false (default), case-insensitive.</param>
		/// <param name="ssGroupedListJson">JSON array of groups: [{&lt;KeyNames[0]&gt;:&lt;v0&gt;, &lt;KeyNames[1]&gt;:&lt;v1&gt;, ..., &lt;ItemsFieldName&gt;:[...items...]}, ...]. Groups appear in first-seen order.</param>
		public void MssList_GroupByMultiple(string ssSourceListJson, RLTextItemRecordList ssPropertyPaths, RLTextItemRecordList ssKeyNames, string ssItemsFieldName, bool ssCaseSensitive, out string ssGroupedListJson) {
			ssGroupedListJson = "";
			// TODO: Write implementation for action
		} // MssList_GroupByMultiple

		/// <summary>
		/// Cogroups two JSON lists by an N-key composite. Each output group carries one field per key plus one named array per input list of items sharing that composite key. Generalisation of List_ZipGroupBy to N keys.
		/// </summary>
		/// <param name="ssListAJson">The first JSON list</param>
		/// <param name="ssListBJson">The second JSON list</param>
		/// <param name="ssKeyPropertiesA">Ordered list of N property paths in ListA that form the composite group key.</param>
		/// <param name="ssKeyPropertiesB">Ordered list of N property paths in ListB that form the composite group key. Must have the same length as KeyPropertiesA.</param>
		/// <param name="ssKeyNames">Ordered list of N labels used as the field names for the group keys in the output. Shorter lists default to &apos;Key0&apos;, &apos;Key1&apos;, ...</param>
		/// <param name="ssKeyNameA">Output field name for the ListA items array within each group. Empty defaults to &apos;ItemsA&apos;.</param>
		/// <param name="ssKeyNameB">Output field name for the ListB items array within each group. Empty defaults to &apos;ItemsB&apos;.</param>
		/// <param name="ssCaseSensitive">When true, key comparison is case-sensitive. When false (default), case-insensitive.</param>
		/// <param name="ssGroupedListJson">JSON array of groups: [{&lt;KeyNames[0]&gt;:&lt;v0&gt;, ..., &lt;KeyNameA&gt;:[...], &lt;KeyNameB&gt;:[...]}, ...]. Union of first-seen composite keys across both lists preserves ordering.</param>
		public void MssList_ZipGroupByMultiple(string ssListAJson, string ssListBJson, RLTextItemRecordList ssKeyPropertiesA, RLTextItemRecordList ssKeyPropertiesB, RLTextItemRecordList ssKeyNames, string ssKeyNameA, string ssKeyNameB, bool ssCaseSensitive, out string ssGroupedListJson) {
			ssGroupedListJson = "";
			// TODO: Write implementation for action
		} // MssList_ZipGroupByMultiple

		/// <summary>
		/// Cogroups M lists by an N-key composite. Each output group carries one field per key plus one named array per input list of items sharing that composite key. Generalisation of List_ZipManyGroupBy to N keys.
		/// </summary>
		/// <param name="ssListsJson">A list of M JSON-array strings — one entry per input list.</param>
		/// <param name="ssKeyCount">Number of key columns N. Determines how many entries per list are consumed from KeyProperties.</param>
		/// <param name="ssKeyProperties">Flat list of M*N property paths in list-major order: entries [i*KeyCount .. i*KeyCount + KeyCount-1] belong to ListsJson[i]. Missing entries default to empty (Unknown bucket).</param>
		/// <param name="ssKeyNames">Ordered list of N labels used as the field names for the group keys in the output. Shorter lists default to &apos;Key0&apos;, &apos;Key1&apos;, ...</param>
		/// <param name="ssItemsFieldNames">Ordered list of M labels — one per input list — used as the field names for that list&apos;s items array. Shorter lists default to &apos;Items0&apos;, &apos;Items1&apos;, ...</param>
		/// <param name="ssCaseSensitive">When true, key comparison is case-sensitive. When false (default), case-insensitive.</param>
		/// <param name="ssGroupedListJson">JSON array of groups: [{&lt;KeyNames[0]&gt;:&lt;v0&gt;, ..., &lt;ItemsFieldNames[0]&gt;:[...], &lt;ItemsFieldNames[1]&gt;:[...], ...}, ...]. Items whose key is null share the &apos;Unknown&apos; bucket.</param>
		public void MssList_ZipManyGroupByMultiple(RLTextItemRecordList ssListsJson, int ssKeyCount, RLTextItemRecordList ssKeyProperties, RLTextItemRecordList ssKeyNames, RLTextItemRecordList ssItemsFieldNames, bool ssCaseSensitive, out string ssGroupedListJson) {
			ssGroupedListJson = "";
			// TODO: Write implementation for action
		} // MssList_ZipManyGroupByMultiple

	} // CssListUtils

} // OutSystems.NssListUtils

