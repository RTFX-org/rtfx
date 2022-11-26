using Microsoft.Extensions.Options;
using Rtfx.Server.Models;
using System.Diagnostics;

namespace Rtfx.Server.Configuration;

public sealed class DatabaseOptions
{
    public required DatabaseType Type { get; init; }
    public required string ConnectionString { get; init; }
}

public sealed class DatabaseOptionsFactory : IOptionsFactory<DatabaseOptions>
{
    private readonly IConfigurationSection _configurationSection;

    public DatabaseOptionsFactory(IConfiguration configuration)
    {
        _configurationSection = configuration.GetSection("Database");
    }

    public DatabaseOptions Create(string name)
    {
        var type = GetDatabaseType();
        return new DatabaseOptions
        {
            Type = type,
            ConnectionString = GetConnectionString(type),
        };
    }

    private DatabaseType GetDatabaseType()
    {
        var rawType = _configurationSection["Type"] ?? "sqlite";
        return rawType?.ToLowerInvariant() switch
        {
            "sqlite" => DatabaseType.Sqlite,
            _ => throw new NotSupportedException($"The database type \"{rawType}\" is not supported."),
        };
    }

    private string GetConnectionString(DatabaseType databaseType)
    {
        return databaseType switch
        {
            DatabaseType.Sqlite => GetSqliteConnectionString(),
            _ => throw new UnreachableException($"DatabaseType is {(int)databaseType}."),
        };
    }

    private string GetSqliteConnectionString()
    {
        var dataSource = _configurationSection["DataSource"] ?? "data.db";
        return $"Data Source={Path.GetFullPath(dataSource, Globals.DataPath)}";
    }
}