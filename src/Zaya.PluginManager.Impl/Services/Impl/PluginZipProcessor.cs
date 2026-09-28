using Zaya.Logging.Models;
using Zaya.PluginManager.Impl.Services;

namespace Zaya.PluginManager.Impl.Services.Impl;

public sealed class PluginZipProcessor : IPluginZipProcessor
{
    private readonly IPluginExtractCache _pluginExtractCache;
    private readonly IPluginPaths _pluginPaths;

    public PluginZipProcessor(IPluginExtractCache pluginExtractCache, IPluginPaths pluginPaths)
    {
        _pluginExtractCache = pluginExtractCache;
        _pluginPaths = pluginPaths;
    }

    [Log(LogLevel.Debug, LogParameters = true)]
    public void Process(string zipPath)
        => _pluginExtractCache.ExtractIfNeeded(zipPath, _pluginPaths.GetExtractedPluginsDirectory());
}
