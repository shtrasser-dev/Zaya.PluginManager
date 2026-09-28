namespace Zaya.PluginManager.Impl.Update;

public sealed class JsonBuiltinPluginCatalog : IBuiltinPluginCatalog
{
    public JsonBuiltinPluginCatalog(IReadOnlyList<BuiltinPluginEntry> entries)
    {
        Entries = entries;
    }

    public IReadOnlyList<BuiltinPluginEntry> Entries { get; }

    public static JsonBuiltinPluginCatalog FromStream(Stream stream)
    {
        var list = PluginJson.Read<List<BuiltinPluginEntry>>(stream);
        return new JsonBuiltinPluginCatalog(list.AsReadOnly());
    }
}
