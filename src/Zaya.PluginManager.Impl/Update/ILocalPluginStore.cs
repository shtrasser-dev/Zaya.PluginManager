using Zaya.PluginManager.Impl.Services;

namespace Zaya.PluginManager.Impl.Update;

public interface ILocalPluginStore
{
    Dictionary<string, PluginManifest> Scan();

    void PurgeIncompatibleInterfaces(Dictionary<string, PluginManifest> localState);

    bool IsIncompatibleWithHost(PluginManifest manifest);
}
