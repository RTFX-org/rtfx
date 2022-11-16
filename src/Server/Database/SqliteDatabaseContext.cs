using Microsoft.EntityFrameworkCore;

namespace Rtfx.Server.Database;

public sealed class SqliteDatabaseContext : DatabaseContext
{
    private readonly IConfiguration _configuration;
    private readonly string _basePath;

    public SqliteDatabaseContext(ILoggerFactory loggerFactory, IConfiguration configuration)
        : base(loggerFactory)
    {
        var processPath = Environment.ProcessPath;
        var dir = Path.GetDirectoryName(processPath);
        if (dir is null || Path.GetFileName(processPath)?.Contains("dotnet") == true)
            dir = Environment.CurrentDirectory;

        _basePath = dir;
        _configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseSqlite($"Data Source={Path.GetFullPath(Path.Combine(_basePath, _configuration["Database:DataSource"] ?? "data.db"))}");
    }
}