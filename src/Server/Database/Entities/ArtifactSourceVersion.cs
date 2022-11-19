using System.ComponentModel.DataAnnotations;

namespace Rtfx.Server.Database.Entities;

public class ArtifactSourceVersion
{
    public long ArtifactSourceVersionId { get; init; }

    public long ArtifactId { get; init; }

    [Required]
    [MaxLength(512)]
    public required string Branch { get; init; }

    [Required]
    public required byte[] Commit { get; init; }

    [Required]
    public required Artifact Artifact { get; init; }
}
