using Rtfx.Server.Database.Entities;
using System.Diagnostics;

namespace Rtfx.Server.Models.Dtos;

[DebuggerDisplay("{Name}")]
public sealed class PackageDto
{
    public required long Id { get; init; }

    public required string Name { get; init; }

    public required DateTime CreationDate { get; init; }

    public static PackageDto Create(Package package)
    {
        return new PackageDto
        {
            Id = package.PackageId,
            Name = package.Name,
            CreationDate = package.CreationDate,
        };
    }
}