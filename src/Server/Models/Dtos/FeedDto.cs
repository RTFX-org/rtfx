using Rtfx.Server.Database.Entities;
using System.Diagnostics;

namespace Rtfx.Server.Models.Dtos;

[DebuggerDisplay("{Name}")]
public sealed class FeedDto
{
    public required long Id { get; init; }

    public required string Name { get; init; }

    public required DateTime CreationDate { get; init; }

    public static FeedDto Create(Feed feed)
    {
        return new FeedDto
        {
            Id = feed.FeedId,
            Name = feed.Name,
            CreationDate = feed.CreationDate,
        };
    }
}