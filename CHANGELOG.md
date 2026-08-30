# Changelog

All notable changes to **OSListUtilsServerSide** are documented here.

Versions correspond to releases published on the [OutSystems Forge](https://www.outsystems.com/forge/). A version number is only assigned when the user explicitly publishes a release to Forge. All in-progress work is tracked under [Unreleased](./docs/versions/UNRELEASED.md).

---

## Versions

| Version | Platform | Date | Notes |
|---------|----------|------|-------|
| [Unreleased](./docs/versions/UNRELEASED.md) | Both | — | In-progress changes not yet published to Forge |
| [v0.4.0](./docs/versions/v0.4.0.md) | Both | 2026-08-30 | 14 in-place (Input/Output) variants of every existing action — 28 Server Actions total. Consumers can mutate their list variable directly without reassignment. |
| [v0.3.0](./docs/versions/v0.3.0.md) | Both | 2026-08-30 | Five new transformation / randomization actions: `List_Chunk`, `List_DistinctBy`, `List_Slice`, `List_Shuffle`, `List_UpdateAt` — 14 Server Actions total. |
| [v0.2.0](./docs/versions/v0.2.0.md) | Both | 2026-08-29 | Multi-condition action family (`List_PopByConditions`, `List_PopMultipleByConditions`), `SearchFromEnd` flag, array indexing in paths, PascalCase parameter rename, partial-class refactor, `List_Difference` fast paths — 9 Server Actions total. |
| [v0.1.0](./docs/versions/v0.1.0.md) | Both | 2026-08-28 | Initial scaffold — 7 Server Actions (`List_Pop`, `List_PopMultiple`, `List_PopByCondition`, `List_PopMultipleByCondition`, `List_Zip`, `List_GroupBy`, `List_Difference`) with nine comparison operators, nested property paths, case-sensitivity flag, and the ODC (net10.0) + O11 (net48) baseline. |

---

*When a Forge release is published, provide the version number to create `docs/versions/v{x.y.z}.md`, add the entry to this table, and reset the Unreleased file.*
