using System.Globalization;
using System.IO.Compression;
using Zaya.Logging.Services;
using Zaya.PluginManager.Impl.Services;
using Zaya.PluginManager.Impl.Services.Impl;
using Zaya.PluginManager.Impl.Update;

namespace Zaya.PluginManager.Impl.Tests;

public sealed class PluginCatalogTests
{
    [Fact]
    public void Register_UsesProbe_AndFindIsCaseInsensitive()
    {
        var catalog = new PluginCatalog([new FakeEngineProbe()], new InvariantPluginUiCulture());
        catalog.Register(
            new PluginManifest
            {
                Id = "fake",
                Type = "ocr",
                EntryPoint = typeof(FakeEngine).FullName!,
            },
            [typeof(FakeEngine).Assembly]);

        var found = catalog.Find("OCR", "FAKE");
        Assert.NotNull(found);
        Assert.Equal("ocr", found.ServiceKind, StringComparer.OrdinalIgnoreCase);
        Assert.Equal("fake", found.EngineId, StringComparer.OrdinalIgnoreCase);
        Assert.Single(catalog.List("ocr"));
    }

    [Fact]
    public void Register_UnknownEngine_ThrowsProbeFailed()
    {
        var catalog = new PluginCatalog([new FakeEngineProbe()], new InvariantPluginUiCulture());
        Assert.Throws<Zaya.PluginManager.Impl.Exceptions.InvalidPluginManifestException>(() =>
            catalog.Register(
                new PluginManifest
                {
                    Id = "other",
                    Type = "x",
                    EntryPoint = typeof(UnknownEngine).FullName!,
                },
                [typeof(UnknownEngine).Assembly]));
    }

    public sealed class FakeEngine
    {
        public FakeEngine(ILoggingWrapper logging) { }
    }

    public sealed class UnknownEngine
    {
        public UnknownEngine(ILoggingWrapper logging) { }
    }

    private sealed class FakeEngineProbe : IPluginEngineProbe
    {
        public bool TryDescribe(object instance, CultureInfo culture, out PluginEngineInfo info)
        {
            if (instance is FakeEngine)
            {
                info = new PluginEngineInfo { ServiceKind = "ocr", EngineId = "fake", DisplayName = "Fake" };
                return true;
            }

            info = null!;
            return false;
        }
    }

    private sealed class InvariantPluginUiCulture : IPluginUiCulture
    {
        public CultureInfo Current => CultureInfo.InvariantCulture;
    }
}

public sealed class PluginJsonTests
{
    [Fact]
    public void ReadsCamelCaseBuiltinCatalog()
    {
        const string json = """[{"id":"OneOcr","repo":"shtrasser-dev/Zaya.OCR","asset":"Zaya.OCR.Impl.OneOcr.zip","interface":"Zaya.OCR","required":true}]""";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));
        var catalog = JsonBuiltinPluginCatalog.FromStream(stream);
        Assert.Single(catalog.Entries);
        Assert.Equal("OneOcr", catalog.Entries[0].Id);
        Assert.True(catalog.Entries[0].Required);
    }
}

public sealed class PluginManifestJsonTests
{
    [Fact]
    public void ReadsCamelCaseManifestViaZip()
    {
        var root = Path.Combine(Path.GetTempPath(), "zaya-pm-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            const string json = """{"id":"OneOcr","type":"ocr","interface":"Zaya.OCR","entryPoint":"Demo.Engine"}""";
            var jsonPath = Path.Combine(root, "plugin.json");
            File.WriteAllText(jsonPath, json);
            var zipPath = Path.Combine(root, "plugin.zip");
            using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
                zip.CreateEntryFromFile(jsonPath, "plugin.json");

            var manifest = new PluginManifestReader().ReadFromZip(zipPath);
            Assert.NotNull(manifest);
            Assert.Equal("OneOcr", manifest.Id);
            Assert.Equal("ocr", manifest.Type);
            Assert.Equal("Zaya.OCR", manifest.Interface);
            Assert.Equal("Demo.Engine", manifest.EntryPoint);
        }
        finally
        {
            try { Directory.Delete(root, recursive: true); } catch { /* ignore */ }
        }
    }
}

public sealed class PluginExtractCacheTests
{
    [Fact]
    public void ExtractIfNeeded_WritesStamp_AndIsUpToDate()
    {
        var cache = new PluginExtractCache();
        var root = Path.Combine(Path.GetTempPath(), "zaya-pm-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var zipPath = Path.Combine(root, "plugin.zip");
            var payload = Path.Combine(root, "payload.txt");
            File.WriteAllText(payload, "hello");
            using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
                zip.CreateEntryFromFile(payload, "payload.txt");

            var extractRoot = Path.Combine(root, "extract");
            cache.ExtractIfNeeded(zipPath, extractRoot);
            var extractDir = Path.Combine(extractRoot, "plugin");
            Assert.True(cache.IsExtractUpToDate(zipPath, extractDir));
            Assert.True(File.Exists(Path.Combine(extractDir, "payload.txt")));
        }
        finally
        {
            try { Directory.Delete(root, recursive: true); } catch { /* ignore */ }
        }
    }
}
