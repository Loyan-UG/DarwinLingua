using DarwinLingua.WebApi.Models;
using DarwinLingua.WebApi.Persistence.Entities;
using DarwinLingua.WebApi.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DarwinLingua.WebApi.Tests;

public sealed class CatalogPublicationHistoryServiceTests
{
    [Fact]
    public async Task GetHistoryAsync_ReturnsDraftPublishedAndSupersededBatchesAsync()
    {
        await using WebApiPostgresTestHost host = await WebApiPostgresTestHost.CreateAsync("darwinlingua-webapi-history-tests");
        ServiceProvider serviceProvider = host.ServiceProvider;

        IServerCatalogImportService importService = serviceProvider.GetRequiredService<IServerCatalogImportService>();
        ICatalogPackagePublisher packagePublisher = serviceProvider.GetRequiredService<ICatalogPackagePublisher>();
        ICatalogPackageReleaseService releaseService = serviceProvider.GetRequiredService<ICatalogPackageReleaseService>();
        ICatalogPublicationHistoryService historyService = serviceProvider.GetRequiredService<ICatalogPublicationHistoryService>();

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

        await Task.Delay(TimeSpan.FromSeconds(1.1));

        CatalogPackagePublicationResult thirdBatch = await packagePublisher.StageDraftAsync("darwin-deutsch", CancellationToken.None);

        IReadOnlyList<AdminCatalogBatchHistoryResponse> history = await historyService.GetHistoryAsync("darwin-deutsch", CancellationToken.None);

        Assert.Equal(3, history.Count);
        Assert.Equal(thirdBatch.PublicationBatchId, history[0].PublicationBatchId);
        Assert.Equal(nameof(PackagePublicationStatus.Draft), history[0].PublicationStatus);
        Assert.False(history[0].CanDelete);

        Assert.Equal(secondBatch.PublicationBatchId, history[1].PublicationBatchId);
        Assert.Equal(nameof(PackagePublicationStatus.Published), history[1].PublicationStatus);
        Assert.NotNull(history[1].PublishedAtUtc);

        Assert.Equal(firstImport.DraftPublicationBatchId, history[2].PublicationBatchId);
        Assert.Equal(nameof(PackagePublicationStatus.Superseded), history[2].PublicationStatus);
        Assert.True(history[2].CanDelete);
        Assert.NotNull(history[2].SupersededAtUtc);
        Assert.NotEmpty(history[2].Packages);
    }

    [Fact]
    public async Task GetSummaryAsync_ReturnsLifecycleCountsAndLatestPublishedBatchAsync()
    {
        await using WebApiPostgresTestHost host = await WebApiPostgresTestHost.CreateAsync("darwinlingua-webapi-history-tests");
        ServiceProvider serviceProvider = host.ServiceProvider;

        IServerCatalogImportService importService = serviceProvider.GetRequiredService<IServerCatalogImportService>();
        ICatalogPackagePublisher packagePublisher = serviceProvider.GetRequiredService<ICatalogPackagePublisher>();
        ICatalogPackageReleaseService releaseService = serviceProvider.GetRequiredService<ICatalogPackageReleaseService>();
        ICatalogPublicationHistoryService historyService = serviceProvider.GetRequiredService<ICatalogPublicationHistoryService>();

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

        AdminCatalogBatchHistorySummaryResponse summary = await historyService.GetSummaryAsync("darwin-deutsch", CancellationToken.None);

        Assert.Equal(2, summary.TotalBatchCount);
        Assert.Equal(0, summary.DraftBatchCount);
        Assert.Equal(1, summary.PublishedBatchCount);
        Assert.Equal(1, summary.SupersededBatchCount);
        Assert.Equal(1, summary.DeletableBatchCount);
        Assert.Equal(secondBatch.PublicationBatchId, summary.LatestPublishedBatchId);
        Assert.NotNull(summary.LatestPublishedAtUtc);
    }
}
