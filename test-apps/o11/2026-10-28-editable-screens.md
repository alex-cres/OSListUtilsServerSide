# O11 Editable Action Screens — ListUtils — 2026-10-28

## Scope

Add one editable runtime screen for every ListUtils action, following the input/run/results
pattern used by `StegoGuardTester` and `FileMetadataStripTester`.

## Environment

| Field | Value |
|-------|-------|
| Server | `alexandre-crespo-realinho.outsystemscloud.com` |
| Application / module | `ListUtilsTester` / `ListUtilsTester` |
| Dashboard | `https://alexandre-crespo-realinho.outsystemscloud.com/ListUtilsTester/ListUtilsTests` |
| Screen URL pattern | `https://alexandre-crespo-realinho.outsystemscloud.com/ListUtilsTester/<Action>Test` |
| Access | Anonymous and Registered, approved by the user for this dedicated test app |
| Browser | VS Code integrated browser (Playwright) |
| Date | 2026-10-28 |

## Build

- Added 33 screens, one per action: `PopTest`, `PopMultipleTest`, `PopByConditionTest`, …,
  `ZipManyGroupByMultipleTest`.
- Each screen has a card with the action description and labeled inputs, prefilled with the
  regression sample, plus **Run** and **Reset sample** buttons.
- **Run** calls the ListUtils extension action directly from a screen action. On success the
  Results card shows every output; on failure it shows the exception message.
- Text inputs use input boxes or text areas, Integer uses number inputs, Boolean uses checkboxes,
  and operator/operation parameters use radio groups.
- `TextItem` list inputs use one value per line; `Condition` list inputs use a JSON array.
- Added `QC` (client JSON-literal decoder) and `LinesToTextItems` (lines → `TextItem` list).
- Added links to all 33 screens on the `ListUtilsTests` dashboard; each screen links back.
- Validation after merge: 0 errors. Publish: success.

## Measurements

| Check | Result |
|-------|--------|
| Default sample on each editable screen | 33 / 33 outputs matched the O11 implementation expectations |
| Custom input: `PopTest`, `Index` changed 1 → 0 | Popped `Alice`; remaining list was Bob and Cara |
| Reset sample on `PopTest` | `Index` restored to 1 and stale results hidden |
| Dashboard regression run | `33 / 33 passed` |
| Dashboard links | 33 links, 33 distinct screens |

`List_Chunk` is displayed as the serialized `TextItem` record list, for example
`[{"TextItem":{"Value":"[1,2]"}}, …]`.

Evidence:

- [2026-10-28-pop-custom-input.png](./2026-10-28-pop-custom-input.png)
- [2026-10-28-dashboard-links.png](./2026-10-28-dashboard-links.png)

## Verdict

**PASS** — all 33 ListUtils actions have a dedicated editable runtime screen, all default samples
match expectations, and custom input changes reach the published extension.
