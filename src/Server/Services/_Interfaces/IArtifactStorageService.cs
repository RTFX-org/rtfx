namespace Rtfx.Server.Services;

public interface IArtifactStorageService
{
    Task SaveArtifactAsync(long feedId, long packageId, long artifactId, Stream artifactStream, CancellationToken cancellation);
    Task<FileStream?> TryLoadArtifactAsync(long feedId, long packageId, long artifactId, CancellationToken cancellation);
}
