using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Rtfx.Server.Database.Entities;

[PrimaryKey(nameof(ArtifactId), nameof(Name))]
public class ArtifactMetadata
{
    public long ArtifactId { get; init; }

    [Required]
    [MaxLength(100)]
    public required string Name { get; init; }

    [Required]
    [MaxLength(1024)]
    public required string Value { get; set; }

    [Required]
    public required Artifact Artifact { get; init; }
}