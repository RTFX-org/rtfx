using ICSharpCode.SharpZipLib.Zip;
using MaSch.Globbing;
using Microsoft.Extensions.Options;
using Rtfx.Server.Common;
using Rtfx.Server.Configuration;
using System.IO.Compression;

namespace Rtfx.Server.Services;

public sealed class ArtifactStorageService : IArtifactStorageService
{
    private readonly IOptionsMonitor<ArtifactStorageOptions> _artifactStorageOptions;

    public ArtifactStorageService(IOptionsMonitor<ArtifactStorageOptions> artifactStorageOptions)
    {
        _artifactStorageOptions = artifactStorageOptions;
    }

    public async Task SaveArtifactAsync(long feedId, long packageId, long artifactId, Stream artifactStream, CancellationToken cancellation)
    {
        var artifactPath = GetArtifactPath(feedId, packageId, artifactId);
        Directory.CreateDirectory(Path.GetDirectoryName(artifactPath)!);

        using var fileLock = await FileStreamFactory.ObtainFileLock(artifactPath, FileAccess.ReadWrite, FileShare.None, cancellation);

        var backupFile = GetTempFilePath();
        if (File.Exists(artifactPath))
            File.Move(artifactPath, backupFile);

        try
        {
            using var fileStream = FileStreamFactory.Create(fileLock, FileMode.Create);

            await artifactStream.CopyToAsync(fileStream, cancellation);

            fileStream.Seek(0, SeekOrigin.Begin);
            using var zip = new ZipArchive(fileStream, ZipArchiveMode.Update);
            zip.GetEntry("artifact.metadata")?.Delete();
        }
        catch
        {
            if (File.Exists(backupFile))
                File.Move(backupFile, artifactPath, true);
            throw;
        }
        finally
        {
            if (File.Exists(backupFile))
                File.Delete(backupFile);
        }
    }

    public async Task<FileStream?> TryLoadArtifactAsync(long feedId, long packageId, long artifactId, CancellationToken cancellation)
    {
        var artifactPath = GetArtifactPath(feedId, packageId, artifactId);

        if (!File.Exists(artifactPath))
            return null;

        return await FileStreamFactory.CreateAsync(artifactPath, FileMode.Open, FileAccess.Read, FileShare.Read, cancellation);
    }

    public async Task<bool> WriteFilteredArtifactAsync(long feedId, long packageId, long artifactId, string[] filter, Stream targetStream, CancellationToken cancellation)
    {
        var artifactPath = GetArtifactPath(feedId, packageId, artifactId);

        if (!File.Exists(artifactPath))
            return false;

        var matcher = new GlobMatcher();
        foreach (var f in filter)
            matcher.Add(f);

        await using var artifactFileStream = await FileStreamFactory.CreateAsync(artifactPath, FileMode.Open, FileAccess.Read, FileShare.Read, cancellation);
        using var artifactZipArchive = new ZipArchive(artifactFileStream, ZipArchiveMode.Read);
        await using var outZipStream = new ZipOutputStream(targetStream);

        foreach (var entry in matcher.MatchFilesInZip(artifactZipArchive))
        {
            await outZipStream.PutNextEntryAsync(new ZipEntry(entry.FullName), cancellation);
            await using var entryStream = entry.Open();

            await entryStream.CopyToAsync(outZipStream, cancellation);

            await outZipStream.CloseEntryAsync(cancellation);
        }

        return true;
    }

    private string GetArtifactPath(long feedId, long packageId, long artifactId)
    {
        var storagePath = _artifactStorageOptions.CurrentValue.Path;
        return Path.Combine(storagePath, "Feeds", feedId.ToString(), "Packages", packageId.ToString(), "Artifacts", artifactId.ToString() + ".rtfct");
    }

    private string GetTempFilePath()
    {
        var storagePath = _artifactStorageOptions.CurrentValue.Path;
        var tmpPath = Path.Combine(storagePath, ".tmp");
        Directory.CreateDirectory(tmpPath);
        return Path.Combine(tmpPath, Guid.NewGuid().ToString() + ".tmp");
    }
}
