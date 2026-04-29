using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.DependencyInjection;
using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.Catalog.Infrastructure.DependencyInjection;
using DarwinLingua.ContentOps.Application.Abstractions;
using DarwinLingua.ContentOps.Application.DependencyInjection;
using DarwinLingua.ContentOps.Infrastructure.DependencyInjection;
using DarwinLingua.Identity;
using DarwinLingua.Infrastructure.DependencyInjection;
using DarwinLingua.Infrastructure.Persistence.Abstractions;
using DarwinLingua.Localization.Infrastructure.DependencyInjection;
using DarwinLingua.SharedKernel.Exceptions;
using DarwinLingua.WebApi.Configuration;
using DarwinLingua.WebApi.Models;
using DarwinLingua.WebApi.Persistence;
using DarwinLingua.WebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

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
builder.Services
    .AddOptions<AdminApiAccessOptions>()
    .Bind(builder.Configuration.GetSection(AdminApiAccessOptions.SectionName))
    .Validate(options => !string.IsNullOrWhiteSpace(options.HeaderName), "AdminApi:HeaderName must be configured.")
    .ValidateOnStart();
builder.Services.Configure<DarwinLinguaIdentityBootstrapOptions>(builder.Configuration.GetSection("IdentityBootstrap"));
builder.Services.Configure<DarwinLinguaEntitlementOptions>(builder.Configuration.GetSection("Entitlements"));
builder.Services.AddSingleton<Microsoft.Extensions.Options.IPostConfigureOptions<DarwinLinguaIdentityBootstrapOptions>, DarwinLinguaIdentityBootstrapOptionsPostConfigure>();
builder.Services.AddSingleton<Microsoft.Extensions.Options.IPostConfigureOptions<DarwinLinguaEntitlementOptions>, DarwinLinguaEntitlementOptionsPostConfigure>();

string serverContentConnectionString = GetRequiredConnectionString(
    builder.Configuration,
    "A ServerContent or ServerContentAdmin connection string must be configured.",
    "ServerContentAdmin",
    "ServerContent");
string sharedCatalogConnectionString = GetOptionalConnectionString(
        builder.Configuration,
        "SharedCatalogAdmin",
        "SharedCatalog")
    ?? serverContentConnectionString;
string identityConnectionString = GetOptionalConnectionString(
        builder.Configuration,
        "IdentityAdmin",
        "Identity")
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
builder.Services.AddScoped<IUserEntitlementService, UserEntitlementService<WebApiIdentityDbContext>>();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole(DarwinLinguaRoles.Admin));
});

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
builder.Services.AddScoped<IConversationEventAdminService, ConversationEventAdminService>();
builder.Services.AddScoped<IOrganizerProfileAdminService, OrganizerProfileAdminService>();
builder.Services.AddScoped<IOrganizerClaimRequestService, OrganizerClaimRequestService>();
builder.Services.AddScoped<IOrganizerProfileOwnerService, OrganizerProfileOwnerService>();
builder.Services.AddScoped<IEventRsvpService, EventRsvpService>();
builder.Services.AddScoped<ILearnerConversationProfileService, LearnerConversationProfileService>();
builder.Services.AddScoped<IPartnerMatchingService, PartnerMatchingService>();
builder.Services.AddScoped<IModerationService, ModerationService>();

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
app.Use(EnforceAdminApiAccessAsync);
app.UseAuthorization();

RouteGroupBuilder authGroup = app.MapGroup("/api/auth");
authGroup.MapIdentityApi<DarwinLinguaIdentityUser>();
authGroup.MapGet(
        "/me",
        [Authorize] async (
            ClaimsPrincipal principal,
            UserManager<DarwinLinguaIdentityUser> userManager,
            IUserEntitlementService userEntitlementService,
            CancellationToken cancellationToken) =>
        {
            DarwinLinguaIdentityUser? user = await userManager.GetUserAsync(principal).ConfigureAwait(false);
            if (user is null)
            {
                return Results.Unauthorized();
            }

            IList<string> roles = await userManager.GetRolesAsync(user).ConfigureAwait(false);
            UserEntitlementSnapshot entitlement = await userEntitlementService
                .GetCurrentAsync(user.Id, cancellationToken)
                .ConfigureAwait(false);

            return Results.Ok(new AuthenticatedUserResponse(
                user.Id,
                user.Email,
                true,
                roles.OrderBy(static role => role).ToArray(),
                entitlement.Tier,
                entitlement.TrialEndsAtUtc,
                entitlement.PremiumEndsAtUtc,
                entitlement.EnabledFeatures));
        })
    .RequireAuthorization();

RouteGroupBuilder adminIdentityGroup = app.MapGroup("/api/admin/identity")
    .RequireAuthorization("Admin");
adminIdentityGroup.MapGet(
    "/users",
    async (
        UserManager<DarwinLinguaIdentityUser> userManager,
        WebApiIdentityDbContext identityDbContext,
        IUserEntitlementService userEntitlementService,
        CancellationToken cancellationToken) =>
    {
        DarwinLinguaIdentityUser[] users = await userManager.Users
            .AsNoTracking()
            .OrderBy(user => user.Email)
            .Take(200)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        string[] userIds = users
            .Select(static user => user.Id)
            .ToArray();
        Dictionary<string, string[]> rolesByUserId = await LoadRolesByUserIdAsync(identityDbContext, userIds, cancellationToken)
            .ConfigureAwait(false);
        IReadOnlyDictionary<string, UserEntitlementSnapshot> entitlementsByUserId = await userEntitlementService
            .GetCurrentManyAsync(userIds, cancellationToken)
            .ConfigureAwait(false);
        IReadOnlyDictionary<string, IReadOnlyList<UserEntitlementAuditEventModel>> auditEventsByUserId = await userEntitlementService
            .GetRecentAuditEventsManyAsync(userIds, 3, cancellationToken)
            .ConfigureAwait(false);

        List<AdminIdentityUserResponse> response = new(users.Length);
        foreach (DarwinLinguaIdentityUser user in users)
        {
            UserEntitlementSnapshot entitlement = entitlementsByUserId[user.Id];
            IReadOnlyList<UserEntitlementAuditEventModel> auditEvents = auditEventsByUserId.TryGetValue(user.Id, out IReadOnlyList<UserEntitlementAuditEventModel>? events)
                ? events
                : [];

            response.Add(new AdminIdentityUserResponse(
                user.Id,
                user.Email,
                rolesByUserId.TryGetValue(user.Id, out string[]? roles) ? roles : [],
                entitlement.Tier,
                entitlement.TrialEndsAtUtc,
                entitlement.PremiumEndsAtUtc,
                entitlement.EnabledFeatures,
                auditEvents
                    .Select(static auditEvent => new AdminIdentityEntitlementAuditEventResponse(
                        auditEvent.EventType,
                        auditEvent.PreviousTier,
                        auditEvent.NewTier,
                        auditEvent.UpdatedBy,
                        auditEvent.CreatedAtUtc))
                    .ToArray()));
        }

        return Results.Ok(response);
    });
adminIdentityGroup.MapPost(
    "/users/{userId}/entitlement",
    async (
        string userId,
        AdminUpdateUserEntitlementRequest request,
        ClaimsPrincipal principal,
        UserManager<DarwinLinguaIdentityUser> userManager,
        IUserEntitlementService userEntitlementService,
        CancellationToken cancellationToken) =>
    {
        if (!DarwinLinguaEntitlementTiers.All.Contains(request.Tier, StringComparer.OrdinalIgnoreCase))
        {
            return Results.BadRequest(new
            {
                code = "unsupported_entitlement_tier",
                message = $"'{request.Tier}' is not a supported entitlement tier.",
            });
        }

        DarwinLinguaIdentityUser? user = await userManager.FindByIdAsync(userId).ConfigureAwait(false);
        if (user is null)
        {
            return Results.NotFound(new
            {
                code = "user_not_found",
                message = "The selected user could not be found.",
            });
        }

        string updatedBy = TryGetPrincipalEmail(principal)
            ?? principal.Identity?.Name
            ?? principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? "admin-api";

        UserEntitlementSnapshot entitlement = await userEntitlementService
            .SetTierAsync(user.Id, request.Tier, request.ExpiresAtUtc, updatedBy, cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new AuthenticatedUserResponse(
            user.Id,
            user.Email,
            true,
            (await userManager.GetRolesAsync(user).ConfigureAwait(false)).OrderBy(static role => role).ToArray(),
            entitlement.Tier,
            entitlement.TrialEndsAtUtc,
            entitlement.PremiumEndsAtUtc,
            entitlement.EnabledFeatures));
    });

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
    "/api/catalog/scenarios",
    async (IScenarioLessonQueryService scenarioQueryService, CancellationToken cancellationToken) =>
        await ResolveQueryRequestAsync(
                async () => await scenarioQueryService.GetPublishedScenariosAsync(cancellationToken).ConfigureAwait(false))
            .ConfigureAwait(false));

app.MapGet(
    "/api/catalog/scenarios/{slug}",
    async (string slug, string primaryMeaningLanguageCode, string? secondaryMeaningLanguageCode, IScenarioLessonQueryService scenarioQueryService, CancellationToken cancellationToken) =>
        await ResolveQueryRequestAsync(
                async () => await scenarioQueryService.GetPublishedScenarioBySlugAsync(slug, primaryMeaningLanguageCode, secondaryMeaningLanguageCode, cancellationToken).ConfigureAwait(false))
            .ConfigureAwait(false));

app.MapGet(
    "/api/catalog/conversation-starters",
    async (
        string? cefrLevel,
        string? situation,
        string? tone,
        string? conversationGoal,
        string? topicKey,
        IConversationStarterQueryService conversationStarterQueryService,
        CancellationToken cancellationToken) =>
        await ResolveQueryRequestAsync(
                async () => await conversationStarterQueryService.GetPublishedStarterPacksAsync(
                    new ConversationStarterListFilterModel(cefrLevel, situation, tone, conversationGoal, topicKey),
                    cancellationToken).ConfigureAwait(false))
            .ConfigureAwait(false));

app.MapGet(
    "/api/catalog/scenarios/{slug}/conversation-starters",
    async (string slug, IConversationStarterQueryService conversationStarterQueryService, CancellationToken cancellationToken) =>
        await ResolveQueryRequestAsync(
                async () => await conversationStarterQueryService.GetPublishedStarterPacksForScenarioAsync(slug, cancellationToken).ConfigureAwait(false))
            .ConfigureAwait(false));

app.MapGet(
    "/api/catalog/conversation-starters/{slug}",
    async (string slug, string primaryMeaningLanguageCode, string? secondaryMeaningLanguageCode, IConversationStarterQueryService conversationStarterQueryService, CancellationToken cancellationToken) =>
        await ResolveQueryRequestAsync(
                async () => await conversationStarterQueryService.GetPublishedStarterPackBySlugAsync(slug, primaryMeaningLanguageCode, secondaryMeaningLanguageCode, cancellationToken).ConfigureAwait(false))
            .ConfigureAwait(false));

app.MapGet(
    "/api/catalog/event-preparation-packs",
    async (
        string? cefrLevel,
        string? category,
        string? eventType,
        string? topicKey,
        IEventPreparationQueryService eventPreparationQueryService,
        CancellationToken cancellationToken) =>
        await ResolveQueryRequestAsync(
                async () => await eventPreparationQueryService.GetPublishedEventPreparationPacksAsync(
                    new EventPreparationListFilterModel(cefrLevel, category, eventType, topicKey),
                    cancellationToken).ConfigureAwait(false))
            .ConfigureAwait(false));

app.MapGet(
    "/api/catalog/scenarios/{slug}/event-preparation-packs",
    async (string slug, IEventPreparationQueryService eventPreparationQueryService, CancellationToken cancellationToken) =>
        await ResolveQueryRequestAsync(
                async () => await eventPreparationQueryService.GetPublishedEventPreparationPacksForScenarioAsync(slug, cancellationToken).ConfigureAwait(false))
            .ConfigureAwait(false));

app.MapGet(
    "/api/catalog/event-preparation-packs/{slug}",
    async (string slug, IEventPreparationQueryService eventPreparationQueryService, CancellationToken cancellationToken) =>
        await ResolveQueryRequestAsync(
                async () => await eventPreparationQueryService.GetPublishedEventPreparationPackBySlugAsync(slug, cancellationToken).ConfigureAwait(false))
            .ConfigureAwait(false));

app.MapGet(
    "/api/catalog/conversation-events",
    async (
        string? city,
        string? cefrLevel,
        string? helperLanguageCode,
        bool? isOnline,
        string? priceType,
        string? category,
        IConversationEventQueryService conversationEventQueryService,
        CancellationToken cancellationToken) =>
        await ResolveQueryRequestAsync(
                async () => await conversationEventQueryService.GetPublishedEventsAsync(
                    new ConversationEventListFilterModel(city, cefrLevel, helperLanguageCode, isOnline, priceType, category),
                    cancellationToken).ConfigureAwait(false))
            .ConfigureAwait(false));

app.MapGet(
    "/api/catalog/conversation-events/{slug}",
    async (string slug, IConversationEventQueryService conversationEventQueryService, CancellationToken cancellationToken) =>
        await ResolveQueryRequestAsync(
                async () => await conversationEventQueryService.GetPublishedEventBySlugAsync(slug, cancellationToken).ConfigureAwait(false))
            .ConfigureAwait(false));

app.MapGet(
    "/api/catalog/conversation-events/{slug}/rsvp-summary",
    async (string slug, IEventRsvpService eventRsvpService, CancellationToken cancellationToken) =>
        await ResolveQueryRequestAsync(
                async () => await eventRsvpService.GetSummaryAsync(slug, cancellationToken).ConfigureAwait(false))
            .ConfigureAwait(false));

app.MapPost(
    "/api/catalog/conversation-events/{slug}/rsvps",
    async (
        string slug,
        SubmitEventRsvpRequest request,
        IEventRsvpService eventRsvpService,
        CancellationToken cancellationToken) =>
        await ResolveQueryRequestAsync(
                async () => await eventRsvpService.SubmitAsync(slug, request, cancellationToken).ConfigureAwait(false))
            .ConfigureAwait(false));

app.MapGet(
    "/api/catalog/organizer-profiles",
    async (IOrganizerProfileQueryService organizerProfileQueryService, CancellationToken cancellationToken) =>
        await ResolveQueryRequestAsync(
                async () => await organizerProfileQueryService.GetPublishedOrganizerProfilesAsync(cancellationToken).ConfigureAwait(false))
            .ConfigureAwait(false));

app.MapGet(
    "/api/catalog/organizer-profiles/{slug}",
    async (string slug, IOrganizerProfileQueryService organizerProfileQueryService, CancellationToken cancellationToken) =>
        await ResolveQueryRequestAsync(
                async () => await organizerProfileQueryService.GetPublishedOrganizerProfileBySlugAsync(slug, cancellationToken).ConfigureAwait(false))
            .ConfigureAwait(false));

app.MapPost(
    "/api/catalog/organizer-profiles/{slug}/claim",
    async (
        string slug,
        SubmitOrganizerClaimRequest request,
        IOrganizerClaimRequestService organizerClaimRequestService,
        CancellationToken cancellationToken) =>
        await ResolveQueryRequestAsync(
                async () => await organizerClaimRequestService.SubmitAsync(slug, request, cancellationToken).ConfigureAwait(false))
            .ConfigureAwait(false));

app.MapGet(
    "/api/catalog/learner-conversation-profiles/me",
    async (HttpRequest httpRequest, ILearnerConversationProfileService learnerProfileService, CancellationToken cancellationToken) =>
        await ResolveQueryRequestAsync(
                async () => await learnerProfileService.GetPrivateAsync(GetNormalizedEmailParameter(httpRequest, "ownerEmail"), cancellationToken).ConfigureAwait(false))
            .ConfigureAwait(false));

app.MapPost(
    "/api/catalog/learner-conversation-profiles/me",
    async (
        HttpRequest httpRequest,
        SaveLearnerConversationProfileRequest request,
        ILearnerConversationProfileService learnerProfileService,
        CancellationToken cancellationToken) =>
        await ResolveQueryRequestAsync(
                async () => await learnerProfileService.SaveAsync(GetNormalizedEmailParameter(httpRequest, "ownerEmail"), request, cancellationToken).ConfigureAwait(false))
            .ConfigureAwait(false));

app.MapPost(
    "/api/catalog/learner-conversation-profiles/me/enabled",
    async (
        HttpRequest httpRequest,
        LearnerConversationProfileVisibilityRequest request,
        ILearnerConversationProfileService learnerProfileService,
        CancellationToken cancellationToken) =>
        await ResolveQueryRequestAsync(
                async () => await learnerProfileService.SetEnabledAsync(GetNormalizedEmailParameter(httpRequest, "ownerEmail"), request, cancellationToken).ConfigureAwait(false))
            .ConfigureAwait(false));

app.MapDelete(
    "/api/catalog/learner-conversation-profiles/me",
    async (HttpRequest httpRequest, ILearnerConversationProfileService learnerProfileService, CancellationToken cancellationToken) =>
    {
        try
        {
            await learnerProfileService.AnonymizeAsync(GetNormalizedEmailParameter(httpRequest, "ownerEmail"), cancellationToken).ConfigureAwait(false);
            return Results.NoContent();
        }
        catch (DomainRuleException exception)
        {
            return Results.BadRequest(new
            {
                code = "invalid_request",
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
    });

app.MapGet(
    "/api/catalog/learner-conversation-profiles/public",
    async (ILearnerConversationProfileService learnerProfileService, CancellationToken cancellationToken) =>
        await ResolveQueryRequestAsync(
                async () => await learnerProfileService.GetPublicProfilesAsync(cancellationToken).ConfigureAwait(false))
            .ConfigureAwait(false));

app.MapPost(
    "/api/catalog/partner-matches/search",
    async (
        HttpRequest httpRequest,
        PartnerMatchSearchRequest request,
        IPartnerMatchingService partnerMatchingService,
        CancellationToken cancellationToken) =>
        await ResolveQueryRequestAsync(
                async () => await partnerMatchingService.SearchAsync(GetNormalizedEmailParameter(httpRequest, "ownerEmail"), request, cancellationToken).ConfigureAwait(false))
            .ConfigureAwait(false));

app.MapPost(
    "/api/catalog/partner-requests",
    async (
        HttpRequest httpRequest,
        SubmitPartnerRequestRequest request,
        IPartnerMatchingService partnerMatchingService,
        CancellationToken cancellationToken) =>
        await ResolveQueryRequestAsync(
                async () => await partnerMatchingService.SubmitRequestAsync(GetNormalizedEmailParameter(httpRequest, "ownerEmail"), request, cancellationToken).ConfigureAwait(false))
            .ConfigureAwait(false));

app.MapGet(
    "/api/catalog/partner-requests",
    async (HttpRequest httpRequest, IPartnerMatchingService partnerMatchingService, CancellationToken cancellationToken) =>
        await ResolveQueryRequestAsync(
                async () => await partnerMatchingService.GetRequestsAsync(GetNormalizedEmailParameter(httpRequest, "ownerEmail"), cancellationToken).ConfigureAwait(false))
            .ConfigureAwait(false));

app.MapPost(
    "/api/catalog/partner-requests/{requestId:guid}/state",
    async (
        HttpRequest httpRequest,
        Guid requestId,
        PartnerRequestStateUpdateRequest request,
        IPartnerMatchingService partnerMatchingService,
        CancellationToken cancellationToken) =>
        await ResolveQueryRequestAsync(
                async () => await partnerMatchingService.UpdateRequestStateAsync(GetNormalizedEmailParameter(httpRequest, "ownerEmail"), requestId, request, cancellationToken).ConfigureAwait(false))
            .ConfigureAwait(false));

app.MapPost(
    "/api/catalog/moderation/reports",
    async (
        HttpRequest httpRequest,
        SubmitUserReportRequest request,
        IModerationService moderationService,
        CancellationToken cancellationToken) =>
        await ResolveQueryRequestAsync(
                async () => await moderationService.SubmitReportAsync(GetNormalizedEmailParameter(httpRequest, "reporterEmail"), request, cancellationToken).ConfigureAwait(false))
            .ConfigureAwait(false));

app.MapPost(
    "/api/catalog/moderation/blocks",
    async (
        HttpRequest httpRequest,
        BlockUserRequest request,
        IModerationService moderationService,
        CancellationToken cancellationToken) =>
        await ResolveQueryRequestAsync(
                async () => await moderationService.BlockUserAsync(GetNormalizedEmailParameter(httpRequest, "blockerEmail"), request, cancellationToken).ConfigureAwait(false))
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
    "/api/admin/catalog/system-report",
    async (IWebsiteAdminQueryService adminQueryService, CancellationToken cancellationToken) =>
        await ResolveQueryRequestAsync(
                async () => await adminQueryService.GetSystemReportAsync(cancellationToken).ConfigureAwait(false))
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

app.MapPost(
    "/api/admin/catalog/conversation-events",
    async (
        AdminSaveConversationEventRequest request,
        IConversationEventAdminService conversationEventAdminService,
        CancellationToken cancellationToken) =>
        await ResolveQueryRequestAsync(
                async () => await conversationEventAdminService.SaveAsync(request, cancellationToken).ConfigureAwait(false))
            .ConfigureAwait(false));

app.MapGet(
    "/api/admin/catalog/conversation-events/by-organizer/{organizerProfileSlug}",
    async (
        string organizerProfileSlug,
        IConversationEventAdminService conversationEventAdminService,
        CancellationToken cancellationToken) =>
        await ResolveQueryRequestAsync(
                async () => await conversationEventAdminService.GetByOrganizerProfileSlugAsync(organizerProfileSlug, cancellationToken).ConfigureAwait(false))
            .ConfigureAwait(false));

app.MapPost(
    "/api/admin/catalog/conversation-events/{slug}/publication-status",
    async (
        string slug,
        AdminSetConversationEventPublicationStatusRequest request,
        IConversationEventAdminService conversationEventAdminService,
        CancellationToken cancellationToken) =>
        await ResolveQueryRequestAsync(
                async () => await conversationEventAdminService.SetPublicationStatusAsync(slug, request, cancellationToken).ConfigureAwait(false))
            .ConfigureAwait(false));

app.MapGet(
    "/api/admin/catalog/conversation-events/{slug}/rsvps",
    async (string slug, IEventRsvpService eventRsvpService, CancellationToken cancellationToken) =>
        await ResolveQueryRequestAsync(
                async () => await eventRsvpService.GetByEventAsync(slug, cancellationToken).ConfigureAwait(false))
            .ConfigureAwait(false));

app.MapPost(
    "/api/admin/catalog/conversation-events/{slug}/rsvps/{rsvpId:guid}/status",
    async (
        string slug,
        Guid rsvpId,
        AdminSetEventRsvpStatusRequest request,
        IEventRsvpService eventRsvpService,
        CancellationToken cancellationToken) =>
        await ResolveMutationRequestAsync(
                async () => await eventRsvpService.SetStatusAsync(slug, rsvpId, request, cancellationToken).ConfigureAwait(false),
                static response => response.Id != Guid.Empty)
            .ConfigureAwait(false));

app.MapPost(
    "/api/admin/catalog/organizer-profiles",
    async (
        AdminSaveOrganizerProfileRequest request,
        IOrganizerProfileAdminService organizerProfileAdminService,
        CancellationToken cancellationToken) =>
        await ResolveQueryRequestAsync(
                async () => await organizerProfileAdminService.SaveAsync(request, cancellationToken).ConfigureAwait(false))
            .ConfigureAwait(false));

app.MapGet(
    "/api/admin/catalog/organizer-claim-requests",
    async (IOrganizerClaimRequestService organizerClaimRequestService, CancellationToken cancellationToken) =>
        await ResolveQueryRequestAsync(
                async () => await organizerClaimRequestService.GetRecentAsync(cancellationToken).ConfigureAwait(false))
            .ConfigureAwait(false));

app.MapPost(
    "/api/admin/catalog/organizer-claim-requests/{claimRequestId:guid}/status",
    async (
        Guid claimRequestId,
        OrganizerClaimDecisionRequest request,
        IOrganizerClaimRequestService organizerClaimRequestService,
        CancellationToken cancellationToken) =>
        await ResolveQueryRequestAsync(
                async () => await organizerClaimRequestService.SetStatusAsync(claimRequestId, request, cancellationToken).ConfigureAwait(false))
            .ConfigureAwait(false));

app.MapGet(
    "/api/admin/catalog/organizer-profile-owners",
    async (IOrganizerProfileOwnerService organizerProfileOwnerService, CancellationToken cancellationToken) =>
        await ResolveQueryRequestAsync(
                async () => await organizerProfileOwnerService.GetRecentAsync(cancellationToken).ConfigureAwait(false))
            .ConfigureAwait(false));

app.MapPost(
    "/api/admin/catalog/organizer-profile-owners",
    async (
        AssignOrganizerProfileOwnerRequest request,
        IOrganizerProfileOwnerService organizerProfileOwnerService,
        CancellationToken cancellationToken) =>
        await ResolveQueryRequestAsync(
                async () => await organizerProfileOwnerService.AssignAsync(request, cancellationToken).ConfigureAwait(false))
            .ConfigureAwait(false));

app.MapGet(
    "/api/admin/catalog/moderation/reports",
    async (string? status, IModerationService moderationService, CancellationToken cancellationToken) =>
        await ResolveQueryRequestAsync(
                async () => await moderationService.GetReportsAsync(status, cancellationToken).ConfigureAwait(false))
            .ConfigureAwait(false));

app.MapPost(
    "/api/admin/catalog/moderation/reports/{reportId:guid}/decision",
    async (
        Guid reportId,
        ModerationDecisionRequest request,
        IModerationService moderationService,
        CancellationToken cancellationToken) =>
        await ResolveQueryRequestAsync(
                async () => await moderationService.DecideReportAsync(reportId, request, cancellationToken).ConfigureAwait(false))
            .ConfigureAwait(false));

app.MapGet(
    "/api/admin/catalog/moderation/audits",
    async (IModerationService moderationService, CancellationToken cancellationToken) =>
        await ResolveQueryRequestAsync(
                async () => await moderationService.GetDecisionAuditsAsync(cancellationToken).ConfigureAwait(false))
            .ConfigureAwait(false));

app.MapGet(
    "/api/catalog/organizer-profile-owners/by-email",
    async (HttpRequest httpRequest, IOrganizerProfileOwnerService organizerProfileOwnerService, CancellationToken cancellationToken) =>
        await ResolveQueryRequestAsync(
                async () => await organizerProfileOwnerService.GetByOwnerEmailAsync(GetNormalizedEmailParameter(httpRequest, "ownerEmail"), cancellationToken).ConfigureAwait(false))
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
    catch (DomainRuleException exception)
    {
        return Results.BadRequest(new
        {
            code = "invalid_request",
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
    catch (DomainRuleException exception)
    {
        return Results.BadRequest(new
        {
            code = "invalid_request",
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
    catch (DomainRuleException exception)
    {
        return Results.BadRequest(new
        {
            code = "invalid_request",
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
    catch (DomainRuleException exception)
    {
        return Results.BadRequest(new
        {
            code = "invalid_request",
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

static string GetRequiredConnectionString(IConfiguration configuration, string errorMessage, params string[] names)
{
    return GetOptionalConnectionString(configuration, names)
        ?? throw new InvalidOperationException(errorMessage);
}

static string? GetOptionalConnectionString(IConfiguration configuration, params string[] names)
{
    foreach (string name in names)
    {
        string? connectionString = configuration.GetConnectionString(name);
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            return connectionString;
        }
    }

    return null;
}

static async Task EnforceAdminApiAccessAsync(HttpContext context, RequestDelegate next)
{
    if (!IsProtectedApiPath(context.Request.Path, context.Request.Method))
    {
        await next(context).ConfigureAwait(false);
        return;
    }

    if (context.User.Identity?.IsAuthenticated == true &&
        context.User.IsInRole(DarwinLinguaRoles.Admin))
    {
        await next(context).ConfigureAwait(false);
        return;
    }

    AdminApiAccessOptions options = context.RequestServices
        .GetRequiredService<IOptions<AdminApiAccessOptions>>()
        .Value;

    if (string.IsNullOrWhiteSpace(options.ApiKey))
    {
        context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        await context.Response.WriteAsync("Admin API access is not configured.", context.RequestAborted)
            .ConfigureAwait(false);
        return;
    }

    string? suppliedKey = context.Request.Headers[options.HeaderName].FirstOrDefault();
    if (IsMatchingSecret(suppliedKey, options.ApiKey))
    {
        await next(context).ConfigureAwait(false);
        return;
    }

    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
    await context.Response.WriteAsync("Admin API credentials are required.", context.RequestAborted)
        .ConfigureAwait(false);
}

static async Task<Dictionary<string, string[]>> LoadRolesByUserIdAsync(
    WebApiIdentityDbContext identityDbContext,
    IReadOnlyCollection<string> userIds,
    CancellationToken cancellationToken)
{
    if (userIds.Count == 0)
    {
        return [];
    }

    var roleRows = await identityDbContext.UserRoles
        .AsNoTracking()
        .Where(userRole => userIds.Contains(userRole.UserId))
        .Join(
            identityDbContext.Roles.AsNoTracking(),
            userRole => userRole.RoleId,
            role => role.Id,
            (userRole, role) => new { userRole.UserId, role.Name })
        .ToArrayAsync(cancellationToken)
        .ConfigureAwait(false);

    return roleRows
        .GroupBy(static row => row.UserId, StringComparer.Ordinal)
        .ToDictionary(
            static group => group.Key,
            static group => group
                .Select(static row => row.Name)
                .Where(static roleName => !string.IsNullOrWhiteSpace(roleName))
                .Select(static roleName => roleName!)
                .OrderBy(static roleName => roleName)
                .ToArray(),
            StringComparer.Ordinal);
}

static string? TryGetPrincipalEmail(ClaimsPrincipal principal)
{
    string? candidate = principal.FindFirstValue(ClaimTypes.Email)
        ?? principal.Identity?.Name;

    return !string.IsNullOrWhiteSpace(candidate) && candidate.Contains('@', StringComparison.Ordinal)
        ? candidate
        : null;
}

static string GetNormalizedEmailParameter(HttpRequest request, string queryParameterName)
{
    string? value = request.Headers["X-DarwinLingua-Actor-Email"].FirstOrDefault();
    if (string.IsNullOrWhiteSpace(value))
    {
        value = request.Query[queryParameterName].FirstOrDefault();
    }

    return NormalizeEmailParameter(value ?? string.Empty, queryParameterName);
}

static string NormalizeEmailParameter(string value, string parameterName)
{
    try
    {
        return LearnerConversationProfile.NormalizeEmail(value);
    }
    catch (DomainRuleException exception)
    {
        throw new DomainRuleException($"{parameterName} is invalid. {exception.Message}");
    }
}

static bool IsProtectedApiPath(PathString path, string method) =>
    path.StartsWithSegments("/api/admin/catalog", StringComparison.OrdinalIgnoreCase) ||
    path.StartsWithSegments("/api/admin/content/catalog", StringComparison.OrdinalIgnoreCase) ||
    path.StartsWithSegments("/api/catalog/learner-conversation-profiles/me", StringComparison.OrdinalIgnoreCase) ||
    path.StartsWithSegments("/api/catalog/partner-matches", StringComparison.OrdinalIgnoreCase) ||
    path.StartsWithSegments("/api/catalog/partner-requests", StringComparison.OrdinalIgnoreCase) ||
    path.StartsWithSegments("/api/catalog/moderation", StringComparison.OrdinalIgnoreCase) ||
    path.StartsWithSegments("/api/catalog/organizer-profile-owners", StringComparison.OrdinalIgnoreCase) ||
    (HttpMethods.IsPost(method) &&
        path.StartsWithSegments("/api/catalog/organizer-profiles", StringComparison.OrdinalIgnoreCase)) ||
    (HttpMethods.IsPost(method) &&
        path.StartsWithSegments("/api/catalog/conversation-events", StringComparison.OrdinalIgnoreCase));

static bool IsMatchingSecret(string? suppliedSecret, string configuredSecret)
{
    if (string.IsNullOrWhiteSpace(suppliedSecret) ||
        string.IsNullOrWhiteSpace(configuredSecret))
    {
        return false;
    }

    byte[] suppliedBytes = Encoding.UTF8.GetBytes(suppliedSecret);
    byte[] configuredBytes = Encoding.UTF8.GetBytes(configuredSecret);
    return suppliedBytes.Length == configuredBytes.Length &&
        CryptographicOperations.FixedTimeEquals(suppliedBytes, configuredBytes);
}

public sealed class AdminApiAccessOptions
{
    public const string SectionName = "AdminApi";

    public string HeaderName { get; set; } = "X-DarwinLingua-Admin-Key";

    public string ApiKey { get; set; } = string.Empty;
}

public partial class Program;
