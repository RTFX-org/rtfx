using Microsoft.Extensions.Options;

namespace Rtfx.Server.Configuration;

public sealed class ArtifactStorageOptions
{
    public required string Path { get; init; }
}

public sealed class ArtifactStorageOptionsFactory : IOptionsFactory<ArtifactStorageOptions>
{
    private readonly IConfigurationSection _configurationSection;

    public ArtifactStorageOptionsFactory(IConfiguration configuration)
    {
        _configurationSection = configuration.GetSection("ArtifactStorage");
    }

    public ArtifactStorageOptions Create(string name)
    {
        return new ArtifactStorageOptions
        {
            Path = Path.GetFullPath(_configurationSection["Path"] ?? ".artifacts", Globals.DataPath),
        };
    }
}
