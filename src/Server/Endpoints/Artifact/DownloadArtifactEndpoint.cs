using MaSch.Core.Extensions;
using Rtfx.Server.Models;
using Rtfx.Server.Repositories;
using Rtfx.Server.Services;

namespace Rtfx.Server.Endpoints.Artifact;

public sealed partial record DwonloadArtifactRequest(string ArtifactId);

public sealed class DownloadArtifactEndpoint : Endpoint<DwonloadArtifactRequest>
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
    }

    public override async Task HandleAsync(DwonloadArtifactRequest req, CancellationToken ct)
    {
        if (!_idHashingService.TryDecodeId(req.ArtifactId, IdType.Artifact, out var artifactId))
        {
            // TODO Error
            return;
        }

        var artifact = await _artifactRepository.TryGetArtifactAsync(artifactId, ct);
        if (artifact is null)
        {
            // TODO Error
            return;
        }

        using var downloadableArtifact = _artifactStorageService.TryGetDownloadableArtifact(artifact.Package.FeedId, artifact.PackageId, artifactId);
        if (downloadableArtifact is null)
        {
            // TODO Error
            return;
        }

        using var stream = new FileStream(downloadableArtifact.FilePath, FileMode.Open, FileAccess.Read, FileShare.None);
        await SendStreamAsync(
            stream,
            fileName: $"{artifact.Package.Feed.Name}-{artifact.Package.Name}-{artifact.SourceHash.ToHexString()[..8]}.zip",
            fileLengthBytes: stream.Length,
            cancellation: ct)
            .ConfigureAwait(true);
    }
}
