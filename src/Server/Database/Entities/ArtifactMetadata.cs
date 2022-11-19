using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace Rtfx.Server.Database.Entities;

[PrimaryKey(nameof(ArtifactId), nameof(Name))]
[DebuggerDisplay("[{Name}] = {Value}")]
public class ArtifactMetadata
{
    public long ArtifactId { get; init; }

    [Required]
    [MaxLength(100)]
    public required string Name { get; init; }

    [MaxLength(1024)]
    public string? Value { get; set; }

    [Required]
    public required Artifact Artifact { get; init; }

    public override int GetHashCode()
    {
        return HashCode.Combine(
            StringComparer.Ordinal.GetHashCode(Name),
            Value is null ? 0 : StringComparer.Ordinal.GetHashCode(Value));
    }

    public override bool Equals(object? obj)
    {
        return obj is ArtifactMetadata md
            && string.Equals(md.Name, Name, StringComparison.Ordinal)
            && string.Equals(md.Value, Value, StringComparison.Ordinal);
    }
}