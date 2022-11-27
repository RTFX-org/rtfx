using Rtfx.Server.Models;

namespace Rtfx.Server.Services;

public interface IArtifactStorageService
{
    void SaveArtifact(long feedId, long packageId, long artifactId, Stream artifactStream);
    DownloadableArtifact? TryGetDownloadableArtifact(long feedId, long packageId, long artifactId);
}
