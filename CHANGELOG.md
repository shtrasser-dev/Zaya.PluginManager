# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/),
and this project adheres to the versioning scheme in [docs/versioning.md](docs/versioning.md).

## [Unreleased]

## [1.0.0.0] - 2026-08-20

Initial release: host plugin pipeline and GitHub updater extracted from ScreenTranslator.

### Added

- Zip extract cache, directory/assembly load, and engine catalog with pluggable `IPluginEngineProbe`.
- GitHub Releases client, builtin catalog download, and plugin update service.
- Host contracts: `IPluginPaths`, `IPluginHostCompatibility`, `IBuiltinPluginCatalog`, `IPluginUiCulture`.
- DI: `AddZayaPluginManager`.
