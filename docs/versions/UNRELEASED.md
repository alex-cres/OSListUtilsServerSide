# Unreleased

Changes in progress — not yet published to OutSystems Forge.

---

## Added

*(nothing yet)*

## Changed

*(nothing yet)*

## Fixed

*(nothing yet)*

## Removed

- Withdrew the fourteen `*InPlace` (Input/Output) variants that were prototyped alongside v0.3.0. ODC Portal upload validation rejects any `[OSAction]` with a C# `ref` parameter (error `OS-ELG-MODL-05016` — "Passing parameters by reference is not supported"), even though the SDK's build-time Roslyn analyzers accept the code. The InPlace approach is therefore unshippable on ODC. Removed files: `ListUtils/ListUtils.InPlace.cs`, `ListUtils.O11/Actions/ListUtilsActions.InPlace.cs`, `ListUtils.Tests/InPlaceTests.cs`, `ListUtils.O11.Tests/InPlaceTests.cs`, plus the 14 InPlace declarations from `IListUtils.cs` / `IssListUtils.cs`, the InPlace adapter methods from `ListUtils.O11.Tests/TestHelpers.cs`, and the 42-test InPlace region from `LoadTests.cs` on both sides. The public surface is back to the fourteen actions shipped in v0.3.0. Consumers should keep using the `MyList = ListUtils.Action(MyList).OutputList` reassignment pattern.
