using MaSch.Core.Extensions;
using Rtfx.Server.Database.Entities;
using Rtfx.Server.Services;
using System.Diagnostics;
using System.Text.Json.Serialization;

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

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[]? Tags { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public SourceVersionDto[]? SourceVersions { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, string?>? Metadata { get; init; }

    public static ArtifactInfoDto Create(Artifact artifact, IIdHashingService idHashingService, bool metadataIncluded)
    {
        return new ArtifactInfoDto
        {
            FeedId = idHashingService.EncodeId(artifact.Package.Feed.FeedId, IdType.Feed),
            FeedName = artifact.Package.Feed.Name,
            PackageId = idHashingService.EncodeId(artifact.Package.PackageId, IdType.Package),
            PackageName = artifact.Package.Name,
            ArtifactId = idHashingService.EncodeId(artifact.ArtifactId, IdType.Artifact),
            SourceHash = artifact.SourceHash.ToHexString(),
            Tags = metadataIncluded ? artifact.Tags.Select(x => x.Tag).ToArray() : null,
            SourceVersions = metadataIncluded
                ? artifact.SourceVersions
                    .Select(x =>
                        new SourceVersionDto
                        {
                            Branch = x.Branch,
                            Commit = x.Commit.ToHexString(),
                        })
                    .ToArray()
                : null,
            Metadata = metadataIncluded ? artifact.Metadata.ToDictionary(x => x.Name, x => x.Value) : null,
        };
    }
}

public sealed class SourceVersionDto
{
    public required string Branch { get; init; }
    public required string Commit { get; init; }
}