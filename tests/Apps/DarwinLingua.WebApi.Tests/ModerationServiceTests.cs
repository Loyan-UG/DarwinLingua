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

public sealed class ModerationServiceTests
{
    [Fact]
    public async Task SubmitReportAsync_ShouldValidateTargetReasonAndReporterContext()
    {
        await using PostgresTestDatabase database = await PostgresTestDatabase.CreateAsync("darwin_moderation_report");
        ServiceProvider? serviceProvider = null;

        try
        {
            serviceProvider = BuildServiceProvider(database.ConnectionString);
            await serviceProvider.GetRequiredService<IDatabaseInitializer>().InitializeAsync(CancellationToken.None);

            IModerationService moderationService = serviceProvider.GetRequiredService<IModerationService>();

            UserReportResponse report = await moderationService.SubmitReportAsync(
                "Reporter@Example.com",
                new SubmitUserReportRequest(
                    "learner-profile",
                    Guid.NewGuid().ToString("D"),
                    "reported@example.com",
                    "spam",
                    "This learner sent repeated off-topic messages."),
                CancellationToken.None);

            Assert.Equal("reporter@example.com", report.ReporterEmail);
            Assert.Equal("reported@example.com", report.ReportedUserEmail);
            Assert.Equal("learner-profile", report.TargetType);
            Assert.Equal("spam", report.Reason);
            Assert.Equal(UserReportStatuses.Pending, report.Status);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                moderationService.SubmitReportAsync(
                    "reporter@example.com",
                    new SubmitUserReportRequest("unknown-target", "abc", null, "spam", "Invalid target."),
                    CancellationToken.None));
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                moderationService.SubmitReportAsync(
                    "reporter@example.com",
                    new SubmitUserReportRequest("learner-profile", "abc", null, "unsupported-reason", "Invalid reason."),
                    CancellationToken.None));
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                moderationService.SubmitReportAsync(
                    "not-an-email",
                    new SubmitUserReportRequest("learner-profile", "abc", null, "spam", "Invalid reporter."),
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
    public async Task GetReportsAsync_ShouldFilterByStatusReasonTargetTypeAndAssignedState()
    {
        await using PostgresTestDatabase database = await PostgresTestDatabase.CreateAsync("darwin_moderation_filters");
        ServiceProvider? serviceProvider = null;

        try
        {
            serviceProvider = BuildServiceProvider(database.ConnectionString);
            await serviceProvider.GetRequiredService<IDatabaseInitializer>().InitializeAsync(CancellationToken.None);

            IModerationService moderationService = serviceProvider.GetRequiredService<IModerationService>();

            UserReportResponse learnerSpam = await moderationService.SubmitReportAsync(
                "reporter@example.com",
                new SubmitUserReportRequest("learner-profile", Guid.NewGuid().ToString("D"), null, "spam", "Spam report."),
                CancellationToken.None);
            await moderationService.SubmitReportAsync(
                "reporter@example.com",
                new SubmitUserReportRequest("conversation-event", "berlin-cafe-a1", null, "inaccurate-listing", "Wrong location."),
                CancellationToken.None);
            await moderationService.DecideReportAsync(
                learnerSpam.Id,
                new ModerationDecisionRequest("action-taken", "Blocked profile.", "admin@example.com"),
                CancellationToken.None);

            IReadOnlyList<UserReportResponse> actionTaken = await moderationService.GetReportsAsync(
                "action-taken",
                null,
                null,
                null,
                CancellationToken.None);
            Assert.Equal(learnerSpam.Id, Assert.Single(actionTaken).Id);

            IReadOnlyList<UserReportResponse> eventListings = await moderationService.GetReportsAsync(
                null,
                "inaccurate-listing",
                "conversation-event",
                "unassigned",
                CancellationToken.None);
            UserReportResponse eventReport = Assert.Single(eventListings);
            Assert.Equal("conversation-event", eventReport.TargetType);
            Assert.Equal("inaccurate-listing", eventReport.Reason);
            Assert.Null(eventReport.DecidedBy);

            IReadOnlyList<UserReportResponse> assigned = await moderationService.GetReportsAsync(
                null,
                null,
                null,
                "assigned",
                CancellationToken.None);
            Assert.Equal(learnerSpam.Id, Assert.Single(assigned).Id);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                moderationService.GetReportsAsync(null, "bad-reason", null, null, CancellationToken.None));
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                moderationService.GetReportsAsync(null, null, "bad-target", null, CancellationToken.None));
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                moderationService.GetReportsAsync(null, null, null, "bad-assignment", CancellationToken.None));
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
    public async Task DecideReportAsync_ShouldRecordDecisionAudit()
    {
        await using PostgresTestDatabase database = await PostgresTestDatabase.CreateAsync("darwin_moderation_audit");
        ServiceProvider? serviceProvider = null;

        try
        {
            serviceProvider = BuildServiceProvider(database.ConnectionString);
            await serviceProvider.GetRequiredService<IDatabaseInitializer>().InitializeAsync(CancellationToken.None);

            IModerationService moderationService = serviceProvider.GetRequiredService<IModerationService>();
            UserReportResponse report = await moderationService.SubmitReportAsync(
                "reporter@example.com",
                new SubmitUserReportRequest("partner-request", Guid.NewGuid().ToString("D"), null, "unsafe-contact", "Unsafe contact."),
                CancellationToken.None);

            UserReportResponse decided = await moderationService.DecideReportAsync(
                report.Id,
                new ModerationDecisionRequest("reviewed", "Reviewed and warned.", "Admin@Example.com"),
                CancellationToken.None);

            Assert.Equal(UserReportStatuses.Reviewed, decided.Status);
            Assert.Equal("admin@example.com", decided.DecidedBy);
            Assert.NotNull(decided.DecidedAtUtc);

            IReadOnlyList<ModerationDecisionAuditResponse> audits =
                await moderationService.GetDecisionAuditsAsync(CancellationToken.None);
            ModerationDecisionAuditResponse audit = Assert.Single(audits);
            Assert.Equal(report.Id, audit.UserReportId);
            Assert.Equal(UserReportStatuses.Reviewed, audit.DecisionStatus);
            Assert.Equal("admin@example.com", audit.DecidedBy);
            Assert.Equal("Reviewed and warned.", audit.DecisionNote);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                moderationService.DecideReportAsync(
                    report.Id,
                    new ModerationDecisionRequest("pending", "Invalid pending decision.", "admin@example.com"),
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
    public async Task BlockUserAsync_ShouldResolveTargetsAndSuppressMatching()
    {
        await using PostgresTestDatabase database = await PostgresTestDatabase.CreateAsync("darwin_moderation_block");
        ServiceProvider? serviceProvider = null;

        try
        {
            serviceProvider = BuildServiceProvider(database.ConnectionString);
            await serviceProvider.GetRequiredService<IDatabaseInitializer>().InitializeAsync(CancellationToken.None);

            ILearnerConversationProfileService profileService =
                serviceProvider.GetRequiredService<ILearnerConversationProfileService>();
            IPartnerMatchingService matchingService = serviceProvider.GetRequiredService<IPartnerMatchingService>();
            IModerationService moderationService = serviceProvider.GetRequiredService<IModerationService>();

            await SaveProfileAsync(profileService, "blocker@example.com", "Blocker");
            Guid blockedProfileId = (await SaveProfileAsync(profileService, "blocked@example.com", "Blocked")).Id;
            Guid requestTargetId = (await SaveProfileAsync(profileService, "request-target@example.com", "Request Target")).Id;

            UserBlockResponse profileBlock = await moderationService.BlockUserAsync(
                "blocker@example.com",
                new BlockUserRequest(null, "No more contact.", null, blockedProfileId),
                CancellationToken.None);
            Assert.Equal("blocker@example.com", profileBlock.BlockerEmail);
            Assert.Equal("blocked@example.com", profileBlock.BlockedEmail);

            UserBlockResponse duplicateBlock = await moderationService.BlockUserAsync(
                "blocker@example.com",
                new BlockUserRequest("blocked@example.com", "Duplicate block.", null),
                CancellationToken.None);
            Assert.Equal(profileBlock.Id, duplicateBlock.Id);

            IReadOnlyList<PartnerMatchProfileResponse> matches = await matchingService.SearchAsync(
                "blocker@example.com",
                new PartnerMatchSearchRequest(null, null, null, null, null),
                CancellationToken.None);
            Assert.DoesNotContain(matches, profile => profile.DisplayName == "Blocked");

            PartnerRequestResponse request = await matchingService.SubmitRequestAsync(
                "blocked@example.com",
                new SubmitPartnerRequestRequest(requestTargetId, "practice-goals", null),
                CancellationToken.None);
            UserBlockResponse requestBlock = await moderationService.BlockUserAsync(
                "request-target@example.com",
                new BlockUserRequest(null, "Block from request.", request.Id),
                CancellationToken.None);

            Assert.Equal("request-target@example.com", requestBlock.BlockerEmail);
            Assert.Equal("blocked@example.com", requestBlock.BlockedEmail);
            Assert.Equal(request.Id, requestBlock.SourcePartnerRequestId);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                moderationService.BlockUserAsync(
                    "blocker@example.com",
                    new BlockUserRequest(null, "Missing target.", null),
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

    private static async Task<LearnerConversationProfilePrivateResponse> SaveProfileAsync(
        ILearnerConversationProfileService profileService,
        string ownerEmail,
        string displayName)
    {
        return await profileService.SaveAsync(
            ownerEmail,
            new SaveLearnerConversationProfileRequest(
                displayName,
                "Berlin",
                "both",
                "B1",
                ["en", "fa"],
                "I want safe conversation practice.",
                "Weekday evenings",
                "public",
                true),
            CancellationToken.None);
    }

    private static ServiceProvider BuildServiceProvider(string connectionString)
    {
        ServiceCollection services = new();
        services.AddDarwinLinguaInfrastructureForPostgres(connectionString);
        services.AddScoped<ILearnerConversationProfileService, LearnerConversationProfileService>();
        services.AddScoped<IPartnerMatchingService, PartnerMatchingService>();
        services.AddScoped<IModerationService, ModerationService>();
        return services.BuildServiceProvider();
    }
}
