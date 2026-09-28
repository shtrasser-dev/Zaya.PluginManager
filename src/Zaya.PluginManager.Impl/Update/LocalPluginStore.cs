using Zaya.PluginManager.Impl.Services;

namespace Zaya.PluginManager.Impl.Update;

/// <summary>Scans and maintains plugin zips on disk.</summary>
public sealed class LocalPluginStore : ILocalPluginStore
{
    private readonly IPluginHostCompatibility _pluginHostCompatibility;
    private readonly IPluginManifestReader _pluginManifestReader;
    private readonly IPluginPaths _pluginPaths;

    public LocalPluginStore(
        IPluginHostCompatibility pluginHostCompatibility,
        IPluginManifestReader pluginManifestReader,
        IPluginPaths pluginPaths)
    {
        _pluginHostCompatibility = pluginHostCompatibility;
        _pluginManifestReader = pluginManifestReader;
        _pluginPaths = pluginPaths;
    }

    public Dictionary<string, PluginManifest> Scan()
    {
        var result = new Dictionary<string, PluginManifest>(StringComparer.OrdinalIgnoreCase);
        var pluginsDirectory = _pluginPaths.GetPluginsDirectory();
        if (!Directory.Exists(pluginsDirectory))
            return result;

        foreach (var zip in Directory.EnumerateFiles(pluginsDirectory, "*.zip"))
        {
            var manifest = _pluginManifestReader.ReadFromZip(zip);
            if (manifest is null)
                continue;
            result[Path.GetFileName(zip)] = manifest;
        }

        return result;
    }

    public void PurgeIncompatibleInterfaces(Dictionary<string, PluginManifest> localState)
    {
        var pluginsDirectory = _pluginPaths.GetPluginsDirectory();
        if (!Directory.Exists(pluginsDirectory))
            return;

        foreach (var zip in Directory.EnumerateFiles(pluginsDirectory, "*.zip"))
        {
            var fileName = Path.GetFileName(zip);
            localState.TryGetValue(fileName, out var manifest);
            if (manifest is null)
                continue;
            if (_pluginHostCompatibility.IsInterfaceCompatible(manifest))
                continue;

            try { File.Delete(zip); }
            catch { /* ignore locked */ }
        }
    }

    public bool IsIncompatibleWithHost(PluginManifest manifest)
        => !_pluginHostCompatibility.IsInterfaceCompatible(manifest);
}
