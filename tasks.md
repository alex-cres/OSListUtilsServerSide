# Future performance tasks

Deferred perf work identified during the v0.7.x optimisation series. Each item is scoped, planned, and priced. Pick from here when new perf pressure appears on the corresponding action family.

---

## Option B — Bytes-in / bytes-out for `List_Pop`, `List_Slice`, `List_UpdateAt`

### Motivation

After v0.7.1, `List_Pop`, `List_Slice`, and `List_UpdateAt` sit at their JSON parse/serialise floor (~6–8 ms per call at 20 000 rows). Every millisecond is spent building a `JsonNode` DOM (`JsonNode.Parse`) and re-emitting it (`ToJsonString`). The algorithms themselves — remove one item, take a slice, mutate one property — are O(1) to O(k). Bypassing the DOM entirely would drop these three actions from ~7 ms to well under 1 ms.

### Plan

1. Add a new helper `Utf8JsonReaderScanner` under `ListUtils.Helpers.cs`.
   - Reads the input string via `Utf8JsonReader` on a `byte[]` (UTF-8 encoded).
   - For each top-level array element, records `(byteStart, byteLength)` into a `List<(int, int)>`.
   - Handles: nested objects / arrays, escaped `"`, escaped `\\`, escaped Unicode `\uXXXX`, whitespace, `null` / `true` / `false` / number literals as top-level array elements.
2. `List_Pop(SourceListJson, Index, out Updated, out Popped)`:
   - Scan → get element ranges.
   - Bounds check `Index`.
   - `Popped = SourceListJson.Substring(range.Start, range.Length)`.
   - `Updated` = the input string with the element and one comma removed. Emit via `StringBuilder` (or `Span<char>` writer) — copy `[start..range.Start]`, `[range.End + comma_span..]`.
3. `List_Slice(SourceListJson, Start, End, Step, out Slice)`:
   - Scan → get element ranges.
   - Apply Python-style slice indexing (start/end/step normalisation is already in place).
   - Emit `[` + comma-joined substrings for selected indices + `]`.
4. `List_UpdateAt(SourceListJson, Index, PropertyName, NewValueJson, out Updated, out Previous)`:
   - Scan top level → find element range.
   - Parse only that element with a nested `Utf8JsonReader` to find `PropertyName` value range.
   - `Previous = element.Substring(prop.Start, prop.Length)`.
   - Emit output = input up to prop start + `NewValueJson` + input from prop end.
   - Fall back to the existing DOM path when the property path contains a `.` (nested paths need the full walker with camelCase fallback and bracket indexing).

### Benefits

- **`List_Pop` at 20 000 rows: ~7 ms → < 1 ms (7–8× improvement).**
- **`List_Slice`: ~7 ms → ~1 ms.**
- **`List_UpdateAt`: ~8 ms → ~1 ms for flat property paths; unchanged for nested paths (they fall back to the DOM path).**
- Allocations drop from ~1.5 MB per call (full DOM + output DOM) to ~1.5 KB (element range list + string builder).
- No effect on other actions.

### Demerits

- **New parser surface**: ~200 lines of `Utf8JsonReader` state machine per bytes-in action. Needs its own dedicated regression tests to cover every JSON edge case: strings containing `[`, `]`, `,`, `\"`, `\\`, `\uXXXX`; nested arrays / objects as elements; whitespace around commas; empty array; single-element array; trailing whitespace.
- **Off-by-one risk**: byte offsets vs UTF-16 char offsets. Input arrives as `string` (UTF-16), reader operates on `byte[]` (UTF-8). Careful conversion required — one strategy is to keep both the UTF-8 byte array and the original UTF-16 string, use the reader to compute byte offsets, then find the equivalent char offsets by re-scanning the array. Slower than pure bytes-in but safer.
- **`List_UpdateAt` fast path only covers flat paths**. Nested property paths (`"Meta.City"`, `"Tags[0]"`, camelCase fallback) still take the DOM path. That's ~50 % of the real-world call sites, so the practical win on `UpdateAt` is smaller than the microbenchmark suggests.
- **Duplication in tests**: existing `List_Pop` / `List_Slice` / `List_UpdateAt` tests validate DOM-emitted JSON bytes. Bytes-in path may emit semantically equivalent but byte-different JSON (e.g. preserved whitespace in the input echoes through to the output). Tests need to be updated to compare parsed structure instead of raw string equality — or the bytes-in emitter must canonicalise whitespace.
- **O11 net48 support**: `Utf8JsonReader` is available on `System.Text.Json 8.0.5` (already referenced by the O11 project), so this works on both platforms. No new dependencies.

### Effort

- **~4 hours** for the parser + three action rewrites.
- **~2 hours** for the new edge-case regression tests (~30 new tests across both platforms).
- **~1 hour** to update existing tests that compare exact JSON bytes.
- One full change cycle + perf validation.

### Risk

**Medium.** The failure mode is silent: a subtle byte-offset bug in the scanner produces JSON that parses correctly but has wrong content. Requires exhaustive edge-case tests.

### Recommendation

Do this when a real caller reports `List_Pop` or `List_Slice` latency as a bottleneck. Not worth the risk of hand-rolled parsing for a theoretical 7 ms win when the current implementation is already well under a hard 300 ms budget.

---

## Option C — `JsonDocument` + `Utf8JsonWriter` on the set-op family

### Motivation

`List_Difference`, `List_Intersect`, and `List_Union` after v0.7.1 sit at 28–33 ms per call. The remaining cost is dominated by:

1. `JsonNode.Parse(arrA)` + `JsonNode.Parse(arrB)` — two full DOMs materialised for ~40 000 rows total (~5 MB of allocation).
2. `result.ToJsonString(JsonOptions)` — the output DOM re-serialised into a fresh string.

`JsonDocument` is the immutable, pooled equivalent of `JsonNode`: 2–3× faster to parse, ~5× lower allocations, and elements can be written directly to a `Utf8JsonWriter` without reconstruction.

### Plan

1. Add a `JsonElement`-based property-path walker: `TryGetPropertyValue(JsonElement element, string[]? segments, out string? value)`.
   - Mirrors the existing `NavigateSegment` walker for `JsonNode`, but operates on `JsonElement` (immutable, struct).
   - Must handle bracket-index syntax, camelCase fallback, and the same nested-object/array navigation.
2. Rewrite each of the three set-op actions:
   - `using var docA = JsonDocument.Parse(ListAJson);` and same for `docB`.
   - Build `bValues`/`bSet` from `docB.RootElement.EnumerateArray()`.
   - `using var buffer = new ArrayBufferWriter<byte>();`
   - `using var writer = new Utf8JsonWriter(buffer);`
   - `writer.WriteStartArray();`
   - Iterate `docA.RootElement.EnumerateArray()`. For each matching element, `element.WriteTo(writer);`
   - `writer.WriteEndArray(); writer.Flush();`
   - `return Encoding.UTF8.GetString(buffer.WrittenSpan);`
3. Preserve the existing operator dispatch (EQUALS, NOTEQUALS, STARTSWITH, ENDSWITH, numeric range fast paths, and the O(A×B) `Contains` slow path).
4. Preserve the null-literal robustness improvement (a bare JSON `null` element must serialise through as `null`, not throw).

### Benefits

- **`List_Difference` at 20 000 rows: ~29 ms → ~14 ms (2× improvement).**
- **`List_Intersect`: ~30 ms → ~15 ms.**
- **`List_Union`: ~33 ms → ~17 ms.**
- Allocation footprint drops from ~3 MB per call to ~500 KB.
- Cumulative improvement vs v0.6.0 baseline goes from 67–74 % to ~85 %.
- No effect on other actions.

### Demerits

- **New immutable path walker**: ~80 lines of `JsonElement`-based logic that must produce byte-identical strings to the existing `JsonNode` walker for `GetPropertyValue`. Case-sensitivity, camelCase fallback, and bracket-index syntax all need to match exactly. Every operator branch (EQUALS, STARTSWITH, ENDSWITH, ≥, ≤, Contains) needs verification.
- **Two parallel walkers to maintain**: the `JsonNode` walker still serves all other actions. Bug fixes and feature additions to path navigation would need to land in both. Alternative: extract a shared segment-parsing primitive (bracket index parsing, camelCase transform) and have both walkers call it. That helps but doesn't eliminate the duplication.
- **`Utf8JsonWriter` write plumbing per action**: ~40 lines of buffer allocation + writer setup + finalisation per action. Extract as a shared helper `WriteFilteredArray(doc, predicate) → string` to reduce duplication.
- **Output whitespace normalisation**: `Utf8JsonWriter` emits canonical JSON with no whitespace. The existing `JsonNode.ToJsonString(JsonOptions)` also emits canonical (WriteIndented=false), so this happens to match. But any downstream test that does string-equality against a hand-crafted expected value must have the same canonical form.
- **`Utf8JsonWriter` requires proper disposal**. Missing `Flush()` or missing `Dispose()` produces truncated output. Compiler warnings help, but this is a new hazard class in the codebase.
- **O11 net48 support**: `JsonDocument`, `Utf8JsonWriter`, and `ArrayBufferWriter<byte>` all exist in `System.Text.Json 8.0.5` — the version already pinned on O11. No new dependencies.

### Effort

- **~3 hours** for the immutable path walker (with tests matching every existing `GetPropertyValue` edge case).
- **~2 hours** for the three action rewrites + shared writer plumbing.
- **~1 hour** to add regression tests for each operator branch × each action.
- One full change cycle + perf validation.

### Risk

**Medium.** The new immutable walker is the highest-risk piece — semantic mismatches with the `JsonNode` walker would silently return wrong keys, causing wrong matches. A property-based test comparing outputs of the two walkers on random inputs would catch this cheaply.

### Recommendation

Do this if `List_Difference` / `List_Intersect` / `List_Union` become a latency bottleneck in production. Given they're already 67–74 % faster than baseline after v0.7.1, this is optimisation for optimisation's sake unless a concrete caller pushes back.

---

## Combined recommendation

Both B and C target focused wins on small subsets of the API surface. They're **complementary** — B helps the index tier, C helps the set-op tier — and could ship together as v0.8.0 if both become relevant. Neither is worth doing pre-emptively; they should wait for a real workload that hits the current v0.7.1 floor.

If forced to pick one, **C has better ROI**: the current v0.7.1 set-op cost (28–33 ms) is meaningful in absolute terms and the change is bounded to three actions and a well-defined immutable walker.
