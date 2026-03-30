using DarwinLingua.WebApi.Configuration;
using DarwinLingua.WebApi.Services;
using Microsoft.Extensions.Options;
using Xunit;

namespace DarwinLingua.WebApi.Tests;

public sealed class ConfiguredMobileContentManifestServiceTests
{
    [Fact]
    public void GetGlobalManifest_UsesOnlyActiveConfiguredProduct_WhenNoProductKeyProvided()
    {
        ConfiguredMobileContentManifestService service = CreateService();

        var manifest = service.GetGlobalManifest(null);

        Assert.Equal("darwin-deutsch", manifest.ClientProductKey);
        Assert.Equal(2, manifest.TotalPackageCount);
        Assert.Equal(53, manifest.TotalWordCount);
    }

    [Fact]
    public void GetAreas_GroupsPackagesByContentArea()
    {
        ConfiguredMobileContentManifestService service = CreateService();

        var areas = service.GetAreas("darwin-deutsch");

        Assert.Single(areas);
        Assert.Equal("catalog", areas[0].ContentAreaKey);
        Assert.Contains("full", areas[0].SliceKeys);
        Assert.Contains("cefr:a1", areas[0].SliceKeys);
    }

    [Fact]
    public void GetCefrManifest_ReturnsOnlyRequestedSlice()
    {
        ConfiguredMobileContentManifestService service = CreateService();

        var manifest = service.GetCefrManifest("darwin-deutsch", "A1");

        Assert.Single(manifest.Packages);
        Assert.Equal("cefr:a1", manifest.SliceKey);
        Assert.Equal("darwin-deutsch-catalog-a1-v1", manifest.Packages[0].PackageId);
    }

    [Fact]
    public void GetPackage_RejectsUnknownProduct()
    {
        ConfiguredMobileContentManifestService service = CreateService();

        Assert.Throws<KeyNotFoundException>(() => service.GetPackage("darwin-english", "darwin-deutsch-catalog-full-v1"));
    }

    private static ConfiguredMobileContentManifestService CreateService()
    {
        ServerContentOptions options = new()
        {
            PublicBaseUrl = "http://localhost:5099",
        };

        options.ClientProducts.Add(new ClientProductOptions
        {
            Key = "darwin-deutsch",
            DisplayName = "Darwin Deutsch",
            LearningLanguageCode = "de",
            DefaultUiLanguageCode = "en",
            IsActive = true,
        });

        options.Packages.Add(new PublishedPackageOptions
        {
            PackageId = "darwin-deutsch-catalog-full-v1",
            ClientProductKey = "darwin-deutsch",
            ContentAreaKey = "catalog",
            SliceKey = "full",
            PackageType = "full-catalog",
            Version = "2026.03.30.1",
            SchemaVersion = 1,
            MinimumAppSchemaVersion = 1,
            Checksum = "checksum-full",
            EntryCount = 41,
            WordCount = 41,
            CreatedAtUtc = new DateTimeOffset(2026, 03, 30, 10, 0, 0, TimeSpan.Zero),
            RelativeDownloadPath = "/downloads/packages/darwin-deutsch-catalog-full-v1.json",
        });

        options.Packages.Add(new PublishedPackageOptions
        {
            PackageId = "darwin-deutsch-catalog-a1-v1",
            ClientProductKey = "darwin-deutsch",
            ContentAreaKey = "catalog",
            SliceKey = "cefr:a1",
            PackageType = "catalog-cefr",
            Version = "2026.03.30.1",
            SchemaVersion = 1,
            MinimumAppSchemaVersion = 1,
            Checksum = "checksum-a1",
            EntryCount = 12,
            WordCount = 12,
            CreatedAtUtc = new DateTimeOffset(2026, 03, 30, 10, 0, 0, TimeSpan.Zero),
            RelativeDownloadPath = "/downloads/packages/darwin-deutsch-catalog-a1-v1.json",
        });

        return new ConfiguredMobileContentManifestService(Options.Create(options));
    }
}
