using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Numerics;

namespace Rtfx.Server.Database.Entities;

[DebuggerDisplay("Branch = {Branch}, Commit = {MaSch.Core.Extensions.ObjectExtensions.ToHexString(Commit)}")]
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

    public override int GetHashCode()
    {
        return HashCode.Combine(
            StringComparer.Ordinal.GetHashCode(Branch),
            new BigInteger(Commit).GetHashCode());
    }

    public override bool Equals(object? obj)
    {
        return obj is ArtifactSourceVersion version
            && string.Equals(version.Branch, Branch, StringComparison.Ordinal)
            && version.Commit.SequenceEqual(Commit);
    }
}
