using Microsoft.EntityFrameworkCore;
using Rtfx.Server.Services;

namespace Rtfx.Server.Database;

public sealed class SqliteDatabaseContext : DatabaseContext
{
    private readonly IConfigurationService _configuration;

    public SqliteDatabaseContext(ILoggerFactory loggerFactory, IConfigurationService configuration)
        : base(loggerFactory)
    {
        _configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseSqlite(_configuration.GetDatabaseConnectionString());
    }
}