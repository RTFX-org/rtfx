using Rtfx.Server.Database.Entities;

namespace Rtfx.Server.Repositories;

public interface IArtifactRepository
{
    Task<(long FeedId, long PackageId, long ArtifactId)> TryGetIdsAsync(string feedName, string packageName, string sourceHash, CancellationToken ct);
    Task<long> InsertArtifact(Artifact artifact, CancellationToken ct);
    Task<Artifact> UpdateArtifactAsync(long artifactId, Action<Artifact> updateAction, CancellationToken ct);
    Task<Artifact?> TryGetArtifactWithMetadataAsync(long artifactId, CancellationToken ct);
}
