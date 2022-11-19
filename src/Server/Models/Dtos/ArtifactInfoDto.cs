using MaSch.Core.Extensions;
using System.Diagnostics;
using System.Text.Json.Serialization;

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

    [JsonPropertyName("_links")]
    public required ArtifactLinksDto Links { get; init; }

    public static ArtifactInfoDto Create(HttpContext httpContext, Db.Artifact artifact, long feedId, string feedName, string packageName)
    {
        return new ArtifactInfoDto
        {
            FeedId = feedId,
            FeedName = feedName,
            PackageId = artifact.PackageId,
            PackageName = packageName,
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
            Links = ArtifactLinksDto.Create(httpContext, feedId, artifact.PackageId, artifact.ArtifactId),
        };
    }

    public static ArtifactInfoDto Create(HttpContext httpContext, ArtifactMetadata artifact, long feedId, long packageId, long artifactId)
    {
        return new ArtifactInfoDto
        {
            FeedId = feedId,
            FeedName = artifact.FeedName,
            PackageId = packageId,
            PackageName = artifact.PackageName,
            ArtifactId = artifactId,
            SourceHash = artifact.SourceHash,
            Tags = artifact.Tags,
            SourceVersions = artifact.SourceVersions
                .Select(x =>
                    new SourceVersionDto
                    {
                        Branch = x.Branch,
                        Commit = x.Commit,
                    })
                .ToArray(),
            Metadata = artifact.Metadata,
            Links = ArtifactLinksDto.Create(httpContext, feedId, packageId, artifactId),
        };
    }
}

public sealed class SourceVersionDto
{
    public required string Branch { get; init; }
    public required string Commit { get; init; }
}

public sealed class ArtifactLinksDto
{
    public required Uri Feed { get; init; }
    public required Uri Package { get; init; }
    public required Uri Artifact { get; init; }
    public required Uri Download { get; init; }

    public static ArtifactLinksDto Create(HttpContext httpContext, long feedId, long packageId, long artifactId)
    {
        var baseUri = httpContext.GetBaseUri();
        return new ArtifactLinksDto
        {
            Feed = new Uri(baseUri, $"api/feeds/{feedId}"),
            Package = new Uri(baseUri, $"api/feeds/{feedId}/packages/{packageId}"),
            Artifact = new Uri(baseUri, $"api/artifacts/{artifactId}"),
            Download = new Uri(baseUri, $"api/artifacts/{artifactId}/download"),
        };
    }
}