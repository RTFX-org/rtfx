using Microsoft.Extensions.Options;

namespace Rtfx.Server.Configuration;

public sealed class SecurityOptions
{
    public required string IdHashSalt { get; init; }
}

public sealed class SecurityOptionsFactory : IOptionsFactory<SecurityOptions>
{
    private const string FallbackIdHashSalt = "5E3C2600-E550-4AE7-914F-409503424DB9";

    private readonly IConfigurationSection _configurationSection;
    private readonly ILogger<SecurityOptionsFactory> _logger;

    public SecurityOptionsFactory(IConfiguration configuration, ILogger<SecurityOptionsFactory> logger)
    {
        _configurationSection = configuration.GetSection("Security");
        _logger = logger;
    }

    public SecurityOptions Create(string name)
    {
        var salt = _configurationSection["IdHashSalt"];
        if (salt is null or [])
        {
            _logger.LogWarning("The IdHashSalt is empty.");
            salt = FallbackIdHashSalt;
        }

        return new SecurityOptions
        {
            IdHashSalt = salt,
        };
    }
}