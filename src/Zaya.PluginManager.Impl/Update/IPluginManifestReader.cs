using Zaya.PluginManager.Impl.Services;

namespace Zaya.PluginManager.Impl.Update;

public interface IPluginManifestReader
{
    PluginManifest? ReadFromZip(string zipPath);
}
