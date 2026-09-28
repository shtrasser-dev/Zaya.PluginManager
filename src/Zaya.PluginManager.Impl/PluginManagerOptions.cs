namespace Zaya.PluginManager.Impl;

/// <summary>Host-supplied options for GitHub User-Agent and optional extra assembly probe path.</summary>
public sealed class PluginManagerOptions
{
    public required string GitHubProductName { get; init; }

    public required string GitHubProductVersion { get; init; }

    /// <summary>Optional extra directory probed during <c>AssemblyResolve</c> (host <c>lib</c> folder).</summary>
    public string? LibDirectory { get; init; }
}
