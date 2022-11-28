using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NSwag.Generation;
using Rtfx.Server.Configuration;
using Rtfx.Server.Database;
using Rtfx.Server.Models;
using Rtfx.Server.Repositories;
using Rtfx.Server.Services;
using CorsOptions = Rtfx.Server.Configuration.CorsOptions;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(
    o =>
    {
        o.Limits.MaxRequestBodySize = null; // Unlimited
        o.Limits.MaxResponseBufferSize = 64000;
        o.AllowSynchronousIO = true; // Currently necessary due to https://github.com/icsharpcode/SharpZipLib/issues/801
    });
builder.Services.Configure<FormOptions>(
    o =>
    {
        o.MultipartBodyLengthLimit = long.MaxValue;
    });
builder.Services.AddSectionOptions<CorsOptions>();
builder.Services.AddOptions<ArtifactStorageOptions, ArtifactStorageOptionsFactory>();
builder.Services.AddOptions<SecurityOptions, SecurityOptionsFactory>();
builder.Services.AddOptions<DatabaseOptions, DatabaseOptionsFactory>();
builder.Services.AddTransient<IOptionsFactory<ArtifactStorageOptions>, ArtifactStorageOptionsFactory>();
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
builder.Services.AddSingleton<IIdHashingService, IdHashingService>();
builder.Services.AddSingleton<IArtifactValidationService, ArtifactValidationService>();
builder.Services.AddSingleton<IArtifactStorageService, ArtifactStorageService>();
builder.Services.AddScoped<IFeedRepository, FeedRepository>();
builder.Services.AddScoped<IPackageRepository, PackageRepository>();
builder.Services.AddScoped<IArtifactRepository, ArtifactRepository>();
builder.Services.AddScoped<ICorsPolicyProvider, CorsPolicyProvider>();
builder.Services.AddCors(
    x =>
    {
        x.AddDefaultPolicy(p => p
            .DisallowCredentials()
            .AllowAnyHeader()
            .WithMethods("GET", "PUT", "DELETE"));
    });

var app = builder.Build();
app.UseDefaultExceptionHandler();
app.UseCors();
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