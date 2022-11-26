using Microsoft.EntityFrameworkCore;
using Rtfx.Server.Database;
using Rtfx.Server.Models;
using Rtfx.Server.Services;

namespace Rtfx.Server.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDatabaseContext(this IServiceCollection services)
    {
        services.AddScoped(ResolveDatabaseContext);
        services.AddScoped<DbContext>(sp => sp.GetRequiredService<DatabaseContext>());
        return services;

        static DatabaseContext ResolveDatabaseContext(IServiceProvider serviceProvider)
        {
            var databaseType = serviceProvider.GetRequiredService<IConfigurationService>().GetDatabaseType();
            return databaseType switch
            {
                DatabaseType.Sqlite => ActivatorUtilities.CreateInstance<SqliteDatabaseContext>(serviceProvider),
                _ => throw new NotSupportedException($"The database type \"{databaseType.ToStringFast()}\" is not supported."),
            };
        }
    }
}
