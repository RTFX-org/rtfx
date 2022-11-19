using Rtfx.Server.Database.Entities;

namespace Rtfx.Server.Repositories;

public interface IPackageRepository
{
    Task<long> TryGetPackageIdAsync(long feedId, string packageName, CancellationToken ct);
    Task<long> InsertPackageAsync(Package package, CancellationToken ct);
}
