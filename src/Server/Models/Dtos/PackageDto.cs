using Rtfx.Server.Database.Entities;
using Rtfx.Server.Services;
using System.Diagnostics;

namespace Rtfx.Server.Models.Dtos;

[DebuggerDisplay("{Name}")]
public sealed class PackageDto
{
    public required string FeedId { get; init; }

    public required string FeedName { get; init; }

    public required string Id { get; init; }

    public required string Name { get; init; }

    public required DateTime CreationDate { get; init; }

    public static PackageDto Create(Package package, IIdHashingService idHashingService)
    {
        return new PackageDto
        {
            FeedId = idHashingService.EncodeId(package.Feed.FeedId, IdType.Feed),
            FeedName = package.Feed.Name,
            Id = idHashingService.EncodeId(package.PackageId, IdType.Package),
            Name = package.Name,
            CreationDate = package.CreationDate,
        };
    }
}