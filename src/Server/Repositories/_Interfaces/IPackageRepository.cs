using Rtfx.Server.Database.Entities;

namespace Rtfx.Server.Repositories;

public interface IPackageRepository
{
    Task<bool> GetPackageExistAsync(long packageId, CancellationToken ct);
    Task<long> TryGetPackageIdAsync(long feedId, string packageName, CancellationToken ct);
    Task<long> InsertPackageAsync(Package package, CancellationToken ct);
    Task<Package?> TryGetPackageAsync(long packageId, CancellationToken ct);
    IQueryable<Package> GetPackages(long feedId, int skip, int take);
}
