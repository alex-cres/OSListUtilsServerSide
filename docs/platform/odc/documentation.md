ADDING THE LIBRARY TO YOUR APP
-------------------------------

1. Open your ODC app or library in ODC Studio.

2. Click the Dependencies icon (puzzle piece) in the toolbar.

3. Search for ListUtilsServerSide and select it.

4. Tick the actions you need (List_Pop, List_PopByCondition, etc.)
   then click Apply.


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
          comparisonOperator (Text)
  Output: updatedListJson (Text), poppedElementJson (Text)
  Finds the first object where propertyName matches targetValue using
  the given comparisonOperator. Removes and returns it. If no match is
  found, updatedListJson is the original and poppedElementJson is "{}".
  propertyName supports dot-separated nested paths (e.g. "Address.City").
  CamelCase fallback is applied at each path segment.

List_PopMultipleByCondition
  Input:  sourceListJson (Text), propertyName (Text), targetValue (Text),
          comparisonOperator (Text)
  Output: updatedListJson (Text), poppedElementsJson (Text)
  Same as PopByCondition but removes ALL matching elements.
  poppedElementsJson is a JSON array of all removed objects.

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
  propertyName supports dot-separated nested paths.

List_Difference
  Input:  listAJson (Text), listBJson (Text), matchKey (Text),
          comparisonOperator (Text)
  Output: differenceListJson (Text)
  Returns elements from list A whose matchKey value does not match
  any matchKey value in list B using the given comparisonOperator.
  matchKey supports dot-separated nested paths.


COMPARISON OPERATORS
--------------------

The condition-based actions accept an operator string:

  Equals or ""              Case-insensitive exact match (default)
  NotEquals or !=           Inverse of Equals
  Contains                  Case-insensitive substring match
  StartsWith                Case-insensitive prefix match
  EndsWith                  Case-insensitive suffix match
  GreaterThan or >          Numeric > comparison
  LessThan or <             Numeric < comparison
  GreaterOrEqual or >=      Numeric >= comparison
  LessOrEqual or <=         Numeric <= comparison

Numeric operators parse both values with InvariantCulture. Non-numeric
values evaluate as no-match.


NESTED PROPERTY PATHS
---------------------

propertyName, matchKey, and the GroupBy property support dot-separated
paths to reach into nested JSON objects:

  Address.City        obj["Address"]["City"]
  Meta.Status         obj["Meta"]["Status"]
  Wrapper.Data.Value  obj["Wrapper"]["Data"]["Value"]

CamelCase fallback is applied at each segment - so "Address.City"
also matches "address.city" in the JSON. If any segment is missing
or the value at any level is not an object, the item is treated as
non-matching.


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
