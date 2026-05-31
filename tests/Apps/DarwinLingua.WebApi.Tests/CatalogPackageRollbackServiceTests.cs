using DarwinLingua.WebApi.Models;
using DarwinLingua.WebApi.Persistence;
using DarwinLingua.WebApi.Persistence.Entities;
using DarwinLingua.WebApi.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DarwinLingua.WebApi.Tests;

public sealed class CatalogPackageRollbackServiceTests
{
    [Fact]
    public async Task RollbackAsync_ReactivatesSupersededBatchAndSupersedesCurrentPublishedBatchAsync()
    {
        await using WebApiPostgresTestHost host = await WebApiPostgresTestHost.CreateAsync("darwinlingua-webapi-rollback-tests");
        ServiceProvider serviceProvider = host.ServiceProvider;

        IServerCatalogImportService importService = serviceProvider.GetRequiredService<IServerCatalogImportService>();
        ICatalogPackagePublisher packagePublisher = serviceProvider.GetRequiredService<ICatalogPackagePublisher>();
        ICatalogPackageReleaseService releaseService = serviceProvider.GetRequiredService<ICatalogPackageReleaseService>();
        ICatalogPackageRollbackService rollbackService = serviceProvider.GetRequiredService<ICatalogPackageRollbackService>();

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

        AdminRollbackCatalogResponse rollbackResponse = await rollbackService.RollbackAsync(
            new AdminRollbackCatalogRequest("darwin-deutsch", firstImport.DraftPublicationBatchId!),
            CancellationToken.None);

        Assert.True(rollbackResponse.IsSuccess);
        Assert.Equal(firstImport.DraftPublicationBatchId, rollbackResponse.PublicationBatchId);
        Assert.Equal(21, rollbackResponse.ReactivatedPackageIds.Count);
        Assert.Equal(21, rollbackResponse.SupersededPackageIds.Count);

        await using AsyncServiceScope verificationScope = serviceProvider.CreateAsyncScope();
        ServerContentDbContext dbContext = verificationScope.ServiceProvider.GetRequiredService<ServerContentDbContext>();

        Assert.Equal(21, await dbContext.PublishedPackages.CountAsync(package =>
            package.PublicationBatchId == firstImport.DraftPublicationBatchId &&
            package.PublicationStatus == PackagePublicationStatus.Published));
        Assert.Equal(21, await dbContext.PublishedPackages.CountAsync(package =>
            package.PublicationBatchId == secondBatch.PublicationBatchId &&
            package.PublicationStatus == PackagePublicationStatus.Superseded));
    }

    [Fact]
    public async Task RollbackAsync_RejectsDraftBatchAsync()
    {
        await using WebApiPostgresTestHost host = await WebApiPostgresTestHost.CreateAsync("darwinlingua-webapi-rollback-tests");
        ServiceProvider serviceProvider = host.ServiceProvider;

        IServerCatalogImportService importService = serviceProvider.GetRequiredService<IServerCatalogImportService>();
        ICatalogPackageRollbackService rollbackService = serviceProvider.GetRequiredService<ICatalogPackageRollbackService>();

        AdminImportCatalogResponse importResponse = await importService.ImportAndStageAsync(
            new AdminImportCatalogRequest(WebApiPostgresTestHost.ResolveFixturePath(), "darwin-deutsch"),
            CancellationToken.None);

        AdminRollbackCatalogResponse rollbackResponse = await rollbackService.RollbackAsync(
            new AdminRollbackCatalogRequest("darwin-deutsch", importResponse.DraftPublicationBatchId!),
            CancellationToken.None);

        Assert.False(rollbackResponse.IsSuccess);
        Assert.Contains("Only superseded package batches can be rolled back.", rollbackResponse.IssueMessages[0], StringComparison.Ordinal);
    }
}
