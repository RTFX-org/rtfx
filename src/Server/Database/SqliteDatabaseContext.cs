using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Rtfx.Server.Configuration;

namespace Rtfx.Server.Database;

public sealed class SqliteDatabaseContext : DatabaseContext
{
    private readonly IOptions<DatabaseOptions> _options;

    public SqliteDatabaseContext(ILoggerFactory loggerFactory, IOptions<DatabaseOptions> options)
        : base(loggerFactory)
    {
        _options = options;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseSqlite(_options.Value.ConnectionString);
    }
}