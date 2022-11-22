using FluentValidation;
using Rtfx.Server.Models;
using Rtfx.Server.Models.Dtos;
using Rtfx.Server.Repositories;

namespace Rtfx.Server.Endpoints.Artifact;

public sealed partial record GetArtifactRequest(long ArtifactId);

public sealed record GetArtifactResponse(ArtifactInfoDto Artifact);

public sealed class GetArtifactRequestValidator : Validator<GetArtifactRequest>
{
    public GetArtifactRequestValidator()
    {
        RuleFor(x => x.ArtifactId)
            .GreaterThan(0);
    }
}

public class GetArtifactEndpoint : Endpoint<GetArtifactRequest, GetArtifactResponse>
{
    private readonly IArtifactRepository _artifactRepository;

    public GetArtifactEndpoint(IArtifactRepository artifactRepository)
    {
        _artifactRepository = artifactRepository;
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
            x.ResponseExamples[Status404NotFound] = new RtfxErrorResponse
            {
                Errors.ArtifactWithIdDoesNotExist.GetError(1337),
            };
        });
    }

    public override async Task HandleAsync(GetArtifactRequest req, CancellationToken ct)
    {
        var artifact = await _artifactRepository.TryGetArtifactWithMetadataAsync(req.ArtifactId, ct);
        if (artifact is null)
        {
            await this.SendErrorAsync(Status404NotFound, Errors.ArtifactWithIdDoesNotExist.GetError(req.ArtifactId), ct);
            return;
        }

        await SendOkAsync(new GetArtifactResponse(ArtifactInfoDto.Create(artifact)), ct);
    }
}
