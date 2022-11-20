namespace Rtfx.Server;

internal static class ErrorMessages
{
    public static string FeedWithNameAlreadyExists(string? name) => $"A feed with the name \"{name}\" already exists.";
    public static string FeedWithIdDoesNotExist(long feedId) => $"A feed with the id {feedId} does not exist.";
    public static string FeedWithNameDoesNotExist(string? name) => $"A feed with the name \"{name}\" does not exist.";

    public static string PackageWithNameDoesNotExist(string? feedName, string? packageName) => $"A package with the name \"{packageName}\" does not exist in feed \"{feedName}\".";

    public static string ArtifactWithIdDoesNotExist(long artifactId) => $"An artifact with the id {artifactId} does not exist.";
    public static string ArtifactAlreadyExists(string? feedName, string? packageName, string? sourceHash) => $"An artifact with source hash \"{sourceHash}\" already exists in the package \"{packageName}\" in feed \"{feedName}\".";
}
