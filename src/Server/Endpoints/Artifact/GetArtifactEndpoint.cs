using FluentValidation;
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
            .ProducesProblemFE(Status400BadRequest)
            .ProducesProblemFE(Status404NotFound));
        Summary(x =>
        {
            x.Summary = "Geta information about an artifact.";
            x.Responses[Status200OK] = "The artifact was found.";
            x.Responses[Status404NotFound] = "The artifact was not found.";
        });
    }

    public override async Task HandleAsync(GetArtifactRequest req, CancellationToken ct)
    {
        var artifact = await _artifactRepository.TryGetArtifactWithMetadataAsync(req.ArtifactId, ct);
        if (artifact is null)
        {
            await this.SendErrorAsync(Status404NotFound, ErrorMessages.ArtifactWithIdDoesNotExist(req.ArtifactId), ct);
            return;
        }

        await SendAsync(new GetArtifactResponse(ArtifactInfoDto.Create(HttpContext, artifact)), Status200OK, ct);
    }
}
