using FluentValidation.Results;
using Rtfx.Server.Models;

namespace Rtfx.Server.Services;

public interface IArtifactValidationService
{
    IEnumerable<ValidationFailure> GetExampleErrors();

    Task<ArtifactValidationResult> ValidateAsync(string artifactFileName, Stream artifactStream);
}
