using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Rtfx.Server.Configuration;
using Rtfx.Server.Database;
using Rtfx.Server.Models;

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
            var databaseType = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value.Type;
            return databaseType switch
            {
                DatabaseType.Sqlite => ActivatorUtilities.CreateInstance<SqliteDatabaseContext>(serviceProvider),
                _ => throw new NotSupportedException($"The database type \"{databaseType.ToStringFast()}\" is not supported."),
            };
        }
    }

    public static OptionsBuilder<TOptions> AddSectionOptions<TOptions>(this IServiceCollection services)
        where TOptions : class, IConfigurationSectionOptions
    {
        return OptionsServiceCollectionExtensions.AddOptions<TOptions>(services)
            .BindConfiguration(TOptions.ConfigurationSectionName);
    }

    public static OptionsBuilder<TOptions> AddOptions<TOptions, TFactory>(this IServiceCollection services)
        where TOptions : class
        where TFactory : class, IOptionsFactory<TOptions>
    {
        services.AddSingleton<IOptionsFactory<TOptions>, TFactory>();
        services.AddSingleton<IOptionsChangeTokenSource<TOptions>, ConfigurationChangeTokenSource<TOptions>>();
        return OptionsServiceCollectionExtensions.AddOptions<TOptions>(services);
    }
}