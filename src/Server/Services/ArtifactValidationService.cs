using FluentValidation;
using FluentValidation.Results;
using Rtfx.Server.Models;
using System.IO.Compression;
using System.Text.Json;

namespace Rtfx.Server.Services;

public sealed class ArtifactValidationService : IArtifactValidationService
{
    private const string ArtifactPropertyName = "Artifact";
    private const string MetadataPropertyName = $"{ArtifactPropertyName}.Metadata";

    private readonly AbstractValidator<ArtifactMetadata> _metadataValidator = new ArtifactMetadataValidator();

    public IEnumerable<ValidationFailure> GetExampleErrors()
    {
        return new[]
        {
            Errors.ArtifactHasWrongFileExtension.GetError("file.zip").WithPropertyName(ArtifactPropertyName),
            Errors.ArtifactHasNoMetadata.GetError("file.rtfct").WithPropertyName(MetadataPropertyName),
            Errors.ArtifactMetadataParseFailure.GetError("file.rtfct", "[ExceptionMessage]").WithPropertyName(MetadataPropertyName),
            Errors.ArtifactMetadataParsedToNull.GetError("file.rtfct").WithPropertyName(MetadataPropertyName),
            new ValidationFailure()
            {
                PropertyName = $"{MetadataPropertyName}.[...]",
                ErrorCode = "string",
                ErrorMessage = "string",
                AttemptedValue = "any",
            },
        };
    }

    public async Task<ArtifactValidationResult> ValidateAsync(string artifactFileName, Stream artifactStream)
    {
        var errors = new List<ValidationFailure>();

        if (!string.Equals(Path.GetExtension(artifactFileName), ".rtfct", StringComparison.OrdinalIgnoreCase))
        {
            errors.Add(Errors.ArtifactHasWrongFileExtension.GetError(artifactFileName).WithPropertyName(ArtifactPropertyName));
            return new ArtifactValidationResult(errors, null);
        }

        using var zipFile = new ZipArchive(artifactStream, ZipArchiveMode.Read, leaveOpen: true);

        var metadata = await ValidateMetadataAsync(artifactFileName, zipFile, errors);
        if (metadata is null)
            return new ArtifactValidationResult(errors, null);

        return new ArtifactValidationResult(errors, metadata);
    }

    private async Task<ArtifactMetadata?> ValidateMetadataAsync(string fileName, ZipArchive artifact, List<ValidationFailure> errors)
    {
        var metadataEntry = artifact.GetEntry("artifact.metadata");
        if (metadataEntry is null)
        {
            errors.Add(Errors.ArtifactHasNoMetadata.GetError(fileName).WithPropertyName(MetadataPropertyName));
            return null;
        }

        ArtifactMetadata? metadata;
        try
        {
            using var entryStream = metadataEntry.Open();
            metadata = await JsonSerializer.DeserializeAsync<ArtifactMetadata>(entryStream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (JsonException ex)
        {
            errors.Add(Errors.ArtifactMetadataParseFailure.GetError(fileName, ex.Message).WithPropertyName(MetadataPropertyName));
            return null;
        }

        if (metadata is null)
        {
            errors.Add(Errors.ArtifactMetadataParsedToNull.GetError(fileName).WithPropertyName(MetadataPropertyName));
            return null;
        }

        var metadataValidationResult = _metadataValidator.Validate(metadata);
        if (!metadataValidationResult.IsValid)
        {
            foreach (var error in metadataValidationResult.Errors)
                error.PropertyName = $"{MetadataPropertyName}.{error.PropertyName}";

            errors.AddRange(metadataValidationResult.Errors);
            return null;
        }

        return metadata;
    }
}
