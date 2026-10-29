ADDING THE LIBRARY TO YOUR APP
-------------------------------

1. Open your ODC app or library in ODC Studio.

2. Click the Dependencies icon (puzzle piece) in the toolbar.

3. Search for ListUtilsServerSide and select it.

4. Tick the actions you need (List_Pop, List_PopByCondition, etc.)
   and the Condition structure if you use multi-condition actions,
   then click Apply.


USAGE
-----

Most actions receive and return JSON arrays as Text. Use JSON Serialize
to convert any Structure List (including a plain List of Text) to Text
before calling the action, then JSON Deserialize the output back to your
target Structure List.

Some inputs and outputs are typed lists instead of JSON Text:

  Condition List   Used by List_PopByConditions,
                   List_PopMultipleByConditions,
                   List_PartitionByConditions, and List_ReplaceWhere.
  List of Text     Used for lists of property paths, key names, item
                   field names, and JSON-array strings (List_Chunk
                   output, List_Flatten input, the multi-key grouping
                   actions, and the ZipMany actions).

Recommended pattern in a Server Action:

1. Receive or fetch the source list.
2. Call JSON Serialize on the source list.
3. Call the appropriate ListUtils action.
4. JSON Deserialize the output Text back to your Structure List.
5. Continue with your business logic using the modified list.

Example:

  JSONSerialize(Customers) -> List_GroupBy(SourceListJson, "Country")
  -> JSONDeserialize(GroupedListJson, CustomerGroup List)


SERVER ACTIONS
--------------

Index actions

List_Pop
  Inputs:  SourceListJson (Text), Index (Integer)
  Outputs: UpdatedListJson (Text), PoppedElementJson (Text)
  Removes the element at a 0-based index and returns the removed element
  plus the updated list. Negative indices count from the end (-1 is the
  last element). Out-of-range indices return the original list and
  PoppedElementJson = "null". Empty input returns "[]" and "null".

List_PopMultiple
  Inputs:  SourceListJson (Text), IndicesToPop (Text)
  Outputs: UpdatedListJson (Text), PoppedElementsJson (Text)
  Removes elements at comma-separated 0-based indices, for example
  "1,3,-1". Negative values count from the end. Whitespace is trimmed;
  duplicates (including a negative and positive index for the same
  element) are removed once; non-numeric and out-of-range entries are
  ignored. Removed elements keep their original order.

List_SplitAt
  Inputs:  SourceListJson (Text), Index (Integer)
  Outputs: LeftListJson (Text), RightListJson (Text)
  Splits a list before Index. Negative indices count from the end.
  Index <= 0 leaves LeftListJson empty; Index >= list length leaves
  RightListJson empty.

Condition actions

List_PopByCondition
  Inputs:  SourceListJson (Text), PropertyName (Text), TargetValue (Text),
           ComparisonOperator (Text), CaseSensitive (Boolean),
           SearchFromEnd (Boolean)
  Outputs: UpdatedListJson (Text), PoppedElementJson (Text)
  Removes the first matching element, or the last match when
  SearchFromEnd = True. No match returns the original list and "{}".

List_PopMultipleByCondition
  Inputs:  SourceListJson (Text), PropertyName (Text), TargetValue (Text),
           ComparisonOperator (Text), CaseSensitive (Boolean)
  Outputs: UpdatedListJson (Text), PoppedElementsJson (Text)
  Removes every element that matches one condition. PoppedElementsJson
  is "[]" when nothing matches.

List_PopByConditions
  Inputs:  SourceListJson (Text), Conditions (Condition List),
           LogicalOperator (Text), SearchFromEnd (Boolean)
  Outputs: UpdatedListJson (Text), PoppedElementJson (Text)
  Removes the first, or last, element matching multiple conditions.
  LogicalOperator is "AND" (default) or "OR". An empty Conditions list
  returns the source unchanged.

List_PopMultipleByConditions
  Inputs:  SourceListJson (Text), Conditions (Condition List),
           LogicalOperator (Text)
  Outputs: UpdatedListJson (Text), PoppedElementsJson (Text)
  Removes every element matching multiple conditions.

List_Partition
  Inputs:  SourceListJson (Text), PropertyName (Text), TargetValue (Text),
           ComparisonOperator (Text), CaseSensitive (Boolean)
  Outputs: MatchingListJson (Text), NonMatchingListJson (Text)
  Splits a list into matching and non-matching elements. Empty
  PropertyName returns every element in NonMatchingListJson.

List_PartitionByConditions
  Inputs:  SourceListJson (Text), Conditions (Condition List),
           LogicalOperator (Text)
  Outputs: MatchingListJson (Text), NonMatchingListJson (Text)
  Splits a list using multiple AND/OR conditions. An empty Conditions
  list returns every element in NonMatchingListJson.

Grouping, join, and set actions

List_Zip
  Inputs:  ListAJson (Text), ListBJson (Text), KeyNameA (Text),
           KeyNameB (Text)
  Outputs: ZippedListJson (Text)
  Pairs two lists by position. Output length is the shorter list length.

List_GroupBy
  Inputs:  SourceListJson (Text), PropertyName (Text)
  Outputs: GroupedListJson (Text)
  Groups elements by one property. Output shape:
  [{"Key":"value","Items":[...]}]. Groups keep first-seen order. Key
  comparison is always case-sensitive. Elements without the property
  go into one "Unknown" group.

List_GroupByMultiple
  Inputs:  SourceListJson (Text), PropertyPaths (List of Text),
           KeyNames (List of Text), ItemsFieldName (Text),
           CaseSensitive (Boolean)
  Outputs: GroupedListJson (Text)
  Groups by a composite key built from several property paths. Each
  group has one field per key (labelled by KeyNames, defaulting to
  Key0, Key1, ...) plus an items array (ItemsFieldName, defaulting to
  Items). Missing key values use "Unknown". Composite keys are joined
  internally with the ASCII Unit Separator so distinct tuples never
  collide.

List_ZipGroupBy
  Inputs:  ListAJson (Text), ListBJson (Text), KeyPropertyA (Text),
           KeyPropertyB (Text), KeyNameA (Text), KeyNameB (Text),
           CaseSensitive (Boolean)
  Outputs: GroupedListJson (Text)
  Cogroups two lists by a shared key. Each group contains Key, the
  matching ListA items, and the matching ListB items. A's keys come
  first, then B-only keys. Empty KeyNameA and KeyNameB default to
  ItemsA and ItemsB.

List_ZipGroupByMultiple
  Inputs:  ListAJson (Text), ListBJson (Text),
           KeyPropertiesA (List of Text), KeyPropertiesB (List of Text),
           KeyNames (List of Text), KeyNameA (Text), KeyNameB (Text),
           CaseSensitive (Boolean)
  Outputs: GroupedListJson (Text)
  Cogroups two lists by composite keys. KeyPropertiesA and
  KeyPropertiesB must have the same length.

List_ZipMany
  Inputs:  ListsJson (List of Text), KeyNamesJson (List of Text)
  Outputs: ZippedListJson (Text)
  Pairs any number of lists by position. Each ListsJson entry is a JSON
  array. Missing key names default to Items0, Items1, and so on. Output
  length is the shortest input length.

List_ZipManyGroupBy
  Inputs:  ListsJson (List of Text), KeyPropertiesJson (List of Text),
           KeyNamesJson (List of Text), CaseSensitive (Boolean)
  Outputs: GroupedListJson (Text)
  Cogroups any number of lists by one key path per list. Output shape:
  [{"Key":..., <KeyNamesJson[0]>:[...], <KeyNamesJson[1]>:[...]}].

List_ZipManyGroupByMultiple
  Inputs:  ListsJson (List of Text), KeyCount (Integer),
           KeyProperties (List of Text), KeyNames (List of Text),
           ItemsFieldNames (List of Text), CaseSensitive (Boolean)
  Outputs: GroupedListJson (Text)
  Cogroups M lists by N composite keys. KeyProperties is a flat list in
  list-major order: entries [i*KeyCount .. i*KeyCount + KeyCount - 1]
  are the key paths for ListsJson[i]. For two lists and two keys:

    KeyProperties = "Country", "City", "Country", "City"

  Missing item field names default to Items0, Items1, and so on.

List_Difference
  Inputs:  ListAJson (Text), ListBJson (Text), MatchKey (Text),
           ComparisonOperator (Text), CaseSensitive (Boolean)
  Outputs: DifferenceListJson (Text)
  Returns ListA elements whose MatchKey has no match in ListB. ListA
  elements without a MatchKey value are always kept. Empty ListBJson
  returns ListA unchanged.

List_Intersect
  Inputs:  ListAJson (Text), ListBJson (Text), MatchKey (Text),
           ComparisonOperator (Text), CaseSensitive (Boolean)
  Outputs: IntersectionListJson (Text)
  Returns ListA elements whose MatchKey has at least one match in ListB,
  in ListA order.

List_Union
  Inputs:  ListAJson (Text), ListBJson (Text), MatchKey (Text),
           CaseSensitive (Boolean)
  Outputs: UnionListJson (Text)
  Concatenates ListA and ListB, then removes duplicates by MatchKey.
  The first occurrence wins. Empty MatchKey compares whole elements.

Transformation actions

List_Chunk
  Inputs:  SourceListJson (Text), ChunkSize (Integer)
  Outputs: ChunksListJson (List of Text)
  Splits a list into fixed-size chunks. Each entry is a standalone JSON
  array ready for JSON Deserialize. The last chunk may be smaller.
  ChunkSize <= 0 or an empty source returns an empty list.

List_Flatten
  Inputs:  ChunksListJson (List of Text)
  Outputs: FlatListJson (Text)
  Concatenates JSON arrays into one array. This is the inverse of
  List_Chunk. Empty, invalid, and non-array entries are skipped.

List_DistinctBy
  Inputs:  SourceListJson (Text), PropertyName (Text),
           CaseSensitive (Boolean)
  Outputs: DistinctListJson (Text)
  Keeps the first element for each unique property value. Empty
  PropertyName compares whole elements. Elements with a missing key
  share one bucket, so at most one keyless element survives.

List_Slice
  Inputs:  SourceListJson (Text), Start (Integer), End (Integer),
           Step (Integer)
  Outputs: SliceListJson (Text)
  Uses Python-style slicing. Start is inclusive, End is exclusive, and
  negative values count from the end. End = 0 means unspecified: to the
  end of the list for a positive Step, past the beginning for a negative
  Step. Step = 0 is treated as 1; a negative Step walks backwards.

List_Reverse
  Inputs:  SourceListJson (Text)
  Outputs: ReversedListJson (Text)
  Returns the list in reverse order.

List_Shuffle
  Inputs:  SourceListJson (Text), Seed (Integer)
  Outputs: ShuffledListJson (Text)
  Randomizes the list with Fisher-Yates. Seed = 0 uses a cryptographic
  RNG and gives a different order each call. Any non-zero Seed produces
  repeatable output.

List_Sample
  Inputs:  SourceListJson (Text), SampleSize (Integer), Seed (Integer)
  Outputs: SampleListJson (Text)
  Returns a random subset without replacement. SampleSize <= 0 returns
  "[]". SampleSize >= list length returns the full list in shuffled
  order. Seed behaves like List_Shuffle.

Update actions

List_UpdateAt
  Inputs:  SourceListJson (Text), Index (Integer), PropertyName (Text),
           NewValueJson (Text)
  Outputs: UpdatedListJson (Text), PreviousValueJson (Text)
  Updates one property on one element. Negative Index counts from the
  end. Missing intermediate objects are created; arrays are not created
  (when an array step is missing or not an array, nothing is written,
  the item is left unchanged, and PreviousValueJson = "null").
  NewValueJson is parsed as JSON; invalid JSON is stored as text.
  PreviousValueJson is "null" when the index is out of range,
  PropertyName is empty, the item is not an object, the property did not
  exist, or the property held JSON null (the last two cases are
  indistinguishable).

List_UpdateMultipleAt
  Inputs:  SourceListJson (Text), IndicesToUpdate (Text),
           PropertyName (Text), NewValueJson (Text)
  Outputs: UpdatedListJson (Text), UpdatedCount (Integer)
  Applies the same update at comma-separated indices. Negative indices
  count from the end. Duplicate, invalid, and out-of-range indices are
  skipped. Path rules match List_UpdateAt. UpdatedCount counts the
  targeted object items.

List_ReplaceWhere
  Inputs:  SourceListJson (Text), Conditions (Condition List),
           LogicalOperator (Text), UpdateProperty (Text),
           NewValueJson (Text)
  Outputs: UpdatedListJson (Text), MatchCount (Integer)
  Updates the same property on every element that matches the
  conditions. Path rules match List_UpdateAt. MatchCount counts the
  matched object elements. An empty Conditions list returns the source
  unchanged and MatchCount = 0.

Aggregation actions

List_MinBy
  Inputs:  SourceListJson (Text), PropertyName (Text),
           NumericMode (Boolean)
  Outputs: ElementJson (Text), MinValue (Text), MinIndex (Integer)
  Returns the element with the smallest property value. NumericMode =
  True compares decimals and skips non-numeric values; False compares
  case-insensitive text. Ties keep the first occurrence. No qualifying
  element returns ElementJson = "null", MinValue = "", MinIndex = -1.

List_MaxBy
  Inputs:  SourceListJson (Text), PropertyName (Text),
           NumericMode (Boolean)
  Outputs: ElementJson (Text), MaxValue (Text), MaxIndex (Integer)
  Returns the element with the largest property value. Same rules as
  List_MinBy.

List_Aggregate
  Inputs:  SourceListJson (Text), PropertyName (Text), Operation (Text)
  Outputs: ResultValue (Text), MatchedCount (Integer)
  Operation is Sum, Avg, Min, Max, Count, or CountDistinct. Empty or
  unrecognised Operation is treated as Sum. CountDistinct compares
  values case-sensitively. Numeric operations skip missing or
  non-numeric values. MatchedCount reports how many values contributed.
  ResultValue is empty when no value contributed to a numeric
  operation.


SUPPORTED INPUT
---------------

Any valid JSON array. Elements can be any structure - the actions do
not need to know the structure schema. JSON Serialize output from any
ODC Structure List, Entity record list, or List of Text is supported.


COMPARISON OPERATORS
--------------------

Use these Text values in ComparisonOperator and Condition.Operator:

  Equals or ""              Exact match (per CaseSensitive flag)
  NotEquals or !=           Inverse of Equals
  Contains                  Substring match
  StartsWith                Prefix match
  EndsWith                  Suffix match
  GreaterThan or >          Numeric > comparison
  LessThan or <             Numeric < comparison
  GreaterOrEqual or >=      Numeric >= comparison
  LessOrEqual or <=         Numeric <= comparison

CaseSensitive (default False) affects text comparisons only. Numeric
operators parse both values as decimals with InvariantCulture;
non-numeric values evaluate as no-match.


PROPERTY PATHS
--------------

PropertyName, MatchKey, UpdateProperty, Condition.Path, and every
key-path list entry support dot navigation and array indexing:

  Address.City             obj["Address"]["City"]
  Items[0].Name            obj["Items"][0]["Name"]
  Tags[-1]                 last element of Tags
  Groups[0].Members[-1]    mix of dots and indexing at multiple depths

CamelCase fallback is applied at each name segment - so "Address.City"
also matches "address.city" in the JSON. Negative indices count from
the end.

If any segment is missing, the array index is out of range, or the
value at any level is not an object or array where expected, the item
is treated as non-matching.


MULTIPLE CONDITIONS
-------------------

List_PopByConditions, List_PopMultipleByConditions,
List_PartitionByConditions, and List_ReplaceWhere accept a List of
Condition Structures. Each Condition entry has four fields:

  Path            property path (nested + array indexing)
  Operator        any operator name from the operators list
  Value           target value as text
  CaseSensitive   Boolean, default False

Combine them with LogicalOperator = "AND" (all must match, default) or
"OR" (at least one must match).

Operator names are also exposed as constants so you get autocomplete
instead of typing strings:

  Operators           Equals, NotEquals, Contains, StartsWith,
                      EndsWith, GreaterThan, LessThan,
                      GreaterOrEqual, LessOrEqual
  LogicalOperators    AND, OR
  AggregateOperations Sum, Avg, Min, Max, Count, CountDistinct
                      (used by List_Aggregate)


SEARCH DIRECTION
----------------

List_PopByCondition and List_PopByConditions accept SearchFromEnd:

  False (default)   Search from the first element; remove the first
                    match.
  True              Search from the last element; remove the last
                    match.

The PopMultiple actions remove every match, so they do not use this
flag.


NOTES
-----

No configuration is required. All actions are stateless and do not
access databases, files, network resources, or external services.

Null or empty list inputs are handled gracefully and return empty
results ("[]", "{}", "null", or an empty list as documented per action).

Invalid JSON in an input that should contain a JSON array raises a
server exception. Catch it in your Server Action when the input comes
from users or integrations.

Actions do not mutate your ODC lists. They return new JSON Text.

Large lists increase memory and serialization cost because each call
parses and writes JSON. Process very large datasets in batches.
