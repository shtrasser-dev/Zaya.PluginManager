namespace Zaya.PluginManager.Impl.Services;

public interface IPluginExtractCache
{
    void ExtractIfNeeded(string zipPath, string tempRoot);

    bool IsExtractUpToDate(string zipPath, string extractDir);

    string MakeStamp(string zipPath);

    bool ZipContentsMatchExtract(string zipPath, string extractDir);

    void DeleteDirectory(string path);
}
