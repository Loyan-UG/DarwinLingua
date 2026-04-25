using DarwinLingua.Catalog.Application.DependencyInjection;
using DarwinLingua.Catalog.Infrastructure.DependencyInjection;
using DarwinLingua.ContentOps.Application.Abstractions;
using DarwinLingua.ContentOps.Application.DependencyInjection;
using DarwinLingua.ContentOps.Infrastructure.DependencyInjection;
using DarwinLingua.Identity;
using DarwinLingua.Infrastructure.DependencyInjection;
using DarwinLingua.Infrastructure.Persistence.Abstractions;
using DarwinLingua.Localization.Infrastructure.DependencyInjection;
using DarwinLingua.WebApi.Configuration;
using DarwinLingua.WebApi.Models;
using DarwinLingua.WebApi.Persistence;
using DarwinLingua.WebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;

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
builder.Services.Configure<DarwinLinguaIdentityBootstrapOptions>(builder.Configuration.GetSection("IdentityBootstrap"));

string serverContentConnectionString = builder.Configuration.GetConnectionString("ServerContentAdmin")
    ?? builder.Configuration.GetConnectionString("ServerContent")
    ?? throw new InvalidOperationException("A ServerContent or ServerContentAdmin connection string must be configured.");
string sharedCatalogConnectionString = builder.Configuration.GetConnectionString("SharedCatalogAdmin")
    ?? builder.Configuration.GetConnectionString("SharedCatalog")
    ?? serverContentConnectionString;
string identityConnectionString = builder.Configuration.GetConnectionString("IdentityAdmin")
    ?? builder.Configuration.GetConnectionString("Identity")
    ?? serverContentConnectionString;

builder.Services.AddDbContext<ServerContentDbContext>(options =>
{
    options.UseNpgsql(serverContentConnectionString);
});
builder.Services.AddDbContext<WebApiIdentityDbContext>(options =>
{
    options.UseNpgsql(identityConnectionString);
});

builder.Services
    .AddIdentityApiEndpoints<DarwinLinguaIdentityUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequiredLength = 8;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<WebApiIdentityDbContext>()
    .AddDefaultTokenProviders();
builder.Services.AddScoped<IUserClaimsPrincipalFactory<DarwinLinguaIdentityUser>, DefaultLearnerRoleClaimsPrincipalFactory>();
builder.Services.AddScoped<IDarwinLinguaIdentityBootstrapper, DarwinLinguaIdentityBootstrapper<WebApiIdentityDbContext>>();
builder.Services.AddAuthorization();

builder.Services
    .AddDarwinLinguaInfrastructureForPostgres(sharedCatalogConnectionString)
    .AddCatalogApplication()
    .AddCatalogInfrastructure()
    .AddContentOpsApplication()
    .AddContentOpsInfrastructure()
    .AddLocalizationInfrastructure();

builder.Services.AddScoped<IServerContentDatabaseBootstrapper, ServerContentDatabaseBootstrapper>();
builder.Services.AddScoped<IMobileContentManifestService, DatabaseMobileContentManifestService>();
builder.Services.AddScoped<IMobileContentPackageDeliveryService, DatabaseMobileContentPackageDeliveryService>();
builder.Services.AddScoped<IContentImportRepository, WebApiContentImportRepository>();
builder.Services.AddScoped<ICatalogPackagePublisher, CatalogPackagePublisher>();
builder.Services.AddScoped<ICatalogPackageDraftQueryService, CatalogPackageDraftQueryService>();
builder.Services.AddScoped<ICatalogPackageCleanupService, CatalogPackageCleanupService>();
builder.Services.AddScoped<ICatalogPublicationHistoryService, CatalogPublicationHistoryService>();
builder.Services.AddScoped<ICatalogPackageRollbackService, CatalogPackageRollbackService>();
builder.Services.AddScoped<IContentPublicationAuditService, ContentPublicationAuditService>();
builder.Services.AddScoped<ICatalogPackageReleaseService, CatalogPackageReleaseService>();
builder.Services.AddScoped<IServerCatalogImportService, ServerCatalogImportService>();
builder.Services.AddScoped<IWebsiteCatalogQueryService, WebsiteCatalogQueryService>();
builder.Services.AddScoped<IWebsiteAdminQueryService, WebsiteAdminQueryService>();

WebApplication app = builder.Build();

await using (AsyncServiceScope bootstrapScope = app.Services.CreateAsyncScope())
{
    IDatabaseInitializer databaseInitializer =
        bootstrapScope.ServiceProvider.GetRequiredService<IDatabaseInitializer>();
    IServerContentDatabaseBootstrapper bootstrapper =
        bootstrapScope.ServiceProvider.GetRequiredService<IServerContentDatabaseBootstrapper>();
    IDarwinLinguaIdentityBootstrapper identityBootstrapper =
        bootstrapScope.ServiceProvider.GetRequiredService<IDarwinLinguaIdentityBootstrapper>();

    await databaseInitializer.InitializeAsync(CancellationToken.None);
    await bootstrapper.InitializeAsync(CancellationToken.None);
    await identityBootstrapper.InitializeAsync(CancellationToken.None);
}

app.UseAuthentication();
app.UseAuthorization();

RouteGroupBuilder authGroup = app.MapGroup("/api/auth");
authGroup.MapIdentityApi<DarwinLinguaIdentityUser>();
authGroup.MapGet(
        "/me",
        [Authorize] async (ClaimsPrincipal principal, UserManager<DarwinLinguaIdentityUser> userManager) =>
        {
            DarwinLinguaIdentityUser? user = await userManager.GetUserAsync(principal).ConfigureAwait(false);
            if (user is null)
            {
                return Results.Unauthorized();
            }

            IList<string> roles = await userManager.GetRolesAsync(user).ConfigureAwait(false);
            return Results.Ok(new AuthenticatedUserResponse(user.Id, user.Email, true, roles.OrderBy(static role => role).ToArray()));
        })
    .RequireAuthorization();

app.MapGet(
    "/api/catalog/topics",
    async (string uiLanguageCode, IWebsiteCatalogQueryService catalogQueryService, CancellationToken cancellationToken) =>
        await ResolveQueryRequestAsync(
                async () => await catalogQueryService.GetTopicsAsync(uiLanguageCode, cancellationToken).ConfigureAwait(false))
            .ConfigureAwait(false));

app.MapGet(
    "/api/catalog/collections",
    async (string meaningLanguageCode, IWebsiteCatalogQueryService catalogQueryService, CancellationToken cancellationToken) =>
        await ResolveQueryRequestAsync(
                async () => await catalogQueryService.GetCollectionsAsync(meaningLanguageCode, cancellationToken).ConfigureAwait(false))
            .ConfigureAwait(false));

app.MapGet(
    "/api/catalog/collections/{slug}",
    async (string slug, string meaningLanguageCode, IWebsiteCatalogQueryService catalogQueryService, CancellationToken cancellationToken) =>
        await ResolveQueryRequestAsync(
                async () => await catalogQueryService.GetCollectionBySlugAsync(slug, meaningLanguageCode, cancellationToken).ConfigureAwait(false))
            .ConfigureAwait(false));

app.MapGet(
    "/api/catalog/words/topic/{topicKey}",
    async (string topicKey, string meaningLanguageCode, int skip, int take, IWebsiteCatalogQueryService catalogQueryService, CancellationToken cancellationToken) =>
        await ResolveQueryRequestAsync(
                async () => await catalogQueryService.GetWordsByTopicPageAsync(topicKey, meaningLanguageCode, skip, take, cancellationToken).ConfigureAwait(false))
            .ConfigureAwait(false));

app.MapGet(
    "/api/catalog/words/cefr/{level}",
    async (string level, string meaningLanguageCode, int skip, int take, IWebsiteCatalogQueryService catalogQueryService, CancellationToken cancellationToken) =>
        await ResolveQueryRequestAsync(
                async () => await catalogQueryService.GetWordsByCefrPageAsync(level, meaningLanguageCode, skip, take, cancellationToken).ConfigureAwait(false))
            .ConfigureAwait(false));

app.MapGet(
    "/api/catalog/words/search",
    async (string q, string meaningLanguageCode, IWebsiteCatalogQueryService catalogQueryService, CancellationToken cancellationToken) =>
        await ResolveQueryRequestAsync(
                async () => await catalogQueryService.SearchWordsAsync(q, meaningLanguageCode, cancellationToken).ConfigureAwait(false))
            .ConfigureAwait(false));

app.MapGet(
    "/api/catalog/words/{publicId:guid}",
    async (Guid publicId, string primaryMeaningLanguageCode, string? secondaryMeaningLanguageCode, string uiLanguageCode, IWebsiteCatalogQueryService catalogQueryService, CancellationToken cancellationToken) =>
        await ResolveQueryRequestAsync(
                async () => await catalogQueryService.GetWordDetailsAsync(publicId, primaryMeaningLanguageCode, secondaryMeaningLanguageCode, uiLanguageCode, cancellationToken).ConfigureAwait(false))
            .ConfigureAwait(false));

app.MapPost(
    "/api/catalog/words/by-ids",
    async (CatalogWordLookupRequest request, IWebsiteCatalogQueryService catalogQueryService, CancellationToken cancellationToken) =>
        await ResolveQueryRequestAsync(
                async () => await catalogQueryService.GetWordsByIdsAsync(request.WordIds, request.MeaningLanguageCode, cancellationToken).ConfigureAwait(false))
            .ConfigureAwait(false));

app.MapGet(
    "/api/admin/catalog/dashboard",
    async (IWebsiteAdminQueryService adminQueryService, CancellationToken cancellationToken) =>
        await ResolveQueryRequestAsync(
                async () => await adminQueryService.GetDashboardAsync(cancellationToken).ConfigureAwait(false))
            .ConfigureAwait(false));

app.MapGet(
    "/api/admin/catalog/imports",
    async (string? status, IWebsiteAdminQueryService adminQueryService, CancellationToken cancellationToken) =>
        await ResolveQueryRequestAsync(
                async () => await adminQueryService.GetImportsAsync(status, cancellationToken).ConfigureAwait(false))
            .ConfigureAwait(false));

app.MapGet(
    "/api/admin/catalog/draft-words",
    async (string? q, IWebsiteAdminQueryService adminQueryService, CancellationToken cancellationToken) =>
        await ResolveQueryRequestAsync(
                async () => await adminQueryService.GetDraftWordsAsync(q, cancellationToken).ConfigureAwait(false))
            .ConfigureAwait(false));

app.MapGet(
    "/api/admin/catalog/history-view",
    async (string? status, IWebsiteAdminQueryService adminQueryService, CancellationToken cancellationToken) =>
        await ResolveQueryRequestAsync(
                async () => await adminQueryService.GetHistoryAsync(status, cancellationToken).ConfigureAwait(false))
            .ConfigureAwait(false));

app.MapGet(
    "/api/admin/catalog/rollback-preview",
    async (IWebsiteAdminQueryService adminQueryService, CancellationToken cancellationToken) =>
        await ResolveQueryRequestAsync(
                async () => await adminQueryService.GetRollbackPreviewAsync(cancellationToken).ConfigureAwait(false))
            .ConfigureAwait(false));

app.MapGet(
    "/api/mobile/content/manifest",
    (string? clientProductKey, IMobileContentManifestService manifestService) =>
        ResolveQueryRequest(() => manifestService.GetGlobalManifest(clientProductKey)));

app.MapGet(
    "/api/mobile/content/areas",
    (string? clientProductKey, IMobileContentManifestService manifestService) =>
        ResolveQueryRequest(() => manifestService.GetAreas(clientProductKey)));

app.MapGet(
    "/api/mobile/content/areas/{areaKey}/manifest",
    (string areaKey, string? clientProductKey, IMobileContentManifestService manifestService) =>
        ResolveQueryRequest(() => manifestService.GetAreaManifest(clientProductKey, areaKey)));

app.MapGet(
    "/api/mobile/content/areas/catalog/cefr/{level}/manifest",
    (string level, string? clientProductKey, IMobileContentManifestService manifestService) =>
        ResolveQueryRequest(() => manifestService.GetCefrManifest(clientProductKey, level)));

app.MapGet(
    "/api/mobile/content/packages/{packageId}",
    (string packageId, string? clientProductKey, IMobileContentManifestService manifestService) =>
        ResolveQueryRequest(() => manifestService.GetPackage(clientProductKey, packageId)));

app.MapPost(
    "/api/admin/content/catalog/import",
    async (AdminImportCatalogRequest request, IServerCatalogImportService importService, CancellationToken cancellationToken) =>
    {
        AdminImportCatalogResponse response = await importService
            .ImportAndStageAsync(request, cancellationToken)
            .ConfigureAwait(false);

        return response.IsSuccess ? Results.Ok(response) : Results.BadRequest(response);
    });

app.MapPost(
    "/api/admin/content/catalog/stage",
    async (AdminStageCatalogRequest request, ICatalogPackagePublisher packagePublisher, ServerContentDbContext dbContext, CancellationToken cancellationToken) =>
        await ResolveMutationRequestAsync(
                async () =>
                {
                    string clientProductKey = await ResolveClientProductKeyAsync(request.ClientProductKey, dbContext, cancellationToken)
                        .ConfigureAwait(false);

                    CatalogPackagePublicationResult result = await packagePublisher
                        .StageDraftAsync(clientProductKey, cancellationToken)
                        .ConfigureAwait(false);

                    return new AdminStageCatalogResponse(
                        true,
                        result.ClientProductKey,
                        result.PublicationBatchId,
                        result.Version,
                        result.PackageIds,
                        []);
                },
                response => response.IsSuccess)
            .ConfigureAwait(false));

app.MapPost(
    "/api/admin/content/catalog/publish",
    async (AdminPublishCatalogRequest request, ICatalogPackageReleaseService releaseService, CancellationToken cancellationToken) =>
        await ResolveMutationRequestAsync(
                async () => await releaseService.PublishAsync(request, cancellationToken).ConfigureAwait(false),
                response => response.IsSuccess)
            .ConfigureAwait(false));

app.MapPost(
    "/api/admin/content/catalog/rollback",
    async (AdminRollbackCatalogRequest request, ICatalogPackageRollbackService rollbackService, CancellationToken cancellationToken) =>
        await ResolveMutationRequestAsync(
                async () => await rollbackService.RollbackAsync(request, cancellationToken).ConfigureAwait(false),
                response => response.IsSuccess)
            .ConfigureAwait(false));

app.MapGet(
    "/api/admin/content/catalog/drafts",
    async (string? clientProductKey, ICatalogPackageDraftQueryService draftQueryService, CancellationToken cancellationToken) =>
        await ResolveQueryRequestAsync(
                async () => await draftQueryService.GetBatchesAsync(clientProductKey, cancellationToken).ConfigureAwait(false))
            .ConfigureAwait(false));

app.MapGet(
    "/api/admin/content/catalog/drafts/{publicationBatchId}",
    async (string publicationBatchId, string? clientProductKey, ICatalogPackageDraftQueryService draftQueryService, CancellationToken cancellationToken) =>
        await ResolveQueryRequestAsync(
                async () => await draftQueryService.GetBatchAsync(publicationBatchId, clientProductKey, cancellationToken).ConfigureAwait(false))
            .ConfigureAwait(false));

app.MapDelete(
    "/api/admin/content/catalog/drafts/{publicationBatchId}",
    async (string publicationBatchId, string? clientProductKey, ICatalogPackageCleanupService cleanupService, CancellationToken cancellationToken) =>
        await ResolveMutationRequestAsync(
                async () => await cleanupService.DeleteSupersededBatchAsync(publicationBatchId, clientProductKey, cancellationToken).ConfigureAwait(false),
                response => response.IsSuccess)
            .ConfigureAwait(false));

app.MapGet(
    "/api/admin/content/catalog/history",
    async (string? clientProductKey, ICatalogPublicationHistoryService historyService, CancellationToken cancellationToken) =>
        await ResolveQueryRequestAsync(
                async () => await historyService.GetHistoryAsync(clientProductKey, cancellationToken).ConfigureAwait(false))
            .ConfigureAwait(false));

app.MapGet(
    "/api/admin/content/catalog/history/summary",
    async (string? clientProductKey, ICatalogPublicationHistoryService historyService, CancellationToken cancellationToken) =>
        await ResolveQueryRequestAsync(
                async () => await historyService.GetSummaryAsync(clientProductKey, cancellationToken).ConfigureAwait(false))
            .ConfigureAwait(false));

app.MapGet(
    "/api/admin/content/catalog/events",
    async (string? clientProductKey, IContentPublicationAuditService auditService, CancellationToken cancellationToken) =>
        await ResolveQueryRequestAsync(
                async () => await auditService.GetRecentEventsAsync(clientProductKey, cancellationToken).ConfigureAwait(false))
            .ConfigureAwait(false));

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
    catch (InvalidOperationException exception)
    {
        return Results.BadRequest(new
        {
            code = "invalid_request",
            message = exception.Message,
        });
    }
}

static IResult ResolveQueryRequest<T>(Func<T> resolver)
{
    try
    {
        return Results.Ok(resolver());
    }
    catch (KeyNotFoundException exception)
    {
        return Results.NotFound(new
        {
            code = "resource_not_found",
            message = exception.Message,
        });
    }
    catch (InvalidOperationException exception)
    {
        return Results.BadRequest(new
        {
            code = "invalid_request",
            message = exception.Message,
        });
    }
}

static async Task<IResult> ResolveQueryRequestAsync<T>(Func<Task<T>> resolver)
{
    try
    {
        return Results.Ok(await resolver().ConfigureAwait(false));
    }
    catch (KeyNotFoundException exception)
    {
        return Results.NotFound(new
        {
            code = "resource_not_found",
            message = exception.Message,
        });
    }
    catch (InvalidOperationException exception)
    {
        return Results.BadRequest(new
        {
            code = "invalid_request",
            message = exception.Message,
        });
    }
}

static async Task<IResult> ResolveMutationRequestAsync<T>(Func<Task<T>> resolver, Func<T, bool> isSuccess)
{
    try
    {
        T response = await resolver().ConfigureAwait(false);
        return isSuccess(response) ? Results.Ok(response) : Results.BadRequest(response);
    }
    catch (KeyNotFoundException exception)
    {
        return Results.NotFound(new
        {
            code = "resource_not_found",
            message = exception.Message,
        });
    }
    catch (InvalidOperationException exception)
    {
        return Results.BadRequest(new
        {
            code = "invalid_request",
            message = exception.Message,
        });
    }
}

static async Task<string> ResolveClientProductKeyAsync(
    string? clientProductKey,
    ServerContentDbContext dbContext,
    CancellationToken cancellationToken)
{
    if (!string.IsNullOrWhiteSpace(clientProductKey))
    {
        string requestedClientProductKey = clientProductKey.Trim();
        bool exists = await dbContext.ClientProducts
            .AnyAsync(product => product.Key == requestedClientProductKey && product.IsActive, cancellationToken)
            .ConfigureAwait(false);

        if (!exists)
        {
            throw new KeyNotFoundException($"No active client product was found for '{requestedClientProductKey}'.");
        }

        return requestedClientProductKey;
    }

    List<string> activeProductKeys = await dbContext.ClientProducts
        .Where(product => product.IsActive)
        .OrderBy(product => product.Key)
        .Select(product => product.Key)
        .ToListAsync(cancellationToken)
        .ConfigureAwait(false);

    return activeProductKeys.Count switch
    {
        1 => activeProductKeys[0],
        0 => throw new InvalidOperationException("No active client product is configured."),
        _ => throw new InvalidOperationException("A client product key is required when multiple active products are configured."),
    };
}

public partial class Program;
