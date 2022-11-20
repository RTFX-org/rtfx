using Rtfx.Server.Database.Entities;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Rtfx.Server.Models.Dtos;

[DebuggerDisplay("{Name}")]
public sealed class FeedDto
{
    public required long Id { get; init; }

    public required string Name { get; init; }

    public required DateTime CreationDate { get; init; }

    [JsonPropertyName("_links")]
    public required FeedLinksDto Links { get; init; }

    public static FeedDto Create(HttpContext context, Feed feed)
    {
        var baseUri = context.GetBaseUri();

        return new FeedDto
        {
            Id = feed.FeedId,
            Name = feed.Name,
            CreationDate = feed.CreationDate,
            Links = new()
            {
                Feed = new Uri(baseUri, $"api/feeds/{feed.FeedId}"),
            },
        };
    }
}

public sealed class FeedLinksDto
{
    public required Uri Feed { get; init; }
}