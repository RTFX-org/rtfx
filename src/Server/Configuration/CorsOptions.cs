namespace Rtfx.Server.Configuration;

public sealed class CorsOptions : IConfigurationSectionOptions
{
    public static string ConfigurationSectionName => "CORS";

    public string[]? AllowedOrigins { get; init; }
}
