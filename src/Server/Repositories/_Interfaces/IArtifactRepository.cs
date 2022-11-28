using Rtfx.Server.Database.Entities;

namespace Rtfx.Server.Repositories;

public interface IArtifactRepository
{
    Task<(long FeedId, long PackageId, long ArtifactId)> TryGetIdsAsync(string feedName, string packageName, string sourceHash, CancellationToken ct);
    Task<long> InsertArtifact(Artifact artifact, CancellationToken ct);
    Task<Artifact> UpdateArtifactAsync(long artifactId, Action<Artifact> updateAction, CancellationToken ct);
    Task<Artifact?> TryGetArtifactWithMetadataAsync(long artifactId, CancellationToken ct);
    Task<Artifact?> TryGetArtifactAsync(long artifactId, CancellationToken ct);
    IQueryable<Artifact> GetArtifactsWithMetadata(long packageId, int skip, int take);
    IQueryable<Artifact> GetArtifacts(long packageId, int skip, int take);
    Task RemoveArtifactAsync(long artifactId, CancellationToken ct);
}
