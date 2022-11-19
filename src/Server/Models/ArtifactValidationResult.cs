using FluentValidation.Results;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Rtfx.Server.Models;

[DebuggerDisplay("IsValid = {IsValid}, Errors: {Errors.Count}")]
public sealed class ArtifactValidationResult
{
    public ArtifactValidationResult(IList<ValidationFailure> errors, ArtifactMetadata? metadata)
    {
        Errors = new ReadOnlyCollection<ValidationFailure>(errors);
        Metadata = metadata;
    }

    public IReadOnlyList<ValidationFailure> Errors { get; }
    public bool IsValid => Errors.Count == 0;
    public ArtifactMetadata? Metadata { get; }
}
