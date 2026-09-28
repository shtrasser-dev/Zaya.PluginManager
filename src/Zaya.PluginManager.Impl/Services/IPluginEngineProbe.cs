using System.Globalization;

namespace Zaya.PluginManager.Impl.Services;

/// <summary>Host-specific recognition of a constructed plugin entry instance.</summary>
public interface IPluginEngineProbe
{
    bool TryDescribe(object instance, CultureInfo culture, out PluginEngineInfo info);
}

/// <summary>Result of a successful <see cref="IPluginEngineProbe"/> match.</summary>
public sealed class PluginEngineInfo
{
    public required string ServiceKind { get; init; }

    public required string EngineId { get; init; }

    public string? DisplayName { get; init; }
}
