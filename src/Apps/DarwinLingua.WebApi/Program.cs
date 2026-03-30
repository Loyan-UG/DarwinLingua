using DarwinLingua.WebApi.Configuration;
using DarwinLingua.WebApi.Models;
using DarwinLingua.WebApi.Persistence;
using DarwinLingua.WebApi.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile(
    $"appsettings.{builder.Environment.EnvironmentName}.Local.json",
    optional: true,
    reloadOnChange: true);

builder.Services
    .AddOptions<ServerContentOptions>()
    .Bind(builder.Configuration.GetSection(ServerContentOptions.SectionName))
    .Validate(options => options.HasAtLeastOneActiveProduct(), "At least one active client product must be configured.")
    .Validate(options => options.HasValidPackages(), "Packages must reference active client products and valid areas.")
    .ValidateOnStart();

string serverContentConnectionString = builder.Configuration.GetConnectionString("ServerContentAdmin")
    ?? builder.Configuration.GetConnectionString("ServerContent")
    ?? throw new InvalidOperationException("A ServerContent or ServerContentAdmin connection string must be configured.");

builder.Services.AddDbContext<ServerContentDbContext>(options =>
{
    options.UseNpgsql(serverContentConnectionString);
});

builder.Services.AddScoped<IServerContentDatabaseBootstrapper, ServerContentDatabaseBootstrapper>();
builder.Services.AddScoped<IMobileContentManifestService, DatabaseMobileContentManifestService>();
builder.Services.AddScoped<IMobileContentPackageDeliveryService, DatabaseMobileContentPackageDeliveryService>();

WebApplication app = builder.Build();

await using (AsyncServiceScope bootstrapScope = app.Services.CreateAsyncScope())
{
    IServerContentDatabaseBootstrapper bootstrapper =
        bootstrapScope.ServiceProvider.GetRequiredService<IServerContentDatabaseBootstrapper>();

    await bootstrapper.InitializeAsync(CancellationToken.None);
}

app.MapGet(
    "/api/mobile/content/manifest",
    (string? clientProductKey, IMobileContentManifestService manifestService) =>
    {
        MobileContentManifestResponse response = manifestService.GetGlobalManifest(clientProductKey);
        return Results.Ok(response);
    });

app.MapGet(
    "/api/mobile/content/areas",
    (string? clientProductKey, IMobileContentManifestService manifestService) =>
    {
        IReadOnlyList<MobileContentAreaSummaryResponse> response = manifestService.GetAreas(clientProductKey);
        return Results.Ok(response);
    });

app.MapGet(
    "/api/mobile/content/areas/{areaKey}/manifest",
    (string areaKey, string? clientProductKey, IMobileContentManifestService manifestService) =>
    {
        MobileContentManifestResponse response = manifestService.GetAreaManifest(clientProductKey, areaKey);
        return Results.Ok(response);
    });

app.MapGet(
    "/api/mobile/content/areas/catalog/cefr/{level}/manifest",
    (string level, string? clientProductKey, IMobileContentManifestService manifestService) =>
    {
        MobileContentManifestResponse response = manifestService.GetCefrManifest(clientProductKey, level);
        return Results.Ok(response);
    });

app.MapGet(
    "/api/mobile/content/packages/{packageId}",
    (string packageId, string? clientProductKey, IMobileContentManifestService manifestService) =>
    {
        PublishedContentPackageResponse response = manifestService.GetPackage(clientProductKey, packageId);
        return Results.Ok(response);
    });

app.MapGet(
    "/api/mobile/content/packages/{packageId}/download",
    (string packageId, string? clientProductKey, int? clientSchemaVersion, IMobileContentPackageDeliveryService deliveryService) =>
        ResolvePackageDownload(() => deliveryService.GetPackageById(clientProductKey, packageId, clientSchemaVersion)));

app.MapGet(
    "/api/mobile/content/download/full",
    (string? clientProductKey, int? clientSchemaVersion, IMobileContentPackageDeliveryService deliveryService) =>
        ResolvePackageDownload(() => deliveryService.GetLatestFullPackage(clientProductKey, clientSchemaVersion)));

app.MapGet(
    "/api/mobile/content/areas/{areaKey}/download",
    (string areaKey, string? clientProductKey, int? clientSchemaVersion, IMobileContentPackageDeliveryService deliveryService) =>
        ResolvePackageDownload(() => deliveryService.GetLatestAreaPackage(clientProductKey, areaKey, clientSchemaVersion)));

app.MapGet(
    "/api/mobile/content/areas/catalog/cefr/{level}/download",
    (string level, string? clientProductKey, int? clientSchemaVersion, IMobileContentPackageDeliveryService deliveryService) =>
        ResolvePackageDownload(() => deliveryService.GetLatestCefrPackage(clientProductKey, level, clientSchemaVersion)));

app.MapGet(
    "/health",
    (IOptions<ServerContentOptions> options) =>
    {
        return Results.Ok(new
        {
            status = "ok",
            configuredProducts = options.Value.ClientProducts.Count,
            configuredPackages = options.Value.Packages.Count,
        });
    });

app.Run();

static IResult ResolvePackageDownload(Func<ContentPackageDownloadDescriptor> resolver)
{
    try
    {
        ContentPackageDownloadDescriptor package = resolver();
        return Results.File(package.FilePath, package.ContentType, package.SuggestedFileName);
    }
    catch (MobileContentSchemaCompatibilityException exception)
    {
        return Results.Conflict(new SchemaCompatibilityErrorResponse(
            "schema_incompatible",
            exception.Message,
            exception.PackageId,
            exception.ClientSchemaVersion,
            exception.MinimumRequiredSchemaVersion,
            exception.PackageSchemaVersion));
    }
    catch (FileNotFoundException exception)
    {
        return Results.NotFound(new
        {
            code = "package_payload_missing",
            message = exception.Message,
        });
    }
    catch (KeyNotFoundException exception)
    {
        return Results.NotFound(new
        {
            code = "package_not_found",
            message = exception.Message,
        });
    }
}

public partial class Program;
