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

    public async Task<ArtifactValidationResult> ValidateAsync(string artifactFileName, Stream artifactStream)
    {
        var errors = new List<ValidationFailure>();

        if (!string.Equals(Path.GetExtension(artifactFileName), ".rtfct", StringComparison.OrdinalIgnoreCase))
        {
            errors.Add(new ValidationFailure(ArtifactPropertyName, "The artifact has the wrong file extension."));
            return new ArtifactValidationResult(errors, null);
        }

        using var zipFile = new ZipArchive(artifactStream, ZipArchiveMode.Read, leaveOpen: true);

        var metadata = await ValidateMetadataAsync(zipFile, errors);
        if (metadata is null)
            return new ArtifactValidationResult(errors, null);

        return new ArtifactValidationResult(errors, metadata);
    }

    private async Task<ArtifactMetadata?> ValidateMetadataAsync(ZipArchive artifact, List<ValidationFailure> errors)
    {
        var metadataEntry = artifact.GetEntry("artifact.metadata");
        if (metadataEntry is null)
        {
            errors.Add(new ValidationFailure(MetadataPropertyName, "The artifact does not contain artifact metadata. Make sure the artifact contains a file named \"artifact.metadata\"."));
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
            errors.Add(new ValidationFailure(MetadataPropertyName, $"Failed to parse artifact metadata: {ex.Message}"));
            return null;
        }

        if (metadata is null)
        {
            errors.Add(new ValidationFailure(MetadataPropertyName, $"Parsing artifact metadata lead to null value. Make sure the artifact.metadata file contains valid JSON data."));
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
