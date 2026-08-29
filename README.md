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
| `indicesToPop` | `List<int>` | Input | The list of 0-based indices to remove. |
| `updatedList` | `List<string>` | Output | The list without the popped elements. |
| `poppedElements` | `List<string>` | Output | The elements that were removed, in their original order. |

### List_PopByCondition

Pops the first element matching a property condition from a JSON list.

| Parameter | Type | Direction | Description |
|-----------|------|-----------|-------------|
| `sourceListJson` | `string` | Input | The source list serialized as a JSON string (via `JSON Serialize`). |
| `propertyName` | `string` | Input | Property path to check. Supports nested paths with dots (e.g. `Address.City` or `Meta.Status`). CamelCase fallback applied at each segment. |
| `targetValue` | `string` | Input | The value to match. |
| `comparisonOperator` | `string` | Input | `Equals` (default), `NotEquals`, `Contains`, `StartsWith`, `EndsWith`, `GreaterThan`, `LessThan`, `GreaterOrEqual`, `LessOrEqual`. Empty = `Equals`. |
| `updatedListJson` | `string` | Output | The JSON list without the matched element. |
| `poppedElementJson` | `string` | Output | The matched JSON object, or `{}` if no match found. |

### List_PopMultipleByCondition

Pops all elements matching a property condition from a JSON list.

| Parameter | Type | Direction | Description |
|-----------|------|-----------|-------------|
| `sourceListJson` | `string` | Input | The source list serialized as a JSON string. |
| `propertyName` | `string` | Input | Property path to check. Supports nested paths with dots (e.g. `Address.City`). |
| `targetValue` | `string` | Input | The value to match. |
| `comparisonOperator` | `string` | Input | Same operators as `List_PopByCondition`. |
| `updatedListJson` | `string` | Output | The JSON list without matched elements. |
| `poppedElementsJson` | `string` | Output | JSON array of all matched elements, or `[]` if none. |

### List_Zip

Combines two JSON lists into paired objects by matching index.

| Parameter | Type | Direction | Description |
|-----------|------|-----------|-------------|
| `listAJson` | `string` | Input | The first JSON list. |
| `listBJson` | `string` | Input | The second JSON list. |
| `keyNameA` | `string` | Input | Key label for List A entries in the output objects. |
| `keyNameB` | `string` | Input | Key label for List B entries in the output objects. |
| `zippedListJson` | `string` | Output | JSON array of paired objects `[{keyNameA: ..., keyNameB: ...}, ...]`. Truncates to the shorter list. |

### List_GroupBy

Groups a flat JSON list by a property value.

| Parameter | Type | Direction | Description |
|-----------|------|-----------|-------------|
| `sourceListJson` | `string` | Input | The source JSON list. |
| `propertyName` | `string` | Input | Property path to group by. Supports nested paths with dots (e.g. `Customer.Country`). CamelCase fallback applied at each segment. |
| `groupedListJson` | `string` | Output | JSON array of `{"Key": "value", "Items": [...]}` groups. Groups appear in first-seen order. |

### List_Difference

Computes the set difference (A − B) of two JSON lists on a key property.

| Parameter | Type | Direction | Description |
|-----------|------|-----------|-------------|
| `listAJson` | `string` | Input | The base JSON list. |
| `listBJson` | `string` | Input | The subtraction JSON list. |
| `matchKey` | `string` | Input | Property path to match on. Supports nested paths with dots (e.g. `Ref.Code`). |
| `comparisonOperator` | `string` | Input | `Equals` (default), `NotEquals`, `Contains`, `StartsWith`, `EndsWith`, `GreaterThan`, `LessThan`, `GreaterOrEqual`, `LessOrEqual`. Empty = `Equals`. |
| `differenceListJson` | `string` | Output | Elements in A whose `matchKey` has no match in B. |

---

## Comparison Operators

The three condition-based actions (`List_PopByCondition`, `List_PopMultipleByCondition`, `List_Difference`) accept an operator that controls how `targetValue` is compared against the property value.

| Operator | Aliases | Behaviour |
|----------|---------|-----------|
| `Equals` | *(default)*, `""` | Case-insensitive exact match |
| `NotEquals` | `!=` | Inverse of Equals |
| `Contains` | | Case-insensitive substring match |
| `StartsWith` | | Case-insensitive prefix match |
| `EndsWith` | | Case-insensitive suffix match |
| `GreaterThan` | `>` | Numeric comparison (property value > target) |
| `LessThan` | `<` | Numeric comparison (property value < target) |
| `GreaterOrEqual` | `>=` | Numeric comparison including boundary |
| `LessOrEqual` | `<=` | Numeric comparison including boundary |

**Numeric operators** parse both values with `InvariantCulture`. Non-numeric values evaluate as no-match.

---

## Nested Property Paths

`propertyName` and `matchKey` support **dot-separated paths** to reach into nested objects:

```
"Address.City"           → obj["Address"]["City"]
"Meta.Status"            → obj["Meta"]["Status"]
"Wrapper.Data.Value"     → obj["Wrapper"]["Data"]["Value"]
```

CamelCase fallback is applied **at each segment** — so `Address.City` also matches `address.city` in the JSON.

If any segment is missing or the value at any level is not an object, the item is treated as non-matching.

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

**Property name matching:** All JSON actions try the exact `propertyName` first, then a camelCase variant (first letter lowered). This handles the mismatch between OutSystems PascalCase attribute names and the camelCase keys produced by `JSON Serialize`.

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
