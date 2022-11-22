﻿using Rtfx.Server.Common;
using System.IO.Compression;

namespace Rtfx.Server.Services;

public sealed class ArtifactStorageService : IArtifactStorageService
{
    private readonly IConfigurationService _configuration;

    public ArtifactStorageService(IConfigurationService configuration)
    {
        _configuration = configuration;
    }

    public async Task SaveArtifact(long feedId, long packageId, long artifactId, Stream artifactStream)
    {
        using var scope = EnterSynchronizationScope(artifactId);

        var artifactPath = GetArtifactPath(feedId, packageId, artifactId);

        Directory.CreateDirectory(Path.GetDirectoryName(artifactPath)!);
        using var fileStream = new FileStream(artifactPath, FileMode.Create);
        await artifactStream.CopyToAsync(fileStream);

        fileStream.Seek(0, SeekOrigin.Begin);
        using var zip = new ZipArchive(fileStream, ZipArchiveMode.Update);
        zip.GetEntry("artifact.metadata")?.Delete();
    }

    private static NamedSynchronizationScope EnterSynchronizationScope(long artifactId)
    {
        return NamedSynchronizationScope.Enter($"Artifact:{artifactId}");
    }

    private string GetArtifactPath(long feedId, long packageId, long artifactId)
    {
        var storagePath = _configuration.GetArtifactStoragePath();
        return Path.Combine(storagePath, "Feeds", feedId.ToString(), "Packages", packageId.ToString(), "Artifacts", artifactId.ToString() + ".rtfct");
    }
}