# OSListUtilsServerSide

**ODC External Library + O11 Integration Studio Extension**

Advanced list manipulation utilities — index-based pops, condition-based pops, zip, group-by, and set difference. Uses JSON serialization for generic structure support.

---

## Objective

OutSystems lists lack common collection operations found in general-purpose languages (pop by index, pop by condition, zip, group-by, set difference). Implementing these natively requires verbose nested `For Each` loops with manual index tracking. This component provides seven server-side actions that cover the most common gaps in a single call each.

The JSON-based actions work with **any OutSystems Structure** — the caller serializes the list with `JSON Serialize`, passes it to the action, and deserializes the result. This generic approach eliminates the need for per-structure custom extensions.

---

## Server Actions

### List_Pop

Removes an element at a specific index. Returns the removed element and the updated list.

| Parameter | Type | Direction | Description |
|-----------|------|-----------|-------------|
| `sourceList` | `List<string>` | Input | The source list to manipulate. |
| `index` | `int` | Input | The 0-based index of the element to remove. |
| `updatedList` | `List<string>` | Output | The list without the popped element. |
| `poppedElement` | `string` | Output | The element that was removed (empty string if index is out of range). |

### List_PopMultiple

Removes multiple elements at specified indices. Returns the removed elements and the updated list.

| Parameter | Type | Direction | Description |
|-----------|------|-----------|-------------|
| `sourceList` | `List<string>` | Input | The source list to manipulate. |
| `IndicesToPop` | `List<int>` | Input | The list of 0-based indices to remove. |
| `updatedList` | `List<string>` | Output | The list without the popped elements. |
| `poppedElements` | `List<string>` | Output | The elements that were removed, in their original order. |

### List_PopByCondition

Pops the first element matching a property condition from a JSON list.

| Parameter | Type | Direction | Description |
|-----------|------|-----------|-------------|
| `SourceListJson` | `string` | Input | The source list serialized as a JSON string (via `JSON Serialize`). |
| `PropertyName` | `string` | Input | Property path to check. Supports nested paths (`Address.City`) and array indexing (`Items[0].Name`, `Tags[-1]`). |
| `TargetValue` | `string` | Input | The value to match. |
| `ComparisonOperator` | `string` | Input | `Equals` (default), `NotEquals`, `Contains`, `StartsWith`, `EndsWith`, `GreaterThan`, `LessThan`, `GreaterOrEqual`, `LessOrEqual`. Empty = `Equals`. |
| `CaseSensitive` | `bool` | Input | When `true`, string comparison is case-sensitive. When `false` (default), case-insensitive. Ignored by numeric operators. |
| `SearchFromEnd` | `bool` | Input | When `true`, searches from the end backwards (pops the LAST match). When `false` (default), searches from the beginning (pops the FIRST match). |
| `UpdatedListJson` | `string` | Output | The JSON list without the matched element. |
| `PoppedElementJson` | `string` | Output | The matched JSON object, or `{}` if no match found. |

### List_PopMultipleByCondition

Pops all elements matching a property condition from a JSON list.

| Parameter | Type | Direction | Description |
|-----------|------|-----------|-------------|
| `SourceListJson` | `string` | Input | The source list serialized as a JSON string. |
| `PropertyName` | `string` | Input | Property path to check. Supports nested paths and array indexing. |
| `TargetValue` | `string` | Input | The value to match. |
| `ComparisonOperator` | `string` | Input | Same operators as `List_PopByCondition`. |
| `CaseSensitive` | `bool` | Input | Case-sensitivity flag (default `false`). |
| `UpdatedListJson` | `string` | Output | The JSON list without matched elements. |
| `PoppedElementsJson` | `string` | Output | JSON array of all matched elements, or `[]` if none. |

### List_PopByConditions

Pops the first element matching multiple conditions combined with AND/OR.

| Parameter | Type | Direction | Description |
|-----------|------|-----------|-------------|
| `SourceListJson` | `string` | Input | The source list serialized as a JSON string. |
| `ConditionsJson` | `string` | Input | JSON array of conditions. Each condition has `path`, `operator`, `value`, optional `CaseSensitive`. Example below. |
| `LogicalOperator` | `string` | Input | `AND` (default) — all conditions must match; `OR` — at least one must match. |
| `SearchFromEnd` | `bool` | Input | When `true`, searches from the end backwards (pops the LAST match). When `false` (default), searches from the beginning. |
| `UpdatedListJson` | `string` | Output | The JSON list without the matched element. |
| `PoppedElementJson` | `string` | Output | The matched JSON object, or `{}` if no match found. |

### List_PopMultipleByConditions

Pops all elements matching multiple conditions combined with AND/OR.

| Parameter | Type | Direction | Description |
|-----------|------|-----------|-------------|
| `SourceListJson` | `string` | Input | The source list serialized as a JSON string. |
| `ConditionsJson` | `string` | Input | JSON array of conditions (same format as `List_PopByConditions`). |
| `LogicalOperator` | `string` | Input | `AND` or `OR`. |
| `UpdatedListJson` | `string` | Output | The JSON list without matched elements. |
| `PoppedElementsJson` | `string` | Output | JSON array of all matched elements. |

**Conditions JSON format:**

```json
[
  {"path": "Status", "operator": "Equals", "value": "Active", "caseSensitive": false},
  {"path": "Score", "operator": "GreaterThan", "value": "50"},
  {"path": "Meta.Region", "operator": "Equals", "value": "EU"}
]
```

Each condition supports:
- `path` — property path (nested + array indexing supported)
- `operator` — any operator from the operators table
- `value` — target value as text
- `CaseSensitive` (optional, default `false`) — per-condition case sensitivity

### List_Zip

Combines two JSON lists into paired objects by matching index.

| Parameter | Type | Direction | Description |
|-----------|------|-----------|-------------|
| `ListAJson` | `string` | Input | The first JSON list. |
| `ListBJson` | `string` | Input | The second JSON list. |
| `KeyNameA` | `string` | Input | Key label for List A entries in the output objects. |
| `KeyNameB` | `string` | Input | Key label for List B entries in the output objects. |
| `ZippedListJson` | `string` | Output | JSON array of paired objects `[{keyNameA: ..., keyNameB: ...}, ...]`. Truncates to the shorter list. |

### List_GroupBy

Groups a flat JSON list by a property value.

| Parameter | Type | Direction | Description |
|-----------|------|-----------|-------------|
| `SourceListJson` | `string` | Input | The source JSON list. |
| `PropertyName` | `string` | Input | Property path to group by. Supports nested paths with dots (e.g. `Customer.Country`). CamelCase fallback applied at each segment. |
| `GroupedListJson` | `string` | Output | JSON array of `{"Key": "value", "Items": [...]}` groups. Groups appear in first-seen order. |

### List_Difference

Computes the set difference (A − B) of two JSON lists on a key property.

| Parameter | Type | Direction | Description |
|-----------|------|-----------|-------------|
| `ListAJson` | `string` | Input | The base JSON list. |
| `ListBJson` | `string` | Input | The subtraction JSON list. |
| `MatchKey` | `string` | Input | Property path to match on. Supports nested paths and array indexing (`Ref.Code`, `Codes[0]`). |
| `ComparisonOperator` | `string` | Input | Any operator from the operators table below. |
| `CaseSensitive` | `bool` | Input | Case-sensitivity flag (default `false`). |
| `DifferenceListJson` | `string` | Output | Elements in A whose `MatchKey` has no match in B. |

---

## Comparison Operators

The condition-based actions (`List_PopByCondition`, `List_PopMultipleByCondition`, `List_Difference`) accept an operator that controls how `TargetValue` is compared against the property value. Multi-condition actions (`List_PopByConditions`, `List_PopMultipleByConditions`) accept the same operators inside each condition entry.

| Operator | Aliases | Behaviour |
|----------|---------|-----------|
| `Equals` | *(default)*, `""` | Exact match (case-sensitive per `CaseSensitive` flag) |
| `NotEquals` | `!=` | Inverse of Equals |
| `Contains` | | Substring match |
| `StartsWith` | | Prefix match |
| `EndsWith` | | Suffix match |
| `GreaterThan` | `>` | Numeric comparison (property value > target) |
| `LessThan` | `<` | Numeric comparison (property value < target) |
| `GreaterOrEqual` | `>=` | Numeric comparison including boundary |
| `LessOrEqual` | `<=` | Numeric comparison including boundary |

**Case sensitivity**: string operators (`Equals`, `NotEquals`, `Contains`, `StartsWith`, `EndsWith`) honour the `CaseSensitive` flag. Numeric operators ignore it.

**Numeric operators** parse both values with `InvariantCulture`. Non-numeric values evaluate as no-match.

---

## Property Paths

`PropertyName`, `MatchKey`, and paths inside condition JSON support both **dot-separated navigation** and **array indexing**:

| Path syntax | Resolves to |
|------------|-------------|
| `Address.City` | `obj["Address"]["City"]` |
| `Meta.Status` | `obj["Meta"]["Status"]` |
| `Wrapper.Data.Value` | `obj["Wrapper"]["Data"]["Value"]` |
| `Tags[0]` | `obj["Tags"][0]` (first array element) |
| `Tags[-1]` | last element of the array |
| `Items[0].Name` | `obj["Items"][0]["Name"]` |
| `Groups[0].Members[-1]` | mix of dots and indexing at multiple depths |

**CamelCase fallback** is applied at each name segment — so `Address.City` also matches `address.city` in the JSON.

**Negative indices** count from the end (`[-1]` is last, `[-2]` is second-to-last).

If any segment is missing, the array index is out of range, or the value at any level is not an object/array where expected, the item is treated as non-matching.

---

## Multiple Conditions (AND/OR)

`List_PopByConditions` and `List_PopMultipleByConditions` accept a JSON array of conditions and a logical operator (`AND` or `OR`).

**Example — find active users over 30:**

```json
[
  {"path": "Status", "operator": "Equals", "value": "Active"},
  {"path": "Age", "operator": "GreaterThan", "value": "30"}
]
```
Combined with `logicalOperator = "AND"`.

**Example — case-sensitive per condition:**

```json
[
  {"path": "Code", "operator": "Equals", "value": "URGENT", "caseSensitive": true},
  {"path": "Priority", "operator": "Equals", "value": "High"}
]
```

Each condition has: `path`, `operator`, `value`, and optional `CaseSensitive` (default `false`). Empty conditions array returns the original list unchanged.

---

## Search Direction

`List_PopByCondition` and `List_PopByConditions` accept a `SearchFromEnd` boolean that controls whether the FIRST or LAST match is popped.

| `SearchFromEnd` | Behaviour |
|-----------------|-----------|
| `false` (default) | Iterates from index 0 upward. Pops the first match. |
| `true` | Iterates from the last index downward. Pops the last match. |

Example — pop the most recent "Pending" order from a chronological list:

```
List_PopByCondition(orders, "Status", "Pending", "Equals", false, true, ...)
                                                            ^^^^  ^^^^
                                                 CaseSensitive  SearchFromEnd
```

`List_PopMultipleByCondition` and `List_PopMultipleByConditions` do NOT have this flag — they always pop every match, so direction is irrelevant.

---

## How It Works

All actions are **stateless** and **in-memory**. There is no file I/O, no network access, and no persistent state between calls.

| Action category | Mechanism | Complexity |
|-----------------|-----------|------------|
| Index-based pops (`List_Pop`, `List_PopMultiple`) | Direct `List<string>` manipulation using `RemoveAt`. Multiple indices are reverse-sorted before removal to avoid index shifting. | O(N) |
| Condition-based pops (`PopByCondition`, `PopMultipleByCondition`) | Parse the JSON string into a `JsonArray`, linear scan matching `propertyName == targetValue`, return modified arrays serialized back to JSON. | O(N) |
| `List_Zip` | Parse both JSON arrays, iterate to `Min(A.Count, B.Count)`, construct paired `JsonObject`s. | O(min(A,B)) |
| `List_GroupBy` | Single-pass scan building a `Dictionary<string, JsonArray>` keyed by property value. Preserves insertion order via a parallel list. | O(N) |
| `List_Difference` | Build a `HashSet<string>` from B's key values, then filter A against it. | O(A + B) |

**Property name matching:** All JSON actions try the exact `PropertyName` first, then a camelCase variant (first letter lowered). This handles the mismatch between OutSystems PascalCase attribute names and the camelCase keys produced by `JSON Serialize`.

**Null/empty safety:** Null or empty inputs never throw — they return empty lists, `"[]"`, or `"{}"` as appropriate.

**Malformed JSON:** Invalid JSON input will throw a platform error (`JsonException`). Validate JSON before calling or wrap in an exception handler.

```
[OutSystems Structure List]
       │
       ▼
┌──────────────────────┐
│    JSON Serialize    │ ───► Converts Structure List to plain text
└──────────────────────┘
       │
       ▼
┌──────────────────────┐
│  ListUtils Action    │ ───► Manipulates the JSON (pop, zip, group, diff)
└──────────────────────┘
       │
       ├───► [updatedListJson]  ───► JSON Deserialize → Structure List
       └───► [poppedElementJson] ──► JSON Deserialize → Structure Record
```

---

## Requirements

| | ODC | O11 |
|-|-----|-----|
| Platform | OutSystems Developer Cloud | OutSystems 11 |
| Runtime | Linux container (ODC Portal) | Windows (.NET Framework 4.8) |
| .NET | 10.0 | Framework 4.8 |

### NuGet Packages

| Package | ODC | O11 | Notes |
|---------|-----|-----|-------|
| `OutSystems.ExternalLibraries.SDK` | 1.5.0 | — | ODC-only; O11 uses Integration Studio DLLs |
| `System.Text.Json` | — | 8.0.5 | Built into net10 BCL; explicit NuGet required on net48 |

All packages are MIT or OutSystems proprietary (SDK only).

---

## Using in ODC

1. Run the packaging script to produce the upload ZIP:
   ```powershell
   .\ListUtils\generate_upload_package.ps1
   ```
2. In **ODC Portal** → **External Logic** → **Upload** the ZIP.
3. Create and publish an External Library.
4. In your ODC app, add `ListUtilsServerSide` as a dependency.
5. In any Server Action, call `JSON Serialize` on your Structure List, pass the result to the desired ListUtils action, then `JSON Deserialize` the output back to your target Structure List.

---

## Using in O11

1. Build the O11 project:
   ```powershell
   cd ListUtils.O11
   dotnet build -c Release
   ```
2. Create an extension in **Integration Studio** with the same action signatures (7 actions, all parameters as Text, Integer Text List, or Integer List).
3. Click **Edit Source Code**, paste the implementation from `Actions/ListUtilsActions.cs` into the generated file.
4. Add the `System.Text.Json` NuGet package (8.0.5) to the IS-generated `.csproj`.
5. Build in Visual Studio, return to Integration Studio → **1-Click Publish**.

---

## Development

### Build

```bash
dotnet build ListUtils.sln
```

Builds all four projects: ODC library, O11 library, ODC tests, O11 tests.

### Test

```bash
dotnet test ListUtils.sln
```

Runs 52 tests (26 ODC net10.0 + 26 O11 net48).

### Package (ODC)

```powershell
.\ListUtils\generate_upload_package.ps1
```

Publishes for `linux-x64`, zips the output to `ExternalLibrary.zip`, and verifies the file is under the 90 MB ODC Portal limit.

---

## Changelog

See [CHANGELOG.md](./CHANGELOG.md) for the full version history.

---

## Third-Party Notices

See [THIRD-PARTY-NOTICES.md](./THIRD-PARTY-NOTICES.md) for the full list of open-source dependencies and their licenses.

---

## License

[MIT](./LICENSE)
