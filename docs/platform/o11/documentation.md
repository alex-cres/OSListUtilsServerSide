INSTALLING THE EXTENSION
------------------------

1. Download the ListUtilsServerSide extension from the OutSystems Forge.

2. Open Service Studio, open your app or module, and go to the
   Manage Dependencies dialog (Ctrl+Q).

3. Locate the ListUtilsServerSide extension in the list, tick the
   actions you need, then click Apply.

4. Publish the module. No server-side configuration is required.

If you are deploying to your own O11 environment for the first time:

1. Sign in to Service Center on the target environment.

2. Go to Factory > Extensions and upload the XIF (or install via
   LifeTime).

3. Publish the extension. System.Text.Json and its transitive
   dependencies ship inside the XIF.


CONFIGURATION
-------------

The extension has no Site Properties, no configuration screen, and
no per-tenant settings. Every call is stateless.


USAGE
-----

All actions consume and return JSON strings. Use JSONSerialize to
convert any Record List (including a Text List) to a JSON string before
calling the action, then JSONDeserialize the output back to your target
Record List.

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
2. Call JSONSerialize on the source list.
3. Call the appropriate ListUtils action.
4. JSONDeserialize the output string back to your Record List.
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
  Input:  sourceListJson (Text), conditions (Record List of Condition),
          logicalOperator (Text), searchFromEnd (Boolean)
  Output: updatedListJson (Text), poppedElementJson (Text)
  Finds the first (or last, if searchFromEnd = True) object matching
  multiple conditions combined with AND (default) or OR. conditions is
  a Record List of Condition Structures (Path, Operator, Value,
  CaseSensitive) built directly in your Server Action - no JSON
  hand-authoring. Prefer the Operators constants (Operators.Equals,
  etc.) and LogicalOperators (LogicalOperators.AND / LogicalOperators.OR).
  Empty conditions list returns the source unchanged.

List_PopMultipleByConditions
  Input:  sourceListJson (Text), conditions (Record List of Condition),
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

List_GroupByMultiple
  Input:  sourceListJson (Text), propertyPaths (Text List),
          keyNames (Text List), itemsFieldName (Text),
          caseSensitive (Boolean)
  Output: groupedListJson (Text)
  N-key generalisation of List_GroupBy. Groups a JSON list by an
  ordered list of property paths (composite key). Each output group
  emits one field per key column, labelled by keyNames[i] (defaulting
  to "Key0", "Key1", ...), plus an items array named itemsFieldName
  (defaulting to "Items"). Composite keys are joined internally with
  the ASCII Unit Separator (U+001F) so distinct tuples never collide.
  Missing key values fall back to the string "Unknown" for that key
  column. Groups appear in first-seen order.

List_ZipGroupByMultiple
  Input:  listAJson (Text), listBJson (Text),
          keyPropertiesA (Text List), keyPropertiesB (Text List),
          keyNames (Text List), keyNameA (Text), keyNameB (Text),
          caseSensitive (Boolean)
  Output: groupedListJson (Text)
  N-key generalisation of the two-list cogroup. keyPropertiesA and
  keyPropertiesB must have the same length N. Each output group holds
  one field per key column (labelled by keyNames[i], defaulting to
  "Key0", "Key1", ...) plus two named arrays (keyNameA / keyNameB,
  defaulting to "ItemsA" / "ItemsB"). Ordering follows A's first-seen
  composite keys, then any B-only composite keys. Items with missing
  key values fall into a single "Unknown" bucket per key column.

List_ZipManyGroupByMultiple
  Input:  listsJson (Text List) [M entries],
          keyCount (Integer) [N],
          keyProperties (Text List) [M*N in list-major order],
          keyNames (Text List) [N],
          itemsFieldNames (Text List) [M],
          caseSensitive (Boolean)
  Output: groupedListJson (Text)
  M-list, N-key cogroup. keyProperties is a FLAT list in list-major
  order: entries [i*keyCount .. i*keyCount + keyCount - 1] are the N
  key paths applied to listsJson[i]. Each output group emits one
  field per key column (labelled by keyNames[i], defaulting to
  "Key0", "Key1", ...) plus M named arrays (itemsFieldNames[i],
  defaulting to "Items0", "Items1", ...). Groups appear in first-seen
  order across all inputs. Missing key values collapse into the
  single "Unknown" bucket per key column.

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


PROPERTY PATHS
--------------

propertyName, matchKey, and every Condition.Path support both
dot navigation and array indexing:

  Address.City             obj["Address"]["City"]
  Items[0].Name            obj["Items"][0]["Name"]
  Tags[-1]                 last element of Tags
  Groups[0].Members[-1]    mix of dots and indexing at multiple depths

CamelCase fallback is applied at each name segment. Negative indices
count from the end. If any segment is missing, the array index is out
of range, or the value at any level is not an object or array where
expected, the item is treated as non-matching.


MULTIPLE CONDITIONS
-------------------

List_PopByConditions, List_PopMultipleByConditions,
List_PartitionByConditions, and List_ReplaceWhere accept a Record
List of Condition Structures. Each Condition entry has four fields:

  Path            property path (nested + array indexing)
  Operator        any operator name from the operators list; prefer the
                  Operators constants for compile-time safety
  Value           target value as text
  CaseSensitive   Boolean, default False

Combined with logicalOperator = "AND" (all must match) or "OR" (at
least one must match). Prefer the LogicalOperators constants
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


SUPPORTED INPUT
---------------

All actions: any valid JSON array string. Elements can be any
structure. The actions are schema-agnostic — they examine only the
property specified by propertyName or matchKey (where applicable).


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

If a JSON string is malformed (not valid JSON), the action will throw
a platform error. Validate JSON before calling the action or wrap the
call in an exception handler.
