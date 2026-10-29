# O11 Per-Action Timings — ListUtils — 2026-10-29

## Scope

Add millisecond timings to every ListUtils action on the O11 dashboard and on all 33 editable
action screens.

## Measurement method

- `NowMs` client action returns `performance.now()` in the browser.
- Timing starts immediately before the extension or test server action call and stops after the
  response returns.
- Values include network latency, OutSystems request handling, and ListUtils execution on O11.
- Values do not include parsing editable list inputs before the call or browser rendering after
  it.
- OutSystems server date/time functions only expose second-level precision, so these values are
  not pure server CPU time.

## Build

- Added `ElapsedMs` to `TestResult`.
- The dashboard now calls each `Test_List_*` action separately, records each call's elapsed ms,
  and shows `<passed> / <total> passed in <total> ms`.
- Each dashboard row ends with its action timing.
- Each editable action screen shows `Elapsed: <n> ms (browser round trip: network + O11 server)`
  after success or error.
- `RunAllTests` remains available but is no longer used by the dashboard.
- Validation after merge: 0 errors. Publish: success.
- Expected warnings: consecutive server calls from intentional per-action timing; public
  anonymous access as approved for the dedicated test app.

## Measurements

| Surface | Result |
|---------|--------|
| Dashboard | `33 / 33 passed in 10729 ms`; every row showed a millisecond value |
| Dashboard observed range | 168–490 ms per action |
| Editable screens | 33 / 33 outputs matched expectations; all 33 showed timings |
| Editable screen observed range | 164–556 ms per action; median 339 ms |
| `List_ZipMany` example | 193 ms, output unchanged |

Evidence:

- [2026-10-29-dashboard-timings.png](./2026-10-29-dashboard-timings.png)
- [2026-10-29-zipmany-timing.png](./2026-10-29-zipmany-timing.png)

## Verdict

**PASS** — every ListUtils action exposes a browser-measured millisecond timing on both the
dashboard and its editable action screen.
