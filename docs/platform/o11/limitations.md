ListUtilsServerSide - O11 Limitations

O11 Forge "Limitations" field - 1000-character max.
Keep this file body under 1000 characters (excluding this header block).

All limitations listed in docs/platform/odc/limitations.md apply here as well.
This file lists limitations that are O11-only.

---

System.Text.Json version - the O11 extension bundles System.Text.Json 8.0.5. If the O11 server has a different version of this assembly loaded (e.g. from another extension), assembly binding conflicts may occur. Use a binding redirect in the eSpace web.config if needed.
.NET Framework 4.8 required - the extension targets net48. Older O11 environments on .NET 4.6.1 or 4.7 are not supported.
