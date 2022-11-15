using System.ComponentModel.DataAnnotations;

namespace Rtfx.Server.Database.Entities;

public class Feed
{
    public Guid FeedId { get; init; }

    [Required]
    [MaxLength(255)]
    public required string Name { get; set; }

    [Required]
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;

    public List<Package> Packages { get; init; } = new();
}