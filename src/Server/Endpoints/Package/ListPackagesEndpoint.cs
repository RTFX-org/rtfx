using FluentValidation;
using MaSch.Core.Extensions;
using Rtfx.Server.Models.Dtos;
using Rtfx.Server.Repositories;
using System.ComponentModel;

namespace Rtfx.Server.Endpoints.Package;

public sealed partial record ListPackagesRequest(
    long FeedId,
    [property: DefaultValue(0)]
    int? Skip,
    [property: DefaultValue(25)]
    int? Take);

public sealed record ListPackagesResponse(PackageDto[] Packages);

public sealed class ListPackagesRequestValidator : Validator<ListPackagesRequest>
{
    public ListPackagesRequestValidator()
    {
        RuleFor(x => x.FeedId)
            .GreaterThan(0);
        RuleFor(x => x.Skip)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Skip.HasValue);
        RuleFor(x => x.Take)
            .GreaterThan(0)
            .When(x => x.Take.HasValue);
    }
}

public class ListPackagesEndpoint : Endpoint<ListPackagesRequest, ListPackagesResponse>
{
    private readonly IFeedRepository _feedRepository;
    private readonly IPackageRepository _packageRepository;

    public ListPackagesEndpoint(IFeedRepository feedRepository, IPackageRepository packageRepository)
    {
        _feedRepository = feedRepository;
        _packageRepository = packageRepository;
    }

    public override void Configure()
    {
        Get("/feeds/{FeedId}/packages");
        Description(x => x
            .WithTags("Packages")
            .ProducesProblemFE(Status400BadRequest)
            .ProducesProblemFE(Status404NotFound));
        Summary(x =>
        {
            x.Summary = "Lists all available packages in a feed.";
            x.Responses[Status200OK] = "The feed was found and the list of packages has ben successfully retrieved";
            x.Responses[Status404NotFound] = "The feed was not found.";
        });
    }

    public override async Task HandleAsync(ListPackagesRequest req, CancellationToken ct)
    {
        var feedExists = await _feedRepository.GetFeedExistAsync(req.FeedId, ct);
        if (!feedExists)
        {
            await this.SendErrorAsync(Status404NotFound, ErrorMessages.FeedWithIdDoesNotExist(req.FeedId), ct);
            return;
        }

        var packages = await _packageRepository
            .GetPackages(req.FeedId, req.Skip ?? 0, req.Take ?? 25)
            .Select(x => PackageDto.Create(x))
            .ToArrayAsync();

        await SendOkAsync(new ListPackagesResponse(packages), ct);
    }
}
