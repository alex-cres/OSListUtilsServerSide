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
