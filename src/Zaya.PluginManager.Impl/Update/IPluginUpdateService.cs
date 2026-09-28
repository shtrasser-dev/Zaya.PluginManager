using Zaya.PluginManager.Impl.Services;

namespace Zaya.PluginManager.Impl.Update;

public interface IPluginUpdateService
{
    Task<PluginUpdateResult> EnsurePluginsAsync(
        string channel,
        bool updateOptional = true,
        bool checkForUpdates = true,
        IProgress<string>? status = null,
        CancellationToken cancellationToken = default);

    PluginManifest? ReadManifestFromZip(string zipPath);
}
