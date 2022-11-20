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

    public static ArtifactInfoDto Create(HttpContext httpContext, Db.Artifact artifact)
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
            Links = ArtifactLinksDto.Create(httpContext, artifact.Package.Feed.FeedId, artifact.Package.PackageId, artifact.ArtifactId),
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