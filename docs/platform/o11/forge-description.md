# ListUtils O11 — O11 Extension Forge Description

> **Forge:** https://www.outsystems.com/forge/component-overview/25935/listutils-o11  
> **Current Forge version:** 0.7.1 (29 October 2026)  
> **Platform:** OutSystems 11
>
> This file is the source of truth for the O11 extension description published on OutSystems Forge.
> Update it whenever the component's behaviour, actions, or interface changes.

---

## Short Description (Forge subtitle — 160 chars max)

33 server actions to pop, filter, partition, group, join, compare, slice, sample, update, aggregate, chunk and flatten JSON lists in OutSystems 11.

---

## Full Description

### What This Component Does

**ListUtils O11** is an OutSystems 11 Integration Studio extension with 33 server actions for list manipulation that is missing or cumbersome in native OutSystems logic.

It works with any Structure, Entity record, or JSON payload: serialize a Record List with **JSON Serialize**, call a ListUtils action, then deserialize the returned JSON with **JSON Deserialize**.

The extension exposes two structures:

- `Condition` — `Path`, `Operator`, `Value`, `CaseSensitive`, used by multi-condition actions.
- `TextItem` — one Text `Value`, used for lists of property paths, key names, labels, and JSON-array strings.

---

### Server Actions

#### Index Actions

- **List_Pop** — Removes the element at a 0-based index and returns it with the updated list.
- **List_PopMultiple** — Removes elements at comma-separated indices.
- **List_SplitAt** — Splits a list into left and right lists at an index.

#### Condition Actions

- **List_PopByCondition** — Removes the first or last element matching one property condition.
- **List_PopMultipleByCondition** — Removes every element matching one property condition.
- **List_PopByConditions** — Removes the first or last element matching multiple AND/OR conditions.
- **List_PopMultipleByConditions** — Removes every element matching multiple AND/OR conditions.
- **List_Partition** — Splits a list into matching and non-matching lists using one condition.
- **List_PartitionByConditions** — Splits a list into matching and non-matching lists using multiple AND/OR conditions.

#### Grouping, Join and Set Actions

- **List_Zip** — Pairs two lists by position.
- **List_GroupBy** — Groups elements by one property path.
- **List_GroupByMultiple** — Groups elements by a composite key built from multiple property paths.
- **List_ZipGroupBy** — Cogroups two lists by a shared key.
- **List_ZipGroupByMultiple** — Cogroups two lists by a composite key.
- **List_ZipMany** — Pairs any number of lists by position.
- **List_ZipManyGroupBy** — Cogroups any number of lists by one key per list.
- **List_ZipManyGroupByMultiple** — Cogroups any number of lists by composite keys.
- **List_Difference** — Returns elements in list A with no matching key in list B.
- **List_Intersect** — Returns elements in list A with a matching key in list B.
- **List_Union** — Combines A and B, then removes duplicates by key.

#### Transformation Actions

- **List_Chunk** — Splits a list into fixed-size JSON-array chunks returned as a TextItem Record List.
- **List_Flatten** — Combines JSON-array chunks back into one list.
- **List_DistinctBy** — Keeps the first element for each unique property value.
- **List_Slice** — Extracts a subset with Python-style Start, End and Step.
- **List_Reverse** — Reverses a list.
- **List_Shuffle** — Randomizes order; a non-zero seed gives repeatable output.
- **List_Sample** — Returns a random subset without replacement; a non-zero seed gives repeatable output.

#### Update Actions

- **List_UpdateAt** — Updates one property on one element and returns the previous value.
- **List_UpdateMultipleAt** — Applies the same property update at multiple indices.
- **List_ReplaceWhere** — Applies the same property update to every element matching AND/OR conditions.

#### Aggregation Actions

- **List_MinBy** — Returns the element with the smallest value at a property path.
- **List_MaxBy** — Returns the element with the largest value at a property path.
- **List_Aggregate** — Calculates Sum, Avg, Min, Max, Count, or CountDistinct over a property path.

---

### Comparison Operators

Use these Text values in `ComparisonOperator` and `Condition.Operator`:

| Operator | Behaviour |
|----------|-----------|
| `Equals` or empty | Exact match |
| `NotEquals` or `!=` | Not equal |
| `Contains` | Text contains the value |
| `StartsWith` | Text starts with the value |
| `EndsWith` | Text ends with the value |
| `GreaterThan` or `>` | Numeric greater than |
| `LessThan` or `<` | Numeric less than |
| `GreaterOrEqual` or `>=` | Numeric greater than or equal |
| `LessOrEqual` or `<=` | Numeric less than or equal |

`LogicalOperator` accepts `AND` or `OR`; empty means `AND`. `List_Aggregate` accepts `Sum`, `Avg`, `Min`, `Max`, `Count`, or `CountDistinct`.

---

### Property Paths

Property inputs and `Condition.Path` support dot navigation and array indexing:

```
Address.City
Items[0].Name
Tags[-1]
Groups[0].Members[-1].Email
```

Negative indices count from the end. Each name segment also tries a camelCase fallback.

---

### How to Use

1. Install **ListUtils O11** from Forge.
2. In Service Studio, open **Manage Dependencies**, select the **ListUtils** extension, and tick the actions and structures you need.
3. Serialize the source Record List with **JSON Serialize**.
4. Call the ListUtils action.
5. Deserialize the returned JSON with **JSON Deserialize**.

For multi-condition actions, build a `Condition` Record List. For path, key-name, label, and multi-list inputs, build a `TextItem` Record List.

---

### Features

- **33 server actions** for common list-processing gaps in OutSystems 11.
- **Any data shape** through JSON Serialize and JSON Deserialize.
- **Typed `Condition` records** for multi-condition filtering and updates.
- **AND/OR conditions** with nine comparison operators.
- **Nested property paths and array indexing**, including negative indices.
- **Composite keys and multi-list cogrouping**.
- **Deterministic shuffle and sample** with non-zero seeds.
- **Stateless** — no Site Properties, database tables, or configuration screens.

---

### Verified Runtime

All 33 actions were verified in a published O11 Reactive test app on 29 October 2026. The regression dashboard used deterministic inputs with 5,000 elements in every data list and passed 33 of 33 cases.

---

### Installation

Install from Forge, or upload the XIF through Service Center or LifeTime and publish it. System.Text.Json 8.0.5 and its eight .NET Framework support assemblies ship inside the extension.

### Compatibility

- Requires an OutSystems 11 environment running .NET Framework 4.8.
- Bundles System.Text.Json 8.0.5 and its runtime support assemblies.
- If another extension loads incompatible versions of the same assemblies, configure binding redirects.
