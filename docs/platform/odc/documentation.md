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
  Input:  sourceListJson (Text), propertyName (Text), targetValue (Text)
  Output: updatedListJson (Text), poppedElementJson (Text)
  Finds the first object where propertyName equals targetValue
  (case-insensitive). Removes and returns it. If no match is found,
  updatedListJson is the original and poppedElementJson is "{}".
  Supports camelCase fallback (e.g. "IsActive" also checks "isActive").

List_PopMultipleByCondition
  Input:  sourceListJson (Text), propertyName (Text), targetValue (Text)
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

List_Difference
  Input:  listAJson (Text), listBJson (Text), matchKey (Text)
  Output: differenceListJson (Text)
  Returns elements from list A whose matchKey value does not appear
  in list B. Matching is case-insensitive. Runs in O(N) time.


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
