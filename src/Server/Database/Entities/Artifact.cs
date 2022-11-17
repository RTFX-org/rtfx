using System.ComponentModel.DataAnnotations;

namespace Rtfx.Server.Database.Entities;

public class Artifact
{
    public Guid ArtifactId { get; init; }

    [Required]
    public required byte[] SourceHash { get; init; }

    public Guid PackageId { get; init; }

    [Required]
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;

    [Required]
    public required Package Package { get; init; }

    public List<ArtifactTag> Tags { get; init; } = new();

    public List<ArtifactMetadata> Metadata { get; init; } = new();
}