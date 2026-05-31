using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.WebApi.Models;
using DarwinLingua.WebApi.Persistence;
using DarwinLingua.WebApi.Persistence.Entities;
using DarwinLingua.WebApi.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using Xunit;

namespace DarwinLingua.WebApi.Tests;

public sealed class ServerCatalogImportServiceTests
{
    [Fact]
    public async Task ImportAndStageAsync_ImportsIntoSharedCatalogAndGeneratesDraftPackagesAsync()
    {
        await using WebApiPostgresTestHost host = await WebApiPostgresTestHost.CreateAsync("darwinlingua-webapi-tests");
        ServiceProvider serviceProvider = host.ServiceProvider;

        IServerCatalogImportService service = serviceProvider.GetRequiredService<IServerCatalogImportService>();
        string fixturePath = WebApiPostgresTestHost.ResolveFixturePath();

        AdminImportCatalogResponse response = await service.ImportAndStageAsync(
            new AdminImportCatalogRequest(fixturePath, "darwin-deutsch"),
            CancellationToken.None);

        Assert.True(response.IsSuccess, string.Join(Environment.NewLine, response.IssueMessages));
        Assert.Equal(12, response.ImportedEntries);
        Assert.Equal(21, response.StagedPackageIds.Count);
        Assert.False(string.IsNullOrWhiteSpace(response.DraftPublicationBatchId));

        await using (DarwinLinguaDbContext catalogDbContext = await serviceProvider
                         .GetRequiredService<IDbContextFactory<DarwinLinguaDbContext>>()
                         .CreateDbContextAsync())
        {
            Assert.Equal(12, await catalogDbContext.WordEntries.CountAsync());
            Assert.Single(await catalogDbContext.ConversationStarterPacks.ToListAsync());
            Assert.Single(await catalogDbContext.EventPreparationPacks.ToListAsync());
            Assert.Single(await catalogDbContext.ContentPackages.ToListAsync());
        }

        await using (AsyncServiceScope statusScope = serviceProvider.CreateAsyncScope())
        {
            ServerContentDbContext serverDbContext = statusScope.ServiceProvider.GetRequiredService<ServerContentDbContext>();
            Assert.Single(await serverDbContext.ContentImportReceipts.ToListAsync());
            Assert.Equal(21, await serverDbContext.PublishedPackages.CountAsync());
            Assert.Equal(21, await serverDbContext.PublishedPackages.CountAsync(package => package.PublicationStatus == PackagePublicationStatus.Draft));
        }

        string fullCatalogPath = Path.Combine(host.PackageRootPath, "darwin-deutsch", response.StagedPackageIds.Single(packageId => packageId.Contains("catalog-full", StringComparison.Ordinal)));
        Assert.True(File.Exists($"{fullCatalogPath}.json"));
        using JsonDocument fullCatalogJson = JsonDocument.Parse(await File.ReadAllTextAsync($"{fullCatalogPath}.json"));
        JsonElement fullCatalogDialogues = fullCatalogJson.RootElement.GetProperty("Dialogues");
        Assert.Equal(1, fullCatalogDialogues.GetArrayLength());
        Assert.Equal("a1-buy-bread-test", fullCatalogDialogues[0].GetProperty("Slug").GetString());
        JsonElement fullCatalogStarterPacks = fullCatalogJson.RootElement.GetProperty("ConversationStarterPacks");
        Assert.Equal(1, fullCatalogStarterPacks.GetArrayLength());
        Assert.Equal("a1-bakery-order-starters", fullCatalogStarterPacks[0].GetProperty("Slug").GetString());
        Assert.Equal("a1-buy-bread-test", fullCatalogStarterPacks[0].GetProperty("LinkedDialogueSlugs")[0].GetString());
        Assert.Equal("a1-bakery-visit-prep", fullCatalogStarterPacks[0].GetProperty("LinkedEventPreparationPackSlugs")[0].GetString());
        JsonElement fullCatalogPreparationPacks = fullCatalogJson.RootElement.GetProperty("EventPreparationPacks");
        Assert.Equal(1, fullCatalogPreparationPacks.GetArrayLength());
        Assert.Equal("a1-bakery-visit-prep", fullCatalogPreparationPacks[0].GetProperty("Slug").GetString());
        Assert.Equal("a1-buy-bread-test", fullCatalogPreparationPacks[0].GetProperty("LinkedDialogueSlugs")[0].GetString());
        Assert.Equal("Brot", fullCatalogPreparationPacks[0].GetProperty("LinkedVocabulary")[0].GetProperty("Word").GetString());
        Assert.Equal("a1-bakery-order-starters", fullCatalogPreparationPacks[0].GetProperty("LinkedConversationStarterPackSlugs")[0].GetString());
        Assert.Equal("Say hello and ask for one bread.", fullCatalogPreparationPacks[0].GetProperty("OpeningPrompts")[0].GetString());

        string a1CatalogPath = Path.Combine(host.PackageRootPath, "darwin-deutsch", response.StagedPackageIds.Single(packageId => packageId.Contains("catalog-a1", StringComparison.Ordinal)));
        using JsonDocument a1CatalogJson = JsonDocument.Parse(await File.ReadAllTextAsync($"{a1CatalogPath}.json"));
        Assert.Equal(1, a1CatalogJson.RootElement.GetProperty("Dialogues").GetArrayLength());
        Assert.Equal(1, a1CatalogJson.RootElement.GetProperty("ConversationStarterPacks").GetArrayLength());
        Assert.Equal(1, a1CatalogJson.RootElement.GetProperty("EventPreparationPacks").GetArrayLength());

        await using (AsyncServiceScope publishScope = serviceProvider.CreateAsyncScope())
        {
            ICatalogPackageReleaseService releaseService = publishScope.ServiceProvider.GetRequiredService<ICatalogPackageReleaseService>();
            AdminPublishCatalogResponse publishResponse = await releaseService.PublishAsync(
                new AdminPublishCatalogRequest("darwin-deutsch", response.DraftPublicationBatchId),
                CancellationToken.None);

            Assert.True(publishResponse.IsSuccess);
            Assert.Equal(21, publishResponse.PublishedPackageIds.Count);
        }

        await using (AsyncServiceScope publishedScope = serviceProvider.CreateAsyncScope())
        {
            ServerContentDbContext publishedContext = publishedScope.ServiceProvider.GetRequiredService<ServerContentDbContext>();
            Assert.Equal(21, await publishedContext.PublishedPackages.CountAsync(package => package.PublicationStatus == PackagePublicationStatus.Published));
            Assert.Equal(0, await publishedContext.PublishedPackages.CountAsync(package => package.PublicationStatus == PackagePublicationStatus.Draft));
        }
    }
}
