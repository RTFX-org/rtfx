using MaSch.Core;
using Microsoft.EntityFrameworkCore;
using Rtfx.Server.Database;
using Rtfx.Server.Database.Entities;

namespace Rtfx.Server.Repositories;

public class FeedRepository : IFeedRepository
{
    private readonly DatabaseContext _database;

    public FeedRepository(DatabaseContext database)
    {
        _database = database;
    }

    public async Task<bool> GetFeedExistAsync(long feedId, CancellationToken ct)
    {
        return await _database.Feeds.AnyAsync(x => x.FeedId == feedId, ct);
    }

    public async Task<bool> GetFeedExistAsync(string feedName, CancellationToken ct)
    {
        return await _database.Feeds.AnyAsync(x => x.Name == feedName, ct);
    }

    public IQueryable<Feed> GetFeeds(int skip, int take)
    {
        return _database.Feeds
            .OrderBy(x => x.CreationDate)
            .Skip(skip)
            .Take(take);
    }

    public async Task<long> InsertFeedAsync(Feed feed, CancellationToken ct)
    {
        if (feed.FeedId != 0)
            throw new ArgumentException("The feed already defines an id. Make sure Feed.FeedId is zero.", nameof(feed));

        _database.Feeds.Add(feed);
        await _database.SaveChangesAsync(ct);

        return feed.FeedId;
    }

    public async Task RemoveFeedAsync(long feedId, CancellationToken ct)
    {
        Guard.NotSmallerThan(feedId, 1);

        var feedToRemove = new Feed
        {
            FeedId = feedId,
            Name = null!,
        };
        _database.Feeds.Remove(feedToRemove);
        await _database.SaveChangesAsync(ct);
    }

    public async Task<Feed?> TryGetFeedAsync(long feedId, CancellationToken ct)
    {
        return await _database.Feeds.FirstOrDefaultAsync(x => x.FeedId == feedId, ct);
    }

    public async Task<Feed?> TryGetFeedAsync(string feedName, CancellationToken ct)
    {
        return await _database.Feeds.FirstOrDefaultAsync(x => x.Name == feedName, ct);
    }

    public async Task<long> TryGetFeedIdAsync(string feedName, CancellationToken ct)
    {
        return await _database.Feeds.Where(x => x.Name == feedName).Select(x => x.FeedId).FirstOrDefaultAsync(ct);
    }
}
