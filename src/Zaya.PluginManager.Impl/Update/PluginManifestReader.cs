using System.IO.Compression;
using Zaya.PluginManager.Impl.Services;

namespace Zaya.PluginManager.Impl.Update;

/// <summary>Reads <c>plugin.json</c> from plugin zip archives.</summary>
public sealed class PluginManifestReader : IPluginManifestReader
{
    public PluginManifest? ReadFromZip(string zipPath)
    {
        if (!File.Exists(zipPath))
            return null;

        try
        {
            using var archive = ZipFile.OpenRead(zipPath);
            var entry = archive.GetEntry(PluginConstants.ManifestFileName)
                ?? archive.Entries.FirstOrDefault(e =>
                    string.Equals(Path.GetFileName(e.FullName), PluginConstants.ManifestFileName, StringComparison.OrdinalIgnoreCase));
            if (entry is null)
                return null;

            using var stream = entry.Open();
            return PluginJson.Read<PluginManifest>(stream);
        }
        catch
        {
            return null;
        }
    }
}
