ADDING THE LIBRARY TO YOUR APP
-------------------------------

1. Open your ODC app or library in ODC Studio.

2. Click the Dependencies icon (puzzle piece) in the toolbar.

3. Search for ListUtilsServerSide and select it.

4. Tick the actions you need (List_Pop, List_PopByCondition, etc.)
   then click Apply.


USAGE
-----

All nine actions consume and return JSON strings. Use JSON Serialize to
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
  Input:  sourceListJson (Text), conditionsJson (Text),
          logicalOperator (Text), searchFromEnd (Boolean)
  Output: updatedListJson (Text), poppedElementJson (Text)
  Finds the first (or last, if searchFromEnd = True) object matching
  multiple conditions combined with AND (default) or OR. conditionsJson
  is a JSON array of objects with path, operator, value, and optional
  caseSensitive fields.

List_PopMultipleByConditions
  Input:  sourceListJson (Text), conditionsJson (Text),
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

propertyName, matchKey, and paths inside condition JSON support both
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

List_PopByConditions and List_PopMultipleByConditions accept a JSON
array of conditions in conditionsJson:

  [
    {"path": "Status", "operator": "Equals", "value": "Active"},
    {"path": "Score", "operator": "GreaterThan", "value": "50"},
    {"path": "Code", "operator": "Equals", "value": "URGENT",
     "caseSensitive": true}
  ]

Each condition entry supports:
  path            property path (nested + array indexing)
  operator        any operator from the operators list
  value           target value as text
  caseSensitive   optional Boolean, default false

Combined with logicalOperator = "AND" (all must match) or "OR" (at
least one must match). Empty conditions array returns the original
list unchanged.


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
