ListUtilsServerSide - Limitations

ODC Forge "Limitations" field - 1000-character max.
Keep this file under 1000 characters (excluding this header block).

---

JSON parsing - malformed JSON input throws a platform error. Validate JSON before calling JSON-based actions.
Case-insensitive matching - all comparison operators are case-insensitive. Exact-case matching is not available.
Numeric operators - GreaterThan, LessThan, and boundary variants parse both values as decimal with InvariantCulture. Non-numeric values evaluate as no-match.
Nested paths - dot-separated paths walk into nested objects but do not support array indexing (e.g. Items[0].Name is not supported).
Zip truncation - List_Zip outputs only as many pairs as the shorter list. Unmatched trailing elements are silently dropped.
Memory - entire JSON string is parsed in memory. Very large lists (100k+ elements) may hit platform memory limits.
No sorting - GroupBy preserves insertion order within groups. The groups themselves appear in first-seen order, not sorted.
