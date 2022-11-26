namespace Rtfx.Server;

internal static class ErrorMessages
{
    public static string InvalidIdHash(string? idHash) => $"The id \"{idHash}\" is not valid.";

    public static string FeedWithNameAlreadyExists(string? feedName) => $"A feed with the name \"{feedName}\" already exists.";
    public static string FeedWithIdDoesNotExist(string feedId) => $"A feed with the id {feedId} does not exist.";
    public static string FeedWithNameDoesNotExist(string? feedName) => $"A feed with the name \"{feedName}\" does not exist.";

    public static string PackageWithNameDoesNotExist(string? feedName, string? packageName) => $"A package with the name \"{packageName}\" does not exist in feed \"{feedName}\".";

    public static string ArtifactWithIdDoesNotExist(string artifactId) => $"An artifact with the id {artifactId} does not exist.";
    public static string ArtifactAlreadyExists(string? feedName, string? packageName, string? sourceHash) => $"An artifact with source hash \"{sourceHash}\" already exists in the package \"{packageName}\" in feed \"{feedName}\".";

    public static string ArtifactHasWrongFileExtension(string? fileName) => "The artifact has the wrong file extension.";
    public static string ArtifactHasNoMetadata(string? fileName) => "The artifact does not contain artifact metadata. Make sure the artifact contains a file named \"artifact.metadata\".";
    public static string ArtifactMetadataParseFailure(string? fileName, [NoValue] string message) => $"Failed to parse artifact metadata: {message}";
    public static string ArtifactMetadataParsedToNull(string? fileName) => $"Parsing artifact metadata lead to null value. Make sure the artifact.metadata file contains valid JSON data.";
}