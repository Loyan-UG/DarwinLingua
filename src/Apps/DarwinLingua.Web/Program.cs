using DarwinLingua.Identity;
using DarwinLingua.SharedKernel.Globalization;
using DarwinLingua.Web.Data;
using DarwinLingua.Web.Localization;
using Microsoft.AspNetCore.ResponseCompression;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Globalization;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile(
    $"appsettings.{builder.Environment.EnvironmentName}.Local.json",
    optional: true,
    reloadOnChange: true);
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>(optional: true);
}

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services
    .AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization(options =>
    {
        options.DataAnnotationLocalizerProvider = (_, factory) =>
            factory.Create(typeof(SharedResource));
    });
builder.Services
    .AddRazorPages()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization(options =>
    {
        options.DataAnnotationLocalizerProvider = (_, factory) =>
            factory.Create(typeof(SharedResource));
    });
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IWebPerformanceTelemetryService, WebPerformanceTelemetryService>();
builder.Services.AddSingleton<IWebProductAnalyticsService, WebProductAnalyticsService>();
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});
builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(policy => policy.Expire(TimeSpan.FromSeconds(60)));
    options.AddPolicy("LandingPage", policy => policy
        .Expire(TimeSpan.FromSeconds(30))
        .SetVaryByHeader("Cookie"));
    options.AddPolicy("CatalogBrowse", policy => policy
        .Expire(TimeSpan.FromMinutes(2))
        .Tag("catalog")
        .SetVaryByHeader("Cookie")
        .SetVaryByQuery(
            "id",
            "skip",
            "city",
            "cefrLevel",
            "helperLanguageCode",
            "isOnline",
            "priceType",
            "category",
            "situation",
            "tone",
            "conversationGoal",
            "topicKey",
            "topic",
            "contentType",
            "speakingGoal",
            "q"));
});
builder.Services.Configure<MemoryCacheOptions>(options => options.SizeLimit = 512);
builder.Services.Configure<DarwinLinguaIdentityBootstrapOptions>(builder.Configuration.GetSection("IdentityBootstrap"));
builder.Services.Configure<DarwinLinguaEntitlementOptions>(builder.Configuration.GetSection("Entitlements"));
builder.Services.AddOptions<TransactionalEmailOptions>()
    .Bind(builder.Configuration.GetSection(TransactionalEmailOptions.SectionName))
    .ValidateOnStart();
builder.Services.AddOptions<BillingOptions>()
    .Bind(builder.Configuration.GetSection(BillingOptions.SectionName))
    .ValidateOnStart();
builder.Services.AddSingleton<Microsoft.Extensions.Options.IPostConfigureOptions<DarwinLinguaIdentityBootstrapOptions>, DarwinLinguaIdentityBootstrapOptionsPostConfigure>();
builder.Services.AddSingleton<Microsoft.Extensions.Options.IPostConfigureOptions<DarwinLinguaEntitlementOptions>, DarwinLinguaEntitlementOptionsPostConfigure>();
builder.Services.AddSingleton<Microsoft.Extensions.Options.IValidateOptions<TransactionalEmailOptions>, TransactionalEmailOptionsValidator>();
builder.Services.AddSingleton<Microsoft.Extensions.Options.IValidateOptions<BillingOptions>, BillingOptionsValidator>();
builder.Services.Configure<EmailConfirmationTokenProviderOptions>(options =>
{
    int hours = builder.Configuration.GetValue("TransactionalEmail:EmailConfirmationTokenHours", 24);
    options.TokenLifespan = TimeSpan.FromHours(hours);
});
builder.Services.Configure<PasswordResetTokenProviderOptions>(options =>
{
    int minutes = builder.Configuration.GetValue("TransactionalEmail:PasswordResetTokenMinutes", 60);
    options.TokenLifespan = TimeSpan.FromMinutes(minutes);
});
builder.Services.Configure<EmailChangeTokenProviderOptions>(options =>
{
    int minutes = builder.Configuration.GetValue("TransactionalEmail:EmailChangeTokenMinutes", 60);
    options.TokenLifespan = TimeSpan.FromMinutes(minutes);
});
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto |
        ForwardedHeaders.XForwardedHost;
    options.KnownProxies.Add(IPAddress.Parse("192.168.178.22"));
});
builder.Services.AddScoped<IWebActorContextAccessor, WebActorContextAccessor>();
builder.Services.AddScoped<IWebLearningProfileAccessor, WebLearningProfileAccessor>();
builder.Services.AddHttpClient("BrevoTransactionalEmail", client =>
{
    client.Timeout = TimeSpan.FromSeconds(20);
});
builder.Services.AddHttpClient("StripeBilling", (serviceProvider, client) =>
{
    BillingOptions billingOptions = serviceProvider
        .GetRequiredService<Microsoft.Extensions.Options.IOptions<BillingOptions>>()
        .Value;
    client.BaseAddress = new Uri(billingOptions.StripeApiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(20);
});
builder.Services.AddScoped<IWebActivityQueryService, WebActivityQueryService>();
builder.Services.AddScoped<IWebAdminDashboardQueryService, WebAdminDashboardQueryService>();
builder.Services.AddScoped<IWebAdminOperationsQueryService, WebAdminOperationsQueryService>();
builder.Services.AddScoped<IWebEntitledFeatureAccessService, WebEntitledFeatureAccessService>();
builder.Services.AddScoped<IWebUserPreferenceService, WebUserPreferenceService>();
builder.Services.AddScoped<IWebFavoriteWordService, WebFavoriteWordService>();
builder.Services.AddScoped<IWebUserWordStateService, WebUserWordStateService>();
builder.Services.AddScoped<IWebWordSuggestionService, WebWordSuggestionService>();
builder.Services.AddScoped<IWebUserStateDatabaseBootstrapper, WebUserStateDatabaseBootstrapper>();
builder.Services.AddScoped<IDarwinLinguaIdentityBootstrapper, DarwinLinguaIdentityBootstrapper<WebIdentityDbContext>>();
builder.Services.AddScoped<IUserEntitlementService, UserEntitlementService<WebIdentityDbContext>>();
builder.Services.AddScoped<IEmailTemplateRenderer, TransactionalEmailTemplateRenderer>();
builder.Services.AddScoped<ITransactionalEmailSender, TransactionalEmailSender>();
builder.Services.AddScoped<IEmailDeliveryLogRepository, EmailDeliveryLogRepository>();
builder.Services.AddScoped<IAccountEmailService, AccountEmailService>();
builder.Services.AddScoped<ICommunityNotificationEmailService, CommunityNotificationEmailService>();
builder.Services.AddScoped<IBillingNotificationEmailService, BillingNotificationEmailService>();
builder.Services.AddScoped<IStripeBillingCheckoutService, StripeBillingCheckoutService>();
builder.Services.AddScoped<IStripeBillingWebhookHandler, StripeBillingWebhookHandler>();
builder.Services.AddScoped<IStripeBillingReconciliationService, StripeBillingReconciliationService>();
builder.Services.AddScoped<IStripeCheckoutFulfillmentService, StripeCheckoutFulfillmentService>();
builder.Services.AddScoped<StripeWebhookVerifier>();
builder.Services.AddSingleton<IAccountEmailRateLimiter, AccountEmailRateLimiter>();
builder.Services.AddSingleton<IBillingOperationRateLimiter, BillingOperationRateLimiter>();
builder.Services.AddHostedService<EmailDeliveryFailureMonitorService>();
builder.Services.AddWebCatalogApiClient(builder.Configuration);
string? webIdentityConnectionString = builder.Configuration.GetConnectionString("IdentityAdmin")
    ?? builder.Configuration.GetConnectionString("Identity")
    ?? builder.Configuration.GetConnectionString("WebIdentity");
if (string.IsNullOrWhiteSpace(webIdentityConnectionString))
{
    throw new InvalidOperationException("A PostgreSQL Identity or WebIdentity connection string must be configured for DarwinLingua.Web.");
}

builder.Services.AddDbContext<WebIdentityDbContext>(options =>
    options.UseNpgsql(webIdentityConnectionString));

builder.Services
    .AddDefaultIdentity<DarwinLinguaIdentityUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequiredLength = 8;
        options.Tokens.EmailConfirmationTokenProvider = DarwinLinguaIdentityTokenProviders.EmailConfirmation;
        options.Tokens.PasswordResetTokenProvider = DarwinLinguaIdentityTokenProviders.PasswordReset;
        options.Tokens.ChangeEmailTokenProvider = DarwinLinguaIdentityTokenProviders.EmailChange;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<WebIdentityDbContext>()
    .AddDefaultTokenProviders()
    .AddTokenProvider<EmailConfirmationTokenProvider>(DarwinLinguaIdentityTokenProviders.EmailConfirmation)
    .AddTokenProvider<PasswordResetTokenProvider>(DarwinLinguaIdentityTokenProviders.PasswordReset)
    .AddTokenProvider<EmailChangeTokenProvider>(DarwinLinguaIdentityTokenProviders.EmailChange)
    .AddDefaultUI();
builder.Services.AddScoped<IUserClaimsPrincipalFactory<DarwinLinguaIdentityUser>, DefaultLearnerRoleClaimsPrincipalFactory>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Operator", policy => policy.RequireRole("Operator", "Admin"));
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
    options.AddPolicy("Organizer", policy => policy.RequireRole(DarwinLinguaRoles.Organizer, DarwinLinguaRoles.Operator, DarwinLinguaRoles.Admin));
});

var app = builder.Build();

await using (AsyncServiceScope bootstrapScope = app.Services.CreateAsyncScope())
{
    IDarwinLinguaIdentityBootstrapper identityBootstrapper = bootstrapScope.ServiceProvider.GetRequiredService<IDarwinLinguaIdentityBootstrapper>();
    IWebUserStateDatabaseBootstrapper webUserStateBootstrapper = bootstrapScope.ServiceProvider.GetRequiredService<IWebUserStateDatabaseBootstrapper>();

    await identityBootstrapper.InitializeAsync(CancellationToken.None);
    await webUserStateBootstrapper.InitializeAsync(CancellationToken.None);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseForwardedHeaders();
app.UseHttpsRedirection();
app.UseResponseCompression();
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
    context.Response.Headers["Content-Security-Policy"] =
        "default-src 'self'; " +
        "script-src 'self'; " +
        "style-src 'self'; " +
        "img-src 'self' data:; " +
        "font-src 'self' data:; " +
        "connect-src 'self'; " +
        "manifest-src 'self'; " +
        "object-src 'none'; " +
        "base-uri 'self'; " +
        "form-action 'self' https://checkout.stripe.com; " +
        "frame-ancestors 'none'; " +
        "worker-src 'self'";

    await next();
});
app.UseRouting();
string[] supportedUiLanguageCodes = ["en", "de"];
RequestLocalizationOptions localizationOptions = new()
{
    DefaultRequestCulture = new RequestCulture("en"),
    SupportedCultures = supportedUiLanguageCodes.Select(static code => new CultureInfo(code)).ToArray(),
    SupportedUICultures = supportedUiLanguageCodes.Select(static code => new CultureInfo(code)).ToArray()
};
localizationOptions.RequestCultureProviders.Insert(
    0,
    new CustomRequestCultureProvider(async context =>
    {
        try
        {
            if (context.User.Identity?.IsAuthenticated != true)
            {
                return null;
            }

            IWebLearningProfileAccessor profileAccessor = context.RequestServices.GetRequiredService<IWebLearningProfileAccessor>();
            string? uiLanguageCode = (await profileAccessor.GetProfileAsync(context.RequestAborted).ConfigureAwait(false)).UiLanguageCode;

            if (ContentLanguageRequirements.RequiredLocalizationLanguageCodes.Contains(uiLanguageCode, StringComparer.OrdinalIgnoreCase) &&
                supportedUiLanguageCodes.Contains(uiLanguageCode, StringComparer.OrdinalIgnoreCase))
            {
                return new ProviderCultureResult(uiLanguageCode);
            }
        }
        catch
        {
            // Fall back to the standard providers below.
        }

        return null;
    }));
app.UseMiddleware<WebRequestTelemetryMiddleware>();
app.UseAuthentication();
app.UseRequestLocalization(localizationOptions);
app.UseOutputCache();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

public partial class Program;
