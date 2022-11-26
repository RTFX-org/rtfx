using Rtfx.Server.Models;

namespace Rtfx.Server.Services;

public sealed class ConfigurationService : IConfigurationService
{
    private const string FallbackIdHashSalt = "5E3C2600-E550-4AE7-914F-409503424DB9";

    private readonly IConfiguration _configuration;
    private readonly ILogger<ConfigurationService> _logger;
    private readonly string _basePath;
    private string? _artifactStoragePath;
    private DatabaseType _databaseType = (DatabaseType)(-1);
    private string? _databaseConnectionString;
    private string? _idHashSalt;

    public ConfigurationService(IConfiguration configuration, ILogger<ConfigurationService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        var processPath = Environment.ProcessPath;
        var dir = Path.GetDirectoryName(processPath);
        if (dir is null || Path.GetFileName(processPath)?.Contains("dotnet") == true)
            dir = Environment.CurrentDirectory;

        _basePath = dir;
    }

    public string GetArtifactStoragePath()
    {
        if (_artifactStoragePath is null)
        {
            var name = _configuration["ArtifactStorage:Path"] ?? ".artifacts";
            _artifactStoragePath = Path.GetFullPath(Path.Combine(_basePath, name));
        }

        return _artifactStoragePath;
    }

    public string GetDatabaseConnectionString()
    {
        if (_databaseConnectionString is null)
        {
            var databaseType = GetDatabaseType();
            _databaseConnectionString = databaseType switch
            {
                DatabaseType.Sqlite => GetSqliteConnectionString(),
                _ => throw new NotSupportedException($"The database type \"{databaseType.ToStringFast()}\" is not supported."),
            };
        }

        return _databaseConnectionString;
    }

    public DatabaseType GetDatabaseType()
    {
        if (_databaseType < 0)
        {
            var rawType = _configuration["Database:Type"] ?? "sqlite";
            _databaseType = rawType?.ToLowerInvariant() switch
            {
                "sqlite" => DatabaseType.Sqlite,
                _ => throw new NotSupportedException($"The database type \"{rawType}\" is not supported."),
            };
        }

        return _databaseType;
    }

    public string GetIdHashSalt()
    {
        if (_idHashSalt is null)
        {
            var salt = _configuration["Security:IdHashSalt"];
            if (salt is null or [])
            {
                _logger.LogWarning("The IdHashSalt is empty.");
                salt = FallbackIdHashSalt;
            }

            _idHashSalt = salt;
        }

        return _idHashSalt;
    }

    private string GetSqliteConnectionString()
    {
        var dataSource = _configuration["Database:DataSource"] ?? "data.db";
        return $"Data Source={Path.GetFullPath(Path.Combine(_basePath, dataSource))}";
    }
}
