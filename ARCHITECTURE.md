# OSListUtilsServerSide — Architecture

Structural reference for the solution, the runtime component, and the test
projects. Keep this file in sync with the code — the `documentation-updater`
agent updates it as part of the change cycle whenever the solution layout,
Server Action surface, dependency set, or project structure changes.

> **Runtime behaviour** and **Server Action signatures** live in
> [README.md](./README.md) and the [docs/platform/](./docs/platform) Forge
> copies. This document describes **structure only**.

---

## 1. Solution layout

```
ListUtils.sln
├── ListUtils/                      ← ODC external library (net10.0)
│   ├── ListUtils.csproj
│   ├── IListUtils.cs               ← [OSInterface] declaration (7 actions)
│   ├── ListUtils.cs                ← Implementation (pop logic, JSON manipulation, helpers)
│   └── generate_upload_package.ps1 ← Publishes linux-x64, zips, enforces 90 MB limit
├── ListUtils.Tests/                ← ODC xUnit test suite (net10.0)
│   ├── ListUtils.Tests.csproj
│   ├── Usings.cs                   ← global using Xunit;
│   ├── ListPopTests.cs             ← Index-based pop tests (List_Pop, List_PopMultiple)
│   └── ListJsonTests.cs            ← JSON-based action tests (PopByCondition, Zip, GroupBy, Difference)
├── ListUtils.O11/                  ← O11 extension (net48)
│   ├── ListUtils.O11.csproj
│   ├── IssListUtils.cs             ← O11 interface (Mss-prefixed void methods with out params)
│   └── Actions/
│       └── ListUtilsActions.cs     ← CssListUtils : IssListUtils (full implementation)
└── ListUtils.O11.Tests/            ← O11 xUnit test suite (net48)
    ├── ListUtils.O11.Tests.csproj
    ├── TestHelpers.cs              ← O11 adapter types (IListUtils interface + wrapper class)
    ├── Usings.cs                   ← global using Xunit;
    ├── ListPopTests.cs             ← identical to ODC
    └── ListJsonTests.cs            ← identical to ODC
```

---

## 2. Dependencies

| Package | Version | Platform | License | Purpose |
|---------|---------|----------|---------|---------|
| `OutSystems.ExternalLibraries.SDK` | 1.5.0 | ODC only | OutSystems proprietary | `[OSInterface]`, `[OSAction]`, `[OSParameter]` attributes |
| `System.Text.Json` | 8.0.5 | O11 only | MIT | `JsonNode` / `JsonArray` / `JsonObject` for dynamic JSON manipulation. Built into net10.0 BCL for ODC. |

---

## 3. Naming conventions

| Concern | ODC (`net10.0`) | O11 (`net48`) |
|---------|----------------|---------------|
| Interface | `IListUtils` | `IssListUtils` |
| Implementation | `ListUtils` | `CssListUtils` |
| Namespace | `ListUtils` | `OutSystems.NssListUtils` |
| Method names | `List_Pop`, `List_PopMultiple`, etc. | `MssList_Pop`, `MssList_PopMultiple`, etc. |
| Parameter prefix | *(none)* | `ss` (e.g. `ssSourceList`, `ssIndex`) |

---

## 4. Public surface

| Server Action | Inputs | Outputs | Category |
|---------------|--------|---------|----------|
| `List_Pop` | `List<string>`, `int` | `List<string>`, `string` | Index-based |
| `List_PopMultiple` | `List<string>`, `List<int>` | `List<string>`, `List<string>` | Index-based |
| `List_PopByCondition` | `string` × 3 | `string` × 2 | Condition (JSON) |
| `List_PopMultipleByCondition` | `string` × 3 | `string` × 2 | Condition (JSON) |
| `List_Zip` | `string` × 4 | `string` | Relational (JSON) |
| `List_GroupBy` | `string` × 2 | `string` | Relational (JSON) |
| `List_Difference` | `string` × 3 | `string` | Set (JSON) |

All input/output types are OutSystems-compatible primitives (`string`, `int`, `List<string>`, `List<int>`). No custom structures are exposed.

---

## 5. Implementation patterns

### JSON manipulation (System.Text.Json.Nodes)

The JSON-based actions use the `System.Text.Json.Nodes` API for dynamic manipulation:
- `JsonNode.Parse()` → `AsArray()` for parsing input
- `JsonObject` / `JsonArray` construction for building output
- `ToJsonString()` with `WriteIndented = false` for serialization
- `JsonNode.Parse(item.ToJsonString())` for cloning nodes (required because `JsonNode` tracks its parent)

### Property lookup

`GetPropertyValue(JsonNode, string)` is a shared helper that:
1. Checks the exact property name first
2. Falls back to camelCase variant (first letter lowered via `ToCamelCase`)
3. Returns `null` if neither is found

### Index pop strategy

`List_PopMultiple` reverse-sorts the indices before iterating. This ensures each `RemoveAt` call operates on the correct position regardless of prior removals.

---

## 6. Test strategy

- All test files are byte-for-byte identical between ODC and O11 test projects.
- The O11 test project defines adapter types in `TestHelpers.cs` that map the `Mss`/`ss` O11 signatures back to the ODC method names.
- `new ListUtils()` resolves to the production class in ODC (via project reference) and to the adapter wrapper in O11 (via the local class definition in `TestHelpers.cs`).
- No committed binary test data — all test inputs are inline string literals.

### Test coverage by action

| Action | Tests | Coverage |
|--------|-------|----------|
| `List_Pop` | 6 | Valid index, out-of-range, negative, null, first, last |
| `List_PopMultiple` | 5 | Valid, null indices, empty indices, OOB ignored, null source |
| `List_PopByCondition` | 4 | Match exists, no match, null input, camelCase fallback |
| `List_PopMultipleByCondition` | 2 | Matches all, no match |
| `List_Zip` | 3 | Equal length, unequal (truncation), empty input |
| `List_GroupBy` | 2 | Multiple groups, empty input |
| `List_Difference` | 4 | Removes matches, null B, null A, case-insensitive |

**Total:** 26 tests × 2 platforms = 52 test runs.

---

## 7. Build & package

### ODC

```powershell
.\ListUtils\generate_upload_package.ps1
# Publishes linux-x64, creates ExternalLibrary.zip, checks < 90 MB
```

### O11

```powershell
cd ListUtils.O11
dotnet build -c Release
# Output: ListUtils.O11\bin\Release\net48\ListUtils.dll
```

The O11 DLL is integrated into an Integration Studio extension (XIF) for deployment to Service Center.
