using FluentValidation.Results;
using MaSch.Core.Extensions;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Net.Http.Headers;
using Rtfx.Server.Models;
using Rtfx.Server.Repositories;
using Rtfx.Server.Services;

namespace Rtfx.Server.Endpoints.Artifact;

public sealed partial record DownloadArtifactRequest(
    string ArtifactId,
    string[] Filter);

public sealed class DownloadArtifactEndpoint : Endpoint<DownloadArtifactRequest>
{
    private readonly IArtifactRepository _artifactRepository;
    private readonly IArtifactStorageService _artifactStorageService;
    private readonly IIdHashingService _idHashingService;

    public DownloadArtifactEndpoint(IArtifactRepository artifactRepository, IArtifactStorageService artifactStorageService, IIdHashingService idHashingService)
    {
        _artifactRepository = artifactRepository;
        _artifactStorageService = artifactStorageService;
        _idHashingService = idHashingService;
    }

    public override void Configure()
    {
        Get("artifacts/{ArtifactId}/download");
        Description(x => x
            .WithTags("Artifacts")
            .Produces(Status204NoContent)
            .ProducesProblemRtfx(Status400BadRequest)
            .ProducesProblemRtfx(Status404NotFound));
        Summary(x =>
        {
            x.Summary = "Downloads an artifact.";
            x.Responses[Status200OK] = "The artifact has been successfully retrieved.";
            x.Responses[Status204NoContent] = "The artifact exists, but no artifact files were found.";
            x.Responses[Status404NotFound] = "The artifact was not found.";
            x.ResponseExamples[Status400BadRequest] = new RtfxErrorResponse
            {
                GetInvalidArtifactIdHashError("[ArtifactId]"),
                RtfxError.DefaultExample,
            };
            x.ResponseExamples[Status404NotFound] = new RtfxErrorResponse
            {
                GetArtifactWithIdDoesNotExistError("[ArtifactId]"),
            };
        });
    }

    public override async Task HandleAsync(DownloadArtifactRequest req, CancellationToken ct)
    {
        if (!_idHashingService.TryDecodeId(req.ArtifactId, IdType.Artifact, out var artifactId))
        {
            await this.SendErrorAsync(Status400BadRequest, GetInvalidArtifactIdHashError(req.ArtifactId), ct);
            return;
        }

        var artifact = await _artifactRepository.TryGetArtifactAsync(artifactId, ct);
        if (artifact is null)
        {
            await this.SendErrorAsync(Status404NotFound, GetArtifactWithIdDoesNotExistError(req.ArtifactId), ct);
            return;
        }

        if (req.Filter is null or [])
        {
            using var artifactStream = await _artifactStorageService.TryLoadArtifactAsync(artifact.Package.FeedId, artifact.PackageId, artifactId, ct);
            if (artifactStream is null)
            {
                await SendNoContentAsync(ct);
                return;
            }

            await SendStreamAsync(
                artifactStream,
                fileName: $"{artifact.Package.Feed.Name}-{artifact.Package.Name}-{artifact.SourceHash.ToHexString()[..8]}.zip",
                fileLengthBytes: artifactStream.Length,
                cancellation: ct)
                .ConfigureAwait(true);
        }
        else
        {
            var g = HttpContext.Features.Get<IHttpResponseBodyFeature>();
            g.DisableBuffering();

            HttpContext.MarkResponseStart();
            HttpContext.Response.StatusCode = Status200OK;
            HttpContext.Response.ContentType = "application/octet-stream";
            var cdh = new ContentDispositionHeaderValue("attachment");
            cdh.SetHttpFileName($"{artifact.Package.Feed.Name}-{artifact.Package.Name}-{artifact.SourceHash.ToHexString()[..8]}.zip");
            HttpContext.Response.Headers.ContentDisposition = cdh.ToString();

            await g.StartAsync();
            await _artifactStorageService.WriteFilteredArtifactAsync(artifact.Package.FeedId, artifact.PackageId, artifactId, req.Filter, g.Stream, ct);
            await g.CompleteAsync();
        }
    }

    private static ValidationFailure GetInvalidArtifactIdHashError(string artifactId)
        => Errors.InvalidIdHash.GetError(artifactId).WithPropertyName(nameof(DownloadArtifactRequest.ArtifactId));

    private static ValidationFailure GetArtifactWithIdDoesNotExistError(string artifactId)
        => Errors.ArtifactWithIdDoesNotExist.GetError(artifactId).WithPropertyName(nameof(DownloadArtifactRequest.ArtifactId));
}
