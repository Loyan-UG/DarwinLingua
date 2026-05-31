using DarwinLingua.Catalog.Infrastructure.DependencyInjection;
using DarwinLingua.ContentOps.Application.Abstractions;
using DarwinLingua.ContentOps.Application.DependencyInjection;
using DarwinLingua.ContentOps.Infrastructure.DependencyInjection;
using DarwinLingua.Infrastructure.DependencyInjection;
using DarwinLingua.Infrastructure.Persistence.Abstractions;
using DarwinLingua.Localization.Infrastructure.DependencyInjection;
using DarwinLingua.WebApi.Configuration;
using DarwinLingua.WebApi.Models;
using DarwinLingua.WebApi.Persistence;
using DarwinLingua.WebApi.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace DarwinLingua.WebApi.Tests;

internal sealed class WebApiPostgresTestHost : IAsyncDisposable
{
    private WebApiPostgresTestHost(
        string tempRoot,
        PostgresTestDatabase catalogDatabase,
        PostgresTestDatabase serverDatabase,
        ServiceProvider serviceProvider)
    {
        TempRoot = tempRoot;
        CatalogDatabase = catalogDatabase;
        ServerDatabase = serverDatabase;
        ServiceProvider = serviceProvider;
    }

    public string TempRoot { get; }

    public string PackageRootPath => Path.Combine(TempRoot, "packages");

    public PostgresTestDatabase CatalogDatabase { get; }

    public PostgresTestDatabase ServerDatabase { get; }

    public ServiceProvider ServiceProvider { get; }

    public static async Task<WebApiPostgresTestHost> CreateAsync(string tempPrefix, CancellationToken cancellationToken = default)
    {
        string tempRoot = Path.Combine(Path.GetTempPath(), tempPrefix, Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempRoot);
        Directory.CreateDirectory(Path.Combine(tempRoot, "packages"));

        PostgresTestDatabase catalogDatabase = await PostgresTestDatabase.CreateAsync("darwin_webapi_catalog", cancellationToken);
        PostgresTestDatabase serverDatabase = await PostgresTestDatabase.CreateAsync("darwin_webapi_server", cancellationToken);

        ServiceCollection services = new();
        services.AddOptions();
        services.AddLogging();

        services
            .AddDarwinLinguaInfrastructureForPostgres(catalogDatabase.ConnectionString)
            .AddCatalogInfrastructure()
            .AddContentOpsApplication()
            .AddContentOpsInfrastructure()
            .AddLocalizationInfrastructure();

        services.AddDbContext<ServerContentDbContext>(options => options.UseNpgsql(serverDatabase.ConnectionString));
        services.AddScoped<IContentImportRepository, WebApiContentImportRepository>();
        services.AddScoped<IServerContentDatabaseBootstrapper, ServerContentDatabaseBootstrapper>();
        services.AddScoped<IContentPublicationAuditService, ContentPublicationAuditService>();
        services.AddScoped<ICatalogPackagePublisher, CatalogPackagePublisher>();
        services.AddScoped<ICatalogPackageDraftQueryService, CatalogPackageDraftQueryService>();
        services.AddScoped<ICatalogPackageReleaseService, CatalogPackageReleaseService>();
        services.AddScoped<ICatalogPublicationHistoryService, CatalogPublicationHistoryService>();
        services.AddScoped<ICatalogPackageRollbackService, CatalogPackageRollbackService>();
        services.AddScoped<ICatalogPackageCleanupService, CatalogPackageCleanupService>();
        services.AddScoped<IServerCatalogImportService, ServerCatalogImportService>();
        services.AddSingleton<IWebHostEnvironment>(new TestWebHostEnvironment(tempRoot));
        services.AddSingleton<IHostEnvironment>(new TestWebHostEnvironment(tempRoot));
        services.Configure<ServerContentOptions>(options =>
        {
            options.PublicBaseUrl = "http://localhost:5099";
            options.DefaultSchemaVersion = 1;
            options.PackageStorage.RootPath = Path.Combine(tempRoot, "packages");
            options.ClientProducts.Add(new ClientProductOptions
            {
                Key = "darwin-deutsch",
                DisplayName = "Darwin Deutsch",
                LearningLanguageCode = "de",
                DefaultUiLanguageCode = "en",
                IsActive = true,
            });
        });

        ServiceProvider serviceProvider = services.BuildServiceProvider();
        WebApiPostgresTestHost host = new(tempRoot, catalogDatabase, serverDatabase, serviceProvider);

        await host.InitializeAsync(cancellationToken);
        return host;
    }

    public static string ResolveFixturePath()
    {
        DirectoryInfo? currentDirectory = new(AppContext.BaseDirectory);

        while (currentDirectory is not null)
        {
            string candidateSolutionPath = Path.Combine(currentDirectory.FullName, "DarwinLingua.slnx");
            if (File.Exists(candidateSolutionPath))
            {
                return Path.Combine(
                    currentDirectory.FullName,
                    "tests",
                    "Modules",
                    "ContentOps",
                    "DarwinLingua.ContentOps.Infrastructure.Tests",
                    "Fixtures",
                    "phase1-sample-content-package.json");
            }

            currentDirectory = currentDirectory.Parent;
        }

        throw new DirectoryNotFoundException("Unable to resolve the repository root for the content import fixture.");
    }

    public async ValueTask DisposeAsync()
    {
        await ServiceProvider.DisposeAsync();
        await CatalogDatabase.DisposeAsync();
        await ServerDatabase.DisposeAsync();
        TryDeleteDirectory(TempRoot);
    }

    private async Task InitializeAsync(CancellationToken cancellationToken)
    {
        IDatabaseInitializer databaseInitializer = ServiceProvider.GetRequiredService<IDatabaseInitializer>();
        await databaseInitializer.InitializeAsync(cancellationToken);

        IServerContentDatabaseBootstrapper bootstrapper = ServiceProvider.GetRequiredService<IServerContentDatabaseBootstrapper>();
        await bootstrapper.InitializeAsync(cancellationToken);
    }

    private static void TryDeleteDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            return;
        }

        try
        {
            Directory.Delete(path, recursive: true);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    private sealed class TestWebHostEnvironment(string contentRootPath) : IWebHostEnvironment, IHostEnvironment
    {
        public string ApplicationName { get; set; } = "DarwinLingua.WebApi.Tests";

        public IFileProvider ContentRootFileProvider { get; set; } = new PhysicalFileProvider(contentRootPath);

        public string ContentRootPath { get; set; } = contentRootPath;

        public string EnvironmentName { get; set; } = "Development";

        public string WebRootPath { get; set; } = contentRootPath;

        public IFileProvider WebRootFileProvider { get; set; } = new PhysicalFileProvider(contentRootPath);
    }
}
