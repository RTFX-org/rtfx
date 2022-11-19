using FastEndpoints.Swagger;
using Microsoft.EntityFrameworkCore;
using Rtfx.Server.Database;
using Rtfx.Server.Repositories;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDatabaseContext(builder.Configuration);
builder.Services.AddFastEndpoints(
    o =>
    {
        o.SourceGeneratorDiscoveredTypes = DiscoveredTypes.All;
    });
builder.Services.AddSwaggerDoc(
    s =>
    {
        s.DocumentName = "v0";
        s.Title = "RTFX API";
        s.Version = "v0.0";
    },
    addJWTBearerAuth: false,
    shortSchemaNames: true,
    removeEmptySchemas: true,
    tagIndex: 0);
builder.Services.AddTransient<IFeedRepository, FeedRepository>();

var app = builder.Build();
app.UseDefaultExceptionHandler();
app.UseAuthorization();
app.UseFastEndpoints(
    c =>
    {
        c.Endpoints.ShortNames = true;
        c.Versioning.Prefix = "v";
    });
app.UseSwaggerGen(
    c => c.Path = "/api/swagger/{documentName}",
    c =>
    {
        c.Path = "/api";
        c.DocumentPath = "/api/swagger/{documentName}";
    });

using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<DatabaseContext>().Database.MigrateAsync();
}

await app.RunAsync();