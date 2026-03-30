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
using DarwinLingua.WebApi.Persistence.Entities;
using DarwinLingua.WebApi.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace DarwinLingua.WebApi.Tests;

public sealed class CatalogPackageCleanupServiceTests
{
    [Fact]
    public async Task DeleteSupersededBatchAsync_RemovesBatchRowsAndPayloadFilesAsync()
    {
        string tempRoot = Path.Combine(Path.GetTempPath(), "darwinlingua-webapi-cleanup-tests", Guid.NewGuid().ToString("N"));
        string catalogDatabasePath = Path.Combine(tempRoot, "catalog", "catalog.db");
        string serverDatabasePath = Path.Combine(tempRoot, "server", "server.db");
        string packageRootPath = Path.Combine(tempRoot, "packages");
        Directory.CreateDirectory(tempRoot);
        Directory.CreateDirectory(Path.GetDirectoryName(serverDatabasePath)!);

        try
        {
            using ServiceProvider serviceProvider = BuildServiceProvider(catalogDatabasePath, serverDatabasePath, packageRootPath, tempRoot);
            await InitializeAsync(serviceProvider);

            IServerCatalogImportService importService = serviceProvider.GetRequiredService<IServerCatalogImportService>();
            ICatalogPackagePublisher packagePublisher = serviceProvider.GetRequiredService<ICatalogPackagePublisher>();
            ICatalogPackageReleaseService releaseService = serviceProvider.GetRequiredService<ICatalogPackageReleaseService>();
            ICatalogPackageCleanupService cleanupService = serviceProvider.GetRequiredService<ICatalogPackageCleanupService>();

            string fixturePath = ResolveFixturePath();
            AdminImportCatalogResponse firstImport = await importService.ImportAndStageAsync(
                new AdminImportCatalogRequest(fixturePath, "darwin-deutsch"),
                CancellationToken.None);

            await releaseService.PublishAsync(
                new AdminPublishCatalogRequest("darwin-deutsch", firstImport.DraftPublicationBatchId!),
                CancellationToken.None);

            await Task.Delay(TimeSpan.FromSeconds(1.1));

            CatalogPackagePublicationResult secondBatch = await packagePublisher.StageDraftAsync("darwin-deutsch", CancellationToken.None);
            await releaseService.PublishAsync(
                new AdminPublishCatalogRequest("darwin-deutsch", secondBatch.PublicationBatchId),
                CancellationToken.None);

            string supersededBatchId = firstImport.DraftPublicationBatchId!;

            await using (AsyncServiceScope verificationScope = serviceProvider.CreateAsyncScope())
            {
                ServerContentDbContext dbContext = verificationScope.ServiceProvider.GetRequiredService<ServerContentDbContext>();
                Assert.Equal(8, await dbContext.PublishedPackages.CountAsync(package => package.PublicationBatchId == supersededBatchId));
                Assert.Equal(8, await dbContext.PublishedPackages.CountAsync(package => package.PublicationBatchId == supersededBatchId && package.PublicationStatus == PackagePublicationStatus.Superseded));
            }

            string samplePayloadPath = Path.Combine(packageRootPath, "darwin-deutsch", $"{firstImport.StagedPackageIds[0]}.json");
            Assert.True(File.Exists(samplePayloadPath));

            AdminDeleteCatalogBatchResponse deleteResponse = await cleanupService.DeleteSupersededBatchAsync(
                supersededBatchId,
                "darwin-deutsch",
                CancellationToken.None);

            Assert.True(deleteResponse.IsSuccess);
            Assert.Equal(8, deleteResponse.DeletedPackageCount);
            Assert.False(File.Exists(samplePayloadPath));

            await using (AsyncServiceScope verificationScope = serviceProvider.CreateAsyncScope())
            {
                ServerContentDbContext dbContext = verificationScope.ServiceProvider.GetRequiredService<ServerContentDbContext>();
                Assert.Equal(0, await dbContext.PublishedPackages.CountAsync(package => package.PublicationBatchId == supersededBatchId));
                Assert.Equal(8, await dbContext.PublishedPackages.CountAsync(package => package.PublicationBatchId == secondBatch.PublicationBatchId && package.PublicationStatus == PackagePublicationStatus.Published));
            }
        }
        finally
        {
            TryDeleteDirectory(tempRoot);
        }
    }

    [Fact]
    public async Task DeleteSupersededBatchAsync_RejectsPublishedBatchAsync()
    {
        string tempRoot = Path.Combine(Path.GetTempPath(), "darwinlingua-webapi-cleanup-tests", Guid.NewGuid().ToString("N"));
        string catalogDatabasePath = Path.Combine(tempRoot, "catalog", "catalog.db");
        string serverDatabasePath = Path.Combine(tempRoot, "server", "server.db");
        string packageRootPath = Path.Combine(tempRoot, "packages");
        Directory.CreateDirectory(tempRoot);
        Directory.CreateDirectory(Path.GetDirectoryName(serverDatabasePath)!);

        try
        {
            using ServiceProvider serviceProvider = BuildServiceProvider(catalogDatabasePath, serverDatabasePath, packageRootPath, tempRoot);
            await InitializeAsync(serviceProvider);

            IServerCatalogImportService importService = serviceProvider.GetRequiredService<IServerCatalogImportService>();
            ICatalogPackageReleaseService releaseService = serviceProvider.GetRequiredService<ICatalogPackageReleaseService>();
            ICatalogPackageCleanupService cleanupService = serviceProvider.GetRequiredService<ICatalogPackageCleanupService>();

            AdminImportCatalogResponse importResponse = await importService.ImportAndStageAsync(
                new AdminImportCatalogRequest(ResolveFixturePath(), "darwin-deutsch"),
                CancellationToken.None);

            await releaseService.PublishAsync(
                new AdminPublishCatalogRequest("darwin-deutsch", importResponse.DraftPublicationBatchId!),
                CancellationToken.None);

            AdminDeleteCatalogBatchResponse deleteResponse = await cleanupService.DeleteSupersededBatchAsync(
                importResponse.DraftPublicationBatchId!,
                "darwin-deutsch",
                CancellationToken.None);

            Assert.False(deleteResponse.IsSuccess);
            Assert.Contains("Only superseded package batches can be deleted.", deleteResponse.IssueMessages[0], StringComparison.Ordinal);
        }
        finally
        {
            TryDeleteDirectory(tempRoot);
        }
    }

    private static ServiceProvider BuildServiceProvider(string catalogDatabasePath, string serverDatabasePath, string packageRootPath, string contentRootPath)
    {
        ServiceCollection services = new();
        services.AddOptions();
        services.AddLogging();

        services
            .AddDarwinLinguaInfrastructure(options => options.DatabasePath = catalogDatabasePath)
            .AddCatalogInfrastructure()
            .AddContentOpsApplication()
            .AddContentOpsInfrastructure()
            .AddLocalizationInfrastructure();

        services.AddDbContext<ServerContentDbContext>(options => options.UseSqlite($"Data Source={serverDatabasePath}"));
        services.AddScoped<IContentImportRepository, WebApiContentImportRepository>();
        services.AddScoped<IServerContentDatabaseBootstrapper, ServerContentDatabaseBootstrapper>();
        services.AddScoped<ICatalogPackagePublisher, CatalogPackagePublisher>();
        services.AddScoped<ICatalogPackageCleanupService, CatalogPackageCleanupService>();
        services.AddScoped<ICatalogPackageReleaseService, CatalogPackageReleaseService>();
        services.AddScoped<IServerCatalogImportService, ServerCatalogImportService>();
        services.AddSingleton<IWebHostEnvironment>(new TestWebHostEnvironment(contentRootPath));
        services.AddSingleton<IHostEnvironment>(new TestWebHostEnvironment(contentRootPath));
        services.Configure<ServerContentOptions>(options =>
        {
            options.PublicBaseUrl = "http://localhost:5099";
            options.DefaultSchemaVersion = 1;
            options.PackageStorage.RootPath = packageRootPath;
            options.ClientProducts.Add(new ClientProductOptions
            {
                Key = "darwin-deutsch",
                DisplayName = "Darwin Deutsch",
                LearningLanguageCode = "de",
                DefaultUiLanguageCode = "en",
                IsActive = true,
            });
        });

        return services.BuildServiceProvider();
    }

    private static async Task InitializeAsync(ServiceProvider serviceProvider)
    {
        IDatabaseInitializer databaseInitializer = serviceProvider.GetRequiredService<IDatabaseInitializer>();
        await databaseInitializer.InitializeAsync(CancellationToken.None);

        IServerContentDatabaseBootstrapper bootstrapper = serviceProvider.GetRequiredService<IServerContentDatabaseBootstrapper>();
        await bootstrapper.InitializeAsync(CancellationToken.None);
    }

    private static string ResolveFixturePath()
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
