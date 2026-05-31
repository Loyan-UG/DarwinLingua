using DarwinLingua.WebApi.Models;
using DarwinLingua.WebApi.Persistence.Entities;
using DarwinLingua.WebApi.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DarwinLingua.WebApi.Tests;

public sealed class ContentPublicationAuditServiceTests
{
    [Fact]
    public async Task GetRecentEventsAsync_ReturnsPublishRollbackAndCleanupEventsAsync()
    {
        await using WebApiPostgresTestHost host = await WebApiPostgresTestHost.CreateAsync("darwinlingua-webapi-audit-tests");
        ServiceProvider serviceProvider = host.ServiceProvider;

        IServerCatalogImportService importService = serviceProvider.GetRequiredService<IServerCatalogImportService>();
        ICatalogPackagePublisher publisher = serviceProvider.GetRequiredService<ICatalogPackagePublisher>();
        ICatalogPackageReleaseService releaseService = serviceProvider.GetRequiredService<ICatalogPackageReleaseService>();
        ICatalogPackageRollbackService rollbackService = serviceProvider.GetRequiredService<ICatalogPackageRollbackService>();
        ICatalogPackageCleanupService cleanupService = serviceProvider.GetRequiredService<ICatalogPackageCleanupService>();
        IContentPublicationAuditService auditService = serviceProvider.GetRequiredService<IContentPublicationAuditService>();

        AdminImportCatalogResponse firstImport = await importService.ImportAndStageAsync(
            new AdminImportCatalogRequest(WebApiPostgresTestHost.ResolveFixturePath(), "darwin-deutsch"),
            CancellationToken.None);
        await releaseService.PublishAsync(new AdminPublishCatalogRequest("darwin-deutsch", firstImport.DraftPublicationBatchId!), CancellationToken.None);

        await Task.Delay(TimeSpan.FromSeconds(1.1));
        CatalogPackagePublicationResult secondBatch = await publisher.StageDraftAsync("darwin-deutsch", CancellationToken.None);
        await releaseService.PublishAsync(new AdminPublishCatalogRequest("darwin-deutsch", secondBatch.PublicationBatchId), CancellationToken.None);

        await rollbackService.RollbackAsync(new AdminRollbackCatalogRequest("darwin-deutsch", firstImport.DraftPublicationBatchId!), CancellationToken.None);
        await cleanupService.DeleteSupersededBatchAsync(secondBatch.PublicationBatchId, "darwin-deutsch", CancellationToken.None);

        IReadOnlyList<AdminPublicationAuditEventResponse> events = await auditService.GetRecentEventsAsync("darwin-deutsch", CancellationToken.None);

        Assert.True(events.Count >= 4);
        Assert.Equal("Cleanup", events[0].EventType);
        Assert.Equal(secondBatch.PublicationBatchId, events[0].PublicationBatchId);
        Assert.Contains(events, entry => entry.EventType == "Rollback" && entry.PublicationBatchId == firstImport.DraftPublicationBatchId);
        Assert.Contains(events, entry => entry.EventType == "Publish" && entry.PublicationBatchId == secondBatch.PublicationBatchId);
    }
}
