LISTUTILS FOR OUTSYSTEMS 11
---------------------------

ListUtils is an OutSystems 11 Integration Studio extension with 33 server
actions for working with lists of records. Records are passed as JSON
arrays, so the same actions work with any Structure, Entity record, or
JSON payload.

Use it to remove, filter, partition, group, join, compare, slice, sample,
update, aggregate, chunk, and flatten lists in server-side logic.


INSTALLING THE EXTENSION
------------------------

1. Install ListUtils O11 from OutSystems Forge:
   https://www.outsystems.com/forge/component-overview/25935/listutils-o11
   It provides the ListUtils extension.

2. In Service Studio, open the module that will use ListUtils.

3. Open Manage Dependencies (Ctrl+Q), select ListUtils, tick the actions
   and structures you need, and click Apply.

4. Publish the module.

To install the extension package manually:

1. Open Service Center for the target O11 environment.

2. Go to Factory > Extensions and upload ListUtils.xif, or deploy it with
   LifeTime.

3. Publish the extension.

System.Text.Json and its eight .NET Framework support assemblies are
included in the extension package. No server configuration is required.


CONFIGURATION
-------------

ListUtils has no Site Properties, configuration screens, database tables,
or tenant settings. Every action is stateless.


HOW TO USE IT
-------------

Most actions receive and return JSON arrays as Text.

Typical server-action flow:

1. Get or build a Record List.
2. Use JSON Serialize to convert the list to Text.
3. Call the ListUtils action.
4. Use JSON Deserialize to convert the returned Text back to your
   Record List type.
5. Continue with your logic.

Example:

  JSONSerialize(Customers) -> List_GroupBy(SourceListJson, "Country")
  -> JSONDeserialize(GroupedListJson, CustomerGroup List)

Typed list parameters use two structures exposed by the extension:

  Condition
    Path            Text     Property path to test
    Operator        Text     Comparison operator
    Value           Text     Value to compare
    CaseSensitive   Boolean  Case-sensitive text comparison

  TextItem
    Value           Text     One text value

Use a Condition Record List for multi-condition actions. Use a TextItem
Record List for lists of property paths, key names, item field names, or
JSON-array strings.

Example Condition list:

  Path = "Dept", Operator = "Equals", Value = "IT", CaseSensitive = False
  Path = "Age",  Operator = "GreaterThan", Value = "30", CaseSensitive = False

Example TextItem list for List_ZipMany:

  Value = "[{\"Id\":1},{\"Id\":2}]"
  Value = "[{\"Id\":10},{\"Id\":20}]"


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
  Removes every element that matches one condition.

List_PopByConditions
  Inputs:  SourceListJson (Text), Conditions (Condition Record List),
           LogicalOperator (Text), SearchFromEnd (Boolean)
  Outputs: UpdatedListJson (Text), PoppedElementJson (Text)
  Removes the first, or last, element matching multiple conditions.
  LogicalOperator is "AND" (default) or "OR". An empty Conditions list
  returns the source unchanged.

List_PopMultipleByConditions
  Inputs:  SourceListJson (Text), Conditions (Condition Record List),
           LogicalOperator (Text)
  Outputs: UpdatedListJson (Text), PoppedElementsJson (Text)
  Removes every element matching multiple conditions.

List_Partition
  Inputs:  SourceListJson (Text), PropertyName (Text), TargetValue (Text),
           ComparisonOperator (Text), CaseSensitive (Boolean)
  Outputs: MatchingListJson (Text), NonMatchingListJson (Text)
  Splits a list into matching and non-matching elements without removing
  anything from the source.

List_PartitionByConditions
  Inputs:  SourceListJson (Text), Conditions (Condition Record List),
           LogicalOperator (Text)
  Outputs: MatchingListJson (Text), NonMatchingListJson (Text)
  Splits a list using multiple AND/OR conditions.

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
  Inputs:  SourceListJson (Text), PropertyPaths (TextItem Record List),
           KeyNames (TextItem Record List), ItemsFieldName (Text),
           CaseSensitive (Boolean)
  Outputs: GroupedListJson (Text)
  Groups by a composite key built from several property paths. Missing
  key labels default to Key0, Key1, and so on. Empty ItemsFieldName
  defaults to Items. Missing key values use "Unknown".

List_ZipGroupBy
  Inputs:  ListAJson (Text), ListBJson (Text), KeyPropertyA (Text),
           KeyPropertyB (Text), KeyNameA (Text), KeyNameB (Text),
           CaseSensitive (Boolean)
  Outputs: GroupedListJson (Text)
  Cogroups two lists by a shared key. Each group contains Key, the
  matching ListA items, and the matching ListB items.

List_ZipGroupByMultiple
  Inputs:  ListAJson (Text), ListBJson (Text),
           KeyPropertiesA (TextItem Record List),
           KeyPropertiesB (TextItem Record List),
           KeyNames (TextItem Record List), KeyNameA (Text),
           KeyNameB (Text), CaseSensitive (Boolean)
  Outputs: GroupedListJson (Text)
  Cogroups two lists by composite keys. KeyPropertiesA and
  KeyPropertiesB must have the same length. Empty KeyNameA and KeyNameB
  default to ItemsA and ItemsB.

List_ZipMany
  Inputs:  ListsJson (TextItem Record List),
           KeyNamesJson (TextItem Record List)
  Outputs: ZippedListJson (Text)
  Pairs any number of lists by position. Each TextItem.Value in ListsJson
  is a JSON array. Missing key names default to Items0, Items1, and so on.
  Output length is the shortest input length.

List_ZipManyGroupBy
  Inputs:  ListsJson (TextItem Record List),
           KeyPropertiesJson (TextItem Record List),
           KeyNamesJson (TextItem Record List), CaseSensitive (Boolean)
  Outputs: GroupedListJson (Text)
  Cogroups any number of lists by one key path per list.

List_ZipManyGroupByMultiple
  Inputs:  ListsJson (TextItem Record List), KeyCount (Integer),
           KeyProperties (TextItem Record List),
           KeyNames (TextItem Record List),
           ItemsFieldNames (TextItem Record List),
           CaseSensitive (Boolean)
  Outputs: GroupedListJson (Text)
  Cogroups M lists by N composite keys. KeyProperties is a flat list in
  list-major order. For two lists and one key:

    KeyProperties = "CustomerId", "CustomerId"

  For two lists and two keys:

    KeyProperties = "Country", "City", "Country", "City"

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
  Returns ListA elements whose MatchKey has at least one match in ListB.

List_Union
  Inputs:  ListAJson (Text), ListBJson (Text), MatchKey (Text),
           CaseSensitive (Boolean)
  Outputs: UnionListJson (Text)
  Concatenates ListA and ListB, then removes duplicates by MatchKey.
  The first occurrence wins. Empty MatchKey compares whole elements.

Transformation actions

List_Chunk
  Inputs:  SourceListJson (Text), ChunkSize (Integer)
  Outputs: ChunksListJson (TextItem Record List)
  Splits a list into fixed-size chunks. Each TextItem.Value is a JSON
  array. ChunkSize <= 0 or an empty source returns an empty list.

List_Flatten
  Inputs:  ChunksListJson (TextItem Record List)
  Outputs: FlatListJson (Text)
  Concatenates JSON arrays into one array. This is the inverse of
  List_Chunk. Non-array entries are skipped.

List_DistinctBy
  Inputs:  SourceListJson (Text), PropertyName (Text),
           CaseSensitive (Boolean)
  Outputs: DistinctListJson (Text)
  Keeps the first element for each unique property value. Empty
  PropertyName compares whole elements.

List_Slice
  Inputs:  SourceListJson (Text), Start (Integer), End (Integer),
           Step (Integer)
  Outputs: SliceListJson (Text)
  Uses Python-style slicing. Start is inclusive, End is exclusive, and
  negative values count from the end. End = 0 means unspecified. Step = 0
  is treated as 1; a negative Step walks backwards.

List_Reverse
  Inputs:  SourceListJson (Text)
  Outputs: ReversedListJson (Text)
  Returns the list in reverse order.

List_Shuffle
  Inputs:  SourceListJson (Text), Seed (Integer)
  Outputs: ShuffledListJson (Text)
  Randomizes the list with Fisher-Yates. Seed = 0 is non-deterministic.
  Any non-zero Seed produces repeatable output.

List_Sample
  Inputs:  SourceListJson (Text), SampleSize (Integer), Seed (Integer)
  Outputs: SampleListJson (Text)
  Returns a random subset without replacement. SampleSize <= 0 returns
  "[]". SampleSize >= list length returns the full list in shuffled order.
  Seed behaves like List_Shuffle.

Update actions

List_UpdateAt
  Inputs:  SourceListJson (Text), Index (Integer), PropertyName (Text),
           NewValueJson (Text)
  Outputs: UpdatedListJson (Text), PreviousValueJson (Text)
  Updates one property on one element. Negative Index counts from the end.
  Missing intermediate objects are created; arrays are not created (when
  an array step is missing, nothing is written, the item is left
  unchanged, and PreviousValueJson = "null"). NewValueJson is parsed as
  JSON. Invalid JSON is stored as text.

List_UpdateMultipleAt
  Inputs:  SourceListJson (Text), IndicesToUpdate (Text),
           PropertyName (Text), NewValueJson (Text)
  Outputs: UpdatedListJson (Text), UpdatedCount (Integer)
  Applies the same update at comma-separated indices. Negative indices
  count from the end. Out-of-range indices are skipped. Path rules match
  List_UpdateAt. UpdatedCount counts the targeted object items.

List_ReplaceWhere
  Inputs:  SourceListJson (Text), Conditions (Condition Record List),
           LogicalOperator (Text), UpdateProperty (Text),
           NewValueJson (Text)
  Outputs: UpdatedListJson (Text), MatchCount (Integer)
  Updates the same property on every element that matches the conditions.
  Path rules match List_UpdateAt. MatchCount counts the matched object
  elements.

Aggregation actions

List_MinBy
  Inputs:  SourceListJson (Text), PropertyName (Text),
           NumericMode (Boolean)
  Outputs: ElementJson (Text), MinValue (Text), MinIndex (Integer)
  Returns the element with the smallest property value. No qualifying
  element returns ElementJson = "null", MinValue = "", MinIndex = -1.

List_MaxBy
  Inputs:  SourceListJson (Text), PropertyName (Text),
           NumericMode (Boolean)
  Outputs: ElementJson (Text), MaxValue (Text), MaxIndex (Integer)
  Returns the element with the largest property value. No qualifying
  element returns ElementJson = "null", MaxValue = "", MaxIndex = -1.

List_Aggregate
  Inputs:  SourceListJson (Text), PropertyName (Text), Operation (Text)
  Outputs: ResultValue (Text), MatchedCount (Integer)
  Operation is Sum, Avg, Min, Max, Count, or CountDistinct. Empty or
  unrecognised Operation is treated as Sum. CountDistinct compares
  values case-sensitively. Numeric operations skip missing or
  non-numeric values. MatchedCount reports how many values contributed.


COMPARISON OPERATORS
--------------------

Use these Text values in ComparisonOperator and Condition.Operator:

  Equals            Exact match. Empty also means Equals.
  NotEquals         Not equal. Alias: !=
  Contains          Text contains the value.
  StartsWith        Text starts with the value.
  EndsWith          Text ends with the value.
  GreaterThan       Numeric greater than. Alias: >
  LessThan          Numeric less than. Alias: <
  GreaterOrEqual    Numeric greater than or equal. Alias: >=
  LessOrEqual       Numeric less than or equal. Alias: <=

CaseSensitive affects text comparisons only. Numeric operators parse
values as decimals with InvariantCulture.

Logical operators are AND and OR. Empty LogicalOperator means AND.


PROPERTY PATHS
--------------

PropertyName, MatchKey, UpdateProperty, Condition.Path, and key-path
inputs support dot navigation and array indexing:

  Address.City
  Items[0].Name
  Tags[-1]
  Groups[0].Members[-1].Email

Negative array indices count from the end. If a path segment is missing,
the element does not match. Each name segment also tries a camelCase
fallback, so "Status" can match "status".


SEARCH DIRECTION
----------------

List_PopByCondition and List_PopByConditions accept SearchFromEnd:

  False   Search from the first element and remove the first match.
  True    Search from the last element and remove the last match.

The PopMultiple actions remove every match, so they do not use this flag.


ERROR HANDLING AND LIMITS
-------------------------

- Inputs that should contain JSON arrays must contain valid JSON. Invalid
  JSON raises a server exception; catch it in your server action when the
  input comes from users or integrations.
- Null or empty list inputs are handled as empty lists where the action
  supports that behavior.
- Actions do not mutate your OutSystems Record Lists. They return new JSON
  strings.
- Large lists increase memory and serialization cost because each call
  parses and writes JSON. Process very large datasets in batches.
- The extension does not access databases, files, network resources, or
  external services.


VERIFIED RUNTIME
----------------

All 33 actions were verified in a published O11 Reactive test app on
2026-10-29. The regression dashboard used deterministic inputs with 5,000
elements in every data list and passed 33 of 33 cases.

In that test environment, warm ListUtils calls measured 0-63 ms per action
with a SQL Server clock that has about 15-16 ms resolution. Most browser
round-trip time came from network and OutSystems request overhead.
Performance depends on payload shape, server load, and environment size.
