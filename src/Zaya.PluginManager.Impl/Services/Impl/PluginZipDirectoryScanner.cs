using Zaya.Logging.Models;
using Zaya.PluginManager.Impl.Services;

namespace Zaya.PluginManager.Impl.Services.Impl;

public sealed class PluginZipDirectoryScanner : IPluginZipDirectoryScanner
{
    private readonly IPluginZipProcessor _pluginZipProcessor;
    private readonly IPluginExtractCache _pluginExtractCache;
    private readonly IPluginPaths _pluginPaths;

    public PluginZipDirectoryScanner(
        IPluginZipProcessor pluginZipProcessor,
        IPluginExtractCache pluginExtractCache,
        IPluginPaths pluginPaths)
    {
        _pluginZipProcessor = pluginZipProcessor;
        _pluginExtractCache = pluginExtractCache;
        _pluginPaths = pluginPaths;
    }

    [Log(LogLevel.Debug, LogParameters = true)]
    public void Process()
    {
        var pluginsPath = _pluginPaths.GetPluginsDirectory();
        var extractRoot = _pluginPaths.GetExtractedPluginsDirectory();

        if (!Directory.Exists(pluginsPath))
            return;

        Directory.CreateDirectory(extractRoot);

        foreach (var zip in Directory.EnumerateFiles(pluginsPath, "*.zip"))
        {
            try
            {
                _pluginZipProcessor.Process(zip);
            }
            catch (Exception)
            {
            }
        }

        foreach (var dir in Directory.EnumerateDirectories(extractRoot))
        {
            var dirName = Path.GetFileName(dir);
            var expectedZip = Path.Combine(pluginsPath, dirName + ".zip");
            if (File.Exists(expectedZip))
                continue;

            try
            {
                _pluginExtractCache.DeleteDirectory(dir);
            }
            catch (Exception)
            {
            }
        }
    }
}
