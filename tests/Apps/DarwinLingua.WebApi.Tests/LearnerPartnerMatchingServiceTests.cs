using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.Infrastructure.DependencyInjection;
using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.Infrastructure.Persistence.Abstractions;
using DarwinLingua.WebApi.Models;
using DarwinLingua.WebApi.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DarwinLingua.WebApi.Tests;

public sealed class LearnerPartnerMatchingServiceTests
{
    [Fact]
    public async Task PublicProfiles_ShouldExcludePrivateContactDetails()
    {
        await using PostgresTestDatabase database = await PostgresTestDatabase.CreateAsync("darwin_partner_profiles");
        ServiceProvider? serviceProvider = null;

        try
        {
            serviceProvider = BuildServiceProvider(database.ConnectionString);
            await serviceProvider.GetRequiredService<IDatabaseInitializer>().InitializeAsync(CancellationToken.None);

            ILearnerConversationProfileService profileService =
                serviceProvider.GetRequiredService<ILearnerConversationProfileService>();

            await SaveProfileAsync(
                profileService,
                "sara@example.com",
                "Sara",
                visibility: "public",
                availabilityNotes: "Only weekends; personal phone is private.");

            IReadOnlyList<LearnerConversationProfilePublicResponse> publicProfiles =
                await profileService.GetPublicProfilesAsync(CancellationToken.None);

            LearnerConversationProfilePublicResponse publicProfile = Assert.Single(publicProfiles);
            Assert.Equal("Sara", publicProfile.DisplayName);
            Assert.Equal("Berlin", publicProfile.CityRegion);
            Assert.DoesNotContain(
                publicProfile.GetType().GetProperties(),
                property => string.Equals(property.Name, "OwnerEmail", StringComparison.Ordinal));
            Assert.DoesNotContain(
                publicProfile.GetType().GetProperties(),
                property => string.Equals(property.Name, "AvailabilityNotes", StringComparison.Ordinal));
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
    public async Task AnonymizeAsync_ShouldDisableMatchingAndClearProfileFields()
    {
        await using PostgresTestDatabase database = await PostgresTestDatabase.CreateAsync("darwin_partner_anonymize");
        ServiceProvider? serviceProvider = null;

        try
        {
            serviceProvider = BuildServiceProvider(database.ConnectionString);
            await serviceProvider.GetRequiredService<IDatabaseInitializer>().InitializeAsync(CancellationToken.None);

            ILearnerConversationProfileService profileService =
                serviceProvider.GetRequiredService<ILearnerConversationProfileService>();
            IPartnerMatchingService matchingService = serviceProvider.GetRequiredService<IPartnerMatchingService>();

            await SaveProfileAsync(profileService, "owner@example.com", "Owner", visibility: "public");
            await SaveProfileAsync(profileService, "viewer@example.com", "Viewer", visibility: "public");

            await profileService.AnonymizeAsync("owner@example.com", CancellationToken.None);

            LearnerConversationProfilePrivateResponse? privateProfile =
                await profileService.GetPrivateAsync("owner@example.com", CancellationToken.None);
            Assert.NotNull(privateProfile);
            Assert.Equal("Deleted learner", privateProfile.DisplayName);
            Assert.Null(privateProfile.CityRegion);
            Assert.Equal("disabled", privateProfile.Visibility);
            Assert.False(privateProfile.HasConfirmedAdult);
            Assert.Equal("Profile deleted by learner.", privateProfile.ConversationGoals);

            IReadOnlyList<LearnerConversationProfilePublicResponse> publicProfiles =
                await profileService.GetPublicProfilesAsync(CancellationToken.None);
            Assert.DoesNotContain(publicProfiles, profile => profile.DisplayName == "Deleted learner");

            IReadOnlyList<PartnerMatchProfileResponse> matches = await matchingService.SearchAsync(
                "viewer@example.com",
                new PartnerMatchSearchRequest(null, null, null, null, null),
                CancellationToken.None);
            Assert.DoesNotContain(matches, profile => profile.DisplayName == "Deleted learner");
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
    public async Task PartnerRequests_ShouldCoverStateTransitionsAndContactRevealOnlyAfterAcceptance()
    {
        await using PostgresTestDatabase database = await PostgresTestDatabase.CreateAsync("darwin_partner_states");
        ServiceProvider? serviceProvider = null;

        try
        {
            serviceProvider = BuildServiceProvider(database.ConnectionString);
            await serviceProvider.GetRequiredService<IDatabaseInitializer>().InitializeAsync(CancellationToken.None);

            ILearnerConversationProfileService profileService =
                serviceProvider.GetRequiredService<ILearnerConversationProfileService>();
            IPartnerMatchingService matchingService = serviceProvider.GetRequiredService<IPartnerMatchingService>();

            await SaveProfileAsync(profileService, "requester@example.com", "Requester", visibility: "public");
            Guid acceptedTargetId = (await SaveProfileAsync(profileService, "accepted@example.com", "Accepted", visibility: "request-only")).Id;
            Guid declinedTargetId = (await SaveProfileAsync(profileService, "declined@example.com", "Declined", visibility: "request-only")).Id;
            Guid cancelledTargetId = (await SaveProfileAsync(profileService, "cancelled@example.com", "Cancelled", visibility: "request-only")).Id;
            Guid blockedTargetId = (await SaveProfileAsync(profileService, "blocked@example.com", "Blocked", visibility: "request-only")).Id;

            PartnerRequestResponse pending = await matchingService.SubmitRequestAsync(
                "requester@example.com",
                new SubmitPartnerRequestRequest(acceptedTargetId, "practice-goals", "Can we practise greetings?"),
                CancellationToken.None);
            Assert.Equal(PartnerRequestStatuses.Pending, pending.Status);
            Assert.Null(pending.ContactEmail);

            PartnerRequestResponse accepted = await matchingService.UpdateRequestStateAsync(
                "accepted@example.com",
                pending.Id,
                new PartnerRequestStateUpdateRequest("accept"),
                CancellationToken.None);
            Assert.Equal(PartnerRequestStatuses.Accepted, accepted.Status);
            Assert.Equal("requester@example.com", accepted.ContactEmail);

            PartnerRequestResponse acceptedForRequester = Assert.Single(
                await matchingService.GetRequestsAsync("requester@example.com", CancellationToken.None),
                request => request.Id == accepted.Id);
            Assert.Equal("accepted@example.com", acceptedForRequester.ContactEmail);

            PartnerRequestResponse declined = await SubmitAndUpdateAsync(
                matchingService,
                "requester@example.com",
                "declined@example.com",
                declinedTargetId,
                "decline");
            Assert.Equal(PartnerRequestStatuses.Declined, declined.Status);
            Assert.Null(declined.ContactEmail);

            PartnerRequestResponse cancelled = await matchingService.SubmitRequestAsync(
                "requester@example.com",
                new SubmitPartnerRequestRequest(cancelledTargetId, "online-practice", null),
                CancellationToken.None);
            cancelled = await matchingService.UpdateRequestStateAsync(
                "requester@example.com",
                cancelled.Id,
                new PartnerRequestStateUpdateRequest("cancel"),
                CancellationToken.None);
            Assert.Equal(PartnerRequestStatuses.Cancelled, cancelled.Status);
            Assert.Null(cancelled.ContactEmail);

            PartnerRequestResponse blocked = await SubmitAndUpdateAsync(
                matchingService,
                "requester@example.com",
                "blocked@example.com",
                blockedTargetId,
                "block");
            Assert.Equal(PartnerRequestStatuses.Blocked, blocked.Status);
            Assert.Null(blocked.ContactEmail);

            await using DarwinLinguaDbContext dbContext = await serviceProvider
                .GetRequiredService<IDbContextFactory<DarwinLinguaDbContext>>()
                .CreateDbContextAsync(CancellationToken.None);
            Assert.True(await dbContext.UserBlocks.AnyAsync(
                block => block.BlockerEmail == "blocked@example.com" && block.BlockedEmail == "requester@example.com",
                CancellationToken.None));
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
    public async Task SubmitRequestAsync_ShouldEnforceDailyLimit()
    {
        await using PostgresTestDatabase database = await PostgresTestDatabase.CreateAsync("darwin_partner_limit");
        ServiceProvider? serviceProvider = null;

        try
        {
            serviceProvider = BuildServiceProvider(database.ConnectionString);
            await serviceProvider.GetRequiredService<IDatabaseInitializer>().InitializeAsync(CancellationToken.None);

            ILearnerConversationProfileService profileService =
                serviceProvider.GetRequiredService<ILearnerConversationProfileService>();
            IPartnerMatchingService matchingService = serviceProvider.GetRequiredService<IPartnerMatchingService>();

            await SaveProfileAsync(profileService, "requester@example.com", "Requester", visibility: "public");
            Guid[] targetIds = new Guid[6];
            for (int index = 0; index < targetIds.Length; index++)
            {
                targetIds[index] = (await SaveProfileAsync(
                    profileService,
                    $"target-{index}@example.com",
                    $"Target {index}",
                    visibility: "request-only")).Id;
            }

            for (int index = 0; index < 5; index++)
            {
                await matchingService.SubmitRequestAsync(
                    "requester@example.com",
                    new SubmitPartnerRequestRequest(targetIds[index], "practice-goals", null),
                    CancellationToken.None);
            }

            InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                matchingService.SubmitRequestAsync(
                    "requester@example.com",
                    new SubmitPartnerRequestRequest(targetIds[5], "practice-goals", null),
                    CancellationToken.None));
            Assert.Contains("limit", exception.Message, StringComparison.OrdinalIgnoreCase);
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
    public async Task BlockedUsers_ShouldBeHiddenFromMatchesAndCannotCreateNewRequests()
    {
        await using PostgresTestDatabase database = await PostgresTestDatabase.CreateAsync("darwin_partner_block");
        ServiceProvider? serviceProvider = null;

        try
        {
            serviceProvider = BuildServiceProvider(database.ConnectionString);
            await serviceProvider.GetRequiredService<IDatabaseInitializer>().InitializeAsync(CancellationToken.None);

            ILearnerConversationProfileService profileService =
                serviceProvider.GetRequiredService<ILearnerConversationProfileService>();
            IPartnerMatchingService matchingService = serviceProvider.GetRequiredService<IPartnerMatchingService>();

            Guid blockerProfileId = (await SaveProfileAsync(profileService, "blocker@example.com", "Blocker", visibility: "public")).Id;
            await SaveProfileAsync(profileService, "blocked@example.com", "Blocked", visibility: "public");
            await AddBlockAsync(serviceProvider, "blocker@example.com", "blocked@example.com");

            IReadOnlyList<PartnerMatchProfileResponse> blockerMatches = await matchingService.SearchAsync(
                "blocker@example.com",
                new PartnerMatchSearchRequest(null, null, null, null, null),
                CancellationToken.None);
            Assert.DoesNotContain(blockerMatches, profile => profile.DisplayName == "Blocked");

            IReadOnlyList<PartnerMatchProfileResponse> blockedMatches = await matchingService.SearchAsync(
                "blocked@example.com",
                new PartnerMatchSearchRequest(null, null, null, null, null),
                CancellationToken.None);
            Assert.DoesNotContain(blockedMatches, profile => profile.DisplayName == "Blocker");

            InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                matchingService.SubmitRequestAsync(
                    "blocked@example.com",
                    new SubmitPartnerRequestRequest(blockerProfileId, "practice-goals", null),
                    CancellationToken.None));
            Assert.Contains("not available", exception.Message, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (serviceProvider is not null)
            {
                await serviceProvider.DisposeAsync();
            }
        }
    }

    private static async Task<PartnerRequestResponse> SubmitAndUpdateAsync(
        IPartnerMatchingService matchingService,
        string requesterEmail,
        string recipientEmail,
        Guid targetProfileId,
        string action)
    {
        PartnerRequestResponse request = await matchingService.SubmitRequestAsync(
            requesterEmail,
            new SubmitPartnerRequestRequest(targetProfileId, "same-city", null),
            CancellationToken.None);

        return await matchingService.UpdateRequestStateAsync(
            recipientEmail,
            request.Id,
            new PartnerRequestStateUpdateRequest(action),
            CancellationToken.None);
    }

    private static async Task<LearnerConversationProfilePrivateResponse> SaveProfileAsync(
        ILearnerConversationProfileService profileService,
        string ownerEmail,
        string displayName,
        string visibility,
        string? availabilityNotes = "Weekday evenings")
    {
        return await profileService.SaveAsync(
            ownerEmail,
            new SaveLearnerConversationProfileRequest(
                displayName,
                "Berlin",
                "both",
                "B1",
                ["en", "fa"],
                "I want regular conversation practice.",
                availabilityNotes,
                visibility,
                true),
            CancellationToken.None);
    }

    private static async Task AddBlockAsync(ServiceProvider serviceProvider, string blockerEmail, string blockedEmail)
    {
        await using DarwinLinguaDbContext dbContext = await serviceProvider
            .GetRequiredService<IDbContextFactory<DarwinLinguaDbContext>>()
            .CreateDbContextAsync(CancellationToken.None);
        dbContext.UserBlocks.Add(new UserBlock(
            Guid.NewGuid(),
            blockerEmail,
            blockedEmail,
            "Safety preference.",
            null,
            DateTime.UtcNow));
        await dbContext.SaveChangesAsync(CancellationToken.None);
    }

    private static ServiceProvider BuildServiceProvider(string connectionString)
    {
        ServiceCollection services = new();
        services.AddDarwinLinguaInfrastructureForPostgres(connectionString);
        services.AddScoped<ILearnerConversationProfileService, LearnerConversationProfileService>();
        services.AddScoped<IPartnerMatchingService, PartnerMatchingService>();
        return services.BuildServiceProvider();
    }
}
