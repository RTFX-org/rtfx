using MaSch.Core.Extensions;
using Rtfx.Server.Database.Entities;
using Rtfx.Server.Services;
using System.Diagnostics;

namespace Rtfx.Server.Models.Dtos;

[DebuggerDisplay("{FeedName}/{PackageName}/{SourceHash}")]
public sealed class ArtifactInfoDto
{
    public required string FeedId { get; init; }

    public required string FeedName { get; init; }

    public required string PackageId { get; init; }

    public required string PackageName { get; init; }

    public required string ArtifactId { get; init; }

    public required string SourceHash { get; init; }

    public required string[] Tags { get; init; }

    public required SourceVersionDto[] SourceVersions { get; init; }

    public required Dictionary<string, string?> Metadata { get; init; }

    public static ArtifactInfoDto Create(Artifact artifact, IIdHashingService idHashingService)
    {
        return new ArtifactInfoDto
        {
            FeedId = idHashingService.EncodeId(artifact.Package.Feed.FeedId),
            FeedName = artifact.Package.Feed.Name,
            PackageId = idHashingService.EncodeId(artifact.Package.PackageId),
            PackageName = artifact.Package.Name,
            ArtifactId = idHashingService.EncodeId(artifact.ArtifactId),
            SourceHash = artifact.SourceHash.ToHexString(),
            Tags = artifact.Tags.Select(x => x.Tag).ToArray(),
            SourceVersions = artifact.SourceVersions
                .Select(x =>
                    new SourceVersionDto
                    {
                        Branch = x.Branch,
                        Commit = x.Commit.ToHexString(),
                    })
                .ToArray(),
            Metadata = artifact.Metadata.ToDictionary(x => x.Name, x => x.Value),
        };
    }
}

public sealed class SourceVersionDto
{
    public required string Branch { get; init; }
    public required string Commit { get; init; }
}