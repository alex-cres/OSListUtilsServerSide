ListUtilsServerSide - Limitations

ODC Forge "Limitations" field - 1000-character max.
Keep this file under 1000 characters (excluding this header block).

---

JSON parsing - malformed JSON throws a platform error. Validate JSON before calling.
Case sensitivity - string operators honour the caseSensitive flag; numeric operators ignore it.
Numeric operators - parse values as decimal with InvariantCulture. Non-numeric = no match.
Array indexing - supports fixed and negative indices (Items[0], Tags[-1]). Wildcards not supported.
Zip truncation - outputs only as many pairs as the shorter list.
Memory - entire JSON parsed in memory. Very large lists (100k+ elements) may hit platform limits.
No sorting - GroupBy preserves insertion order within groups; groups appear in first-seen order.
Nested conditions - AND/OR combinations are flat only. Nested logic requires chaining action calls.
