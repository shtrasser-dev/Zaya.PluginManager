# Zaya.PluginManager

Host plugin pipeline for the Zaya ecosystem: discover zip plugins, extract, load assemblies, catalog engines via host probes, and update from GitHub Releases.

## Package

| Package | Role | Who references |
|---------|------|----------------|
| **Zaya.PluginManager.Impl** | Pipeline + GitHub updater. Interfaces live in this package until an abstract package exists. | **Host only** |

This is a **host NuGet**, not an AppData plugin zip.

The host must register:

- `IPluginPaths` (plugins + extract directories)
- `IPluginEngineProbe` (one or more)
- `IPluginHostCompatibility`
- `IBuiltinPluginCatalog`
- `IPluginUiCulture`
- `HttpClient`

Then call `services.AddZayaPluginManager(options)`.

See [versioning](docs/versioning.md) and [CHANGELOG](CHANGELOG.md).
