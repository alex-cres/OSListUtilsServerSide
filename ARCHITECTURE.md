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
│   ├── IListUtils.cs               ← [OSInterface] declaration
│   ├── ListUtils.cs                ← Implementation (pop, JSON ops, helpers)
│   └── generate_upload_package.ps1
├── ListUtils.Tests/                ← ODC xUnit test suite (net10.0)
│   ├── ListUtils.Tests.csproj
│   ├── Usings.cs
│   ├── ListPopTests.cs
│   └── ListJsonTests.cs
├── ListUtils.O11/                  ← O11 extension (net48)
│   ├── ListUtils.O11.csproj
│   ├── IssListUtils.cs             ← O11 interface (Mss-prefixed void methods with out params)
│   └── Actions/
│       └── ListUtilsActions.cs     ← CssListUtils : IssListUtils
└── ListUtils.O11.Tests/            ← O11 xUnit test suite (net48)
    ├── ListUtils.O11.Tests.csproj
    ├── TestHelpers.cs              ← O11 adapter types (IListUtils wrapper)
    ├── Usings.cs
    ├── ListPopTests.cs             ← identical to ODC
    └── ListJsonTests.cs            ← identical to ODC
```

## 2. Dependencies

| Package | Version | Platform | Purpose |
|---------|---------|----------|---------|
| `OutSystems.ExternalLibraries.SDK` | 1.5.0 | ODC only | `[OSInterface]`, `[OSAction]`, `[OSParameter]` attributes |
| `System.Text.Json` | 8.0.5 | O11 only | JSON node manipulation (built-in on net10.0) |

## 3. Naming conventions

| Concern | ODC (`net10.0`) | O11 (`net48`) |
|---------|----------------|---------------|
| Interface | `IListUtils` | `IssListUtils` |
| Implementation | `ListUtils` | `CssListUtils` |
| Namespace | `ListUtils` | `OutSystems.NssListUtils` |
| Method prefix | *(none)* | `Mss` |
| Parameter prefix | *(none)* | `ss` |

## 4. Public surface

| Server Action | Parameters |
|---------------|-----------|
| `List_Pop` | `List<string>` + `int` → `List<string>` + `string` |
| `List_PopMultiple` | `List<string>` + `List<int>` → `List<string>` + `List<string>` |
| `List_PopByCondition` | 3 × `string` → 2 × `string` |
| `List_PopMultipleByCondition` | 3 × `string` → 2 × `string` |
| `List_Zip` | 4 × `string` → `string` |
| `List_GroupBy` | 2 × `string` → `string` |
| `List_Difference` | 3 × `string` → `string` |

## 5. Test strategy

All test files are byte-for-byte identical between ODC and O11 test projects.
The O11 test project defines adapter types in `TestHelpers.cs` that map the
`Mss`/`ss` O11 signatures back to the ODC method names, allowing `new ListUtils()`
to work transparently in both projects.
