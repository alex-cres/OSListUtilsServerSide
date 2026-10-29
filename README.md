# OSListUtilsServerSide

**ODC External Library + O11 Integration Studio Extension**

Advanced list manipulation utilities — 33 Server Actions for index- and condition-based pops, split / partition, zip, group-by and cogroup (single-key, multi-key, multi-list), set operations, transformations, randomization, mass updates, and aggregations. Uses JSON serialization for generic structure support.

---

## Objective

OutSystems lists lack common collection operations found in general-purpose languages (pop by index, pop by condition, zip, cogroup, group-by, multi-key group-by / cogroup, set difference / intersection / union, chunk, distinct-by, slice, shuffle, sample, sort by property, min / max by property, aggregation, partition, mass update). Implementing these natively requires verbose nested `For Each` loops with manual index tracking. This component provides **thirty-three** server-side actions that cover the most common gaps in a single call each — grouped into pop-by-index / pop-by-condition, relational (zip / group-by / cogroup / multi-key group-by / set ops), transformations, aggregations, split / partition, mass update, and multi-list zip.

The JSON-based actions work with **any OutSystems Structure** — the caller serializes the list with `JSON Serialize`, passes it to the action, and deserializes the result. This generic approach eliminates the need for per-structure custom extensions.

Multi-condition actions (`List_PopByConditions`, `List_PopMultipleByConditions`, `List_PartitionByConditions`, `List_ReplaceWhere`) accept a first-class **`Condition` Structure list** (`Path`, `Operator`, `Value`, `CaseSensitive`) — no hand-authored JSON. Operator strings, logical operators, and aggregate operations are also exposed as compile-time constants under `Operators`, `LogicalOperators`, and `AggregateOperations` so consumers get IDE autocomplete instead of magic strings.

The ODC interface and handwritten O11 implementation use typed `List<Condition>` and `List<string>` parameters. Integration Studio's XIF does not expose a List of Text or the handwritten C# list types directly: it presents condition lists as a Record List of `Condition` and text lists as a Record List of `TextItem` (one Text attribute, `Value`). The generated XIF adapter maps these records to and from the O11 implementation's `List<T>` values.

---

## Server Actions

### List_Pop

Removes an element at a specific index. Returns the removed element and the updated list.

| Parameter | Type | Direction | Description |
|-----------|------|-----------|-------------|
| `SourceListJson` | `string` (JSON array) | Input | The source list serialized as a JSON array (via `JSON Serialize`). |
| `Index` | `int` | Input | The 0-based index of the element to remove. Negative values count from the end (`-1` = last element). |
| `UpdatedListJson` | `string` (JSON array) | Output | The JSON array without the popped element. Empty input returns `"[]"`; out-of-range index returns the source unchanged. |
| `PoppedElementJson` | `string` (JSON) | Output | The removed element serialized as JSON, or `"null"` when no element was removed. |

### List_PopMultiple

Removes multiple elements at specified indices. Returns the removed elements and the updated list.

| Parameter | Type | Direction | Description |
|-----------|------|-----------|-------------|
| `SourceListJson` | `string` (JSON array) | Input | The source list serialized as a JSON array. |
| `IndicesToPop` | `string` | Input | Comma-separated 0-based indices to remove (e.g. `"1,3,-1"`). Negative values count from the end. Whitespace is trimmed; duplicates (including a negative and positive index that point to the same element) are removed once; non-numeric and out-of-range entries are ignored. |
| `UpdatedListJson` | `string` (JSON array) | Output | The JSON array without the popped elements. |
| `PoppedElementsJson` | `string` (JSON array) | Output | JSON array of removed elements, in their original order. Out-of-range indices are silently ignored. |

### List_SplitAt

Splits a JSON list at an index into a left part (elements before `Index`) and a right part (`Index` and later).

| Parameter | Type | Direction | Description |
|-----------|------|-----------|-------------|
| `SourceListJson` | `string` (JSON array) | Input | The source list serialized as a JSON array. |
| `Index` | `int` | Input | 0-based split index. Negative values count from the end. Values `<= 0` leave the left part empty; values `>=` the list length leave the right part empty. |
| `LeftListJson` | `string` (JSON array) | Output | Elements with position `< Index`. |
| `RightListJson` | `string` (JSON array) | Output | Elements with position `>= Index`. |

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

### List_Partition

Splits a JSON list into matching and non-matching lists using one property condition. Non-mutating counterpart of `List_PopMultipleByCondition` — both sides are returned.

| Parameter | Type | Direction | Description |
|-----------|------|-----------|-------------|
| `SourceListJson` | `string` (JSON array) | Input | The source list. |
| `PropertyName` | `string` | Input | Property path to check. Supports nested paths and array indexing. Empty returns every element in `NonMatchingListJson`. |
| `TargetValue` | `string` | Input | The value to compare against. |
| `ComparisonOperator` | `string` | Input | Any operator from the operators table. Empty = `Equals`. |
| `CaseSensitive` | `bool` | Input | Case-sensitivity flag (default `false`). |
| `MatchingListJson` | `string` (JSON array) | Output | Elements that satisfy the condition, in source order. |
| `NonMatchingListJson` | `string` (JSON array) | Output | Elements that do not satisfy the condition (including elements where the path is missing), in source order. |

### List_PartitionByConditions

Splits a JSON list into matching and non-matching lists using multiple conditions combined with AND / OR.

| Parameter | Type | Direction | Description |
|-----------|------|-----------|-------------|
| `SourceListJson` | `string` (JSON array) | Input | The source list. |
| `Conditions` | `List<Condition>` | Input | List of `Condition` Structures. An empty list returns every element in `NonMatchingListJson`. |
| `LogicalOperator` | `string` | Input | `AND` (default) or `OR`. |
| `MatchingListJson` | `string` (JSON array) | Output | Elements that satisfy the combined condition. |
| `NonMatchingListJson` | `string` (JSON array) | Output | Elements that do not satisfy the combined condition. |

### List_Zip

Combines two JSON lists into paired objects by matching index.

| Parameter | Type | Direction | Description |
|-----------|------|-----------|-------------|
| `ListAJson` | `string` | Input | The first JSON list. |
| `ListBJson` | `string` | Input | The second JSON list. |
| `KeyNameA` | `string` | Input | Key label for List A entries in the output objects. |
| `KeyNameB` | `string` | Input | Key label for List B entries in the output objects. |
| `ZippedListJson` | `string` | Output | JSON array of paired objects `[{<KeyNameA>: ..., <KeyNameB>: ...}, ...]`. Truncates to the shorter list. |

### List_GroupBy

Groups a flat JSON list by a property value.

| Parameter | Type | Direction | Description |
|-----------|------|-----------|-------------|
| `SourceListJson` | `string` | Input | The source JSON list. |
| `PropertyName` | `string` | Input | Property path to group by. Supports nested paths with dots (e.g. `Customer.Country`). CamelCase fallback applied at each segment. |
| `GroupedListJson` | `string` | Output | JSON array of `{"Key": "value", "Items": [...]}` groups. Groups appear in first-seen order. Key comparison is **always case-sensitive** (there is no `CaseSensitive` flag on this action). Items with no value at `PropertyName` (or every item, when `PropertyName` is empty) go into a single `"Unknown"` group. |

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

### List_GroupByMultiple

Groups a flat JSON list by an **N-key composite** — i.e. by more than one property path at once. Generalisation of `List_GroupBy`.

| Parameter | Type | Direction | Description |
|-----------|------|-----------|-------------|
| `SourceListJson` | `string` (JSON array) | Input | The source JSON list. |
| `PropertyPaths` | `List<string>` | Input | Ordered list of N property paths that together form the composite group key. Supports nested paths and array indexing. Missing values fall back to the string `"Unknown"`. |
| `KeyNames` | `List<string>` | Input | Ordered list of N labels used as the field names for the group keys in the output. Missing / shorter entries default to `"Key0"`, `"Key1"`, … |
| `ItemsFieldName` | `string` | Input | Field name for the items array within each group. Blank falls back to `"Items"`. |
| `CaseSensitive` | `bool` | Input | Key comparison flag (default `false`). |
| `GroupedListJson` | `string` | Output | JSON array of `{<KeyNames[0]>: <v0>, <KeyNames[1]>: <v1>, …, <ItemsFieldName>: [...]}` groups, in first-seen order. |

Internally the composite key is joined with the ASCII **Unit Separator** (`\u001F`), so distinct tuples cannot collide even when their string representations share prefixes.

### List_ZipGroupByMultiple

Cogroup two JSON lists by an **N-key composite** — the N-key generalisation of `List_ZipGroupBy`. For every distinct composite key across the two inputs, emits one group holding one field per key plus two named arrays.

| Parameter | Type | Direction | Description |
|-----------|------|-----------|-------------|
| `ListAJson` | `string` (JSON array) | Input | The first JSON list. |
| `ListBJson` | `string` (JSON array) | Input | The second JSON list. |
| `KeyPropertiesA` | `List<string>` | Input | Ordered list of N property paths in `ListA` that form the composite key. |
| `KeyPropertiesB` | `List<string>` | Input | Ordered list of N property paths in `ListB` that form the composite key. Must have the same length as `KeyPropertiesA`. |
| `KeyNames` | `List<string>` | Input | Ordered list of N labels used as the field names for the composite key values in the output. Missing / shorter entries default to `"Key0"`, `"Key1"`, … |
| `KeyNameA` | `string` | Input | Output field name for the `ListA` items array within each group. Blank falls back to `"ItemsA"`. |
| `KeyNameB` | `string` | Input | Output field name for the `ListB` items array within each group. Blank falls back to `"ItemsB"`. |
| `CaseSensitive` | `bool` | Input | Key comparison flag (default `false`). |
| `GroupedListJson` | `string` | Output | JSON array of `{<KeyNames[0]>: <v0>, …, <KeyNameA>: [...], <KeyNameB>: [...]}` groups. Ordering follows A's first-seen composite keys, then any B-only composite keys. Items with missing key values fall into the single `"Unknown"` bucket per key column. |

### List_ZipMany

Zips any number of JSON lists into paired objects by position — the N-list generalisation of `List_Zip`.

| Parameter | Type | Direction | Description |
|-----------|------|-----------|-------------|
| `ListsJson` | `List<string>` | Input | A list of JSON-array strings — one entry per input list. An empty entry is treated as an empty list. |
| `KeyNamesJson` | `List<string>` | Input | One output label per input list. Missing / blank entries default to `"Items0"`, `"Items1"`, … |
| `ZippedListJson` | `string` (JSON array) | Output | JSON array with one object per position, each holding one field per input list. Output length is the shortest input length. |

### List_ZipManyGroupBy

Cogroups any number of JSON lists by one key path per list — the N-list generalisation of `List_ZipGroupBy`.

| Parameter | Type | Direction | Description |
|-----------|------|-----------|-------------|
| `ListsJson` | `List<string>` | Input | A list of JSON-array strings — one entry per input list. |
| `KeyPropertiesJson` | `List<string>` | Input | Position `i` is the key path applied to the items of `ListsJson[i]`. Supports nested paths and array indexing. Missing entries put that list's items in the `"Unknown"` bucket. |
| `KeyNamesJson` | `List<string>` | Input | Position `i` is the output field name for `ListsJson[i]`'s items array. Missing / blank entries default to `"Items0"`, `"Items1"`, … |
| `CaseSensitive` | `bool` | Input | Key comparison flag (default `false`). |
| `GroupedListJson` | `string` (JSON array) | Output | JSON array of `{"Key": <keyValue>, <KeyNames[0]>: [...], <KeyNames[1]>: [...], …}` groups in first-seen order across the inputs. Items whose key is missing share the `"Unknown"` bucket. |

### List_ZipManyGroupByMultiple

Cogroup **M lists** by an **N-key composite** — the N-key generalisation of `List_ZipManyGroupBy`. For every distinct composite key across all M inputs, emits one group holding one field per key plus M named arrays.

| Parameter | Type | Direction | Description |
|-----------|------|-----------|-------------|
| `ListsJson` | `List<string>` | Input | A list of M JSON-array strings — one entry per input list. |
| `KeyCount` | `int` | Input | Number of key columns N. Determines how many entries per list are consumed from `KeyProperties`. |
| `KeyProperties` | `List<string>` | Input | **Flat** list of M×N property paths in **list-major order**: entries `[i*KeyCount .. i*KeyCount + KeyCount - 1]` are the N key paths applied to `ListsJson[i]`. Missing entries default to empty (`"Unknown"` bucket). |
| `KeyNames` | `List<string>` | Input | Ordered list of N labels for the composite-key fields in the output. Missing / shorter entries default to `"Key0"`, `"Key1"`, … |
| `ItemsFieldNames` | `List<string>` | Input | Ordered list of M labels — one per input list — for that list's items array in each group. Missing / shorter entries default to `"Items0"`, `"Items1"`, … |
| `CaseSensitive` | `bool` | Input | Key comparison flag (default `false`). |
| `GroupedListJson` | `string` | Output | JSON array of `{<KeyNames[0]>: <v0>, …, <ItemsFieldNames[0]>: [...], <ItemsFieldNames[1]>: [...], …}` groups. Groups appear in first-seen order across all inputs. Missing key values collapse into the single `"Unknown"` bucket. |

### List_Difference

Computes the set difference (A − B) of two JSON lists on a key property.

| Parameter | Type | Direction | Description |
|-----------|------|-----------|-------------|
| `ListAJson` | `string` | Input | The base JSON list. |
| `ListBJson` | `string` | Input | The subtraction JSON list. |
| `MatchKey` | `string` | Input | Property path to match on. Supports nested paths and array indexing (`Ref.Code`, `Codes[0]`). |
| `ComparisonOperator` | `string` | Input | Any operator from the operators table below. |
| `CaseSensitive` | `bool` | Input | Case-sensitivity flag (default `false`). |
| `DifferenceListJson` | `string` | Output | Elements in A whose `MatchKey` has no match in B. Elements of A with no value at `MatchKey` are always kept. An empty `ListBJson` returns A unchanged; an empty `ListAJson` returns `"[]"`. |

### List_Intersect

Computes the set intersection (A ∩ B) of two JSON lists on a key property, preserving A's order.

| Parameter | Type | Direction | Description |
|-----------|------|-----------|-------------|
| `ListAJson` | `string` (JSON array) | Input | The base JSON list. |
| `ListBJson` | `string` (JSON array) | Input | The list to intersect with. |
| `MatchKey` | `string` | Input | Property path to match on. Supports nested paths and array indexing. |
| `ComparisonOperator` | `string` | Input | Any operator from the operators table. Empty = `Equals`. |
| `CaseSensitive` | `bool` | Input | Case-sensitivity flag (default `false`). |
| `IntersectionListJson` | `string` (JSON array) | Output | Elements in A whose `MatchKey` matches at least one `MatchKey` value in B. Elements without a key value are excluded. |

### List_Union

Concatenates two JSON lists and removes duplicates by a key property. The first occurrence wins, so A's items appear first.

| Parameter | Type | Direction | Description |
|-----------|------|-----------|-------------|
| `ListAJson` | `string` (JSON array) | Input | The first JSON list. |
| `ListBJson` | `string` (JSON array) | Input | The second JSON list. |
| `MatchKey` | `string` | Input | Property path used as the uniqueness key. Empty compares whole elements by their JSON. |
| `CaseSensitive` | `bool` | Input | Key comparison flag (default `false`). |
| `UnionListJson` | `string` (JSON array) | Output | A + B with duplicates removed. Elements whose key is missing share one bucket, so at most one keyless element survives. |

### List_Chunk

Splits a JSON list into a **List of Text** where each entry is a standalone JSON array (one chunk). The last chunk may be smaller than `ChunkSize`. Useful for batching API payloads and throttled loops — each entry is directly consumable by `JSON Deserialize` targeting the caller's Structure List, so there is no nested `List of List` to unwrap.

| Parameter | Type | Direction | Description |
|-----------|------|-----------|-------------|
| `SourceListJson` | `string` (JSON array) | Input | The source list serialized as a JSON array. |
| `ChunkSize` | `int` | Input | Maximum number of elements per chunk. `<= 0` (or empty source) returns an empty list. |
| `ChunksListJson` | `List<string>` (Text List — each entry a JSON array) | Output | One entry per chunk. Each entry is a self-contained JSON array string (e.g. `"[{...},{...}]"`) ready for `JSON Deserialize` into the caller's Structure List. The last entry may hold fewer elements than `ChunkSize`. |

### List_Flatten

Concatenates a list of JSON-array strings into one JSON array. Inverse of `List_Chunk`.

| Parameter | Type | Direction | Description |
|-----------|------|-----------|-------------|
| `ChunksListJson` | `List<string>` | Input | A list of JSON-array strings, typically `List_Chunk`'s output. Empty, invalid-JSON, and non-array entries are skipped. |
| `FlatListJson` | `string` (JSON array) | Output | The elements of every inner array, in order. |

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

### List_Reverse

Reverses a JSON list. Equivalent to `List_Slice(-1, 0, -1)` but explicit and cheaper.

| Parameter | Type | Direction | Description |
|-----------|------|-----------|-------------|
| `SourceListJson` | `string` (JSON array) | Input | The source list. |
| `ReversedListJson` | `string` (JSON array) | Output | The elements in reverse order. |

### List_Sample

Returns a random subset of a JSON list without replacement, using the same seed rules as `List_Shuffle`.

| Parameter | Type | Direction | Description |
|-----------|------|-----------|-------------|
| `SourceListJson` | `string` (JSON array) | Input | The source list. |
| `SampleSize` | `int` | Input | Number of elements to return. `<= 0` returns `"[]"`; values `>=` the list length return the full list in shuffled order. |
| `Seed` | `int` | Input | `0` uses a CSPRNG (`RandomNumberGenerator`); any non-zero value uses `System.Random(Seed)` for reproducible output. |
| `SampleListJson` | `string` (JSON array) | Output | JSON array of `min(SampleSize, list length)` elements. |

### List_UpdateAt

Sets a single property of the item at a given `Index`. Returns the modified list and the previous value of that property.

| Parameter | Type | Direction | Description |
|-----------|------|-----------|-------------|
| `SourceListJson` | `string` (JSON array) | Input | The source list. |
| `Index` | `int` | Input | 0-based index of the item to modify. Negative values count from the end. Out-of-range indices return the source unchanged and `PreviousValueJson = "null"`. |
| `PropertyName` | `string` | Input | Property path to update on the target item. Supports nested paths (`Address.City`) and array indexing (`Tags[0]`). **Missing intermediate objects are created; arrays are NOT auto-created** — if an array indexing step encounters a missing or non-array node, nothing is written (the item is left unchanged) and `PreviousValueJson = "null"`. |
| `NewValueJson` | `string` (JSON) | Input | The new value as JSON (`"Active"`, `42`, `true`, `{"City":"Lisbon"}`, `null`). Falls back to a raw string when the value is not valid JSON. |
| `UpdatedListJson` | `string` (JSON array) | Output | The updated JSON array with the property changed. |
| `PreviousValueJson` | `string` (JSON) | Output | The previous JSON value of the modified property, or `"null"` when: the index was out of range, `PropertyName` was empty, the item was not an object, the property did not exist, OR the property existed with a JSON `null` value (the last two cases are indistinguishable). |

### List_UpdateMultipleAt

Sets the same property to the same value on the items at several indices.

| Parameter | Type | Direction | Description |
|-----------|------|-----------|-------------|
| `SourceListJson` | `string` (JSON array) | Input | The source list. |
| `IndicesToUpdate` | `string` | Input | Comma-separated 0-based indices (e.g. `"1,3,5"`). Negative values count from the end. Whitespace is trimmed; duplicates, invalid entries, and out-of-range indices are skipped. |
| `PropertyName` | `string` | Input | Property path to overwrite. Missing intermediate objects are created; arrays are not — when an array step is missing, that item is left unchanged. |
| `NewValueJson` | `string` (JSON) | Input | The new value as JSON. Falls back to a raw string when the value is not valid JSON. |
| `UpdatedListJson` | `string` (JSON array) | Output | The updated JSON array. |
| `UpdatedCount` | `int` | Output | Number of distinct in-range object items targeted. An item whose write was skipped because an array step was missing is still counted. |

### List_ReplaceWhere

Sets the same property to the same value on every element that matches a multi-condition filter.

| Parameter | Type | Direction | Description |
|-----------|------|-----------|-------------|
| `SourceListJson` | `string` (JSON array) | Input | The source list. |
| `Conditions` | `List<Condition>` | Input | List of `Condition` Structures. An empty list returns the source unchanged with `MatchCount = 0`. |
| `LogicalOperator` | `string` | Input | `AND` (default) or `OR`. |
| `UpdateProperty` | `string` | Input | Property path to overwrite on each matched element. Missing intermediate objects are created; arrays are not — when an array step is missing, that element is left unchanged. |
| `NewValueJson` | `string` (JSON) | Input | The new value as JSON. Falls back to a raw string when the value is not valid JSON. |
| `UpdatedListJson` | `string` (JSON array) | Output | The updated JSON array. |
| `MatchCount` | `int` | Output | Number of matched object elements. An element whose write was skipped because an array step was missing is still counted. |

### List_MinBy

Returns the element with the smallest value at a property path.

| Parameter | Type | Direction | Description |
|-----------|------|-----------|-------------|
| `SourceListJson` | `string` (JSON array) | Input | The source list. |
| `PropertyName` | `string` | Input | Property path to compare on. Supports nested paths and array indexing. |
| `NumericMode` | `bool` | Input | `true` parses values as decimals (`InvariantCulture`) and skips non-numeric values; `false` compares values as case-insensitive ordinal text. |
| `ElementJson` | `string` (JSON) | Output | The full winning element, or `"null"` when no element qualified. Ties keep the first occurrence. |
| `MinValue` | `string` | Output | The raw value at `PropertyName` for the winning element, or `""` when no element qualified. |
| `MinIndex` | `int` | Output | 0-based index of the winning element, or `-1` when no element qualified. |

### List_MaxBy

Returns the element with the largest value at a property path. Same parameters and rules as `List_MinBy`, with outputs `ElementJson`, `MaxValue`, and `MaxIndex`.

| Parameter | Type | Direction | Description |
|-----------|------|-----------|-------------|
| `SourceListJson` | `string` (JSON array) | Input | The source list. |
| `PropertyName` | `string` | Input | Property path to compare on. |
| `NumericMode` | `bool` | Input | `true` = decimal comparison; `false` = case-insensitive ordinal text comparison. |
| `ElementJson` | `string` (JSON) | Output | The full winning element, or `"null"` when no element qualified. Ties keep the first occurrence. |
| `MaxValue` | `string` | Output | The raw winning value, or `""`. |
| `MaxIndex` | `int` | Output | 0-based index of the winning element, or `-1`. |

### List_Aggregate

Calculates a scalar aggregation over a property path.

| Parameter | Type | Direction | Description |
|-----------|------|-----------|-------------|
| `SourceListJson` | `string` (JSON array) | Input | The source list. |
| `PropertyName` | `string` | Input | Property path to aggregate. Supports nested paths and array indexing. |
| `Operation` | `string` | Input | `Sum`, `Avg`, `Min`, `Max`, `Count`, or `CountDistinct` (case-insensitive; use `AggregateOperations`). Empty or unrecognised values are treated as `Sum`. `CountDistinct` compares values case-sensitively. |
| `ResultValue` | `string` | Output | The result as text. Numeric operations return a decimal formatted with `InvariantCulture`; `Count` / `CountDistinct` return an integer. Empty when no value contributed to a numeric operation. |
| `MatchedCount` | `int` | Output | Number of contributing items: non-null values for `Count` / `CountDistinct`, or values parseable as decimals for numeric operations. |

---

## Comparison Operators

The single-condition actions (`List_PopByCondition`, `List_PopMultipleByCondition`, `List_Partition`) and the keyed set actions (`List_Difference`, `List_Intersect`) accept an operator that controls how values are compared. Multi-condition actions (`List_PopByConditions`, `List_PopMultipleByConditions`, `List_PartitionByConditions`, `List_ReplaceWhere`) accept the same operators inside each `Condition` Structure's `Operator` field.

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

`PropertyName`, `MatchKey`, `UpdateProperty`, every `Condition.Path`, and every entry of the key-path lists (`PropertyPaths`, `KeyProperties*`) support both **dot-separated navigation** and **array indexing**:

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
| `List_SplitAt` | Normalize the index, detach every element once, and route each into the left or right array. | O(N) |
| Condition-based pops and partitions (`PopByCondition`, `PopMultipleByCondition`, `Partition`, and the `…ByConditions` variants) | Parse the JSON array, split each property path once, then make a linear scan evaluating the condition(s) per element. | O(N × conditions) |
| `List_Zip` / `List_ZipMany` | Parse every JSON array, iterate to the shortest length, construct paired `JsonObject`s. | O(total elements) |
| `List_GroupBy` / `List_GroupByMultiple` | Single-pass scan building a `Dictionary<string, JsonArray>` keyed by the property value (or the Unit-Separator-joined composite key). Preserves first-seen order. | O(N × keys) |
| `List_ZipGroupBy` / `List_ZipManyGroupBy` / `…GroupByMultiple` variants | One pass per input list into per-list buckets sharing the same key order. | O(total elements × keys) |
| `List_Difference` | Build a `HashSet<string>` from B's key values, then filter A against it. Numeric operators use precomputed `min(B)` / `max(B)`; `StartsWith` / `EndsWith` scan key prefixes / suffixes; `Contains` stays a linear scan. | O(A + B); `Contains` O(A × B) |
| `List_Intersect` | `Equals` uses a `HashSet<string>` of B's keys; every other operator compares each A key against every B key. | O(A + B) for `Equals`; otherwise O(A × B) |
| `List_Union` | Single pass over A then B with a `HashSet<string>` of seen keys (or whole-element JSON when `MatchKey` is empty). | O(A + B) |
| `List_Chunk` / `List_Flatten` | Single pass over the source, sliding a `JsonArray` buffer of `ChunkSize` elements / concatenating each parsed inner array. | O(N) |
| `List_DistinctBy` | Single pass building a `HashSet<string>` on the property-derived key (or full-item JSON when `PropertyName` is empty). First occurrence wins. | O(N) |
| `List_Slice` | Normalise `Start` / `End` / `Step`, then walk the source once with the normalised step (negative walks in reverse). | O(N) |
| `List_Reverse` | Detach elements from the tail and append them to a new array. | O(N) |
| `List_Shuffle` / `List_Sample` | Detach the parsed elements into a buffer and run Fisher-Yates (full for Shuffle; partial over the first `SampleSize` slots for Sample). `Seed == 0` uses `RandomNumberGenerator`; `Seed != 0` uses `System.Random(Seed)`. | O(N) |
| `List_UpdateAt` / `List_UpdateMultipleAt` / `List_ReplaceWhere` | Parse the source, walk `PropertyName` / `UpdateProperty` on each targeted item, set the leaf (auto-creating missing objects), and serialize the parsed array. | O(N × path length) |
| `List_MinBy` / `List_MaxBy` / `List_Aggregate` | Single pass reading the property value from each item and updating the running result. | O(N) |

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
│  ListUtils Action    │ ───► Manipulates the JSON (pop, partition, group, join, update, …)
└──────────────────────┘
       │
       ├───► [UpdatedListJson]  ───► JSON Deserialize → Structure List
       └───► [PoppedElementJson] ──► JSON Deserialize → Structure Record
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
| `Microsoft.Bcl.AsyncInterfaces` | — | 8.0.0 | O11 XIF transitive runtime dependency |
| `System.Buffers` | — | 4.5.1 | O11 XIF transitive runtime dependency |
| `System.Memory` | — | 4.5.5 | O11 XIF transitive runtime dependency |
| `System.Numerics.Vectors` | — | 4.5.0 | O11 XIF transitive runtime dependency |
| `System.Runtime.CompilerServices.Unsafe` | — | 6.0.0 | O11 XIF transitive runtime dependency |
| `System.Text.Encodings.Web` | — | 8.0.0 | O11 XIF transitive runtime dependency |
| `System.Threading.Tasks.Extensions` | — | 4.5.4 | O11 XIF transitive runtime dependency |
| `System.ValueTuple` | — | 4.5.0 | O11 XIF transitive runtime dependency |

All runtime packages are MIT or OutSystems proprietary (SDK only).

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

The checked-in XIF metadata and implementation sources are generated from the active interfaces and O11 implementation. After changing those sources, regenerate them in this order:

1. Build the O11 project in Release so its .NET Framework assemblies are available:
   ```powershell
   dotnet build .\ListUtils.O11\ListUtils.O11.csproj -c Release
   ```
2. Generate the XIF specification and source files from the active O11/ODC interfaces and the Release build output:
   ```powershell
   python .\xif-spec\build_spec.py
   ```
3. Close the XIF in Integration Studio, then use the Integration Studio UI automation workflow to inject the generated specification, sources, and bundled DLLs into the closed `.xif` archive. The generated extracted files on disk are inputs to this workflow, not a substitute for updating the embedded XIF.
4. Open the updated XIF in Integration Studio and use Verify and Save (F4) to compile the generated wrapper. All nine bundled DLLs have `CompileAction=Binaries` and are included for deployment with the XIF.

For the XIF boundary, `Condition` is a Record List with `Path`, `Operator`, `Value`, and `CaseSensitive` fields. Every O11 text-list input or output is represented as a Record List of `TextItem`, whose single Text attribute is `Value`; the generated adapter converts these record lists to or from the handwritten implementation's `List<Condition>` and `List<string>`. This UI-specific record-list representation does not change the ODC interface or handwritten O11 method signatures.

The published v0.7.1 O11 extension was verified at runtime in an O11 Reactive test app on 2026-10-29: all 33 actions passed deterministic regression cases with 5,000-element inputs. See [the O11 release notes](./docs/versions/v0.7.1-o11.md).

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

Runs 1,388 .NET tests (694 ODC net10.0 + 694 O11 net48). The XIF model tests used by the Integration Studio automation (12 Python tests in `.github/skills/integrationstudio-ui-automation/tests/test_xif_model.py` at the workspace root) are run separately.

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
