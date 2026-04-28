using DarwinLingua.SharedKernel.Exceptions;

namespace DarwinLingua.Catalog.Domain.Entities;

public sealed class LearnerConversationProfile
{
    private LearnerConversationProfile()
    {
    }

    public LearnerConversationProfile(
        Guid id,
        string ownerEmail,
        string displayName,
        string? cityRegion,
        string interactionPreference,
        string germanLevel,
        string helperLanguageCodes,
        string conversationGoals,
        string? availabilityNotes,
        string visibility,
        bool hasConfirmedAdult,
        DateTime createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new DomainRuleException("Learner conversation profile identifier cannot be empty.");
        }

        Id = id;
        OwnerEmail = NormalizeEmail(ownerEmail);
        DisplayName = ConversationEvent.NormalizeRequiredText(displayName, nameof(displayName), 128);
        CityRegion = ConversationEvent.NormalizeOptionalText(cityRegion, 128);
        InteractionPreference = ConversationEvent.NormalizeTaxonomyKey(interactionPreference, LearnerConversationProfileTaxonomy.InteractionPreferences, "Learner interaction preference");
        GermanLevel = ConversationEvent.NormalizeRequiredText(germanLevel, nameof(germanLevel), 8).ToUpperInvariant();
        HelperLanguageCodes = ConversationEvent.NormalizeRequiredText(helperLanguageCodes, nameof(helperLanguageCodes), 256);
        ConversationGoals = ConversationEvent.NormalizeRequiredText(conversationGoals, nameof(conversationGoals), 1000);
        AvailabilityNotes = ConversationEvent.NormalizeOptionalText(availabilityNotes, 1000);
        Visibility = ConversationEvent.NormalizeTaxonomyKey(visibility, LearnerConversationProfileTaxonomy.VisibilityStates, "Learner profile visibility");
        HasConfirmedAdult = hasConfirmedAdult;
        CreatedAtUtc = ConversationEvent.NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
        UpdatedAtUtc = CreatedAtUtc;
    }

    public Guid Id { get; private set; }

    public string OwnerEmail { get; private set; } = string.Empty;

    public string DisplayName { get; private set; } = string.Empty;

    public string? CityRegion { get; private set; }

    public string InteractionPreference { get; private set; } = string.Empty;

    public string GermanLevel { get; private set; } = string.Empty;

    public string HelperLanguageCodes { get; private set; } = string.Empty;

    public string ConversationGoals { get; private set; } = string.Empty;

    public string? AvailabilityNotes { get; private set; }

    public string Visibility { get; private set; } = string.Empty;

    public bool HasConfirmedAdult { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public void Update(
        string displayName,
        string? cityRegion,
        string interactionPreference,
        string germanLevel,
        string helperLanguageCodes,
        string conversationGoals,
        string? availabilityNotes,
        string visibility,
        bool hasConfirmedAdult,
        DateTime updatedAtUtc)
    {
        DisplayName = ConversationEvent.NormalizeRequiredText(displayName, nameof(displayName), 128);
        CityRegion = ConversationEvent.NormalizeOptionalText(cityRegion, 128);
        InteractionPreference = ConversationEvent.NormalizeTaxonomyKey(interactionPreference, LearnerConversationProfileTaxonomy.InteractionPreferences, "Learner interaction preference");
        GermanLevel = ConversationEvent.NormalizeRequiredText(germanLevel, nameof(germanLevel), 8).ToUpperInvariant();
        HelperLanguageCodes = ConversationEvent.NormalizeRequiredText(helperLanguageCodes, nameof(helperLanguageCodes), 256);
        ConversationGoals = ConversationEvent.NormalizeRequiredText(conversationGoals, nameof(conversationGoals), 1000);
        AvailabilityNotes = ConversationEvent.NormalizeOptionalText(availabilityNotes, 1000);
        Visibility = ConversationEvent.NormalizeTaxonomyKey(visibility, LearnerConversationProfileTaxonomy.VisibilityStates, "Learner profile visibility");
        HasConfirmedAdult = hasConfirmedAdult;
        UpdatedAtUtc = ConversationEvent.NormalizeUtc(updatedAtUtc, nameof(updatedAtUtc));
    }

    public void SetVisibility(string visibility, DateTime updatedAtUtc)
    {
        Visibility = ConversationEvent.NormalizeTaxonomyKey(visibility, LearnerConversationProfileTaxonomy.VisibilityStates, "Learner profile visibility");
        UpdatedAtUtc = ConversationEvent.NormalizeUtc(updatedAtUtc, nameof(updatedAtUtc));
    }

    public void Anonymize(DateTime updatedAtUtc)
    {
        DisplayName = "Deleted learner";
        CityRegion = null;
        InteractionPreference = "online";
        GermanLevel = "A1";
        HelperLanguageCodes = "en";
        ConversationGoals = "Profile deleted by learner.";
        AvailabilityNotes = null;
        Visibility = "disabled";
        HasConfirmedAdult = false;
        UpdatedAtUtc = ConversationEvent.NormalizeUtc(updatedAtUtc, nameof(updatedAtUtc));
    }

    public static string NormalizeEmail(string value)
    {
        string normalized = ConversationEvent.NormalizeRequiredText(value, "Owner email", 320).ToLowerInvariant();
        if (!normalized.Contains('@', StringComparison.Ordinal))
        {
            throw new DomainRuleException("Owner email must be a valid email-like value.");
        }

        return normalized;
    }
}

public static class LearnerConversationProfileTaxonomy
{
    public static readonly IReadOnlySet<string> InteractionPreferences = new HashSet<string>(StringComparer.Ordinal)
    {
        "online",
        "in-person",
        "both",
    };

    public static readonly IReadOnlySet<string> VisibilityStates = new HashSet<string>(StringComparer.Ordinal)
    {
        "private",
        "request-only",
        "public",
        "disabled",
    };
}
