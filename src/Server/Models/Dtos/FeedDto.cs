using Rtfx.Server.Database.Entities;
using Rtfx.Server.Services;
using System.Diagnostics;

namespace Rtfx.Server.Models.Dtos;

[DebuggerDisplay("{Name}")]
public sealed class FeedDto
{
    public required string Id { get; init; }

    public required string Name { get; init; }

    public required DateTime CreationDate { get; init; }

    public static FeedDto Create(Feed feed, IIdHashingService idHashingService)
    {
        return new FeedDto
        {
            Id = idHashingService.EncodeId(feed.FeedId, IdType.Feed),
            Name = feed.Name,
            CreationDate = feed.CreationDate,
        };
    }
}