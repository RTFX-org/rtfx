using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Rtfx.Server.Database.Entities;

namespace Rtfx.Server.Database;

public abstract class DatabaseContext : DbContext
{
    private static readonly ValueConverter<DateTime, DateTime>  _dateTimeConverter = new(
        v => v.ToUniversalTime(),
        v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

    private static readonly ValueConverter<DateTime?, DateTime?> _nullableDateTimeConverter = new(
        v => v.HasValue ? v.Value.ToUniversalTime() : v,
        v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

    private readonly ILoggerFactory _loggerFactory;

    public DbSet<Feed> Feeds { get; set; }
    public DbSet<Package> Packages { get; set; }
    public DbSet<Artifact> Artifacts { get; set; }
    public DbSet<ArtifactMetadata> ArtifactMetdata { get; set; }
    public DbSet<ArtifactTag> ArtifactTags { get; set; }

    public DatabaseContext(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Feed>()
            .HasIndex(x => x.Name)
            .IsUnique(true);

        modelBuilder.Entity<Package>()
            .HasIndex(x => x.Name)
            .IsUnique(true);

        modelBuilder.Entity<Artifact>()
            .HasIndex(x => x.SourceHash)
            .IsUnique(true);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (entityType.IsKeyless)
            {
                continue;
            }

            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime))
                {
                    property.SetValueConverter(_dateTimeConverter);
                }
                else if (property.ClrType == typeof(DateTime?))
                {
                    property.SetValueConverter(_nullableDateTimeConverter);
                }
            }
        }
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseLoggerFactory(_loggerFactory);
    }
}

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDatabaseContext(this IServiceCollection services, IConfiguration configuration)
    {
        return configuration["Database:Type"]?.ToLowerInvariant() switch
        {
            "sqlite" => services.AddDbContext<DatabaseContext, SqliteDatabaseContext>(),
            _ => throw new NotSupportedException($"The database type \"{configuration["Database:Type"] ?? "(null)"}\" is not supported."),
        };
    }
}