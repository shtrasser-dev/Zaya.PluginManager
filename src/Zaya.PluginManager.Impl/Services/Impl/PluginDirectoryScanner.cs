using Zaya.Logging.Models;
using Zaya.PluginManager.Impl.Services;

namespace Zaya.PluginManager.Impl.Services.Impl;

public sealed class PluginDirectoryScanner : IPluginDirectoryScanner
{
    private readonly IPluginDirectoryProcessor _pluginDirectoryProcessor;
    private readonly IPluginPaths _pluginPaths;

    public PluginDirectoryScanner(
        IPluginDirectoryProcessor pluginDirectoryProcessor,
        IPluginPaths pluginPaths)
    {
        _pluginDirectoryProcessor = pluginDirectoryProcessor;
        _pluginPaths = pluginPaths;
    }

    [Log(LogLevel.Debug, LogParameters = true)]
    public void Process()
    {
        var extractRoot = _pluginPaths.GetExtractedPluginsDirectory();
        if (!Directory.Exists(extractRoot))
            return;

        foreach (var dir in Directory.EnumerateDirectories(extractRoot))
        {
            try
            {
                _pluginDirectoryProcessor.Process(dir);
            }
            catch (Exception)
            {
            }
        }
    }
}
