# Unreleased

Changes in progress — not yet published to OutSystems Forge.

> When publishing a Forge release, tell the OutSystems Extension Builder agent the version number.

---

## Added

- Added the generated O11 `xif/ListUtils.xif`, extracted source tree, and `xif-spec/build_spec.py` specification/source generator. Opening, saving, and F4 recompilation passed persistence checks; restarting Integration Studio and server deployment remain unverified.
- Bundled eight MIT-licensed transitive .NET dependencies with the O11 XIF so the System.Text.Json runtime dependencies are available with the extension.

## Changed

- Updated the O11 Integration Studio XIF to expose the active 33-action interface. XIF Condition and TextItem Record Lists are adapted to the handwritten O11 `List<T>` API; the XIF compiles in Integration Studio, but server deployment and runtime invocation have not been verified.

## Fixed

*(nothing yet)*

## Removed

*(nothing yet)*

## Security

*(nothing yet)*
