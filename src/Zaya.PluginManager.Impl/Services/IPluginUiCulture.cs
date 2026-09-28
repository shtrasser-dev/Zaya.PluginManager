using System.Globalization;

namespace Zaya.PluginManager.Impl.Services;

/// <summary>Host culture for probe display names and updater status strings.</summary>
public interface IPluginUiCulture
{
    CultureInfo Current { get; }
}
