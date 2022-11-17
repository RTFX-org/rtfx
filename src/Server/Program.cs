using FastEndpoints.Swagger;
using Microsoft.EntityFrameworkCore;
using Rtfx.Server.Database;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDatabaseContext(builder.Configuration);
builder.Services.AddFastEndpoints(o =>
{
    o.SourceGeneratorDiscoveredTypes = DiscoveredTypes.All;
});
builder.Services.AddSwaggerDoc(s =>
{
    s.DocumentName = "v1";
});

var app = builder.Build();
app.UseAuthorization();
app.UseFastEndpoints();
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