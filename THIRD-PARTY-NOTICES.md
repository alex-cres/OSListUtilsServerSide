# Third-Party Notices

This file lists the open-source packages used by **OSListUtilsServerSide** at runtime, along with their licenses and source locations. Test-only packages are excluded.

---

## Direct Runtime Dependencies

### OutSystems.ExternalLibraries.SDK

- **Version:** 1.5.0
- **License:** OutSystems proprietary (required to build and publish OutSystems ODC External Libraries)
- **Source:** https://www.nuget.org/packages/OutSystems.ExternalLibraries.SDK
- **Used by:** ODC project only (`ListUtils/`)

### System.Text.Json

- **Version:** 8.0.5
- **License:** MIT
- **Source:** https://www.nuget.org/packages/System.Text.Json
- **Used by:** O11 project only (`ListUtils.O11/`) — built-in on .NET 10 for the ODC project

## Transitive Runtime Dependencies

The O11 XIF bundles the following MIT-licensed dependencies resolved for `System.Text.Json` 8.0.5. Each DLL is included in the XIF as a runtime binary.

### Microsoft.Bcl.AsyncInterfaces

- **Version:** 8.0.0
- **License:** MIT
- **Source:** https://github.com/dotnet/runtime
- **NuGet:** https://www.nuget.org/packages/Microsoft.Bcl.AsyncInterfaces/8.0.0
- **Used by:** O11 XIF — async interface compatibility required by the System.Text.Json dependency graph.

### System.Buffers

- **Version:** 4.5.1
- **License:** MIT
- **Source:** https://github.com/dotnet/runtime
- **NuGet:** https://www.nuget.org/packages/System.Buffers/4.5.1
- **Used by:** O11 XIF — buffer primitives required by System.Text.Json dependencies.

### System.Memory

- **Version:** 4.5.5
- **License:** MIT
- **Source:** https://github.com/dotnet/runtime
- **NuGet:** https://www.nuget.org/packages/System.Memory/4.5.5
- **Used by:** O11 XIF — memory/span APIs required by System.Text.Json dependencies on .NET Framework.

### System.Numerics.Vectors

- **Version:** 4.5.0
- **License:** MIT
- **Source:** https://github.com/dotnet/runtime
- **NuGet:** https://www.nuget.org/packages/System.Numerics.Vectors/4.5.0
- **Used by:** O11 XIF — vector primitives required by System.Memory.

### System.Runtime.CompilerServices.Unsafe

- **Version:** 6.0.0
- **License:** MIT
- **Source:** https://github.com/dotnet/runtime
- **NuGet:** https://www.nuget.org/packages/System.Runtime.CompilerServices.Unsafe/6.0.0
- **Used by:** O11 XIF — low-level runtime support required by System.Memory and System.Text.Json dependencies.

### System.Text.Encodings.Web

- **Version:** 8.0.0
- **License:** MIT
- **Source:** https://github.com/dotnet/runtime
- **NuGet:** https://www.nuget.org/packages/System.Text.Encodings.Web/8.0.0
- **Used by:** O11 XIF — text escaping and encoding support for System.Text.Json.

### System.Threading.Tasks.Extensions

- **Version:** 4.5.4
- **License:** MIT
- **Source:** https://github.com/dotnet/runtime
- **NuGet:** https://www.nuget.org/packages/System.Threading.Tasks.Extensions/4.5.4
- **Used by:** O11 XIF — task and ValueTask compatibility required by Microsoft.Bcl.AsyncInterfaces and System.Text.Json.

### System.ValueTuple

- **Version:** 4.5.0
- **License:** MIT
- **Source:** https://github.com/dotnet/runtime
- **NuGet:** https://www.nuget.org/packages/System.ValueTuple/4.5.0
- **Used by:** O11 XIF — tuple type support required by System.Text.Json on .NET Framework.
