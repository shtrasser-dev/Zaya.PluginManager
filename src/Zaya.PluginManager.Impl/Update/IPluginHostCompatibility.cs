using System.Reflection;
using Zaya.PluginManager.Impl.Services;

namespace Zaya.PluginManager.Impl.Update;

public interface IPluginHostCompatibility
{
    Assembly? ResolveHostInterfaceAssembly(string interfaceName);

    string? ResolveUpdateChannel(string interfaceName);

    string? ResolveUpdateChannel(BuiltinPluginEntry entry);

    bool IsInterfaceCompatible(PluginManifest manifest);
}
