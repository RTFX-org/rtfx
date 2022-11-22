using MaSch.Core.Extensions;
using Rtfx.Server.Database.Entities;
using System.Diagnostics;

namespace Rtfx.Server.Models.Dtos;

[DebuggerDisplay("{FeedName}/{PackageName}/{SourceHash}")]
public sealed class ArtifactInfoDto
{
    public required long FeedId { get; init; }

    public required string FeedName { get; init; }

    public required long PackageId { get; init; }

    public required string PackageName { get; init; }

    public required long ArtifactId { get; init; }

    public required string SourceHash { get; init; }

    public required string[] Tags { get; init; }

    public required SourceVersionDto[] SourceVersions { get; init; }

    public required Dictionary<string, string?> Metadata { get; init; }

    public static ArtifactInfoDto Create(Artifact artifact)
    {
        return new ArtifactInfoDto
        {
            FeedId = artifact.Package.Feed.FeedId,
            FeedName = artifact.Package.Feed.Name,
            PackageId = artifact.Package.PackageId,
            PackageName = artifact.Package.Name,
            ArtifactId = artifact.ArtifactId,
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