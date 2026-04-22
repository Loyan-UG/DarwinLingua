using DarwinLingua.Catalog.Application.DependencyInjection;
using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Infrastructure.DependencyInjection;
using DarwinLingua.Infrastructure.DependencyInjection;
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
builder.Services.Configure<WebIdentityBootstrapOptions>(builder.Configuration.GetSection("IdentityBootstrap"));
builder.Services.AddScoped<IWebActorContextAccessor, WebActorContextAccessor>();
builder.Services.AddScoped<IWebLearningProfileAccessor, WebLearningProfileAccessor>();
builder.Services.AddScoped<IWebActivityQueryService, WebActivityQueryService>();
builder.Services.AddScoped<IWebAdminDashboardQueryService, WebAdminDashboardQueryService>();
builder.Services.AddScoped<IWebAdminOperationsQueryService, WebAdminOperationsQueryService>();
builder.Services.AddScoped<IWebUserPreferenceService, WebUserPreferenceService>();
builder.Services.AddScoped<IWebFavoriteWordService, WebFavoriteWordService>();
builder.Services.AddScoped<IWebUserWordStateService, WebUserWordStateService>();
builder.Services.AddScoped<IWebIdentityBootstrapper, WebIdentityBootstrapper>();

string? sharedCatalogConnectionString = builder.Configuration.GetConnectionString("SharedCatalogAdmin")
    ?? builder.Configuration.GetConnectionString("SharedCatalog");
string? webIdentityConnectionString = builder.Configuration.GetConnectionString("WebIdentity");

if (!string.IsNullOrWhiteSpace(webIdentityConnectionString))
{
    builder.Services.AddDbContext<WebIdentityDbContext>(options =>
        options.UseNpgsql(webIdentityConnectionString));
}
else
{
    string identityDirectory = Path.Combine(builder.Environment.ContentRootPath, "App_Data");
    string identityDatabasePath = Path.Combine(identityDirectory, "darwin-lingua.web-identity.db");

    builder.Services.AddDbContext<WebIdentityDbContext>(options =>
        options.UseSqlite($"Data Source={identityDatabasePath}"));
}

if (!string.IsNullOrWhiteSpace(sharedCatalogConnectionString))
{
    builder.Services.AddDarwinLinguaInfrastructureForPostgres(sharedCatalogConnectionString);
}
else
{
    string databaseDirectory = Path.Combine(builder.Environment.ContentRootPath, "App_Data");
    string databasePath = Path.Combine(databaseDirectory, "darwin-lingua.web.db");

    builder.Services.AddDarwinLinguaInfrastructure(options => options.DatabasePath = databasePath);
}

builder.Services
    .AddCatalogApplication()
    .AddCatalogInfrastructure()
    .AddLearningApplication()
    .AddLearningInfrastructure()
    .AddLocalizationApplication()
    .AddLocalizationInfrastructure();

builder.Services
    .AddDefaultIdentity<WebApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequiredLength = 8;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<WebIdentityDbContext>()
    .AddDefaultTokenProviders()
    .AddDefaultUI();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Operator", policy => policy.RequireRole("Operator", "Admin"));
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
});

var app = builder.Build();

await using (AsyncServiceScope bootstrapScope = app.Services.CreateAsyncScope())
{
    IDatabaseInitializer databaseInitializer = bootstrapScope.ServiceProvider.GetRequiredService<IDatabaseInitializer>();
    IWebIdentityBootstrapper identityBootstrapper = bootstrapScope.ServiceProvider.GetRequiredService<IWebIdentityBootstrapper>();

    await databaseInitializer.InitializeAsync(CancellationToken.None);
    await identityBootstrapper.InitializeAsync(CancellationToken.None);
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
