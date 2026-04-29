using DarwinLingua.Infrastructure.DependencyInjection;
using DarwinLingua.Identity;
using DarwinLingua.Infrastructure.Persistence.Abstractions;
using DarwinLingua.Learning.Application.Abstractions;
using DarwinLingua.Learning.Application.DependencyInjection;
using DarwinLingua.Learning.Infrastructure.DependencyInjection;
using DarwinLingua.Localization.Application.DependencyInjection;
using DarwinLingua.Localization.Infrastructure.DependencyInjection;
using DarwinLingua.Web.Data;
using Microsoft.AspNetCore.ResponseCompression;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
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
    options.AddPolicy("LandingPage", policy => policy.Expire(TimeSpan.FromSeconds(30)));
    options.AddPolicy("CatalogBrowse", policy => policy
        .Expire(TimeSpan.FromMinutes(2))
        .SetVaryByQuery("id", "skip"));
});
builder.Services.Configure<MemoryCacheOptions>(options => options.SizeLimit = 512);
builder.Services.Configure<DarwinLinguaIdentityBootstrapOptions>(builder.Configuration.GetSection("IdentityBootstrap"));
builder.Services.Configure<DarwinLinguaEntitlementOptions>(builder.Configuration.GetSection("Entitlements"));
builder.Services.AddOptions<TransactionalEmailOptions>()
    .Bind(builder.Configuration.GetSection(TransactionalEmailOptions.SectionName))
    .ValidateOnStart();
builder.Services.AddSingleton<Microsoft.Extensions.Options.IPostConfigureOptions<DarwinLinguaIdentityBootstrapOptions>, DarwinLinguaIdentityBootstrapOptionsPostConfigure>();
builder.Services.AddSingleton<Microsoft.Extensions.Options.IPostConfigureOptions<DarwinLinguaEntitlementOptions>, DarwinLinguaEntitlementOptionsPostConfigure>();
builder.Services.AddSingleton<Microsoft.Extensions.Options.IValidateOptions<TransactionalEmailOptions>, TransactionalEmailOptionsValidator>();
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
builder.Services.AddScoped<IWebActorContextAccessor, WebActorContextAccessor>();
builder.Services.AddScoped<IWebLearningProfileAccessor, WebLearningProfileAccessor>();
builder.Services.AddHttpClient("BrevoTransactionalEmail");
builder.Services.AddScoped<IWebActivityQueryService, WebActivityQueryService>();
builder.Services.AddScoped<IWebAdminDashboardQueryService, WebAdminDashboardQueryService>();
builder.Services.AddScoped<IWebAdminOperationsQueryService, WebAdminOperationsQueryService>();
builder.Services.AddScoped<IWebEntitledFeatureAccessService, WebEntitledFeatureAccessService>();
builder.Services.AddScoped<IWebUserPreferenceService, WebUserPreferenceService>();
builder.Services.AddScoped<IWebFavoriteWordService, WebFavoriteWordService>();
builder.Services.AddScoped<IWebUserWordStateService, WebUserWordStateService>();
builder.Services.AddScoped<IWebUserStateDatabaseBootstrapper, WebUserStateDatabaseBootstrapper>();
builder.Services.AddScoped<IDarwinLinguaIdentityBootstrapper, DarwinLinguaIdentityBootstrapper<WebIdentityDbContext>>();
builder.Services.AddScoped<IUserEntitlementService, UserEntitlementService<WebIdentityDbContext>>();
builder.Services.AddScoped<IEmailTemplateRenderer, TransactionalEmailTemplateRenderer>();
builder.Services.AddScoped<ITransactionalEmailSender, TransactionalEmailSender>();
builder.Services.AddScoped<IEmailDeliveryLogRepository, EmailDeliveryLogRepository>();
builder.Services.AddScoped<IAccountEmailService, AccountEmailService>();
builder.Services.AddScoped<ICommunityNotificationEmailService, CommunityNotificationEmailService>();
builder.Services.AddSingleton<IAccountEmailRateLimiter, AccountEmailRateLimiter>();
builder.Services.AddHostedService<EmailDeliveryFailureMonitorService>();
builder.Services.AddWebCatalogApiClient(builder.Configuration);
string? webIdentityConnectionString = builder.Configuration.GetConnectionString("IdentityAdmin")
    ?? builder.Configuration.GetConnectionString("Identity")
    ?? builder.Configuration.GetConnectionString("WebIdentity");
string appDataDirectory = Path.Combine(builder.Environment.ContentRootPath, "App_Data");
string webLearningDatabasePath = Path.Combine(appDataDirectory, "darwin-lingua.web.db");

if (!string.IsNullOrWhiteSpace(webIdentityConnectionString))
{
    builder.Services.AddDbContext<WebIdentityDbContext>(options =>
        options.UseNpgsql(webIdentityConnectionString));
}
else
{
    string identityDatabasePath = Path.Combine(appDataDirectory, "darwin-lingua.web-identity.db");

    builder.Services.AddDbContext<WebIdentityDbContext>(options =>
        options.UseSqlite($"Data Source={identityDatabasePath}"));
}

builder.Services
    .AddDarwinLinguaInfrastructure(options => options.DatabasePath = webLearningDatabasePath)
    .AddLearningApplication()
    .AddLearningInfrastructure()
    .AddLocalizationApplication()
    .AddLocalizationInfrastructure();

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
    IDatabaseInitializer databaseInitializer = bootstrapScope.ServiceProvider.GetRequiredService<IDatabaseInitializer>();
    IDarwinLinguaIdentityBootstrapper identityBootstrapper = bootstrapScope.ServiceProvider.GetRequiredService<IDarwinLinguaIdentityBootstrapper>();
    IWebUserStateDatabaseBootstrapper webUserStateBootstrapper = bootstrapScope.ServiceProvider.GetRequiredService<IWebUserStateDatabaseBootstrapper>();

    await databaseInitializer.InitializeAsync(CancellationToken.None);
    await identityBootstrapper.InitializeAsync(CancellationToken.None);
    await webUserStateBootstrapper.InitializeAsync(CancellationToken.None);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

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
        "script-src 'self' https://unpkg.com; " +
        "style-src 'self'; " +
        "img-src 'self' data:; " +
        "font-src 'self' data:; " +
        "connect-src 'self'; " +
        "manifest-src 'self'; " +
        "object-src 'none'; " +
        "base-uri 'self'; " +
        "form-action 'self'; " +
        "frame-ancestors 'none'; " +
        "worker-src 'self'";

    await next();
});
app.UseRouting();
app.UseMiddleware<WebRequestTelemetryMiddleware>();
app.UseAuthentication();
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
