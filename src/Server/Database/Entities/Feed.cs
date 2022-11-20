using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace Rtfx.Server.Database.Entities;

[DebuggerDisplay("[{FeedId}] {Name}")]
public class Feed
{
    public long FeedId { get; init; }

    [Required]
    [MaxLength(255)]
    public required string Name { get; set; }

    [Required]
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;

    public List<Package> Packages { get; init; } = new();
}