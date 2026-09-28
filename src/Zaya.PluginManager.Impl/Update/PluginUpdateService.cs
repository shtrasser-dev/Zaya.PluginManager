using Zaya.PluginManager.Impl.Resources;
using Zaya.PluginManager.Impl.Services;

namespace Zaya.PluginManager.Impl.Update;

public sealed class PluginUpdateService : IPluginUpdateService
{
    private readonly IPluginCatalogDownloader _pluginCatalogDownloader;
    private readonly IBuiltinPluginCatalog _builtinPluginCatalog;
    private readonly ILocalPluginStore _localPluginStore;
    private readonly IPluginUiCulture _uiCulture;
    private readonly IPluginManifestReader _pluginManifestReader;
    private readonly IPluginPaths _pluginPaths;

    public PluginUpdateService(
        IPluginCatalogDownloader pluginCatalogDownloader,
        IBuiltinPluginCatalog builtinPluginCatalog,
        ILocalPluginStore localPluginStore,
        IPluginUiCulture uiCulture,
        IPluginManifestReader pluginManifestReader,
        IPluginPaths pluginPaths)
    {
        _pluginCatalogDownloader = pluginCatalogDownloader;
        _builtinPluginCatalog = builtinPluginCatalog;
        _localPluginStore = localPluginStore;
        _uiCulture = uiCulture;
        _pluginManifestReader = pluginManifestReader;
        _pluginPaths = pluginPaths;
    }

    /// <inheritdoc />
    public async Task<PluginUpdateResult> EnsurePluginsAsync(
        string channel,
        bool updateOptional = true,
        bool checkForUpdates = true,
        IProgress<string>? status = null,
        CancellationToken cancellationToken = default)
    {
        var culture = _uiCulture.Current;
        var pluginsDirectory = _pluginPaths.GetPluginsDirectory();
        Directory.CreateDirectory(pluginsDirectory);
        var catalog = _builtinPluginCatalog.Entries;
        var downloaded = new List<string>();

        try
        {
            var localState = _localPluginStore.Scan();
            var missingRequired = catalog.Any(e => e.Required
                && !File.Exists(Path.Combine(pluginsDirectory, e.Asset)));
            var needsBootstrap = localState.Count == 0
                || missingRequired
                || localState.Values.Any(_localPluginStore.IsIncompatibleWithHost);

            if (!checkForUpdates && !needsBootstrap)
            {
                return new PluginUpdateResult
                {
                    Success = true,
                    DownloadedAssets = downloaded,
                };
            }

            if (needsBootstrap)
                return await BootstrapAsync(
                    catalog, channel, updateOptional, downloaded, status, cancellationToken)
                    .ConfigureAwait(false);

            if (!checkForUpdates)
            {
                return new PluginUpdateResult
                {
                    Success = true,
                    DownloadedAssets = downloaded,
                };
            }

            try
            {
                await _pluginCatalogDownloader.UpdateFromReleasesAsync(
                        catalog, channel, updateOptional, downloaded, status, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (RequiredPluginMissingException ex)
            {
                return new PluginUpdateResult
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    DownloadedAssets = downloaded,
                };
            }

            foreach (var entry in catalog.Where(e => e.Required))
            {
                if (!File.Exists(Path.Combine(pluginsDirectory, entry.Asset)))
                {
                    return new PluginUpdateResult
                    {
                        Success = false,
                        ErrorMessage = string.Format(culture, PluginUserStrings.Get("Plugin_RequiredMissing", culture), entry.Asset),
                        DownloadedAssets = downloaded,
                    };
                }
            }

            return new PluginUpdateResult
            {
                Success = true,
                DownloadedAssets = downloaded,
                RequiresRestart = downloaded.Count > 0,
            };
        }
        catch (HttpRequestException ex)
        {
            var hasRequired = catalog.Where(e => e.Required)
                .All(e => File.Exists(Path.Combine(pluginsDirectory, e.Asset)));
            if (hasRequired)
            {
                return new PluginUpdateResult
                {
                    Success = true,
                    ErrorMessage = null,
                    DownloadedAssets = downloaded,
                };
            }

            return new PluginUpdateResult
            {
                Success = false,
                ErrorMessage = string.Format(culture, PluginUserStrings.Get("Plugin_NoNetwork", culture), ex.Message),
                DownloadedAssets = downloaded,
            };
        }
        catch (Exception ex)
        {
            return new PluginUpdateResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                DownloadedAssets = downloaded,
            };
        }
    }

    private async Task<PluginUpdateResult> BootstrapAsync(
        IReadOnlyList<BuiltinPluginEntry> catalog,
        string channel,
        bool updateOptional,
        List<string> downloaded,
        IProgress<string>? status,
        CancellationToken cancellationToken)
    {
        _ = updateOptional;
        var culture = _uiCulture.Current;
        var pluginsDirectory = _pluginPaths.GetPluginsDirectory();
        status?.Report(PluginUserStrings.Get("Plugin_RemovingIncompatible", culture));
        var localState = _localPluginStore.Scan();
        _localPluginStore.PurgeIncompatibleInterfaces(localState);

        foreach (var entry in catalog.Where(e => e.Required))
        {
            status?.Report(string.Format(culture, PluginUserStrings.Get("Plugin_Downloading", culture), entry.Asset));
            await _pluginCatalogDownloader.DownloadCatalogEntryAsync(entry, channel, downloaded, cancellationToken)
                .ConfigureAwait(false);
        }

        var stillMissing = catalog.Where(e => e.Required
            && !File.Exists(Path.Combine(pluginsDirectory, e.Asset))).ToList();

        if (stillMissing.Count > 0)
        {
            return new PluginUpdateResult
            {
                Success = false,
                ErrorMessage = string.Format(
                    culture,
                    PluginUserStrings.Get("Plugin_RequiredInstallFailed", culture),
                    string.Join("\n", stillMissing.Select(e => e.Asset))),
                DownloadedAssets = downloaded,
            };
        }

        foreach (var entry in catalog.Where(e => !e.Required))
        {
            if (File.Exists(Path.Combine(pluginsDirectory, entry.Asset)))
                continue;

            try
            {
                status?.Report(string.Format(culture, PluginUserStrings.Get("Plugin_Downloading", culture), entry.Asset));
                await _pluginCatalogDownloader.DownloadCatalogEntryAsync(
                        entry, channel, downloaded, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                status?.Report(string.Format(
                    culture, PluginUserStrings.Get("Plugin_OptionalSkipped", culture), entry.Asset, ex.Message));
            }
        }

        return new PluginUpdateResult
        {
            Success = true,
            DownloadedAssets = downloaded,
            RequiresRestart = false,
        };
    }

    public PluginManifest? ReadManifestFromZip(string zipPath)
        => _pluginManifestReader.ReadFromZip(zipPath);
}
