using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace Rtfx.Server.Database.Entities;

[PrimaryKey(nameof(ArtifactId), nameof(Tag))]
[DebuggerDisplay("{Tag}")]
public class ArtifactTag
{
    public long ArtifactId { get; init; }

    [Required]
    [MaxLength(255)]
    public required string Tag { get; init; }

    [Required]
    public required Artifact Artifact { get; init; }

    public override int GetHashCode()
    {
        return StringComparer.Ordinal.GetHashCode(Tag);
    }

    public override bool Equals(object? obj)
    {
        return obj is ArtifactTag tag && string.Equals(tag.Tag, Tag, StringComparison.Ordinal);
    }
}