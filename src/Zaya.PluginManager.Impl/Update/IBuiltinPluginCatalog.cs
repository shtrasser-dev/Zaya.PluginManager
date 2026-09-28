namespace Zaya.PluginManager.Impl.Update;

public interface IBuiltinPluginCatalog
{
    IReadOnlyList<BuiltinPluginEntry> Entries { get; }
}
