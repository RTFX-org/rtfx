using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace Rtfx.Server.Database.Entities;

[DebuggerDisplay("[{PackageId}] {Name} (Feed: {FeedId})")]
public class Package
{
    public long PackageId { get; init; }

    [Required]
    [MaxLength(255)]
    public required string Name { get; set; }

    public long FeedId { get; set; }

    [Required]
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;

    [Required]
    public required Feed Feed { get; set; }

    public List<Artifact> Artifacts { get; init; } = new();
}