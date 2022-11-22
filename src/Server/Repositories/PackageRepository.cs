using Microsoft.EntityFrameworkCore;
using Rtfx.Server.Database;
using Rtfx.Server.Database.Entities;

namespace Rtfx.Server.Repositories;

public class PackageRepository : IPackageRepository
{
    private readonly DatabaseContext _database;

    public PackageRepository(DatabaseContext database)
    {
        _database = database;
    }

    public IQueryable<Package> GetPackages(long feedId, int skip, int take)
    {
        return _database.Packages
            .Where(x => x.FeedId == feedId)
            .OrderBy(x => x.CreationDate)
            .Skip(skip)
            .Take(take);
    }

    public async Task<long> InsertPackageAsync(Package package, CancellationToken ct)
    {
        if (package.PackageId != 0)
            throw new ArgumentException("The package already defines an id. Make sure Package.PackageId is zero.", nameof(package));

        _database.Packages.Add(package);
        await _database.SaveChangesAsync(ct);
        return package.PackageId;
    }

    public async Task<long> TryGetPackageIdAsync(long feedId, string packageName, CancellationToken ct)
    {
        return await _database.Packages.Where(x => x.FeedId == feedId && x.Name == packageName).Select(x => x.PackageId).FirstOrDefaultAsync(ct);
    }
}
