using DarwinLingua.WebApi.Models;
using DarwinLingua.WebApi.Persistence;
using DarwinLingua.WebApi.Persistence.Entities;
using DarwinLingua.WebApi.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DarwinLingua.WebApi.Tests;

public sealed class CatalogPackageCleanupServiceTests
{
    [Fact]
    public async Task DeleteSupersededBatchAsync_RemovesBatchRowsAndPayloadFilesAsync()
    {
        await using WebApiPostgresTestHost host = await WebApiPostgresTestHost.CreateAsync("darwinlingua-webapi-cleanup-tests");
        ServiceProvider serviceProvider = host.ServiceProvider;

        IServerCatalogImportService importService = serviceProvider.GetRequiredService<IServerCatalogImportService>();
        ICatalogPackagePublisher packagePublisher = serviceProvider.GetRequiredService<ICatalogPackagePublisher>();
        ICatalogPackageReleaseService releaseService = serviceProvider.GetRequiredService<ICatalogPackageReleaseService>();
        ICatalogPackageCleanupService cleanupService = serviceProvider.GetRequiredService<ICatalogPackageCleanupService>();

        AdminImportCatalogResponse firstImport = await importService.ImportAndStageAsync(
            new AdminImportCatalogRequest(WebApiPostgresTestHost.ResolveFixturePath(), "darwin-deutsch"),
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
            Assert.Equal(21, await dbContext.PublishedPackages.CountAsync(package => package.PublicationBatchId == supersededBatchId));
            Assert.Equal(21, await dbContext.PublishedPackages.CountAsync(package => package.PublicationBatchId == supersededBatchId && package.PublicationStatus == PackagePublicationStatus.Superseded));
        }

        string samplePayloadPath = Path.Combine(host.PackageRootPath, "darwin-deutsch", $"{firstImport.StagedPackageIds[0]}.json");
        Assert.True(File.Exists(samplePayloadPath));

        AdminDeleteCatalogBatchResponse deleteResponse = await cleanupService.DeleteSupersededBatchAsync(
            supersededBatchId,
            "darwin-deutsch",
            CancellationToken.None);

        Assert.True(deleteResponse.IsSuccess);
        Assert.Equal(21, deleteResponse.DeletedPackageCount);
        Assert.False(File.Exists(samplePayloadPath));

        await using (AsyncServiceScope verificationScope = serviceProvider.CreateAsyncScope())
        {
            ServerContentDbContext dbContext = verificationScope.ServiceProvider.GetRequiredService<ServerContentDbContext>();
            Assert.Equal(0, await dbContext.PublishedPackages.CountAsync(package => package.PublicationBatchId == supersededBatchId));
            Assert.Equal(21, await dbContext.PublishedPackages.CountAsync(package => package.PublicationBatchId == secondBatch.PublicationBatchId && package.PublicationStatus == PackagePublicationStatus.Published));
        }
    }

    [Fact]
    public async Task DeleteSupersededBatchAsync_RejectsPublishedBatchAsync()
    {
        await using WebApiPostgresTestHost host = await WebApiPostgresTestHost.CreateAsync("darwinlingua-webapi-cleanup-tests");
        ServiceProvider serviceProvider = host.ServiceProvider;

        IServerCatalogImportService importService = serviceProvider.GetRequiredService<IServerCatalogImportService>();
        ICatalogPackageReleaseService releaseService = serviceProvider.GetRequiredService<ICatalogPackageReleaseService>();
        ICatalogPackageCleanupService cleanupService = serviceProvider.GetRequiredService<ICatalogPackageCleanupService>();

        AdminImportCatalogResponse importResponse = await importService.ImportAndStageAsync(
            new AdminImportCatalogRequest(WebApiPostgresTestHost.ResolveFixturePath(), "darwin-deutsch"),
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
}
