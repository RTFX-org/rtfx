using Rtfx.Server.Database;
using Rtfx.Server.Models;
using Rtfx.Server.Services;

namespace Rtfx.Server.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDatabaseContext(this IServiceCollection services, IConfigurationService configurationService)
    {
        var databaseType = configurationService.GetDatabaseType();
        return databaseType switch
        {
            DatabaseType.Sqlite => services.AddDbContext<DatabaseContext, SqliteDatabaseContext>(),
            _ => throw new NotSupportedException($"The database type \"{databaseType.ToStringFast()}\" is not supported."),
        };
    }
}
