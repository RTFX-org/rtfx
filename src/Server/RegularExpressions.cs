namespace Rtfx.Server;

public static partial class RegularExpressions
{
    [GeneratedRegex("""^[a-zA-Z0-9]{8,}$""", RegexOptions.Compiled, 2000)]
    public static partial Regex IdHash();

    [GeneratedRegex("""^[a-zA-Z\.\-_][a-zA-Z0-9\.\-_]*$""", RegexOptions.Compiled, 2000)]
    public static partial Regex FeedName();

    [GeneratedRegex("""^([a-fA-F0-9]{2})+$""", RegexOptions.Compiled, 2000)]
    public static partial Regex SourceHash();

    [GeneratedRegex("""^[a-fA-F0-9]{40}$""", RegexOptions.Compiled, 2000)]
    public static partial Regex GitCommitHash();
}
