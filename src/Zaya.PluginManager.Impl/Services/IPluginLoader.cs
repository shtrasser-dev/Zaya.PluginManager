using System.Reflection;

namespace Zaya.PluginManager.Impl.Services;

public interface IPluginLoader
{
    IReadOnlyList<Assembly> LoadedAssemblies { get; }

    void LoadPlugins();
}
