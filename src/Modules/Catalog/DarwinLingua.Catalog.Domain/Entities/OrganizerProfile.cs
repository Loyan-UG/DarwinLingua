using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Exceptions;
using DarwinLingua.SharedKernel.Lexicon;

namespace DarwinLingua.Catalog.Domain.Entities;

public sealed class OrganizerProfile
{
    private readonly List<OrganizerProfileSupportedLevel> _supportedLevels = [];
    private readonly List<OrganizerProfileHelperLanguage> _helperLanguages = [];

    private OrganizerProfile()
    {
    }

    public OrganizerProfile(
        Guid id,
        string slug,
        string displayName,
        string organizerType,
        string description,
        string? cityRegion,
        bool isOnlineAvailable,
        string? websiteUrl,
        string? publicContactMethod,
        string verificationStatus,
        string planKey,
        PublicationStatus publicationStatus,
        int historicalEventCount,
        DateTime createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new DomainRuleException("Organizer profile identifier cannot be empty.");
        }

        Id = id;
        Slug = ConversationEvent.NormalizeKey(slug, "Organizer profile slug");
        DisplayName = ConversationEvent.NormalizeRequiredText(displayName, nameof(displayName), 256);
        OrganizerType = ConversationEvent.NormalizeTaxonomyKey(organizerType, OrganizerProfileTaxonomy.OrganizerTypes, "Organizer type");
        Description = ConversationEvent.NormalizeRequiredText(description, nameof(description), 4000);
        CityRegion = ConversationEvent.NormalizeOptionalText(cityRegion, 128);
        IsOnlineAvailable = isOnlineAvailable;
        WebsiteUrl = ConversationEvent.NormalizeOptionalText(websiteUrl, 1024);
        PublicContactMethod = ConversationEvent.NormalizeOptionalText(publicContactMethod, 512);
        VerificationStatus = ConversationEvent.NormalizeTaxonomyKey(verificationStatus, ConversationEventTaxonomy.VerificationStatuses, "Organizer verification status");
        PlanKey = ConversationEvent.NormalizeKey(planKey, "Organizer plan key");
        PublicationStatus = publicationStatus;
        HistoricalEventCount = historicalEventCount < 0
            ? throw new DomainRuleException("Historical event count cannot be negative.")
            : historicalEventCount;
        CreatedAtUtc = ConversationEvent.NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
        UpdatedAtUtc = CreatedAtUtc;
    }

    public Guid Id { get; private set; }

    public string Slug { get; private set; } = string.Empty;

    public string DisplayName { get; private set; } = string.Empty;

    public string OrganizerType { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public string? CityRegion { get; private set; }

    public bool IsOnlineAvailable { get; private set; }

    public string? WebsiteUrl { get; private set; }

    public string? PublicContactMethod { get; private set; }

    public string VerificationStatus { get; private set; } = string.Empty;

    public string PlanKey { get; private set; } = string.Empty;

    public PublicationStatus PublicationStatus { get; private set; }

    public int HistoricalEventCount { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<OrganizerProfileSupportedLevel> SupportedLevels => _supportedLevels.AsReadOnly();

    public IReadOnlyCollection<OrganizerProfileHelperLanguage> HelperLanguages => _helperLanguages.AsReadOnly();

    public void AddSupportedLevel(Guid id, CefrLevel cefrLevel, int sortOrder, DateTime createdAtUtc)
    {
        _supportedLevels.Add(new OrganizerProfileSupportedLevel(id, Id, cefrLevel, sortOrder, createdAtUtc));
        UpdatedAtUtc = ConversationEvent.NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
    }

    public void AddHelperLanguage(Guid id, string languageCode, int sortOrder, DateTime createdAtUtc)
    {
        _helperLanguages.Add(new OrganizerProfileHelperLanguage(id, Id, languageCode, sortOrder, createdAtUtc));
        UpdatedAtUtc = ConversationEvent.NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
    }
}

public sealed class OrganizerProfileSupportedLevel
{
    private OrganizerProfileSupportedLevel()
    {
    }

    internal OrganizerProfileSupportedLevel(Guid id, Guid organizerProfileId, CefrLevel cefrLevel, int sortOrder, DateTime createdAtUtc)
    {
        Id = id == Guid.Empty ? throw new DomainRuleException("Organizer profile level identifier cannot be empty.") : id;
        OrganizerProfileId = organizerProfileId == Guid.Empty ? throw new DomainRuleException("Organizer profile level profile identifier cannot be empty.") : organizerProfileId;
        CefrLevel = cefrLevel;
        SortOrder = ConversationEvent.NormalizeSortOrder(sortOrder);
        CreatedAtUtc = ConversationEvent.NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
    }

    public Guid Id { get; private set; }

    public Guid OrganizerProfileId { get; private set; }

    public CefrLevel CefrLevel { get; private set; }

    public int SortOrder { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }
}

public sealed class OrganizerProfileHelperLanguage
{
    private OrganizerProfileHelperLanguage()
    {
    }

    internal OrganizerProfileHelperLanguage(Guid id, Guid organizerProfileId, string languageCode, int sortOrder, DateTime createdAtUtc)
    {
        Id = id == Guid.Empty ? throw new DomainRuleException("Organizer profile helper language identifier cannot be empty.") : id;
        OrganizerProfileId = organizerProfileId == Guid.Empty ? throw new DomainRuleException("Organizer profile helper language profile identifier cannot be empty.") : organizerProfileId;
        LanguageCode = ConversationEvent.NormalizeKey(languageCode, "Organizer profile helper language code");
        SortOrder = ConversationEvent.NormalizeSortOrder(sortOrder);
        CreatedAtUtc = ConversationEvent.NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
    }

    public Guid Id { get; private set; }

    public Guid OrganizerProfileId { get; private set; }

    public string LanguageCode { get; private set; } = string.Empty;

    public int SortOrder { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }
}

public static class OrganizerProfileTaxonomy
{
    public static readonly IReadOnlySet<string> OrganizerTypes = new HashSet<string>(StringComparer.Ordinal)
    {
        "teacher",
        "cafe",
        "club",
        "association",
        "school",
        "company",
        "library",
        "other",
    };
}
