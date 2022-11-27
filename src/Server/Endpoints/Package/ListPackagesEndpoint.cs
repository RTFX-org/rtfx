using FluentValidation;
using FluentValidation.Results;
using MaSch.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using Rtfx.Server.Database.Entities;
using Rtfx.Server.Models;
using Rtfx.Server.Models.Dtos;
using Rtfx.Server.Repositories;
using Rtfx.Server.Services;
using System.ComponentModel;

namespace Rtfx.Server.Endpoints.Package;

public sealed partial record ListPackagesRequest(
    string FeedId,
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
            .NotEmpty()
            .Matches(RegularExpressions.IdHash());
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
    private readonly IIdHashingService _idHashingService;

    public ListPackagesEndpoint(IFeedRepository feedRepository, IPackageRepository packageRepository, IIdHashingService idHashingService)
    {
        _feedRepository = feedRepository;
        _packageRepository = packageRepository;
        _idHashingService = idHashingService;
    }

    public override void Configure()
    {
        Get("/feeds/{FeedId}/packages");
        Description(x => x
            .WithTags("Packages")
            .ProducesProblemRtfx(Status400BadRequest)
            .ProducesProblemRtfx(Status404NotFound));
        Summary(x =>
        {
            x.Summary = "Lists all available packages in a feed.";
            x.Responses[Status200OK] = "The feed was found and the list of packages has been successfully retrieved";
            x.Responses[Status404NotFound] = "The feed was not found.";
            x.ResponseExamples[Status400BadRequest] = new RtfxErrorResponse
            {
                GetInvalidFeedIdHashError("[FeedId]"),
                RtfxError.DefaultExample,
            };
            x.ResponseExamples[Status404NotFound] = new RtfxErrorResponse
            {
                GetFeedWithIdDoesNotExistError("[FeedId]"),
            };
        });
    }

    public override async Task HandleAsync(ListPackagesRequest req, CancellationToken ct)
    {
        if (!_idHashingService.TryDecodeId(req.FeedId, IdType.Feed, out long feedId))
        {
            await this.SendErrorAsync(Status400BadRequest, GetInvalidFeedIdHashError(req.FeedId), ct);
            return;
        }

        var feedExists = await _feedRepository.GetFeedExistAsync(feedId, ct);
        if (!feedExists)
        {
            await this.SendErrorAsync(Status404NotFound, GetFeedWithIdDoesNotExistError(req.FeedId), ct);
            return;
        }

        var packages = await _packageRepository
            .GetPackages(feedId, req.Skip ?? 0, req.Take ?? 25)
            .Select(x => PackageDto.Create(x, _idHashingService))
            .ToArrayAsync();

        await SendOkAsync(new ListPackagesResponse(packages), ct);
    }

    private static ValidationFailure GetInvalidFeedIdHashError(string id)
        => Errors.InvalidIdHash.GetError(id).WithPropertyName(nameof(ListPackagesRequest.FeedId));

    private static ValidationFailure GetFeedWithIdDoesNotExistError(string id)
        => Errors.FeedWithIdDoesNotExist.GetError(id).WithPropertyName(nameof(ListPackagesRequest.FeedId));
}
