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

Index-based actions (List_Pop, List_PopMultiple) operate directly on
List of Text inputs. Use them when your data is already a text list.

JSON-based actions (List_PopByCondition, List_PopMultipleByCondition,
List_Zip, List_GroupBy, List_Difference) operate on serialized JSON
strings. Use JSON Serialize to convert any Structure List to a JSON
string before calling these actions, then JSON Deserialize the output
back to your target Structure List.

Recommended pattern in a Server Action:

1. Receive or fetch the source list.
2. For JSON actions: call JSON Serialize on the source list.
3. Call the appropriate ListUtils action.
4. Use the output list (or JSON Deserialize the output string).
5. Continue with your business logic using the modified list.


SERVER ACTIONS
--------------

List_Pop
  Input:  sourceList (List of Text), index (Integer)
  Output: updatedList (List of Text), poppedElement (Text)
  Removes the element at the given 0-based index. Returns the
  removed element and the list without it. Out-of-range indices
  return the original list unchanged with an empty poppedElement.

List_PopMultiple
  Input:  sourceList (List of Text), indicesToPop (List of Integer)
  Output: updatedList (List of Text), poppedElements (List of Text)
  Removes multiple elements by index. Indices are processed in
  reverse-sorted order so removals do not shift remaining positions.
  Out-of-range indices are silently ignored.

List_PopByCondition
  Input:  sourceListJson (Text), propertyName (Text), targetValue (Text),
          comparisonOperator (Text), caseSensitive (Boolean)
  Output: updatedListJson (Text), poppedElementJson (Text)
  Finds the first object where propertyName matches targetValue using
  the given comparisonOperator. Removes and returns it. If no match is
  found, updatedListJson is the original and poppedElementJson is "{}".
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
          logicalOperator (Text)
  Output: updatedListJson (Text), poppedElementJson (Text)
  Finds the first object matching multiple conditions combined with
  AND (default) or OR. conditionsJson is a JSON array of objects with
  path, operator, value, and optional caseSensitive fields.

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


PROPERTY PATHS
--------------

propertyName, matchKey, and paths inside condition JSON support both
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


SUPPORTED INPUT
---------------

Index-based actions: any List of Text.

JSON-based actions: any valid JSON array string. Elements can be any
structure. The actions are schema-agnostic - they examine only the
property specified by propertyName or matchKey.


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
