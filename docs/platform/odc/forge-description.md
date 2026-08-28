# ListUtilsServerSide — OutSystems Forge Description (ODC)

> This file is the source of truth for the component description published on OutSystems Forge.
> Update it whenever the component's behaviour, actions, or interface changes.
> It is versioned alongside the codebase — a copy is kept per release under `docs/versions/`.

---

## Short Description (Forge subtitle — 160 chars max)

Advanced list manipulation utilities — pop by index, pop by condition, zip, group-by, and set difference. Works with any Structure via JSON serialization.

---

## Full Description

### What This Component Does

**ListUtilsServerSide** is a Library that provides seven high-utility list manipulation actions that are absent or cumbersome to implement natively in OutSystems. It covers index-based removal, condition-based filtering, list pairing (zip), grouping, and set difference — all in single server-side calls.

For generic structure support, the JSON-based actions accept any Structure List serialized with `JSON Serialize` and return JSON strings that can be deserialized back with `JSON Deserialize`. This eliminates the need to build separate extensions for each data structure.

---

### Server Actions

#### Index-Based Actions

**List_Pop** — Removes an element at a specific 0-based index from a Text List. Returns the removed element and the updated list. Out-of-range indices return the original list unchanged with an empty popped element.

**List_PopMultiple** — Removes multiple elements at specified indices from a Text List. Indices are processed in reverse-sorted order so earlier removals do not shift the positions of later ones. Out-of-range indices are silently ignored.

#### Condition-Based Actions (JSON)

**List_PopByCondition** — Finds the first object in a JSON array where a named property equals a target value (case-insensitive). Removes and returns it. Supports camelCase fallback for property names.

**List_PopMultipleByCondition** — Same as above but removes ALL matching elements. Returns the list of removed elements as a JSON array.

#### Relational & Set Actions (JSON)

**List_Zip** — Pairs two JSON lists element-by-element into objects with caller-specified key names. Truncates to the shorter list.

**List_GroupBy** — Groups a flat JSON list by a property value. Returns an array of `{Key, Items}` objects in first-seen order.

**List_Difference** — Computes the set difference (A − B) matching on a key property. Runs in O(N) time using a hash set.

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
│  ListUtils Action    │ ───► Manipulates (pop, zip, group, diff)
└──────────────────────┘
       │
       ├───► [updatedListJson]  ───► JSON Deserialize → Structure List
       └───► [poppedElementJson] ──► JSON Deserialize → Structure Record
```

4. For index-based actions (`List_Pop`, `List_PopMultiple`), pass `List of Text` directly — no JSON serialization needed.

---

### Features

- **7 server-side actions** covering the most common list manipulation gaps
- **Generic structure support** via JSON serialization — works with any OutSystems Structure
- **Case-insensitive property matching** with automatic camelCase fallback
- **O(N) performance** for all operations — no nested loops
- **Null-safe** — empty or null inputs return empty results, never exceptions
- **No external dependencies** on ODC — uses built-in `System.Text.Json`
- **Stateless** — no configuration, no site properties, no persistent state
