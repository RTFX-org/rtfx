namespace Rtfx.Server;

internal static class ErrorMessages
{
    public static string FeedWithNameAlreadyExists(string? name) => $"A feed with the name \"{name}\" already exists.";
    public static string FeedWithIdDoesNotExist<T>(T feedId) => $"A feed with the id {feedId} does not exist.";
    public static string FeedWithNameDoesNotExist(string? name) => $"A feed with the name \"{name}\" does not exist.";
}
