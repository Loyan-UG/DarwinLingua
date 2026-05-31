using DarwinLingua.WebApi.Models;
using DarwinLingua.WebApi.Persistence.Entities;
using DarwinLingua.WebApi.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DarwinLingua.WebApi.Tests;

public sealed class CatalogPackageDraftQueryServiceTests
{
    [Fact]
    public async Task GetBatchesAsync_ReturnsDraftAndPublishedBatchesWithAggregatesAsync()
    {
        await using WebApiPostgresTestHost host = await WebApiPostgresTestHost.CreateAsync("darwinlingua-webapi-draft-tests");
        ServiceProvider serviceProvider = host.ServiceProvider;

        IServerCatalogImportService importService = serviceProvider.GetRequiredService<IServerCatalogImportService>();
        ICatalogPackageReleaseService releaseService = serviceProvider.GetRequiredService<ICatalogPackageReleaseService>();
        ICatalogPackagePublisher packagePublisher = serviceProvider.GetRequiredService<ICatalogPackagePublisher>();
        ICatalogPackageDraftQueryService queryService = serviceProvider.GetRequiredService<ICatalogPackageDraftQueryService>();

        string fixturePath = WebApiPostgresTestHost.ResolveFixturePath();

        AdminImportCatalogResponse firstImport = await importService
            .ImportAndStageAsync(new AdminImportCatalogRequest(fixturePath, "darwin-deutsch"), CancellationToken.None);

        await releaseService
            .PublishAsync(new AdminPublishCatalogRequest("darwin-deutsch", firstImport.DraftPublicationBatchId!), CancellationToken.None);

        await Task.Delay(TimeSpan.FromSeconds(1.1));

        CatalogPackagePublicationResult secondBatch = await packagePublisher
            .StageDraftAsync("darwin-deutsch", CancellationToken.None);

        IReadOnlyList<AdminDraftCatalogBatchResponse> batches = await queryService
            .GetBatchesAsync("darwin-deutsch", CancellationToken.None);

        Assert.Equal(2, batches.Count);
        Assert.Equal(secondBatch.PublicationBatchId, batches[0].PublicationBatchId);
        Assert.Equal(nameof(PackagePublicationStatus.Draft), batches[0].PublicationStatus);
        Assert.Equal(21, batches[0].PackageCount);
        Assert.True(batches[0].TotalWordCount > 0);
        Assert.NotEmpty(batches[0].Packages);
        Assert.All(batches[0].Packages, package => Assert.False(string.IsNullOrWhiteSpace(package.Checksum)));

        Assert.Equal(firstImport.DraftPublicationBatchId, batches[1].PublicationBatchId);
        Assert.Equal(nameof(PackagePublicationStatus.Published), batches[1].PublicationStatus);
        Assert.NotNull(batches[1].PublishedAtUtc);
    }

    [Fact]
    public async Task GetBatchAsync_ReturnsOneSpecificBatchAsync()
    {
        await using WebApiPostgresTestHost host = await WebApiPostgresTestHost.CreateAsync("darwinlingua-webapi-draft-tests");
        ServiceProvider serviceProvider = host.ServiceProvider;

        IServerCatalogImportService importService = serviceProvider.GetRequiredService<IServerCatalogImportService>();
        ICatalogPackageDraftQueryService queryService = serviceProvider.GetRequiredService<ICatalogPackageDraftQueryService>();

        AdminImportCatalogResponse importResponse = await importService
            .ImportAndStageAsync(new AdminImportCatalogRequest(WebApiPostgresTestHost.ResolveFixturePath(), "darwin-deutsch"), CancellationToken.None);

        AdminDraftCatalogBatchResponse batch = await queryService
            .GetBatchAsync(importResponse.DraftPublicationBatchId!, "darwin-deutsch", CancellationToken.None);

        Assert.Equal(importResponse.DraftPublicationBatchId, batch.PublicationBatchId);
        Assert.Equal(nameof(PackagePublicationStatus.Draft), batch.PublicationStatus);
        Assert.Equal(importResponse.StagedPackageIds.Count, batch.PackageCount);
        Assert.Contains(batch.Packages, package => package.PackageType == "full-database");
    }
}
