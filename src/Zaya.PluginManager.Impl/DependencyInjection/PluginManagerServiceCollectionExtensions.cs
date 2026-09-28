using Microsoft.Extensions.DependencyInjection;
using Zaya.PluginManager.Impl.Services;
using Zaya.PluginManager.Impl.Services.Impl;
using Zaya.PluginManager.Impl.Update;

namespace Zaya.PluginManager.Impl.DependencyInjection;

public static class PluginManagerServiceCollectionExtensions
{
    /// <summary>
    /// Registers the plugin pipeline and updater. The host must already register
    /// <see cref="IPluginPaths"/>, <see cref="IPluginEngineProbe"/>,
    /// <see cref="IPluginHostCompatibility"/>, <see cref="IBuiltinPluginCatalog"/>,
    /// <see cref="IPluginUiCulture"/>, and <see cref="HttpClient"/>.
    /// </summary>
    public static IServiceCollection AddZayaPluginManager(
        this IServiceCollection services,
        PluginManagerOptions options)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.GitHubProductName);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.GitHubProductVersion);

        services.AddSingleton(options);
        services.AddSingleton<IGitHubReleasesClient>(sp =>
            new GitHubReleasesClient(
                sp.GetRequiredService<HttpClient>(),
                options.GitHubProductName,
                options.GitHubProductVersion));
        services.AddSingleton<IPluginManifestReader, PluginManifestReader>();
        services.AddSingleton<ILocalPluginStore, LocalPluginStore>();
        services.AddSingleton<IPluginCatalogDownloader, PluginCatalogDownloader>();
        services.AddSingleton<IPluginUpdateService, PluginUpdateService>();
        services.AddSingleton<IPluginExtractCache, PluginExtractCache>();
        services.AddSingleton<IPluginZipProcessor, PluginZipProcessor>();
        services.AddSingleton<IPluginZipDirectoryScanner, PluginZipDirectoryScanner>();
        services.AddSingleton<IPluginAssemblyLoader, PluginAssemblyLoader>();
        services.AddSingleton<IPluginDirectoryProcessor, PluginDirectoryProcessor>();
        services.AddSingleton<IPluginDirectoryScanner, PluginDirectoryScanner>();
        services.AddSingleton<IPluginCatalog, PluginCatalog>();
        services.AddSingleton<IPluginLoader, PluginLoader>();
        return services;
    }
}
