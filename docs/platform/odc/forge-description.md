# ListUtilsServerSide — OutSystems Forge Description (ODC)

> This file is the source of truth for the component description published on OutSystems Forge.
> Update it whenever the component's behaviour, actions, or interface changes.
> It is versioned alongside the codebase — a copy is kept per release under `docs/versions/`.
>
> **Current version:** 0.7.2 (29 October 2026) — see [v0.7.2](../../versions/v0.7.2.md)  
> **O11 version:** https://www.outsystems.com/forge/component-overview/25935/listutils-o11

---

## Short Description (Forge subtitle — 160 chars max)

33 server actions to pop, filter, partition, group, join, compare, slice, sample, update, aggregate, chunk and flatten lists. Any Structure via JSON.

---

## Full Description

### What This Component Does

**ListUtilsServerSide** is a Library that provides thirty-three high-utility list manipulation actions that are absent or cumbersome to implement natively in OutSystems. It covers index-based removal, condition-based filtering (single and multi-condition with AND/OR), partition, split, list pairing (zip), grouping (single-key, N-key composite, cogroup across 2 or M lists), set operations (difference, intersect, union), aggregation (min-by, max-by, sum/avg/min/max/count/count-distinct), chunking, distinct-by, Python-style slicing, shuffling, reverse, flatten, sampling, replace-where, and in-place property updates — all in single server-side calls.

For generic structure support, the JSON-based actions accept any Structure List serialized with `JSON Serialize` and return JSON strings that can be deserialized back with `JSON Deserialize`. This eliminates the need to build separate extensions for each data structure.

---

### Server Actions

#### Index-Based Actions

**List_Pop** — Removes an element at a specific 0-based index from a JSON list (negative indices count from the end). Returns the removed element and the updated list.

**List_PopMultiple** — Removes multiple elements at specified indices (comma-separated string; negative indices count from the end). Indices are processed in reverse-sorted order so earlier removals do not shift later positions.

**List_SplitAt** — Splits a list into left and right lists at an index. Negative indices count from the end.

#### Condition-Based Actions (JSON)

**List_PopByCondition** — Finds the first object in a JSON array matching a property condition. Supports 9 comparison operators, dot-separated nested paths (e.g. `Address.City`), array indexing (e.g. `Items[0].Name`, `Tags[-1]`), and a `CaseSensitive` flag.

**List_PopMultipleByCondition** — Same as above but removes ALL matching elements. Returns the list of removed elements as a JSON array.

**List_PopByConditions** — Multi-condition version. Accepts a list of `Condition` Structures (each with `Path`, `Operator`, `Value`, `CaseSensitive`) and combines them with AND (default) or OR logical operator. No JSON hand-authoring — build the list in your Server Action.

**List_PopMultipleByConditions** — Multi-condition version that removes ALL matching elements.

**List_Partition** — Splits a list into matching and non-matching lists using one property condition. Nothing is removed — both sides are returned.

**List_PartitionByConditions** — Splits a list into matching and non-matching lists using multiple AND/OR conditions.

#### Relational & Set Actions (JSON)

**List_Zip** — Pairs two JSON lists element-by-element into objects with caller-specified key names. Truncates to the shorter list.

**List_GroupBy** — Groups a flat JSON list by a property value (nested paths + array indexing supported). Returns an array of `{Key, Items}` objects in first-seen order.

**List_GroupByMultiple** — N-key generalisation of `List_GroupBy`. Groups a JSON list by an **ordered list of property paths** (composite key). Each output group emits one field per key (labelled by `KeyNames[i]`, defaulting to `Key0`, `Key1`, …) plus the items array (`ItemsFieldName`, defaulting to `Items`). Composite keys are joined internally with the ASCII Unit Separator so distinct tuples never collide.

**List_ZipGroupBy** — Cogroups two lists by a shared key. Each group holds `Key` plus two named arrays (`KeyNameA` / `KeyNameB`) of the items from each list sharing that key.

**List_ZipGroupByMultiple** — N-key generalisation of the two-list cogroup. Cogroups two lists by an ordered list of property paths per side (both sides must supply the same number of paths). Each output group holds one field per key column plus two named arrays (`KeyNameA` / `KeyNameB`).

**List_ZipMany** — Pairs any number of lists by position (takes a List of Text of JSON arrays). Output length is the shortest input.

**List_ZipManyGroupBy** — Cogroups any number of lists by one key path per list.

**List_ZipManyGroupByMultiple** — M-list, N-key cogroup. Generalises `List_ZipManyGroupBy` to composite keys. `KeyProperties` is a flat list in list-major order — entries `[i*KeyCount .. i*KeyCount + KeyCount - 1]` supply the key paths for `ListsJson[i]`. Missing key values collapse into the shared `"Unknown"` bucket per key column.

**List_Difference** — Computes the set difference (A − B) matching on a key property. Supports nested paths, array indexing, comparison operators, and case sensitivity.

**List_Intersect** — Computes the set intersection (A ∩ B) on a key property, preserving A's order.

**List_Union** — Concatenates A and B, then removes duplicates by key (first occurrence wins). An empty key compares whole elements.

#### Transformation & Randomization Actions (JSON)

**List_Chunk** — Splits a JSON list into fixed-size chunks returned as a List of Text, where each entry is a standalone JSON array ready for `JSON Deserialize`. The last chunk may be smaller. `ChunkSize <= 0` or an empty source returns an empty list. Ideal for batching API payloads.

**List_Flatten** — Concatenates a List of Text of JSON arrays back into one JSON array. Inverse of `List_Chunk`.

**List_DistinctBy** — Filters to unique elements by a property key (first occurrence wins). Empty property dedupes on the entire item. Works on structures — native ODC `Distinct` only supports basic types.

**List_Slice** — Python/JavaScript-style slicing with `Start`, `End`, `Step`. Negative indices count from the end. `End == 0` is a sentinel meaning "to end of list" (forward step) or "past the beginning" (backward step). Negative `Step` reverses the walk.

**List_Shuffle** — Randomises order via Fisher-Yates. `Seed == 0` uses a cryptographically-seeded RNG (`RandomNumberGenerator`); any non-zero seed produces a reproducible permutation. Source list is not mutated.

**List_Reverse** — Reverses a list.

**List_Sample** — Returns a random subset without replacement, with the same seed rules as `List_Shuffle`.

#### Update Actions (JSON)

**List_UpdateAt** — Sets a property of the item at a given index and returns the previous value. Negative indices count from the end. Nested paths are supported; missing intermediate objects are created (arrays are not auto-created).

**List_UpdateMultipleAt** — Applies the same property update at multiple comma-separated indices and returns how many items were updated.

**List_ReplaceWhere** — Applies the same property update to every element matching AND/OR `Condition` entries and returns the match count.

#### Aggregation Actions (JSON)

**List_MinBy** / **List_MaxBy** — Return the element with the smallest / largest value at a property path, plus the value and its index. Numeric or text comparison.

**List_Aggregate** — Calculates `Sum`, `Avg`, `Min`, `Max`, `Count`, or `CountDistinct` over a property path.

---

### Comparison Operators

The condition-based actions accept an operator:

| Operator | Behaviour |
|----------|-----------|
| `Equals` (default), `""` | Exact match (per `CaseSensitive` flag) |
| `NotEquals`, `!=` | Inverse of Equals |
| `Contains` | Substring match |
| `StartsWith`, `EndsWith` | Prefix / suffix match |
| `GreaterThan`, `>` / `LessThan`, `<` | Numeric comparison |
| `GreaterOrEqual`, `>=` / `LessOrEqual`, `<=` | Numeric with boundary |

The `CaseSensitive` boolean (default `false`) toggles case sensitivity for string operators. Numeric operators ignore it.

---

### Property Paths

`PropertyName`, `MatchKey`, `UpdateProperty`, every `Condition.Path`, and every key-path list entry support both dot navigation and array indexing:

```
Address.City             →  obj["Address"]["City"]
Items[0].Name            →  obj["Items"][0]["Name"]
Tags[-1]                 →  last element of Tags
Groups[0].Members[-1]    →  mix of dots and indexing at multiple depths
```

CamelCase fallback is applied at each segment. Negative indices count from the end.

---

### Multiple Conditions

`List_PopByConditions`, `List_PopMultipleByConditions`, `List_PartitionByConditions`, and `List_ReplaceWhere` accept a `Condition` Structure list. Each entry:

```
Condition {
  Path          = "Status"       // property path (nested + array indexing)
  Operator      = Operators.Equals
  Value         = "Active"       // target value as text
  CaseSensitive = false          // optional, default false
}
```

Combine any number of entries and pass `LogicalOperator = LogicalOperators.AND` (all must match) or `LogicalOperators.OR` (at least one). Passing an empty list returns the source unchanged.

### Constant classes

Operator strings are exposed as compile-time constants — no more magic strings:

- `Operators` — `Equals`, `NotEquals`, `Contains`, `StartsWith`, `EndsWith`, `GreaterThan`, `LessThan`, `GreaterOrEqual`, `LessOrEqual`
- `LogicalOperators` — `AND`, `OR`
- `AggregateOperations` — `Sum`, `Avg`, `Min`, `Max`, `Count`, `CountDistinct` (used by `List_Aggregate`)

Symbol aliases (`!=`, `>`, `<`, `>=`, `<=`) are still accepted for backwards compatibility.

---

### Search Direction

`List_PopByCondition` and `List_PopByConditions` accept a `SearchFromEnd` boolean:

- `false` (default) — pops the FIRST match, iterating from the beginning
- `true` — pops the LAST match, iterating from the end

`List_PopMultipleByCondition` and `List_PopMultipleByConditions` always pop every match, so they do not need this flag.

---

### How to Use

1. Upload the component ZIP to **ODC Portal → External Logic** and publish it as an External Library.

2. In your ODC application, add **ListUtilsServerSide** as a dependency.

3. In any Server Action:

```
[Your Structure List]
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

4. For index-based actions (`List_Pop`, `List_PopMultiple`), serialize your `List of Text` (or any Structure List) with `JSON Serialize` first — every action consumes and returns JSON strings.

---

### Features

- **33 server-side actions** covering the most common list manipulation gaps
- **Generic structure support** via JSON serialization — works with any OutSystems Structure
- **First-class `Condition` Structure** for multi-condition filtering — no hand-authored JSON
- **9 comparison operators** (Equals, NotEquals, Contains, StartsWith, EndsWith, GreaterThan, LessThan, GreaterOrEqual, LessOrEqual) with symbol aliases
- **`Operators`, `LogicalOperators`, `AggregateOperations` constant classes** — IDE autocomplete and compile-time typo detection instead of magic strings
- **Multiple conditions with AND/OR** — combine any number of conditions per query
- **Case sensitivity toggle** — per-action flag or per-condition field in multi-condition mode
- **Nested property paths + array indexing** — dot navigation, positive/negative indices, mixed at any depth
- **CamelCase fallback** applied at every path segment
- **Linear-time core operations** — most actions are a single O(N) pass; `List_Difference` with `Contains` and `List_Intersect` with any operator other than `Equals` compare every A key with every B key
- **Null-safe** — null or empty inputs return empty results instead of exceptions (malformed JSON still raises an error)
- **No external dependencies** on ODC — uses built-in `System.Text.Json`
- **Stateless** — no configuration, no site properties, no persistent state
