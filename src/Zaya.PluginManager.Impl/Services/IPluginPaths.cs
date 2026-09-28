namespace Zaya.PluginManager.Impl.Services;

/// <summary>Host filesystem locations used by the plugin pipeline.</summary>
public interface IPluginPaths
{
    string GetPluginsDirectory();

    string GetExtractedPluginsDirectory();
}
