using System.Reflection;
using Zaya.Logging.Services;
using Zaya.PluginManager.Impl.Exceptions;
using Zaya.PluginManager.Impl.Services;

namespace Zaya.PluginManager.Impl.Services.Impl;

/// <summary>
/// Catalog of plugin engines discovered from <c>plugin.json</c> <c>entryPoint</c> values.
/// </summary>
public sealed class PluginCatalog : IPluginCatalog
{
    private readonly IReadOnlyList<IPluginEngineProbe> _probes;
    private readonly IPluginUiCulture _uiCulture;
    private readonly List<PluginEngineRegistration> _entries = [];

    public PluginCatalog(IEnumerable<IPluginEngineProbe> probes, IPluginUiCulture uiCulture)
    {
        _probes = probes.ToList();
        _uiCulture = uiCulture;
    }

    public IReadOnlyList<PluginEngineRegistration> Entries => _entries;

    public void Clear() => _entries.Clear();

    public void Register(PluginManifest manifest, IReadOnlyList<Assembly> assemblies)
    {
        if (string.IsNullOrWhiteSpace(manifest.EntryPoint))
            throw new InvalidPluginManifestException(InvalidPluginManifestReason.MissingEntryPoint);

        var entryType = ResolveType(manifest.EntryPoint.Trim(), assemblies);
        if (entryType is null)
            throw new InvalidPluginManifestException(InvalidPluginManifestReason.EntryPointNotFound);

        if (!TryProbe(entryType, out var engineId, out var displayName, out var serviceKind))
            throw new InvalidPluginManifestException(InvalidPluginManifestReason.ProbeFailed);

        _entries.RemoveAll(e =>
            string.Equals(e.EngineId, engineId, StringComparison.OrdinalIgnoreCase)
            && string.Equals(e.ServiceKind, serviceKind, StringComparison.OrdinalIgnoreCase));

        _entries.Add(new PluginEngineRegistration
        {
            PluginId = manifest.Id,
            PluginType = manifest.Type,
            EngineId = engineId,
            DisplayName = displayName,
            EntryType = entryType,
            ServiceKind = serviceKind,
        });
    }

    public PluginEngineRegistration? Find(string kind, string engineId)
        => _entries.FirstOrDefault(e =>
            string.Equals(e.ServiceKind, kind, StringComparison.OrdinalIgnoreCase)
            && string.Equals(e.EngineId, engineId, StringComparison.OrdinalIgnoreCase));

    public IReadOnlyList<PluginEngineRegistration> List(string kind)
        => _entries
            .Where(e => string.Equals(e.ServiceKind, kind, StringComparison.OrdinalIgnoreCase))
            .ToList();

    public object Create(Type entryType, ILoggingWrapper loggingWrapper)
    {
        ArgumentNullException.ThrowIfNull(entryType);
        ArgumentNullException.ThrowIfNull(loggingWrapper);

        var ctor = entryType.GetConstructor(
            BindingFlags.Instance | BindingFlags.Public,
            binder: null,
            types: [typeof(ILoggingWrapper)],
            modifiers: null);

        if (ctor is null)
            throw new MissingMethodException(entryType.FullName, ".ctor(ILoggingWrapper)");

        return ctor.Invoke([loggingWrapper]);
    }

    private static Type? ResolveType(string entryPoint, IReadOnlyList<Assembly> assemblies)
    {
        foreach (var asm in assemblies)
        {
            var type = asm.GetType(entryPoint, throwOnError: false, ignoreCase: false);
            if (type is not null)
                return type;
        }

        var comma = entryPoint.IndexOf(',');
        if (comma > 0)
        {
            var typeName = entryPoint[..comma].Trim();
            foreach (var asm in assemblies)
            {
                var type = asm.GetType(typeName, throwOnError: false, ignoreCase: false);
                if (type is not null)
                    return type;
            }
        }

        return null;
    }

    private bool TryProbe(
        Type entryType,
        out string engineId,
        out string? displayName,
        out string serviceKind)
    {
        engineId = "";
        displayName = null;
        serviceKind = "";

        object? instance = null;
        try
        {
            instance = Create(entryType, EmptyLoggingWrapper.Instance);
            if (instance is null)
                return false;

            var culture = _uiCulture.Current;
            foreach (var probe in _probes)
            {
                if (!probe.TryDescribe(instance, culture, out var info))
                    continue;
                if (string.IsNullOrWhiteSpace(info.EngineId) || string.IsNullOrWhiteSpace(info.ServiceKind))
                    continue;

                engineId = info.EngineId;
                displayName = info.DisplayName;
                serviceKind = info.ServiceKind;
                return true;
            }

            return false;
        }
        catch (Exception ex) when (ex is not InvalidPluginManifestException)
        {
            throw new InvalidPluginManifestException(InvalidPluginManifestReason.ProbeFailed, ex);
        }
        finally
        {
            (instance as IDisposable)?.Dispose();
        }
    }
}
