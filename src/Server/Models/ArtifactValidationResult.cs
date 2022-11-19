using FluentValidation.Results;
using System.Collections.ObjectModel;

namespace Rtfx.Server.Models;

public sealed class ArtifactValidationResult
{
    public IReadOnlyList<ValidationFailure> Errors { get; }
    public bool IsValid => Errors.Count == 0;
    public ArtifactMetadata? Metadata { get; }

    public ArtifactValidationResult(IList<ValidationFailure> errors, ArtifactMetadata? metadata)
    {
        Errors = new ReadOnlyCollection<ValidationFailure>(errors);
        Metadata = metadata;
    }
}
