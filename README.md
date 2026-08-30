# OSListUtilsServerSide

**ODC External Library + O11 Integration Studio Extension**

Advanced list manipulation utilities — index-based pops, condition-based pops, zip, group-by, and set difference. Uses JSON serialization for generic structure support.

---

## Objective

OutSystems lists lack common collection operations found in general-purpose languages (pop by index, pop by condition, zip, cogroup, group-by, set difference / intersection / union, chunk, distinct-by, slice, shuffle, sample, sort by property, min / max by property, aggregation, partition, mass update). Implementing these natively requires verbose nested `For Each` loops with manual index tracking. This component provides **twenty-nine** server-side actions that cover the most common gaps in a single call each — grouped into pop-by-index / pop-by-condition, relational (zip / group-by / cogroup / set ops), transformations, aggregations, split / partition, mass update, and multi-list zip.

The JSON-based actions work with **any OutSystems Structure** — the caller serializes the list with `JSON Serialize`, passes it to the action, and deserializes the result. This generic approach eliminates the need for per-structure custom extensions.

Multi-condition actions (`List_PopByConditions`, `List_PopMultipleByConditions`, `List_PartitionByConditions`, `List_ReplaceWhere`) accept a first-class **`Condition` Structure list** (`Path`, `Operator`, `Value`, `CaseSensitive`) — no hand-authored JSON. Operator strings, logical operators, and aggregate operations are also exposed as compile-time constants under `Operators`, `LogicalOperators`, and `AggregateOperations` so consumers get IDE autocomplete instead of magic strings.

---

## Server Actions

### List_Pop

Removes an element at a specific index. Returns the removed element and the updated list.

| Parameter | Type | Direction | Description |
|-----------|------|-----------|-------------|
| `SourceListJson` | `string` (JSON array) | Input | The source list serialized as a JSON array (via `JSON Serialize`). |
| `Index` | `int` | Input | The 0-based index of the element to remove. |
| `UpdatedListJson` | `string` (JSON array) | Output | The JSON array without the popped element. Empty input returns `"[]"`; out-of-range index returns the source unchanged. |
| `PoppedElementJson` | `string` (JSON) | Output | The removed element serialized as JSON, or `"null"` when no element was removed. |

### List_PopMultiple

Removes multiple elements at specified indices. Returns the removed elements and the updated list.

| Parameter | Type | Direction | Description |
|-----------|------|-----------|-------------|
| `SourceListJson` | `string` (JSON array) | Input | The source list serialized as a JSON array. |
| `IndicesToPop` | `string` | Input | Comma-separated 0-based indices to remove (e.g. `"1,3,5"`). Whitespace is trimmed. |
| `UpdatedListJson` | `string` (JSON array) | Output | The JSON array without the popped elements. |
| `PoppedElementsJson` | `string` (JSON array) | Output | JSON array of removed elements, in their original order. Out-of-range indices are silently ignored. |

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
| `Conditions` | `List<Condition>` | Input | List of `Condition` Structures. Each entry has `Path`, `Operator`, `Value`, `CaseSensitive`. Empty list = source returned unchanged. |
| `LogicalOperator` | `string` | Input | `AND` (default) — all conditions must match; `OR` — at least one must match. Use `LogicalOperators.AND` / `LogicalOperators.OR`. |
| `SearchFromEnd` | `bool` | Input | When `true`, searches from the end backwards (pops the LAST match). When `false` (default), searches from the beginning. |
| `UpdatedListJson` | `string` | Output | The JSON list without the matched element. |
| `PoppedElementJson` | `string` | Output | The matched JSON object, or `{}` if no match found. |

### List_PopMultipleByConditions

Pops all elements matching multiple conditions combined with AND/OR.

| Parameter | Type | Direction | Description |
|-----------|------|-----------|-------------|
| `SourceListJson` | `string` | Input | The source list serialized as a JSON string. |
| `Conditions` | `List<Condition>` | Input | List of `Condition` Structures (same shape as `List_PopByConditions`). |
| `LogicalOperator` | `string` | Input | `AND` or `OR`. |
| `UpdatedListJson` | `string` | Output | The JSON list without matched elements. |
| `PoppedElementsJson` | `string` | Output | JSON array of all matched elements. |

**Condition Structure:**

| Field | Type | Description |
|-------|------|-------------|
| `Path` | `string` | Property path (nested + array indexing supported). |
| `Operator` | `string` | Any operator from the operators table. Prefer `Operators.Equals`, `Operators.GreaterThan`, … Empty = `Equals`. |
| `Value` | `string` | Target value as text. Numeric operators parse it as `decimal` with `InvariantCulture`. |
| `CaseSensitive` | `bool` | Per-condition case sensitivity (default `false`). Ignored by numeric operators. |

The same `List<Condition>` shape is reused by `List_PartitionByConditions` and `List_ReplaceWhere`.

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

### List_ZipGroupBy

Cogroup / two-list join by a shared key. For every distinct key value across two lists, produces a group object containing the key plus two named arrays — one holding items from `ListA` that share the key, one holding items from `ListB`. Perfect for scenarios like "orders + payments per customer" or "employees + timesheets per department" that would otherwise need nested `For Each` loops.

| Parameter | Type | Direction | Description |
|-----------|------|-----------|-------------|
| `ListAJson` | `string` (JSON array) | Input | The first JSON list. |
| `ListBJson` | `string` (JSON array) | Input | The second JSON list. |
| `KeyPropertyA` | `string` | Input | Property path in `ListA` that supplies the group key. Supports nested paths and array indexing. |
| `KeyPropertyB` | `string` | Input | Property path in `ListB` that supplies the group key. Supports nested paths and array indexing. |
| `KeyNameA` | `string` | Input | Output field name for the `ListA` items array within each group (e.g. `"Orders"`). Blank falls back to `"ItemsA"`. |
| `KeyNameB` | `string` | Input | Output field name for the `ListB` items array (e.g. `"Payments"`). Blank falls back to `"ItemsB"`. |
| `CaseSensitive` | `bool` | Input | Key comparison flag (default `false`). |
| `GroupedListJson` | `string` | Output | JSON array of `{"Key": <keyValue>, <KeyNameA>: [...], <KeyNameB>: [...]}` groups. Ordering: A's keys first (in A's order), then any B-only keys (in B's order). Items with no key value fall into a single `"Unknown"` bucket. |

**Example:**

```
ListA (Orders)   → [{"CustomerId":"1","OrderId":101},
                    {"CustomerId":"2","OrderId":102},
                    {"CustomerId":"1","OrderId":103}]
ListB (Payments) → [{"CustomerId":"1","PaymentId":201},
                    {"CustomerId":"2","PaymentId":202},
                    {"CustomerId":"2","PaymentId":203}]

List_ZipGroupBy(..., "CustomerId", "CustomerId", "Orders", "Payments", false)
→
[
  {"Key":"1", "Orders":[{...OrderId:101},{...OrderId:103}], "Payments":[{...PaymentId:201}]},
  {"Key":"2", "Orders":[{...OrderId:102}],                    "Payments":[{...PaymentId:202},{...PaymentId:203}]}
]
```

A key that appears only on one side still produces a group — the other array is empty. The two `KeyProperty*` parameters can differ (e.g. `CustomerId` on ListA vs `customer_id` on ListB) which is handy for joining data from two systems with different naming conventions.

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

### List_Chunk

Splits a JSON list into a **List of Text** where each entry is a standalone JSON array (one chunk). The last chunk may be smaller than `ChunkSize`. Useful for batching API payloads and throttled loops — each entry is directly consumable by `JSON Deserialize` targeting the caller's Structure List, so there is no nested `List of List` to unwrap.

| Parameter | Type | Direction | Description |
|-----------|------|-----------|-------------|
| `SourceListJson` | `string` (JSON array) | Input | The source list serialized as a JSON array. |
| `ChunkSize` | `int` | Input | Maximum number of elements per chunk. `<= 0` (or empty source) returns an empty list. |
| `ChunksListJson` | `List<string>` (Text List — each entry a JSON array) | Output | One entry per chunk. Each entry is a self-contained JSON array string (e.g. `"[{...},{...}]"`) ready for `JSON Deserialize` into the caller's Structure List. The last entry may hold fewer elements than `ChunkSize`. |

### List_DistinctBy

Filters a JSON list to unique elements by a property key. First occurrence wins. Works on structures — native ODC `Distinct` only supports basic types.

| Parameter | Type | Direction | Description |
|-----------|------|-----------|-------------|
| `SourceListJson` | `string` (JSON array) | Input | The source list. |
| `PropertyName` | `string` | Input | Property path used as the uniqueness key. Supports nested paths (`Address.City`) and array indexing (`Tags[0]`). CamelCase fallback applied at each segment. Empty `PropertyName` dedupes on the entire item's serialised JSON. |
| `CaseSensitive` | `bool` | Input | When `true`, key comparison is case-sensitive. When `false` (default), case-insensitive. |
| `DistinctListJson` | `string` (JSON array) | Output | JSON array containing only the first occurrence of each unique key, preserving source order. Items whose key is missing share a single "null-key" bucket, so at most one keyless item survives. |

### List_Slice

Extracts a sub-range of a JSON list using Python/JavaScript-style `Start`, `End`, `Step`.

| Parameter | Type | Direction | Description |
|-----------|------|-----------|-------------|
| `SourceListJson` | `string` (JSON array) | Input | The source list. |
| `Start` | `int` | Input | 0-based inclusive start. Negative values count from the end (`-2` = second-to-last). Clamped to the list bounds. |
| `End` | `int` | Input | 0-based exclusive end. Negative values count from the end. **Sentinel: `End == 0` is treated as "unspecified"** — for positive `Step` it means "to end of list"; for negative `Step` it means "past the beginning" (matches Python defaults). |
| `Step` | `int` | Input | Step between selected elements. `0` is treated as `1`. Positive walks forward; **negative walks backward (reverse slice)**. |
| `SliceListJson` | `string` (JSON array) | Output | The sliced JSON array. |

### List_Shuffle

Randomizes the order of a JSON list using the Fisher-Yates algorithm. Source list is not mutated.

| Parameter | Type | Direction | Description |
|-----------|------|-----------|-------------|
| `SourceListJson` | `string` (JSON array) | Input | The source list. |
| `Seed` | `int` | Input | Random seed. **`Seed == 0` uses `RandomNumberGenerator` (CSPRNG per swap)** — suitable for security-sensitive shuffles. Any non-zero value uses `System.Random(Seed)` and produces the same permutation every call (useful for reproducible tests). |
| `ShuffledListJson` | `string` (JSON array) | Output | The shuffled JSON array. |

### List_UpdateAt

Sets a single property of the item at a given `Index`. Returns the modified list and the previous value of that property.

| Parameter | Type | Direction | Description |
|-----------|------|-----------|-------------|
| `SourceListJson` | `string` (JSON array) | Input | The source list. |
| `Index` | `int` | Input | 0-based index of the item to modify. Negative values count from the end. Out-of-range indices return the source unchanged and `PreviousValueJson = "null"`. |
| `PropertyName` | `string` | Input | Property path to update on the target item. Supports nested paths (`Address.City`) and array indexing (`Tags[0]`). **Missing intermediate objects are created; arrays are NOT auto-created** — if an array indexing step encounters a missing or non-array node, the action returns the source unchanged. |
| `NewValueJson` | `string` (JSON) | Input | The new value as JSON (`"Active"`, `42`, `true`, `{"City":"Lisbon"}`, `null`). Falls back to a raw string when the value is not valid JSON. |
| `UpdatedListJson` | `string` (JSON array) | Output | The updated JSON array with the property changed. |
| `PreviousValueJson` | `string` (JSON) | Output | The previous JSON value of the modified property, or `"null"` when: the index was out of range, `PropertyName` was empty, the item was not an object, the property did not exist, OR the property existed with a JSON `null` value (the last two cases are indistinguishable). |

---

## Comparison Operators

The condition-based actions (`List_PopByCondition`, `List_PopMultipleByCondition`, `List_Difference`) accept an operator that controls how `TargetValue` is compared against the property value. Multi-condition actions (`List_PopByConditions`, `List_PopMultipleByConditions`, `List_PartitionByConditions`, `List_ReplaceWhere`) accept the same operators inside each `Condition` Structure's `Operator` field.

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

All nine names are also exposed as compile-time constants on the `Operators` helper class — prefer `Operators.Equals`, `Operators.GreaterThan`, … over raw string literals so typos are caught at build time.

**Case sensitivity**: string operators (`Equals`, `NotEquals`, `Contains`, `StartsWith`, `EndsWith`) honour the `CaseSensitive` flag. Numeric operators ignore it.

**Numeric operators** parse both values with `InvariantCulture`. Non-numeric values evaluate as no-match.

---

## Property Paths

`PropertyName`, `MatchKey`, and every `Condition.Path` support both **dot-separated navigation** and **array indexing**:

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

`List_PopByConditions`, `List_PopMultipleByConditions`, `List_PartitionByConditions`, and `List_ReplaceWhere` all accept a `List<Condition>` and a `LogicalOperator` (`AND` or `OR`). Build the list once in your Server Action — no JSON hand-authoring required.

**Example — find active users over 30 (pseudocode):**

```
conditions = new List<Condition> {
    new Condition { Path = "Status", Operator = Operators.Equals,      Value = "Active" },
    new Condition { Path = "Age",    Operator = Operators.GreaterThan, Value = "30"     }
};
List_PopByConditions(usersJson, conditions, LogicalOperators.AND, false, out ..., out ...);
```

**Example — case-sensitive per condition:**

```
conditions = new List<Condition> {
    new Condition { Path = "Code",     Operator = Operators.Equals, Value = "URGENT", CaseSensitive = true },
    new Condition { Path = "Priority", Operator = Operators.Equals, Value = "High"                        }
};
```

Each `Condition` has: `Path`, `Operator`, `Value`, and `CaseSensitive` (default `false`). An empty `Conditions` list returns the source unchanged — byte-for-byte identical output.

### Constants classes

| Class | Constants | Use for |
|-------|-----------|---------|
| `Operators` | `Equals`, `NotEquals`, `Contains`, `StartsWith`, `EndsWith`, `GreaterThan`, `LessThan`, `GreaterOrEqual`, `LessOrEqual` | `Condition.Operator`, `ComparisonOperator` parameters on the single-condition actions. |
| `LogicalOperators` | `AND`, `OR` | `LogicalOperator` parameter on the multi-condition actions. |
| `AggregateOperations` | `Sum`, `Avg`, `Min`, `Max`, `Count`, `CountDistinct` | `Operation` parameter on `List_Aggregate`. |

Symbol aliases (`!=`, `>`, `<`, `>=`, `<=`) are still accepted at runtime for backwards compatibility.

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
| Index-based pops (`List_Pop`, `List_PopMultiple`) | Parse the JSON string into a `JsonArray`, `RemoveAt` by index. `List_PopMultiple` reverse-sorts and dedupes the comma-separated indices before removal to avoid index shifting. | O(N) |
| Condition-based pops (`PopByCondition`, `PopMultipleByCondition`) | Parse the JSON string into a `JsonArray`, linear scan matching `propertyName == targetValue`, return modified arrays serialized back to JSON. | O(N) |
| `List_Zip` | Parse both JSON arrays, iterate to `Min(A.Count, B.Count)`, construct paired `JsonObject`s. | O(min(A,B)) |
| `List_GroupBy` | Single-pass scan building a `Dictionary<string, JsonArray>` keyed by property value. Preserves insertion order via a parallel list. | O(N) |
| `List_ZipGroupBy` | Two single-pass scans (A then B) into per-side `Dictionary<string, JsonArray>` buckets sharing the same key order. Union of first-seen keys drives the output. | O(A + B) |
| `List_Difference` | Build a `HashSet<string>` from B's key values, then filter A against it (except for `Contains`, which stays O(A×B)). | O(A + B) |
| `List_Chunk` | Single pass over the source, sliding a `JsonArray` buffer of `ChunkSize` elements. | O(N) |
| `List_DistinctBy` | Single pass building a `HashSet<string>` on the property-derived key (or full-item JSON when `PropertyName` is empty). First occurrence wins. | O(N) |
| `List_Slice` | Normalise `Start` / `End` / `Step`, then walk the source once with the normalised step (negative walks in reverse). | O(N) |
| `List_Shuffle` | Deep-clone the source, run Fisher-Yates in place. `Seed == 0` calls `RandomNumberGenerator.GetInt32` per swap; `Seed != 0` uses `System.Random(Seed)`. | O(N) |
| `List_UpdateAt` | Deep-clone the source, walk `PropertyName` on the item at `Index`, set the leaf and return the previous value. | O(path length) |

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
5. In any Server Action, call `JSON Serialize` on your Structure List, pass the result to the desired ListUtils action, then `JSON Deserialize` the output back to your target Structure List. `List_Chunk` returns a **Text List** where each entry is a standalone JSON-array string ready for a per-entry `JSON Deserialize`. `List_UpdateAt` returns a `PreviousValueJson` that may need a separate `JSON Deserialize` targeting the property's Structure or basic type.

---

## Using in O11

1. Build the O11 project:
   ```powershell
   cd ListUtils.O11
   dotnet build -c Release
   ```
2. Create an extension in **Integration Studio** with the same action signatures (14 actions, all parameters as Text, Integer, or Boolean).
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

Runs 906 tests (453 ODC net10.0 + 453 O11 net48) — 228 functional + 245 load tests per platform.

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
