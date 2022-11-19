using FluentValidation.Results;
using Rtfx.Server.Models;
using System.IO.Compression;
using System.Text.Json;

namespace Rtfx.Server.Services;

public class ArtifactValidationService : IArtifactValidationService
{
    public async Task<ArtifactValidationResult> ValidateAsync(string artifactPath)
    {
        var errors = new List<ValidationFailure>();

        if (!string.Equals(Path.GetExtension(artifactPath), ".rtfct", StringComparison.OrdinalIgnoreCase))
        {
            errors.Add(new ValidationFailure("Artifact", "The artifact has the wrong file extension."));
            return new ArtifactValidationResult(errors, null);
        }

        using var zipFile = ZipFile.OpenRead(artifactPath);

        ArtifactMetadata? metadata = null; //TODO: await ValidateMetadataAsync(zipFile, errors);
        if (metadata is null)
            return new ArtifactValidationResult(errors, null);

        return new ArtifactValidationResult(errors, metadata);
    }

    // TODO:
    //private static async Task<ArtifactMetadata?> ValidateMetadataAsync(ZipArchive artifact, List<ValidationFailure> errors)
    //{
    //    var metadataEntry = artifact.GetEntry("artifact.metadata");
    //    if (metadataEntry is null)
    //    {
    //        errors.Add(new ValidationFailure("Artifact.Metadata", "The artifact does not contain artifact metadata. Make sure the artifact contains a file named \"artifact.metadata\"."));
    //        return null;
    //    }

    //    ArtifactMetadata? metadata;
    //    try
    //    {
    //        using var entryStream = metadataEntry.Open();
    //        metadata = await JsonSerializer.DeserializeAsync<ArtifactMetadata>(entryStream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    //    }
    //    catch (JsonException ex)
    //    {
    //        throw new Exception($"TODO: Failed to parse artifact metadata: {ex.Message}");
    //    }

    //    if (metadata is null)
    //    {
    //        errors.Add()
    //        return null;
    //    }

    //    var validator = new ArtifactMetadataValidator();
    //    var validationResult = validator.Validate(metadata);
    //    if (!validationResult.IsValid)
    //    {
    //        validationResult.Errors.Dump();
    //        throw new Exception($"TODO: Validation errors in artifact metadata: \n{string.Join("\n", validationResult.Errors.Select(x => x.ErrorMessage))}");
    //    }
    //}
}
