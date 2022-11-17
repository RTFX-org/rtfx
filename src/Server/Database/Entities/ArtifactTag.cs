using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Rtfx.Server.Database.Entities;

[PrimaryKey(nameof(ArtifactId), nameof(Tag))]
public class ArtifactTag
{
    public Guid ArtifactId { get; init; }

    [Required]
    [MaxLength(255)]
    public required string Tag { get; init; }

    [Required]
    public required Artifact Artifact { get; init; }
}