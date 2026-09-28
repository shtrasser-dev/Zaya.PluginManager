using System.Globalization;
using System.Resources;

namespace Zaya.PluginManager.Impl.Resources;

internal static class PluginUserStrings
{
    private static readonly ResourceManager Manager =
        new("Zaya.PluginManager.Impl.Resources.Strings", typeof(PluginUserStrings).Assembly);

    public static string Get(string name, CultureInfo culture)
        => Manager.GetString(name, culture)
           ?? Manager.GetString(name, CultureInfo.GetCultureInfo("en"))
           ?? Manager.GetString(name, CultureInfo.InvariantCulture)
           ?? name;
}
