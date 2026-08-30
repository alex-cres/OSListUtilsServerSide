ListUtilsServerSide - Limitations

ODC Forge "Limitations" field - 1000-character max.
Keep this file under 1000 characters (excluding this header block).

---

JSON parsing - malformed JSON throws a platform error. Validate JSON before calling.
Case sensitivity - string operators honour the caseSensitive flag; numeric operators ignore it.
Numeric operators - parse as decimal with InvariantCulture. Non-numeric = no match.
Array indexing - fixed and negative indices only (Items[0], Tags[-1]). No wildcards.
Zip - truncates to the shorter list. GroupBy - preserves first-seen order within/between groups.
Memory - entire JSON parsed in memory. 100k+ element lists may hit platform limits.
Conditions - AND/OR is flat; nested logic requires chaining action calls.
Slice - End == 0 is a sentinel for "unspecified" (end of list for positive Step, past the beginning for negative Step). Pass an explicit End to bound.
UpdateAt - auto-creates missing objects but NOT missing arrays (short-circuits). PreviousValueJson = "null" cannot distinguish "missing" from "existing JSON null".
Shuffle - Seed == 0 uses a CSPRNG; any non-zero seed is reproducible.
