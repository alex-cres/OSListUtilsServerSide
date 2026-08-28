ListUtilsServerSide - Limitations

ODC Forge "Limitations" field - 1000-character max.
Keep this file under 1000 characters (excluding this header block).

---

JSON parsing - malformed JSON input throws a platform error. Validate JSON before calling JSON-based actions.
String lists only - List_Pop and List_PopMultiple operate on List of Text. Use JSON-based actions for other data types.
No deep matching - property matching is single-level only. Nested properties require a compound key in the JSON structure.
Case-insensitive matching - PopByCondition and Difference match property values case-insensitively. Exact-case matching is not available.
Zip truncation - List_Zip outputs only as many pairs as the shorter list. Unmatched trailing elements are silently dropped.
Memory - entire JSON string is parsed in memory. Very large lists (100k+ elements) may hit platform memory limits.
No sorting - GroupBy preserves insertion order within groups. The groups themselves appear in first-seen order, not sorted.
