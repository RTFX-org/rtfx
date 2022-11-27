using FluentValidation;
using FluentValidation.Results;
using Rtfx.Server.Models;
using Rtfx.Server.Models.Dtos;
using Rtfx.Server.Repositories;
using Rtfx.Server.Services;

namespace Rtfx.Server.Endpoints.Package;

public sealed partial record GetPackageRequest(string PackageId);

public sealed record GetPackageResponse(PackageDto Package);

public sealed class GetPackageRequestValidator : Validator<GetPackageRequest>
{
    public GetPackageRequestValidator()
    {
        RuleFor(x => x.PackageId)
            .NotEmpty()
            .Matches(RegularExpressions.IdHash());
    }
}

public sealed class GetPackageEndpoint : Endpoint<GetPackageRequest, GetPackageResponse>
{
    private readonly IPackageRepository _packageRepository;
    private readonly IIdHashingService _idHashingService;

    public GetPackageEndpoint(IPackageRepository packageRepository, IIdHashingService idHashingService)
    {
        _packageRepository = packageRepository;
        _idHashingService = idHashingService;
    }

    public override void Configure()
    {
        Get("/packages/{PackageId}");
        Description(x => x
            .WithTags("Packages")
            .ProducesProblemRtfx(Status400BadRequest)
            .ProducesProblemRtfx(Status404NotFound));
        Summary(x =>
        {
            x.Summary = "Gets a package.";
            x.Responses[Status200OK] = "The package was found.";
            x.Responses[Status404NotFound] = "The feed or package was not found.";
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

    public override async Task HandleAsync(GetPackageRequest req, CancellationToken ct)
    {
        if (!_idHashingService.TryDecodeId(req.PackageId, IdType.Package, out long packageId))
        {
            await this.SendErrorAsync(Status400BadRequest, GetInvalidPackageIdHashError(req.PackageId), ct);
            return;
        }

        var package = await _packageRepository.TryGetPackageAsync(packageId, ct);
        if (package is null)
        {
            await this.SendErrorAsync(Status404NotFound, GetPackageWithIdDoesNotExistError(req.PackageId), ct);
            return;
        }

        await SendOkAsync(new GetPackageResponse(PackageDto.Create(package, _idHashingService)), ct);
    }

    private static ValidationFailure GetInvalidPackageIdHashError(string packageId)
        => Errors.InvalidIdHash.GetError(packageId).WithPropertyName(nameof(GetPackageRequest.PackageId));

    private static ValidationFailure GetPackageWithIdDoesNotExistError(string packageId)
        => Errors.PackageWithIdDoesNotExist.GetError(packageId).WithPropertyName(nameof(GetPackageRequest.PackageId));
}
