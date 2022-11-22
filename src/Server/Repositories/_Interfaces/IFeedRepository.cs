using Rtfx.Server.Database.Entities;

namespace Rtfx.Server.Repositories;

public interface IFeedRepository
{
    Task<bool> GetFeedExistAsync(long feedId, CancellationToken ct);
    Task<bool> GetFeedExistAsync(string feedName, CancellationToken ct);
    Task<Feed?> TryGetFeedAsync(long feedId, CancellationToken ct);
    Task<Feed?> TryGetFeedAsync(string feedName, CancellationToken ct);
    Task<long> TryGetFeedIdAsync(string feedName, CancellationToken ct);
    Task<long> InsertFeedAsync(Feed feed, CancellationToken ct);
    Task RemoveFeedAsync(long feedId, CancellationToken ct);
    IQueryable<Feed> GetFeeds(int skip, int take);
}
