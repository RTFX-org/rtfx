using FluentValidation;
using FluentValidation.Results;
using Rtfx.Server.Endpoints.Package;
using Rtfx.Server.Models;
using Rtfx.Server.Models.Dtos;
using Rtfx.Server.Repositories;
using Rtfx.Server.Services;

namespace Rtfx.Server.Endpoints.Artifact;

public sealed partial record GetArtifactRequest(string ArtifactId);

public sealed record GetArtifactResponse(ArtifactInfoDto Artifact);

public sealed class GetArtifactRequestValidator : Validator<GetArtifactRequest>
{
    public GetArtifactRequestValidator()
    {
        RuleFor(x => x.ArtifactId)
            .NotEmpty()
            .Matches(RegularExpressions.IdHash());
    }
}

public class GetArtifactEndpoint : Endpoint<GetArtifactRequest, GetArtifactResponse>
{
    private readonly IArtifactRepository _artifactRepository;
    private readonly IIdHashingService _idHashingService;

    public GetArtifactEndpoint(IArtifactRepository artifactRepository, IIdHashingService idHashingService)
    {
        _artifactRepository = artifactRepository;
        _idHashingService = idHashingService;
    }

    public override void Configure()
    {
        Get("/artifacts/{ArtifactId}");
        Description(x => x
            .WithTags("Artifacts")
            .ProducesProblemRtfx(Status400BadRequest)
            .ProducesProblemRtfx(Status404NotFound));
        Summary(x =>
        {
            x.Summary = "Geta information about an artifact.";
            x.Responses[Status200OK] = "The artifact was found.";
            x.Responses[Status404NotFound] = "The artifact was not found.";
            x.ResponseExamples[Status400BadRequest] = new RtfxErrorResponse
            {
                GetInvalidArtifactIdHashError("[ArtifactId]"),
                RtfxError.DefaultExample,
            };
            x.ResponseExamples[Status404NotFound] = new RtfxErrorResponse
            {
                GetArtifactWithIdDoesNotExistError("[ArtifactId]"),
            };
        });
    }

    public override async Task HandleAsync(GetArtifactRequest req, CancellationToken ct)
    {
        if (!_idHashingService.TryDecodeId(req.ArtifactId, out long artifactId))
        {
            await this.SendErrorAsync(Status400BadRequest, GetInvalidArtifactIdHashError(req.ArtifactId), ct);
            return;
        }

        var artifact = await _artifactRepository.TryGetArtifactWithMetadataAsync(artifactId, ct);
        if (artifact is null)
        {
            await this.SendErrorAsync(Status404NotFound, GetArtifactWithIdDoesNotExistError(req.ArtifactId), ct);
            return;
        }

        await SendOkAsync(new GetArtifactResponse(ArtifactInfoDto.Create(artifact, _idHashingService)), ct);
    }

    private static ValidationFailure GetInvalidArtifactIdHashError(string id)
        => Errors.InvalidIdHash.GetError(id).WithPropertyName(nameof(GetArtifactRequest.ArtifactId));

    private static ValidationFailure GetArtifactWithIdDoesNotExistError(string id)
        => Errors.ArtifactWithIdDoesNotExist.GetError(id).WithPropertyName(nameof(GetArtifactRequest.ArtifactId));
}
