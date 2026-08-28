# OSListUtilsServerSide — O11 Forge Description

> **Current Forge version:** unreleased

Advanced list manipulation utilities for OutSystems 11. Provides index-based pops, condition-based pops on JSON-serialized lists, zip, group-by, and set difference operations that are absent or cumbersome to implement natively in O11.

## Server Actions

**List_Pop** — Removes an element at a specific index from a string list. Returns the removed element and the updated list.

**List_PopMultiple** — Removes multiple elements at specified indices. Returns the removed elements (in original order) and the updated list.

**List_PopByCondition** — Pops the first element matching a property condition from a JSON-serialized list. Returns the popped element and the modified list as JSON.

**List_PopMultipleByCondition** — Pops all elements matching a property condition from a JSON-serialized list. Returns the popped elements and the modified list as JSON.

**List_Zip** — Combines two JSON lists into paired objects based on matching indexes. Truncates to the shorter list.

**List_GroupBy** — Groups a flat JSON list by a specific property into `{Key, Items}` groups.

**List_Difference** — Computes the set difference (A − B) of two JSON lists, matching on a specified key property.

## Features

- Index-based and condition-based element removal
- Generic structure support via JSON serialization (works with any OutSystems Structure)
- Case-insensitive property matching with camelCase fallback
- Set operations (difference) with O(N) performance

## Installation

Import the extension XIF through Service Center or LifeTime, then reference it in Service Studio.
