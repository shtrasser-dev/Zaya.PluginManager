using System.Reflection;

namespace Zaya.PluginManager.Impl.Services;

public interface IPluginAssemblyLoader
{
    IReadOnlyList<Assembly> LoadedAssemblies { get; }

    void Process();

    void TryLoad(string dllPath);
}
