using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Zaya.PluginManager.Impl;

internal static class PluginJson
{
    public static JsonSerializerOptions Options { get; } = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public static T Read<T>(Stream stream)
        => JsonSerializer.Deserialize<T>(stream, Options) ?? throw new InvalidOperationException("JSON payload was empty.");

    public static bool TryReadFile<T>(string path, [NotNullWhen(true)] out T? value)
    {
        try
        {
            if (!File.Exists(path))
            {
                value = default;
                return false;
            }

            using var stream = File.OpenRead(path);
            value = Read<T>(stream);
            return value is not null;
        }
        catch
        {
            value = default;
            return false;
        }
    }
}
