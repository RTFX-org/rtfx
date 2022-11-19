using Rtfx.Server.Models;

namespace Rtfx.Server.Services;

public interface IArtifactValidationService
{
    Task<ArtifactValidationResult> ValidateAsync(string artifactPath);
}
