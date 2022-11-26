using FastEndpoints.Swagger;
using Microsoft.EntityFrameworkCore;
using NSwag.Generation;
using Rtfx.Server.Database;
using Rtfx.Server.Models;
using Rtfx.Server.Repositories;
using Rtfx.Server.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDatabaseContext();
builder.Services.AddFastEndpoints(
    o =>
    {
        o.SourceGeneratorDiscoveredTypes = DiscoveredTypes.All;
    });
builder.Services.AddSwaggerDoc(
    s =>
    {
        s.DocumentName = "v1";
        s.Title = "RTFX API";
        s.Version = "v1.0";
    },
    maxEndpointVersion: 1,
    addJWTBearerAuth: false,
    shortSchemaNames: true,
    removeEmptySchemas: true,
    tagIndex: 0);
builder.Services.AddSingleton<IConfigurationService, ConfigurationService>();
builder.Services.AddSingleton<IIdHashingService, IdHashingService>();
builder.Services.AddSingleton<IArtifactValidationService, ArtifactValidationService>();
builder.Services.AddSingleton<IArtifactStorageService, ArtifactStorageService>();
builder.Services.AddScoped<IFeedRepository, FeedRepository>();
builder.Services.AddScoped<IPackageRepository, PackageRepository>();
builder.Services.AddScoped<IArtifactRepository, ArtifactRepository>();

var app = builder.Build();
app.UseDefaultExceptionHandler();
app.UseAuthorization();
app.UseFastEndpoints(
    c =>
    {
        c.Versioning.Prefix = "v";
        c.Versioning.DefaultVersion = 1;
        c.Versioning.PrependToRoute = true;
        c.Endpoints.ShortNames = true;
        c.Endpoints.RoutePrefix = "api";
        c.Endpoints.Configurator = e =>
        {
            e.AllowAnonymous();
            e.Summary(x =>
            {
                if (!x.ResponseExamples.ContainsKey(Status400BadRequest))
                {
                    x.ResponseExamples[Status400BadRequest] = RtfxErrorResponse.DefaultExample;
                }
            });
        };
        c.Errors.ResponseBuilder = (failures, ctx, statusCode) =>
        {
            var response = new RtfxErrorResponse();
            foreach (var failure in failures)
                response.Add(new RtfxError(failure));
            return response;
        };
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
    if (Environment.GetCommandLineArgs().Contains("--generateOpenApiFile"))
    {
        var generator = scope.ServiceProvider.GetRequiredService<IOpenApiDocumentGenerator>();
        var doc = await generator.GenerateAsync("v1");

        var json = doc.ToJson();
        await File.WriteAllTextAsync("api-v1.json", json);
        Environment.Exit(0);
    }

    await scope.ServiceProvider.GetRequiredService<DatabaseContext>().Database.MigrateAsync();
}

await app.RunAsync();