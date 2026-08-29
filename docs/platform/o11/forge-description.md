# ListUtilsServerSide — O11 Extension Forge Description

> This file is the source of truth for the O11 extension description published on OutSystems Forge.
> Update it whenever the component's behaviour, actions, or interface changes.
> It is versioned alongside the codebase — a copy is kept per release under `docs/versions/`.

---

## Short Description (Forge subtitle — 160 chars max)

Advanced list manipulation utilities — pop by index, pop by condition, zip, group-by, and set difference. Works with any Structure via JSON serialization.

---

## Full Description

### What This Component Does

**ListUtilsServerSide** is an Extension that provides seven high-utility list manipulation actions that are absent or cumbersome to implement natively in OutSystems 11. It covers index-based removal, condition-based filtering, list pairing (zip), grouping, and set difference — all in single server-side calls.

For generic structure support, the JSON-based actions accept any Structure List serialized with `JSON Serialize` and return JSON strings that can be deserialized back with `JSON Deserialize`. This eliminates the need to build separate extensions for each data structure.

---

### Server Actions

#### Index-Based Actions

**List_Pop** — Removes an element at a specific 0-based index from a JSON list. Returns the removed element and the updated list.

**List_PopMultiple** — Removes multiple elements at specified indices (comma-separated string). Indices are processed in reverse-sorted order so earlier removals do not shift later positions.

#### Condition-Based Actions (JSON)

**List_PopByCondition** — Finds the first object in a JSON array matching a property condition. Supports 9 comparison operators, dot-separated nested paths (e.g. `Address.City`), array indexing (e.g. `Items[0].Name`, `Tags[-1]`), and a `caseSensitive` flag.

**List_PopMultipleByCondition** — Same as above but removes ALL matching elements. Returns the list of removed elements as a JSON array.

**List_PopByConditions** — Multi-condition version. Accepts a JSON array of conditions with per-condition path, operator, value, and caseSensitive fields; combines them with AND (default) or OR logical operator.

**List_PopMultipleByConditions** — Multi-condition version that removes ALL matching elements.

#### Relational & Set Actions (JSON)

**List_Zip** — Pairs two JSON lists element-by-element into objects with caller-specified key names. Truncates to the shorter list.

**List_GroupBy** — Groups a flat JSON list by a property value (nested paths + array indexing supported). Returns an array of `{Key, Items}` objects in first-seen order.

**List_Difference** — Computes the set difference (A − B) matching on a key property. Supports nested paths, array indexing, comparison operators, and case sensitivity.

---

### Comparison Operators

The condition-based actions accept an operator:

| Operator | Behaviour |
|----------|-----------|
| `Equals` (default), `""` | Exact match (per `caseSensitive` flag) |
| `NotEquals`, `!=` | Inverse of Equals |
| `Contains` | Substring match |
| `StartsWith`, `EndsWith` | Prefix / suffix match |
| `GreaterThan`, `>` / `LessThan`, `<` | Numeric comparison |
| `GreaterOrEqual`, `>=` / `LessOrEqual`, `<=` | Numeric with boundary |

The `caseSensitive` boolean (default `false`) toggles case sensitivity for string operators. Numeric operators ignore it.

---

### Property Paths

`PropertyName`, `MatchKey`, and paths inside condition JSON support both dot navigation and array indexing:

```
Address.City             →  obj["Address"]["City"]
Items[0].Name            →  obj["Items"][0]["Name"]
Tags[-1]                 →  last element of Tags
Groups[0].Members[-1]    →  mix of dots and indexing at multiple depths
```

CamelCase fallback is applied at each segment. Negative indices count from the end.

---

### Multiple Conditions

`List_PopByConditions` and `List_PopMultipleByConditions` accept a JSON array of conditions:

```
[
  {"path": "Status", "operator": "Equals", "value": "Active"},
  {"path": "Score", "operator": "GreaterThan", "value": "50"},
  {"path": "Meta.Region", "operator": "Equals", "value": "EU", "caseSensitive": true}
]
```

Combined with `logicalOperator = "AND"` (all must match) or `"OR"` (at least one).

---

### How to Use

1. Import the extension XIF through **Service Center** or **LifeTime**.

2. In **Service Studio**, add **ListUtilsServerSide** as a dependency (Ctrl+Q → search → tick the actions).

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
│  ListUtils Action    │ ───► Manipulates (pop, zip, group, diff)
└──────────────────────┘
       │
       ├───► [updatedListJson]  ───► JSON Deserialize → Structure List
       └───► [poppedElementJson] ──► JSON Deserialize → Structure Record
```

4. For index-based actions (`List_Pop`, `List_PopMultiple`), pass `List of Text` directly — no JSON serialization needed.

---

### Features

- **9 server-side actions** covering the most common list manipulation gaps
- **Generic structure support** via JSON serialization — works with any OutSystems Structure
- **9 comparison operators** (Equals, NotEquals, Contains, StartsWith, EndsWith, GreaterThan, LessThan, GreaterOrEqual, LessOrEqual) with symbol aliases
- **Multiple conditions with AND/OR** — combine any number of conditions per query
- **Case sensitivity toggle** — per-action flag or per-condition field in multi-condition mode
- **Nested property paths + array indexing** — dot navigation, positive/negative indices, mixed at any depth
- **CamelCase fallback** applied at every path segment
- **O(N) performance** for all operations — no nested loops
- **Null-safe** — empty or null inputs return empty results, never exceptions
- **Stateless** — no configuration, no site properties, no persistent state

---

### Installation

Import the extension XIF through Service Center or LifeTime, then reference it in Service Studio. The `System.Text.Json` dependency and its transitive assemblies ship inside the XIF — nothing else needs to be installed on the server.

### Compatibility

- Requires .NET Framework 4.8 on the O11 server.
- Bundles `System.Text.Json` 8.0.5 (MIT license). If another extension on the same server bundles a different version, use a binding redirect in the eSpace web.config.
