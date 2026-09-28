using System.Reflection;
using Zaya.Logging.Models;
using Zaya.PluginManager.Impl.Services;

namespace Zaya.PluginManager.Impl.Services.Impl;

/// <summary>
/// Orchestrates zip extract and plugin directory load.
/// </summary>
public sealed class PluginLoader : IPluginLoader
{
    private readonly IPluginCatalog _pluginCatalog;
    private readonly IPluginZipDirectoryScanner _pluginZipDirectoryScanner;
    private readonly IPluginDirectoryScanner _pluginDirectoryScanner;
    private readonly IPluginAssemblyLoader _pluginAssemblyLoader;
    private readonly IPluginPaths _pluginPaths;

    public PluginLoader(
        IPluginCatalog pluginCatalog,
        IPluginZipDirectoryScanner pluginZipDirectoryScanner,
        IPluginDirectoryScanner pluginDirectoryScanner,
        IPluginAssemblyLoader pluginAssemblyLoader,
        IPluginPaths pluginPaths)
    {
        _pluginCatalog = pluginCatalog;
        _pluginZipDirectoryScanner = pluginZipDirectoryScanner;
        _pluginDirectoryScanner = pluginDirectoryScanner;
        _pluginAssemblyLoader = pluginAssemblyLoader;
        _pluginPaths = pluginPaths;
    }

    public IReadOnlyList<Assembly> LoadedAssemblies => _pluginAssemblyLoader.LoadedAssemblies;

    [Log(LogLevel.Debug)]
    public void LoadPlugins()
    {
        var plugins = _pluginPaths.GetPluginsDirectory();
        if (!Directory.Exists(plugins))
            return;

        _pluginCatalog.Clear();
        Directory.CreateDirectory(_pluginPaths.GetExtractedPluginsDirectory());
        _pluginAssemblyLoader.Process();
        _pluginZipDirectoryScanner.Process();
        _pluginDirectoryScanner.Process();

        foreach (var dll in Directory.EnumerateFiles(plugins, "*.dll"))
            _pluginAssemblyLoader.TryLoad(dll);
    }
}
