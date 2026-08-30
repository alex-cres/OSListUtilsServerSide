ADDING THE LIBRARY TO YOUR APP
-------------------------------

1. Open your ODC app or library in ODC Studio.

2. Click the Dependencies icon (puzzle piece) in the toolbar.

3. Search for ListUtilsServerSide and select it.

4. Tick the actions you need (List_Pop, List_PopByCondition, etc.)
   then click Apply.


USAGE
-----

All fourteen actions consume and return JSON strings. Use JSON Serialize to
convert any Structure List (including a plain List of Text) to a JSON
string before calling the action, then JSON Deserialize the output back
to your target Structure List.

Index-based actions (List_Pop, List_PopMultiple) take a JSON array
string plus positional information (an index, or a comma-separated
list of indices).

Condition-based actions (List_PopByCondition, List_PopMultipleByCondition,
List_PopByConditions, List_PopMultipleByConditions), relational actions
(List_Zip), grouping (List_GroupBy) and set difference (List_Difference)
all follow the same JSON-in / JSON-out contract.

Transformation actions (List_Chunk, List_DistinctBy, List_Slice,
List_Shuffle, List_UpdateAt) also consume and return JSON strings.
List_Chunk returns a nested JSON array of arrays. List_UpdateAt also
returns the previous value of the updated property as a separate JSON
string.

Recommended pattern in a Server Action:

1. Receive or fetch the source list.
2. Call JSON Serialize on the source list.
3. Call the appropriate ListUtils action.
4. JSON Deserialize the output string back to your Structure List.
5. Continue with your business logic using the modified list.


SERVER ACTIONS
--------------

List_Pop
  Input:  sourceListJson (Text), index (Integer)
  Output: updatedListJson (Text), poppedElementJson (Text)
  Removes the element at the given 0-based index from a JSON array.
  Returns the removed element (as JSON) and the list without it.
  Out-of-range indices return the original list unchanged and
  poppedElementJson = "null". Empty input returns updatedListJson = "[]"
  and poppedElementJson = "null".

List_PopMultiple
  Input:  sourceListJson (Text), indicesToPop (Text)
  Output: updatedListJson (Text), poppedElementsJson (Text)
  Removes multiple elements by index. indicesToPop is a comma-separated
  list of 0-based indices (e.g. "1,3,5"). Whitespace is trimmed and
  duplicates ignored. Indices are processed in reverse-sorted order so
  removals do not shift remaining positions. Out-of-range indices are
  silently ignored. poppedElementsJson is a JSON array of the removed
  elements in their original order.

List_PopByCondition
  Input:  sourceListJson (Text), propertyName (Text), targetValue (Text),
          comparisonOperator (Text), caseSensitive (Boolean),
          searchFromEnd (Boolean)
  Output: updatedListJson (Text), poppedElementJson (Text)
  Finds the first (or last, if searchFromEnd = True) object where
  propertyName matches targetValue using the given comparisonOperator.
  Removes and returns it. If no match is found, updatedListJson is the
  original and poppedElementJson is "{}".
  propertyName supports dot-separated nested paths and array indexing
  (e.g. "Address.City", "Items[0].Name", "Tags[-1]").

List_PopMultipleByCondition
  Input:  sourceListJson (Text), propertyName (Text), targetValue (Text),
          comparisonOperator (Text), caseSensitive (Boolean)
  Output: updatedListJson (Text), poppedElementsJson (Text)
  Same as PopByCondition but removes ALL matching elements.
  poppedElementsJson is a JSON array of all removed objects.

List_PopByConditions
  Input:  sourceListJson (Text), conditions (List<Condition>),
          logicalOperator (Text), searchFromEnd (Boolean)
  Output: updatedListJson (Text), poppedElementJson (Text)
  Finds the first (or last, if searchFromEnd = True) object matching
  multiple conditions combined with AND (default) or OR. conditions is
  a List of Condition Structures (Path, Operator, Value, CaseSensitive)
  built directly in your Server Action - no JSON hand-authoring.
  Prefer the ListUtils.Operators constants (Operators.Equals, etc.) and
  ListUtils.LogicalOperators (LogicalOperators.AND / LogicalOperators.OR).
  Empty conditions list returns the source unchanged.

List_PopMultipleByConditions
  Input:  sourceListJson (Text), conditions (List<Condition>),
          logicalOperator (Text)
  Output: updatedListJson (Text), poppedElementsJson (Text)
  Same as PopByConditions but removes ALL matching elements.

List_Zip
  Input:  listAJson (Text), listBJson (Text), keyNameA (Text), keyNameB (Text)
  Output: zippedListJson (Text)
  Pairs elements from two lists by index into objects with the given
  key names. Truncates to the shorter list length.

List_GroupBy
  Input:  sourceListJson (Text), propertyName (Text)
  Output: groupedListJson (Text)
  Groups elements by the value of propertyName. Output is a JSON
  array of objects with "Key" (the group value) and "Items" (array
  of elements in that group). Groups appear in first-seen order.
  propertyName supports nested paths and array indexing.

List_Difference
  Input:  listAJson (Text), listBJson (Text), matchKey (Text),
          comparisonOperator (Text), caseSensitive (Boolean)
  Output: differenceListJson (Text)
  Returns elements from list A whose matchKey value does not match
  any matchKey value in list B using the given comparisonOperator.
  matchKey supports nested paths and array indexing.

List_Chunk
  Input:  sourceListJson (Text), chunkSize (Integer)
  Output: chunksListJson (Text)
  Splits a JSON list into an array of sublists of a fixed size. The
  last chunk may be smaller than chunkSize. chunkSize <= 0 or an empty
  source returns "[]". Useful for batching API payloads.

List_DistinctBy
  Input:  sourceListJson (Text), propertyName (Text),
          caseSensitive (Boolean)
  Output: distinctListJson (Text)
  Filters a JSON list to unique elements by a property value (first
  occurrence wins). Empty propertyName dedupes on the entire item's
  JSON. Missing keys share a single null-key bucket. propertyName
  supports nested paths and array indexing.

List_Slice
  Input:  sourceListJson (Text), start (Integer), end (Integer),
          step (Integer)
  Output: sliceListJson (Text)
  Extracts a subset using Python/JavaScript-style Start, End, Step.
  Negative start/end count from the end. Sentinel: end == 0 means
  "unspecified" - for positive step it is "to end of list", for
  negative step it is "past the beginning". step == 0 is treated as 1;
  negative step reverses the walk.

List_Shuffle
  Input:  sourceListJson (Text), seed (Integer)
  Output: shuffledListJson (Text)
  Randomises the order of a JSON list using Fisher-Yates. seed == 0
  uses a cryptographically-seeded RNG (RandomNumberGenerator per swap)
  and produces a different permutation each call. Any non-zero seed
  uses System.Random(seed) and produces the same permutation every
  call (useful for reproducible tests). Source list is not mutated.

List_UpdateAt
  Input:  sourceListJson (Text), index (Integer), propertyName (Text),
          newValueJson (Text)
  Output: updatedListJson (Text), previousValueJson (Text)
  Sets a single property of the item at index. Negative index counts
  from the end. propertyName supports nested paths and array indexing.
  Missing intermediate objects are auto-created; arrays are NOT auto-
  created (a missing or non-array indexing step returns the source
  unchanged). newValueJson is parsed as JSON; if it is not valid JSON
  it is stored as a raw string. previousValueJson is "null" when: the
  index was out of range, propertyName was empty, the item was not an
  object, the property did not exist, OR the property existed with a
  JSON null value (the last two cases are indistinguishable).


COMPARISON OPERATORS
--------------------

The condition-based actions accept an operator string:

  Equals or ""              Exact match (per caseSensitive flag)
  NotEquals or !=           Inverse of Equals
  Contains                  Substring match
  StartsWith                Prefix match
  EndsWith                  Suffix match
  GreaterThan or >          Numeric > comparison
  LessThan or <             Numeric < comparison
  GreaterOrEqual or >=      Numeric >= comparison
  LessOrEqual or <=         Numeric <= comparison

The caseSensitive Boolean (default False) toggles case sensitivity for
string operators. Numeric operators ignore it.

Numeric operators parse both values with InvariantCulture. Non-numeric
values evaluate as no-match.


PROPERTY PATHS
--------------

propertyName, matchKey, and every Condition.Path support both
dot navigation and array indexing:

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
  Operator        any operator name from the operators list; prefer the
                  ListUtils.Operators constants for compile-time safety
  Value           target value as text
  CaseSensitive   Boolean, default false

Combined with logicalOperator = "AND" (all must match) or "OR" (at
least one must match). Prefer the ListUtils.LogicalOperators constants
(LogicalOperators.AND / LogicalOperators.OR). Empty conditions list
returns the source unchanged.

All operator names are also exposed as constants on three helper
classes so consumers get IDE autocomplete and compile-time typo
detection instead of magic strings:

  Operators           Equals, NotEquals, Contains, StartsWith,
                      EndsWith, GreaterThan, LessThan,
                      GreaterOrEqual, LessOrEqual
  LogicalOperators    AND, OR
  AggregateOperations Sum, Avg, Min, Max, Count, CountDistinct
                      (used by List_Aggregate)

Symbol aliases (!=, >, <, >=, <=) are still accepted for backwards
compatibility.


SEARCH DIRECTION
----------------

List_PopByCondition and List_PopByConditions accept a searchFromEnd
Boolean:

  False (default)   Iterates from index 0 upward. Pops the FIRST match.
  True              Iterates from the last index downward. Pops the
                    LAST match.

List_PopMultipleByCondition and List_PopMultipleByConditions always
pop every match, so they do not need this flag.


NOTES
-----

No configuration is required. All actions are stateless.

Property name matching uses a camelCase fallback: if "Status" is not
found, "status" is also checked. This handles the case where
OutSystems JSON Serialize produces camelCase keys.

Empty or null inputs are handled gracefully: null lists return empty
lists, null JSON strings return "[]" or "{}" as appropriate.

All JSON-based actions accept any valid JSON array as input. The
elements can be any structure - the actions do not need to know the
structure schema.
