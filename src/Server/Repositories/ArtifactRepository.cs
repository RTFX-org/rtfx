using Microsoft.EntityFrameworkCore;
using Rtfx.Server.Database;
using Rtfx.Server.Database.Entities;

namespace Rtfx.Server.Repositories;

public class ArtifactRepository : IArtifactRepository
{
    private readonly DatabaseContext _database;

    public ArtifactRepository(DatabaseContext database)
    {
        _database = database;
    }

    public async Task<Artifact?> TryGetArtifactWithMetadataAsync(long artifactId, CancellationToken ct)
    {
        return await GetArtifactWithMetadataQuery(artifactId).FirstOrDefaultAsync(ct);
    }

    public async Task<long> InsertArtifact(Artifact artifact, CancellationToken ct)
    {
        if (artifact.ArtifactId != 0)
            throw new ArgumentException("The artifact already defines an id. Make sure Artifact.ArtifactId is zero.", nameof(artifact));

        _database.Feeds.Attach(artifact.Package.Feed);
        _database.Packages.Attach(artifact.Package);
        _database.Artifacts.Add(artifact);
        await _database.SaveChangesAsync(ct);
        return artifact.ArtifactId;
    }

    public async Task<(long FeedId, long PackageId, long ArtifactId)> TryGetIdsAsync(string feedName, string packageName, string sourceHash, CancellationToken ct)
    {
        var rawSourceHash = Convert.FromHexString(sourceHash);
        var ids = await _database
            .Feeds.Where(x => x.Name == feedName).Select(x => x.FeedId)
            .Concat(_database
                .Packages.Where(x => x.Feed.Name == feedName && x.Name == packageName).Select(x => x.PackageId))
            .Concat(_database
                .Artifacts.Where(x => x.Package.Feed.Name == feedName && x.Package.Name == packageName && x.SourceHash == rawSourceHash).Select(x => x.ArtifactId))
            .ToArrayAsync(ct);

        return (
            ids.Length > 0 ? ids[0] : 0,
            ids.Length > 1 ? ids[1] : 0,
            ids.Length > 2 ? ids[2] : 0);
    }

    public async Task<Artifact> UpdateArtifactAsync(long artifactId, Action<Artifact> updateAction, CancellationToken ct)
    {
        var artifact = await GetArtifactWithMetadataQuery(artifactId).FirstAsync(ct);
        updateAction(artifact);
        await _database.SaveChangesAsync(ct);
        return artifact;
    }

    private IQueryable<Artifact> GetArtifactWithMetadataQuery(long artifactId)
    {
        return _database.Artifacts
            .Where(x => x.ArtifactId == artifactId)
            .Include(x => x.Tags)
            .Include(x => x.SourceVersions)
            .Include(x => x.Metadata)
            .Include(x => x.Package)
            .Include(x => x.Package.Feed)
            .AsSplitQuery();
    }
}
