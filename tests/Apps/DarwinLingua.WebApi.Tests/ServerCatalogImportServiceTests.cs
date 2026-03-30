using DarwinLingua.Catalog.Infrastructure.DependencyInjection;
using DarwinLingua.ContentOps.Application.Abstractions;
using DarwinLingua.ContentOps.Application.DependencyInjection;
using DarwinLingua.ContentOps.Infrastructure.DependencyInjection;
using DarwinLingua.Infrastructure.DependencyInjection;
using DarwinLingua.Infrastructure.Persistence;
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
using Microsoft.Extensions.Options;
using Xunit;

namespace DarwinLingua.WebApi.Tests;

public sealed class ServerCatalogImportServiceTests
{
    [Fact]
    public async Task ImportAndStageAsync_ImportsIntoSharedCatalogAndGeneratesDraftPackagesAsync()
    {
        string tempRoot = Path.Combine(Path.GetTempPath(), "darwinlingua-webapi-tests", Guid.NewGuid().ToString("N"));
        string catalogDatabasePath = Path.Combine(tempRoot, "catalog", "catalog.db");
        string serverDatabasePath = Path.Combine(tempRoot, "server", "server.db");
        string packageRootPath = Path.Combine(tempRoot, "packages");
        Directory.CreateDirectory(tempRoot);
        Directory.CreateDirectory(Path.GetDirectoryName(serverDatabasePath)!);

        try
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
        services.AddScoped<IContentPublicationAuditService, ContentPublicationAuditService>();
        services.AddScoped<ICatalogPackagePublisher, CatalogPackagePublisher>();
        services.AddScoped<ICatalogPackageReleaseService, CatalogPackageReleaseService>();
        services.AddScoped<IServerCatalogImportService, ServerCatalogImportService>();
            services.AddSingleton<IWebHostEnvironment>(new TestWebHostEnvironment(tempRoot));
            services.AddSingleton<IHostEnvironment>(new TestWebHostEnvironment(tempRoot));
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

            using ServiceProvider serviceProvider = services.BuildServiceProvider();

            IDatabaseInitializer databaseInitializer = serviceProvider.GetRequiredService<IDatabaseInitializer>();
            await databaseInitializer.InitializeAsync(CancellationToken.None);

            IServerContentDatabaseBootstrapper bootstrapper = serviceProvider.GetRequiredService<IServerContentDatabaseBootstrapper>();
            await bootstrapper.InitializeAsync(CancellationToken.None);

            IServerCatalogImportService service = serviceProvider.GetRequiredService<IServerCatalogImportService>();
            string fixturePath = ResolveFixturePath();

            AdminImportCatalogResponse response = await service.ImportAndStageAsync(
                new AdminImportCatalogRequest(fixturePath, "darwin-deutsch"),
                CancellationToken.None);

            Assert.True(response.IsSuccess);
            Assert.Equal(12, response.ImportedEntries);
            Assert.Equal(8, response.StagedPackageIds.Count);
            Assert.False(string.IsNullOrWhiteSpace(response.DraftPublicationBatchId));

            await using (DarwinLinguaDbContext catalogDbContext = await serviceProvider
                             .GetRequiredService<IDbContextFactory<DarwinLinguaDbContext>>()
                             .CreateDbContextAsync())
            {
                Assert.Equal(12, await catalogDbContext.WordEntries.CountAsync());
                Assert.Single(await catalogDbContext.ContentPackages.ToListAsync());
            }

            await using (AsyncServiceScope statusScope = serviceProvider.CreateAsyncScope())
            {
                ServerContentDbContext serverDbContext = statusScope.ServiceProvider.GetRequiredService<ServerContentDbContext>();
                Assert.Single(await serverDbContext.ContentImportReceipts.ToListAsync());
                Assert.Equal(8, await serverDbContext.PublishedPackages.CountAsync());
                Assert.Equal(8, await serverDbContext.PublishedPackages.CountAsync(package => package.PublicationStatus == PackagePublicationStatus.Draft));
            }

            string fullCatalogPath = Path.Combine(packageRootPath, "darwin-deutsch", response.StagedPackageIds.Single(packageId => packageId.Contains("catalog-full", StringComparison.Ordinal)));
            Assert.True(File.Exists($"{fullCatalogPath}.json"));

            await using (AsyncServiceScope publishScope = serviceProvider.CreateAsyncScope())
            {
                ICatalogPackageReleaseService releaseService = publishScope.ServiceProvider.GetRequiredService<ICatalogPackageReleaseService>();
                AdminPublishCatalogResponse publishResponse = await releaseService.PublishAsync(
                    new AdminPublishCatalogRequest("darwin-deutsch", response.DraftPublicationBatchId),
                    CancellationToken.None);

                Assert.True(publishResponse.IsSuccess);
                Assert.Equal(8, publishResponse.PublishedPackageIds.Count);
            }

            await using (AsyncServiceScope publishedScope = serviceProvider.CreateAsyncScope())
            {
                ServerContentDbContext publishedContext = publishedScope.ServiceProvider.GetRequiredService<ServerContentDbContext>();
                Assert.Equal(8, await publishedContext.PublishedPackages.CountAsync(package => package.PublicationStatus == PackagePublicationStatus.Published));
                Assert.Equal(0, await publishedContext.PublishedPackages.CountAsync(package => package.PublicationStatus == PackagePublicationStatus.Draft));
            }
        }
        finally
        {
            if (Directory.Exists(tempRoot))
            {
                try
                {
                    Directory.Delete(tempRoot, recursive: true);
                }
                catch (IOException)
                {
                }
                catch (UnauthorizedAccessException)
                {
                }
            }
        }
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
