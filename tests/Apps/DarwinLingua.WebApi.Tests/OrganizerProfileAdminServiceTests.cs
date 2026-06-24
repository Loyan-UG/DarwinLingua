using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.DependencyInjection;
using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Catalog.Infrastructure.DependencyInjection;
using DarwinLingua.Infrastructure.DependencyInjection;
using DarwinLingua.Infrastructure.Persistence.Abstractions;
using DarwinLingua.SharedKernel.Globalization;
using DarwinLingua.WebApi.Models;
using DarwinLingua.WebApi.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DarwinLingua.WebApi.Tests;

public sealed class OrganizerProfileAdminServiceTests
{
    [Fact]
    public async Task SaveAsync_ShouldCreatePublishedOrganizerProfile()
    {
        await using PostgresTestDatabase database = await PostgresTestDatabase.CreateAsync("darwin_admin_organizers");
        ServiceProvider? serviceProvider = null;

        try
        {
            serviceProvider = BuildServiceProvider(database.ConnectionString);
            await serviceProvider.GetRequiredService<IDatabaseInitializer>().InitializeAsync(CancellationToken.None);

            IOrganizerProfileAdminService adminService = serviceProvider.GetRequiredService<IOrganizerProfileAdminService>();
            OrganizerProfileDetailModel savedProfile = await adminService.SaveAsync(
                CreateRequest("berlin-language-club", "Berlin Language Club", "club"),
                CancellationToken.None);

            Assert.Equal("berlin-language-club", savedProfile.Slug);
            Assert.Equal(["A1", "A2"], savedProfile.SupportedLearnerLevels);
            Assert.Equal(["en", "fa"], savedProfile.HelperLanguageCodes);
            Assert.Equal("reviewed", savedProfile.VerificationStatus);

            IOrganizerProfileQueryService queryService = serviceProvider.GetRequiredService<IOrganizerProfileQueryService>();
            IReadOnlyList<OrganizerProfileListItemModel> profiles = await queryService.GetPublishedOrganizerProfilesAsync(
                ContentLanguageRequirements.DefaultTargetLearningLanguageCode,
                CancellationToken.None);

            Assert.Single(profiles);
        }
        finally
        {
            if (serviceProvider is not null)
            {
                await serviceProvider.DisposeAsync();
            }

        }
    }

    [Fact]
    public async Task SaveAsync_ShouldRejectUnsupportedOrganizerType()
    {
        await using PostgresTestDatabase database = await PostgresTestDatabase.CreateAsync("darwin_admin_organizers");
        ServiceProvider? serviceProvider = null;

        try
        {
            serviceProvider = BuildServiceProvider(database.ConnectionString);
            await serviceProvider.GetRequiredService<IDatabaseInitializer>().InitializeAsync(CancellationToken.None);

            IOrganizerProfileAdminService adminService = serviceProvider.GetRequiredService<IOrganizerProfileAdminService>();

            InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                adminService.SaveAsync(CreateRequest("berlin-language-club", "Berlin Language Club", "private"), CancellationToken.None));

            Assert.Contains("Organizer type", exception.Message, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (serviceProvider is not null)
            {
                await serviceProvider.DisposeAsync();
            }

        }
    }

    private static ServiceProvider BuildServiceProvider(string connectionString)
    {
        ServiceCollection services = new();
        services
            .AddDarwinLinguaInfrastructureForPostgres(connectionString)
            .AddCatalogApplication()
            .AddCatalogInfrastructure();
        services.AddScoped<IOrganizerProfileAdminService, OrganizerProfileAdminService>();

        return services.BuildServiceProvider();
    }

    private static AdminSaveOrganizerProfileRequest CreateRequest(
        string slug,
        string displayName,
        string organizerType) =>
        new(
            slug,
            ContentLanguageRequirements.DefaultTargetLearningLanguageCode,
            displayName,
            organizerType,
            "A reviewed German practice organizer.",
            "Berlin",
            true,
            ["A1", "A2"],
            ["en", "fa"],
            "https://example.local",
            "organizer@example.local",
            "reviewed",
            "free-organizer",
            3);

}
