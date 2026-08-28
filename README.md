# OSListUtilsServerSide

**ODC External Library + O11 Integration Studio Extension**

Advanced list manipulation utilities — index-based pops, condition-based pops, zip, group-by, and set difference. Uses JSON serialization for generic structure support.

---

## Objective

OutSystems lists lack common collection operations found in general-purpose languages (pop, zip, group-by, set difference). Implementing these in Server Actions requires verbose nested loops. This component provides seven server-side actions that cover the most common gaps:

1. **Pop / PopMultiple** — remove elements by index from a string list.
2. **PopByCondition / PopMultipleByCondition** — remove elements from a JSON-serialized list by matching a property value.
3. **Zip** — pair two JSON lists element-by-element.
4. **GroupBy** — group a flat JSON list by a property.
5. **Difference** — compute the set difference of two JSON lists on a key.

JSON-based actions accept any OutSystems structure list serialized with `JSON Serialize` and return JSON that can be deserialized back with `JSON Deserialize`.

---

## Server Actions

### List_Pop

Removes an element at a specific index. Returns the removed element and the updated list.

| Parameter | Type | Direction | Description |
|-----------|------|-----------|-------------|
| `sourceList` | `List<string>` | Input | The source list to manipulate. |
| `index` | `int` | Input | The 0-based index of the element to remove. |
| `updatedList` | `List<string>` | Output | The list without the popped element. |
| `poppedElement` | `string` | Output | The element that was removed. |

### List_PopMultiple

Removes multiple elements at specified indices. Returns the removed elements and the updated list.

| Parameter | Type | Direction | Description |
|-----------|------|-----------|-------------|
| `sourceList` | `List<string>` | Input | The source list to manipulate. |
| `indicesToPop` | `List<int>` | Input | The list of 0-based indices to remove. |
| `updatedList` | `List<string>` | Output | The list without the popped elements. |
| `poppedElements` | `List<string>` | Output | The elements that were removed, in original order. |

### List_PopByCondition

Pops the first element matching a property condition from a JSON list.

| Parameter | Type | Direction | Description |
|-----------|------|-----------|-------------|
| `sourceListJson` | `string` | Input | The source list serialized as a JSON string. |
| `propertyName` | `string` | Input | The attribute name to check (e.g. `IsActive`, `Id`). |
| `targetValue` | `string` | Input | The value to match (case-insensitive). |
| `updatedListJson` | `string` | Output | The JSON list without the matched element. |
| `poppedElementJson` | `string` | Output | The matched JSON object, or `{}` if none. |

### List_PopMultipleByCondition

Pops all elements matching a property condition from a JSON list.

| Parameter | Type | Direction | Description |
|-----------|------|-----------|-------------|
| `sourceListJson` | `string` | Input | The source list serialized as a JSON string. |
| `propertyName` | `string` | Input | The attribute name to check. |
| `targetValue` | `string` | Input | The value to match (case-insensitive). |
| `updatedListJson` | `string` | Output | The JSON list without matched elements. |
| `poppedElementsJson` | `string` | Output | JSON array of all matched elements. |

### List_Zip

Combines two JSON lists into paired objects by matching index.

| Parameter | Type | Direction | Description |
|-----------|------|-----------|-------------|
| `listAJson` | `string` | Input | The first JSON list. |
| `listBJson` | `string` | Input | The second JSON list. |
| `keyNameA` | `string` | Input | Key label for List A entries in the output. |
| `keyNameB` | `string` | Input | Key label for List B entries in the output. |
| `zippedListJson` | `string` | Output | JSON array of paired objects. |

### List_GroupBy

Groups a flat JSON list by a property value.

| Parameter | Type | Direction | Description |
|-----------|------|-----------|-------------|
| `sourceListJson` | `string` | Input | The source JSON list. |
| `propertyName` | `string` | Input | The property to group by. |
| `groupedListJson` | `string` | Output | JSON array of `{Key, Items}` groups. |

### List_Difference

Computes the set difference (A − B) of two JSON lists on a key property.

| Parameter | Type | Direction | Description |
|-----------|------|-----------|-------------|
| `listAJson` | `string` | Input | The base JSON list. |
| `listBJson` | `string` | Input | The subtraction JSON list. |
| `matchKey` | `string` | Input | The property to match on (e.g. `Id`). |
| `differenceListJson` | `string` | Output | Elements in A with no match in B. |

---

## Platforms

| Platform | Target Framework |
|----------|-----------------|
| ODC | .NET 10 |
| O11 | .NET Framework 4.8 |

---

## Build

```bash
dotnet build ListUtils.sln
```

## Test

```bash
dotnet test ListUtils.sln
```

## Package (ODC)

```powershell
.\ListUtils\generate_upload_package.ps1
```

---

## License

[MIT](./LICENSE)
