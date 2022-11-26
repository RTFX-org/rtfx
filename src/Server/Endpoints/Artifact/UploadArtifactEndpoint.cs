using FluentValidation;
using FluentValidation.Results;
using MaSch.Core.Extensions;
using Rtfx.Server.Models;
using Rtfx.Server.Models.Dtos;
using Rtfx.Server.Repositories;
using Rtfx.Server.Services;
using System.ComponentModel;

namespace Rtfx.Server.Endpoints.Artifact;

public sealed partial record UploadArtifactRequest(
    IFormFile Artifact,
    [property: DefaultValue(false)]
    bool? CreateFeedAndPackage,
    [property: DefaultValue(false)]
    bool? OverwriteExisting);

public sealed record UploadArtifactResponse(ArtifactInfoDto Artifact);

public sealed class UploadArtifactRequestValidator : Validator<UploadArtifactRequest>
{
    public UploadArtifactRequestValidator()
    {
        RuleFor(x => x.Artifact)
            .NotEmpty();
    }
}

public class UploadArtifactEndpoint : Endpoint<UploadArtifactRequest, UploadArtifactResponse>
{
    private readonly IArtifactRepository _artifactRepository;
    private readonly IArtifactValidationService _artifactValidationService;
    private readonly IArtifactStorageService _artifactStorageService;
    private readonly IIdHashingService _idHashingService;

    public UploadArtifactEndpoint(
        IArtifactRepository artifactRepository,
        IArtifactValidationService artifactValidationService,
        IArtifactStorageService artifactStorageService,
        IIdHashingService idHashingService)
    {
        _artifactRepository = artifactRepository;
        _artifactValidationService = artifactValidationService;
        _artifactStorageService = artifactStorageService;
        _idHashingService = idHashingService;
    }

    public override void Configure()
    {
        Put("/artifacts/upload");
        AllowFileUploads();
        Description(x => x
            .WithTags("Artifacts")
            .Produces<UploadArtifactResponse>(Status201Created)
            .ProducesProblemRtfx(Status400BadRequest)
            .ProducesProblemRtfx(Status404NotFound)
            .ProducesProblemRtfx(Status409Conflict));
        Summary(x =>
        {
            x.Summary = "Stores an artifact.";
            x.Responses[Status200OK] = "The artifact has been successfully modified.";
            x.Responses[Status201Created] = "The artifact has been successfully created.";
            x.Responses[Status404NotFound] = "The feed or package was not found.";
            x.Responses[Status409Conflict] = "The artifact already exists but the overwriteExisting query parameter is not set to true.";
            x.ResponseExamples[Status400BadRequest] = new RtfxErrorResponse
            {
                _artifactValidationService.GetExampleErrors(),
                RtfxError.DefaultExample,
            };
            x.ResponseExamples[Status404NotFound] = new RtfxErrorResponse
            {
                GetFeedWithNameDoesNotExistError("[FeedName]"),
                GetPackageWithNameDoesNotExistError("[FeedName]", "[PackageName]"),
            };
            x.ResponseExamples[Status409Conflict] = new RtfxErrorResponse
            {
                GetArtifactAlreadyExistsError("[FeedName]", "[PackageName]", "[SourceHash]"),
            };
        });
    }

    public override async Task HandleAsync(UploadArtifactRequest req, CancellationToken ct)
    {
        using var artifactStream = req.Artifact.OpenReadStream();

        var validationResult = await _artifactValidationService.ValidateAsync(req.Artifact.FileName, artifactStream);
        artifactStream.Seek(0, SeekOrigin.Begin);

        if (!validationResult.IsValid)
        {
            ValidationFailures.AddRange(validationResult.Errors);
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        var metadata = validationResult.Metadata!;
        var (feedId, packageId, artifactId) = await _artifactRepository.TryGetIdsAsync(metadata.FeedName, metadata.PackageName, metadata.SourceHash, ct);

        if (feedId <= 0 && req.CreateFeedAndPackage != true)
        {
            await this.SendErrorAsync(Status404NotFound, GetFeedWithNameDoesNotExistError(metadata.FeedName), ct);
            return;
        }

        if (packageId <= 0 && req.CreateFeedAndPackage != true)
        {
            await this.SendErrorAsync(Status404NotFound, GetPackageWithNameDoesNotExistError(metadata.FeedName, metadata.PackageName), ct);
            return;
        }

        if (artifactId > 0 && req.OverwriteExisting != true)
        {
            await this.SendErrorAsync(Status409Conflict, GetArtifactAlreadyExistsError(metadata.FeedName, metadata.PackageName, metadata.SourceHash), ct);
            return;
        }

        await _artifactStorageService.SaveArtifact(feedId, packageId, artifactId, artifactStream);

        Db.Artifact artifact;
        int statusCode;
        if (artifactId == 0)
        {
            artifact = new Db.Artifact
            {
                ArtifactId = artifactId,
                SourceHash = Convert.FromHexString(metadata.SourceHash),
                Package = new Db.Package
                {
                    PackageId = packageId,
                    Name = metadata.PackageName,
                    Feed = new Db.Feed
                    {
                        FeedId = feedId,
                        Name = metadata.FeedName,
                    },
                },
            };

            foreach (var tag in metadata.Tags)
                artifact.Tags.Add(new Db.ArtifactTag { Artifact = artifact, Tag = tag });
            foreach (var version in metadata.SourceVersions)
                artifact.SourceVersions.Add(new Db.ArtifactSourceVersion { Artifact = artifact, Branch = version.Branch, Commit = Convert.FromHexString(version.Commit) });
            foreach (var md in metadata.Metadata)
                artifact.Metadata.Add(new Db.ArtifactMetadata { Artifact = artifact, Name = md.Key, Value = md.Value });

            await _artifactRepository.InsertArtifact(artifact, ct);
            statusCode = Status201Created;
        }
        else
        {
            artifact = await _artifactRepository.UpdateArtifactAsync(
                artifactId,
                a =>
                {
                    a.LastModifierDate = DateTime.UtcNow;
                    foreach (var tag in metadata.Tags)
                        a.Tags.AddIfNotExists(new Db.ArtifactTag { Artifact = a, Tag = tag });
                    foreach (var version in metadata.SourceVersions)
                        a.SourceVersions.AddIfNotExists(new Db.ArtifactSourceVersion { Artifact = a, Branch = version.Branch, Commit = Convert.FromHexString(version.Commit) });
                    foreach (var md in metadata.Metadata)
                        a.Metadata.AddIfNotExists(new Db.ArtifactMetadata { Artifact = a, Name = md.Key, Value = md.Value });
                },
                ct);
            statusCode = Status200OK;
        }

        await SendAsync(new UploadArtifactResponse(ArtifactInfoDto.Create(artifact, _idHashingService)), statusCode, ct);
    }

    private static ValidationFailure GetFeedWithNameDoesNotExistError(string feedName)
        => Errors.FeedWithNameDoesNotExist.GetError(feedName).WithPropertyName($"Artifact.Metadata.{nameof(ArtifactMetadata.FeedName)}");

    private static ValidationFailure GetPackageWithNameDoesNotExistError(string feedName, string packageName)
        => Errors.PackageWithNameDoesNotExist.GetError(feedName, packageName).WithPropertyName($"Artifact.Metadata.{nameof(ArtifactMetadata.PackageName)}");

    private static ValidationFailure GetArtifactAlreadyExistsError(string feedName, string packageName, string sourceHash)
        => Errors.ArtifactAlreadyExists.GetError(feedName, packageName, sourceHash).WithPropertyName($"Artifact.Metadata.{nameof(ArtifactMetadata.SourceHash)}");
}
