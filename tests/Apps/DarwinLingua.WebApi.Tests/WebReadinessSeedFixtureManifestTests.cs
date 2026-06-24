using System.Text.Json;
using Xunit;

namespace DarwinLingua.WebApi.Tests;

public sealed class WebReadinessSeedFixtureManifestTests
{
    [Fact]
    public void WebReadinessSeedFixtureManifest_ShouldIncludeMultiRecordSocialSafetyAndContentReferences()
    {
        using JsonDocument document = JsonDocument.Parse(File.ReadAllText(ResolveRepositoryPath("tools", "Web", "WebReadinessSeedFixtureManifest.json")));
        JsonElement root = document.RootElement;

        Assert.Equal(1, root.GetProperty("schemaVersion").GetInt32());

        AssertAtLeast(root.GetProperty("contentReferences").GetProperty("dialogues"), 2, "dialogues");
        AssertAtLeast(root.GetProperty("contentReferences").GetProperty("conversationStarterPacks"), 2, "conversation starter packs");
        AssertAtLeast(root.GetProperty("contentReferences").GetProperty("eventPreparationPacks"), 2, "event preparation packs");
        AssertAtLeast(root.GetProperty("organizerProfiles"), 2, "organizer profiles");
        AssertAtLeast(root.GetProperty("organizerProfileOwners"), 2, "organizer profile owners");
        AssertAtLeast(root.GetProperty("conversationEvents"), 2, "conversation events");
        AssertAtLeast(root.GetProperty("eventRsvps"), 2, "event RSVPs");
        AssertAtLeast(root.GetProperty("organizerClaims"), 2, "organizer claims");
        AssertAtLeast(root.GetProperty("learnerProfiles"), 2, "learner profiles");
        AssertAtLeast(root.GetProperty("partnerRequests"), 2, "partner requests");
        AssertAtLeast(root.GetProperty("userReports"), 2, "user reports");
        AssertAtLeast(root.GetProperty("userBlocks"), 2, "user blocks");
        AssertAtLeast(root.GetProperty("moderationAudits"), 2, "moderation audits");

        HashSet<string> organizerSlugs = ReadStringSet(root.GetProperty("organizerProfiles"), "slug");
        HashSet<string> eventSlugs = ReadStringSet(root.GetProperty("conversationEvents"), "slug");
        HashSet<string> eventPreparationSlugs = ReadStringSet(root.GetProperty("contentReferences").GetProperty("eventPreparationPacks"));
        HashSet<string> learnerKeys = ReadStringSet(root.GetProperty("learnerProfiles"), "key");
        HashSet<string> partnerRequestKeys = ReadStringSet(root.GetProperty("partnerRequests"), "key");
        HashSet<string> reportKeys = ReadStringSet(root.GetProperty("userReports"), "key");

        foreach (JsonElement conversationEvent in root.GetProperty("conversationEvents").EnumerateArray())
        {
            AssertRequiredStringProperties(
                conversationEvent,
                "slug",
                "name",
                "description",
                "countryRegion",
                "category",
                "organizerName",
                "scheduleText",
                "priceType",
                "verificationStatus");
            Assert.Contains(GetRequiredString(conversationEvent, "organizerProfileSlug"), organizerSlugs);

            foreach (JsonElement linkedPackSlug in conversationEvent.GetProperty("linkedEventPreparationPackSlugs").EnumerateArray())
            {
                Assert.Contains(GetRequiredString(linkedPackSlug), eventPreparationSlugs);
            }
        }

        foreach (JsonElement rsvp in root.GetProperty("eventRsvps").EnumerateArray())
        {
            Assert.Contains(GetRequiredString(rsvp, "eventSlug"), eventSlugs);
            AssertRequiredStringProperties(rsvp, "participantName", "participantEmail", "status");
        }

        foreach (JsonElement owner in root.GetProperty("organizerProfileOwners").EnumerateArray())
        {
            Assert.Contains(GetRequiredString(owner, "organizerProfileSlug"), organizerSlugs);
            AssertRequiredStringProperties(owner, "ownerEmail", "assignedBy");
        }

        foreach (JsonElement claim in root.GetProperty("organizerClaims").EnumerateArray())
        {
            Assert.Contains(GetRequiredString(claim, "organizerProfileSlug"), organizerSlugs);
            AssertRequiredStringProperties(claim, "requesterName", "requesterEmail", "relationshipToOrganizer", "evidenceText", "status");
        }

        foreach (JsonElement learnerProfile in root.GetProperty("learnerProfiles").EnumerateArray())
        {
            AssertRequiredStringProperties(
                learnerProfile,
                "key",
                "ownerEmail",
                "displayName",
                "interactionPreference",
                "learningLevel",
                "conversationGoals",
                "visibility");
            AssertAtLeast(learnerProfile.GetProperty("helperLanguageCodes"), 1, "learner helper languages");
        }

        foreach (JsonElement partnerRequest in root.GetProperty("partnerRequests").EnumerateArray())
        {
            Assert.Contains(GetRequiredString(partnerRequest, "requesterProfileKey"), learnerKeys);
            Assert.Contains(GetRequiredString(partnerRequest, "targetProfileKey"), learnerKeys);
            AssertRequiredStringProperties(partnerRequest, "key", "openerTemplateKey", "note", "status");
        }

        foreach (JsonElement report in root.GetProperty("userReports").EnumerateArray())
        {
            AssertRequiredStringProperties(report, "key", "reporterEmail", "targetType", "targetKey", "reason", "details", "status");
        }

        foreach (JsonElement block in root.GetProperty("userBlocks").EnumerateArray())
        {
            Assert.Contains(GetRequiredString(block, "blockerProfileKey"), learnerKeys);
            Assert.Contains(GetRequiredString(block, "blockedProfileKey"), learnerKeys);

            if (block.TryGetProperty("sourcePartnerRequestKey", out JsonElement sourcePartnerRequestKey))
            {
                Assert.Contains(GetRequiredString(sourcePartnerRequestKey), partnerRequestKeys);
            }
        }

        foreach (JsonElement audit in root.GetProperty("moderationAudits").EnumerateArray())
        {
            Assert.Contains(GetRequiredString(audit, "userReportKey"), reportKeys);
            Assert.NotEqual("pending", GetRequiredString(audit, "decisionStatus"), StringComparer.OrdinalIgnoreCase);
            Assert.False(string.IsNullOrWhiteSpace(GetRequiredString(audit, "decidedBy")));
            Assert.False(string.IsNullOrWhiteSpace(GetRequiredString(audit, "decisionNote")));
        }
    }

    [Fact]
    public void OperationalSeedScript_ShouldApplyWebReadinessManifestThroughRealWebApiEndpoints()
    {
        string script = File.ReadAllText(ResolveRepositoryPath("tools", "Server", "Initialize-LocalWebOperationalSeeds.ps1"));

        Assert.Contains("[string]$SeedPath = \"tools\\Web\\WebReadinessSeedFixtureManifest.json\"", script, StringComparison.Ordinal);
        Assert.Contains("Set-StrictMode -Version Latest", script, StringComparison.Ordinal);
        Assert.Contains("Get-SeedValue", script, StringComparison.Ordinal);
        Assert.Contains("Get-SeedValueFromAny", script, StringComparison.Ordinal);
        Assert.Contains("Get-FirstSeedItems", script, StringComparison.Ordinal);

        Assert.Contains("api/admin/catalog/organizer-profiles", script, StringComparison.Ordinal);
        Assert.Contains("api/admin/catalog/conversation-events", script, StringComparison.Ordinal);
        Assert.Contains("api/admin/catalog/organizer-profile-owners", script, StringComparison.Ordinal);
        Assert.Contains("api/catalog/organizer-profiles/$(Escape-Url $organizerProfileSlug)/claim", script, StringComparison.Ordinal);
        Assert.Contains("api/admin/catalog/organizer-claim-requests/$claimId/status", script, StringComparison.Ordinal);
        Assert.Contains("api/catalog/conversation-events/$(Escape-Url $eventSlug)/rsvps", script, StringComparison.Ordinal);
        Assert.Contains("api/catalog/learner-conversation-profiles/me?ownerEmail=", script, StringComparison.Ordinal);
        Assert.Contains("api/catalog/partner-requests?ownerEmail=", script, StringComparison.Ordinal);
        Assert.Contains("api/catalog/moderation/reports?reporterEmail=", script, StringComparison.Ordinal);
        Assert.Contains("api/admin/catalog/moderation/reports/$reportId/decision", script, StringComparison.Ordinal);
        Assert.Contains("api/catalog/moderation/blocks?blockerEmail=", script, StringComparison.Ordinal);

        Assert.Contains("$profileIdsByKey", script, StringComparison.Ordinal);
        Assert.Contains("$emailsByProfileKey", script, StringComparison.Ordinal);
        Assert.Contains("$partnerRequestIdsByKey", script, StringComparison.Ordinal);
        Assert.Contains("$reportIdsByKey", script, StringComparison.Ordinal);
        Assert.Contains("sourcePartnerRequestKey", script, StringComparison.Ordinal);
        Assert.Contains("moderationAudits", script, StringComparison.Ordinal);
    }

    private static void AssertAtLeast(JsonElement array, int minimumCount, string label)
    {
        Assert.Equal(JsonValueKind.Array, array.ValueKind);
        Assert.True(array.GetArrayLength() >= minimumCount, $"Expected at least {minimumCount} {label}.");
    }

    private static void AssertRequiredStringProperties(JsonElement item, params string[] propertyNames)
    {
        foreach (string propertyName in propertyNames)
        {
            Assert.False(string.IsNullOrWhiteSpace(GetRequiredString(item, propertyName)), $"Expected non-empty '{propertyName}'.");
        }
    }

    private static HashSet<string> ReadStringSet(JsonElement array) =>
        array.EnumerateArray()
            .Select(item => item.GetString())
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Select(item => item!)
            .ToHashSet(StringComparer.Ordinal);

    private static HashSet<string> ReadStringSet(JsonElement array, string propertyName) =>
        array.EnumerateArray()
            .Select(item => item.GetProperty(propertyName).GetString())
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Select(item => item!)
            .ToHashSet(StringComparer.Ordinal);

    private static string GetRequiredString(JsonElement element, string propertyName) =>
        GetRequiredString(element.GetProperty(propertyName));

    private static string GetRequiredString(JsonElement element) =>
        element.GetString() ?? throw new InvalidOperationException("Expected a non-null string value.");

    private static string ResolveRepositoryPath(params string[] segments)
    {
        string? directory = AppContext.BaseDirectory;
        while (!string.IsNullOrWhiteSpace(directory))
        {
            string candidate = Path.Combine(new[] { directory }.Concat(segments).ToArray());
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = Directory.GetParent(directory)?.FullName;
        }

        throw new FileNotFoundException($"Could not resolve repository file '{string.Join(Path.DirectorySeparatorChar, segments)}'.");
    }
}
