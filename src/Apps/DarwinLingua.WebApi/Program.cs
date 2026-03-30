using DarwinLingua.WebApi.Configuration;
using DarwinLingua.WebApi.Models;
using DarwinLingua.WebApi.Services;
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

builder.Services.AddSingleton<IMobileContentManifestService, ConfiguredMobileContentManifestService>();

WebApplication app = builder.Build();

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

public partial class Program;
