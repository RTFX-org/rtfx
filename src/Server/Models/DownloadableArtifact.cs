namespace Rtfx.Server.Models;

public sealed class DownloadableArtifact : IDisposable
{
    public DownloadableArtifact(string filePath)
    {
        FilePath = filePath;
    }

    public string FilePath { get; }

    public void Dispose()
    {
        File.Delete(FilePath);
    }
}
