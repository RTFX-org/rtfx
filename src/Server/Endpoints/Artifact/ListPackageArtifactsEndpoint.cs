using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Rtfx.Server.Models;
using Rtfx.Server.Models.Dtos;
using Rtfx.Server.Repositories;
using Rtfx.Server.Services;
using System.ComponentModel;

namespace Rtfx.Server.Endpoints.Artifact;

public sealed partial record ListPackageArtifactsRequest(
    string PackageId,
    [property: DefaultValue(0)]
    int? Skip,
    [property: DefaultValue(25)]
    int? Take,
    [property: DefaultValue(false)]
    bool? IncludeMetadata);

public sealed record ListPackageArtifactsResponse(ArtifactInfoDto[] Artifacts);

public sealed class ListPackageArtifactsRequestValidator : Validator<ListPackageArtifactsRequest>
{
    public ListPackageArtifactsRequestValidator()
    {
        RuleFor(x => x.PackageId)
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

public sealed class ListPackageArtifactsEndpoint : Endpoint<ListPackageArtifactsRequest, ListPackageArtifactsResponse>
{
    private readonly IPackageRepository _packageRepository;
    private readonly IArtifactRepository _artifactRepository;
    private readonly IIdHashingService _idHashingService;

    public ListPackageArtifactsEndpoint(
        IPackageRepository packageRepository,
        IArtifactRepository artifactRepository,
        IIdHashingService idHashingService)
    {
        _packageRepository = packageRepository;
        _artifactRepository = artifactRepository;
        _idHashingService = idHashingService;
    }

    public override void Configure()
    {
        Get("/packages/{PackageId}/artifacts");
        Description(x => x
            .WithTags("Artifacts")
            .ProducesProblemRtfx(Status400BadRequest)
            .ProducesProblemRtfx(Status404NotFound));
        Summary(x =>
        {
            x.Summary = "Lists all available artifacts of a packages.";
            x.Responses[Status200OK] = "The package was found and the list of artifacts has been successfully retrieved";
            x.Responses[Status404NotFound] = "The package was not found.";
            x.ResponseExamples[Status400BadRequest] = new RtfxErrorResponse
            {
                GetInvalidPackageIdHashError("[PackageId]"),
                RtfxError.DefaultExample,
            };
            x.ResponseExamples[Status404NotFound] = new RtfxErrorResponse
            {
                GetPackageWithIdDoesNotExistError("[PackageId]"),
            };
        });
    }

    public override async Task HandleAsync(ListPackageArtifactsRequest req, CancellationToken ct)
    {
        if (!_idHashingService.TryDecodeId(req.PackageId, IdType.Package, out long packageId))
        {
            await this.SendErrorAsync(Status400BadRequest, GetInvalidPackageIdHashError(req.PackageId), ct);
            return;
        }

        var packageExists = await _packageRepository.GetPackageExistAsync(packageId, ct);
        if (!packageExists)
        {
            await this.SendErrorAsync(Status404NotFound, GetPackageWithIdDoesNotExistError(req.PackageId), ct);
            return;
        }

        var artifactQuery = req.IncludeMetadata == true
            ? _artifactRepository.GetArtifactsWithMetadata(packageId, req.Skip ?? 0, req.Take ?? 25)
            : _artifactRepository.GetArtifacts(packageId, req.Skip ?? 0, req.Take ?? 25);

        var artifacts = await artifactQuery
            .Select(x => ArtifactInfoDto.Create(x, _idHashingService, req.IncludeMetadata == true))
            .ToArrayAsync();

        await SendOkAsync(new ListPackageArtifactsResponse(artifacts), ct);
    }

    private static ValidationFailure GetInvalidPackageIdHashError(string packageId)
        => Errors.InvalidIdHash.GetError(packageId).WithPropertyName(nameof(ListPackageArtifactsRequest.PackageId));

    private static ValidationFailure GetPackageWithIdDoesNotExistError(string packageId)
        => Errors.PackageWithIdDoesNotExist.GetError(packageId).WithPropertyName(nameof(ListPackageArtifactsRequest.PackageId));
}
