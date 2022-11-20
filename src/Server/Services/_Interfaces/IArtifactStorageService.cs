namespace Rtfx.Server.Services;

public interface IArtifactStorageService
{
    Task SaveArtifact(long feedId, long packageId, long artifactId, Stream artifactStream);
}
