namespace Zaya.PluginManager.Impl.Update;

public sealed class BuiltinPluginEntry
{
    public string Id { get; set; } = string.Empty;

    public string Repo { get; set; } = string.Empty;

    public string Asset { get; set; } = string.Empty;

    /// <summary>Interface package id (e.g. <c>Zaya.Translator</c>) used for channel and compatibility checks.</summary>
    public string Interface { get; set; } = string.Empty;

    public bool Required { get; set; }
}

public sealed class GitHubReleaseInfo
{
    public string TagName { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public string HtmlUrl { get; init; } = string.Empty;
    public bool Prerelease { get; init; }
    public IReadOnlyList<GitHubReleaseAsset> Assets { get; init; } = [];

    /// <summary>Semver from release name or body "version: x.y.z".</summary>
    public Version? ParsedVersion => ReleaseVersionParser.TryParse(Name, Body);
}

public sealed class GitHubReleaseAsset
{
    public string Name { get; init; } = string.Empty;
    public string BrowserDownloadUrl { get; init; } = string.Empty;
    public long Size { get; init; }
    public string? Digest { get; init; }
}

public sealed class PluginUpdateResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public IReadOnlyList<string> DownloadedAssets { get; init; } = [];
    public bool RequiresRestart { get; init; }
}

public static class ReleaseVersionParser
{
    private const string VersionCapture = @"(?<ver>\d+\.\d+\.\d+(?:\.\d+)?)";

    public static Version? TryParse(string? name, string? body)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            var m = System.Text.RegularExpressions.Regex.Match(
                name,
                $@"\bv{VersionCapture}\b",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (m.Success && Version.TryParse(m.Groups["ver"].Value, out var fromName))
                return fromName;
        }

        if (!string.IsNullOrWhiteSpace(body))
        {
            using var reader = new StringReader(body);
            var first = reader.ReadLine();
            if (first is not null)
            {
                var m = System.Text.RegularExpressions.Regex.Match(
                    first,
                    $@"^\s*version\s*:\s*{VersionCapture}\s*$",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (m.Success && Version.TryParse(m.Groups["ver"].Value, out var fromBody))
                    return fromBody;
            }
        }

        return null;
    }

    public static IReadOnlyDictionary<string, Version> ParseAssetVersions(string? body)
    {
        var map = new Dictionary<string, Version>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(body))
            return map;

        foreach (var raw in body.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries))
        {
            var line = raw.Trim();
            var m = System.Text.RegularExpressions.Regex.Match(
                line,
                $@"^(?<asset>[^\s=:]+\.zip)\s*[=:]\s*{VersionCapture}\s*$",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (m.Success && Version.TryParse(m.Groups["ver"].Value, out var ver))
                map[m.Groups["asset"].Value] = ver;
        }

        return map;
    }

    public static Version? ResolveAssetVersion(GitHubReleaseInfo release, string assetName)
    {
        var map = ParseAssetVersions(release.Body);
        if (map.TryGetValue(assetName, out var perAsset))
            return perAsset;
        return release.ParsedVersion;
    }
}
