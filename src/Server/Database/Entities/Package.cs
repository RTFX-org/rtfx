using System.ComponentModel.DataAnnotations;

namespace Rtfx.Server.Database.Entities;

public class Package
{
    public Guid PackageId { get; init; }

    [Required]
    [MaxLength(255)]
    public required string Name { get; set; }

    public Guid FeedId { get; init; }

    [Required]
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;

    [Required]
    public required Feed Feed { get; init; }

    public List<Artifact> Artifacts { get; init; } = new();
}