using System.Text.RegularExpressions;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Exceptions;
using DarwinLingua.SharedKernel.Lexicon;

namespace DarwinLingua.Catalog.Domain.Entities;

public sealed partial class ConversationEvent
{
    private readonly List<ConversationEventLevel> _supportedLevels = [];
    private readonly List<ConversationEventHelperLanguage> _helperLanguages = [];
    private readonly List<ConversationEventPreparationPackLink> _preparationPackLinks = [];

    private ConversationEvent()
    {
    }

    public ConversationEvent(
        Guid id,
        string slug,
        string name,
        string description,
        string? city,
        string countryRegion,
        string? approximateLocation,
        bool isOnline,
        string category,
        string organizerName,
        string? organizerProfileSlug,
        string? externalLink,
        string? contactMethod,
        string scheduleText,
        string priceType,
        string verificationStatus,
        PublicationStatus publicationStatus,
        int sortOrder,
        DateTime createdAtUtc,
        DateTime? startsAtUtc = null,
        DateTime? endsAtUtc = null)
    {
        if (id == Guid.Empty)
        {
            throw new DomainRuleException("Conversation event identifier cannot be empty.");
        }

        Id = id;
        Slug = NormalizeKey(slug, "Conversation event slug");
        Name = NormalizeRequiredText(name, nameof(name), 256);
        Description = NormalizeRequiredText(description, nameof(description), 4000);
        City = NormalizeOptionalText(city, 128);
        CountryRegion = NormalizeRequiredText(countryRegion, nameof(countryRegion), 128);
        ApproximateLocation = NormalizeOptionalText(approximateLocation, 512);
        IsOnline = isOnline;
        Category = NormalizeTaxonomyKey(category, ConversationEventTaxonomy.Categories, "Conversation event category");
        OrganizerName = NormalizeRequiredText(organizerName, nameof(organizerName), 256);
        OrganizerProfileSlug = NormalizeOptionalKey(organizerProfileSlug, "Conversation event organizer profile slug");
        ExternalLink = NormalizeOptionalText(externalLink, 1024);
        ContactMethod = NormalizeOptionalText(contactMethod, 512);
        ScheduleText = NormalizeRequiredText(scheduleText, nameof(scheduleText), 1000);
        PriceType = NormalizeTaxonomyKey(priceType, ConversationEventTaxonomy.PriceTypes, "Conversation event price type");
        VerificationStatus = NormalizeTaxonomyKey(verificationStatus, ConversationEventTaxonomy.VerificationStatuses, "Conversation event verification status");
        PublicationStatus = publicationStatus;
        SortOrder = NormalizeSortOrder(sortOrder);
        SetEventTiming(startsAtUtc, endsAtUtc);
        CreatedAtUtc = NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
        UpdatedAtUtc = CreatedAtUtc;
    }

    public Guid Id { get; private set; }

    public string Slug { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public string? City { get; private set; }

    public string CountryRegion { get; private set; } = string.Empty;

    public string? ApproximateLocation { get; private set; }

    public bool IsOnline { get; private set; }

    public string Category { get; private set; } = string.Empty;

    public string OrganizerName { get; private set; } = string.Empty;

    public string? OrganizerProfileSlug { get; private set; }

    public string? ExternalLink { get; private set; }

    public string? ContactMethod { get; private set; }

    public string ScheduleText { get; private set; } = string.Empty;

    public string? RecurrenceRule { get; private set; }

    public int? Capacity { get; private set; }

    public DateTime? StartsAtUtc { get; private set; }

    public DateTime? EndsAtUtc { get; private set; }

    public string PriceType { get; private set; } = string.Empty;

    public string VerificationStatus { get; private set; } = string.Empty;

    public string? SourceName { get; private set; }

    public string? SourceUrl { get; private set; }

    public DateTime? LastVerifiedAtUtc { get; private set; }

    public PublicationStatus PublicationStatus { get; private set; }

    public int SortOrder { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<ConversationEventLevel> SupportedLevels => _supportedLevels.AsReadOnly();

    public IReadOnlyCollection<ConversationEventHelperLanguage> HelperLanguages => _helperLanguages.AsReadOnly();

    public IReadOnlyCollection<ConversationEventPreparationPackLink> PreparationPackLinks => _preparationPackLinks.AsReadOnly();

    public void SetSourceMetadata(string? sourceName, string? sourceUrl, DateTime? lastVerifiedAtUtc)
    {
        SourceName = NormalizeOptionalText(sourceName, 256);
        SourceUrl = NormalizeOptionalText(sourceUrl, 1024);
        LastVerifiedAtUtc = lastVerifiedAtUtc.HasValue
            ? NormalizeUtc(lastVerifiedAtUtc.Value, nameof(lastVerifiedAtUtc))
            : null;
    }

    public void SetOperationalDetails(
        string? recurrenceRule,
        int? capacity,
        DateTime updatedAtUtc,
        DateTime? startsAtUtc = null,
        DateTime? endsAtUtc = null)
    {
        RecurrenceRule = NormalizeOptionalText(recurrenceRule, 256);
        Capacity = capacity switch
        {
            null => null,
            <= 0 => throw new DomainRuleException("Conversation event capacity must be greater than zero when specified."),
            > 10000 => throw new DomainRuleException("Conversation event capacity cannot exceed 10000."),
            _ => capacity,
        };
        SetEventTiming(startsAtUtc, endsAtUtc);
        UpdatedAtUtc = NormalizeUtc(updatedAtUtc, nameof(updatedAtUtc));
    }

    public void SetPublicationStatus(PublicationStatus publicationStatus, DateTime updatedAtUtc)
    {
        PublicationStatus = publicationStatus;
        UpdatedAtUtc = NormalizeUtc(updatedAtUtc, nameof(updatedAtUtc));
    }

    public void AddSupportedLevel(Guid id, CefrLevel cefrLevel, int sortOrder, DateTime createdAtUtc)
    {
        _supportedLevels.Add(new ConversationEventLevel(id, Id, cefrLevel, sortOrder, createdAtUtc));
        UpdatedAtUtc = NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
    }

    public void AddHelperLanguage(Guid id, string languageCode, int sortOrder, DateTime createdAtUtc)
    {
        _helperLanguages.Add(new ConversationEventHelperLanguage(id, Id, languageCode, sortOrder, createdAtUtc));
        UpdatedAtUtc = NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
    }

    public void AddPreparationPackLink(Guid id, string preparationPackSlug, int sortOrder, DateTime createdAtUtc)
    {
        _preparationPackLinks.Add(new ConversationEventPreparationPackLink(id, Id, preparationPackSlug, sortOrder, createdAtUtc));
        UpdatedAtUtc = NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
    }

    internal static string NormalizeRequiredText(string value, string parameterName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainRuleException($"{parameterName} cannot be empty.");
        }

        string normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new DomainRuleException($"{parameterName} cannot exceed {maxLength} characters.");
        }

        return normalized;
    }

    internal static string? NormalizeOptionalText(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        string normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new DomainRuleException($"Value cannot exceed {maxLength} characters.");
        }

        return normalized;
    }

    internal static string? NormalizeOptionalKey(string? value, string label)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : NormalizeKey(value, label);
    }

    public static string NormalizeKey(string value, string label)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainRuleException($"{label} cannot be empty.");
        }

        string normalized = value.Trim().ToLowerInvariant();
        if (!KeyPattern().IsMatch(normalized))
        {
            throw new DomainRuleException($"{label} must use lowercase kebab-case characters only.");
        }

        return normalized;
    }

    internal static string NormalizeTaxonomyKey(string value, IReadOnlySet<string> allowedValues, string label)
    {
        string normalized = NormalizeKey(value, label);
        if (!allowedValues.Contains(normalized))
        {
            throw new DomainRuleException($"{label} is not supported.");
        }

        return normalized;
    }

    internal static int NormalizeSortOrder(int value)
    {
        if (value < 0)
        {
            throw new DomainRuleException("Conversation event sort order cannot be negative.");
        }

        return value;
    }

    internal static DateTime NormalizeUtc(DateTime value, string parameterName)
    {
        if (value == default)
        {
            throw new DomainRuleException($"{parameterName} cannot be empty.");
        }

        return value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime();
    }

    private void SetEventTiming(DateTime? startsAtUtc, DateTime? endsAtUtc)
    {
        DateTime? normalizedStartsAtUtc = startsAtUtc.HasValue
            ? NormalizeUtc(startsAtUtc.Value, nameof(startsAtUtc))
            : null;
        DateTime? normalizedEndsAtUtc = endsAtUtc.HasValue
            ? NormalizeUtc(endsAtUtc.Value, nameof(endsAtUtc))
            : null;

        if (normalizedStartsAtUtc is null && normalizedEndsAtUtc is not null)
        {
            throw new DomainRuleException("Conversation event end time requires a start time.");
        }

        if (normalizedStartsAtUtc is not null &&
            normalizedEndsAtUtc is not null &&
            normalizedEndsAtUtc <= normalizedStartsAtUtc)
        {
            throw new DomainRuleException("Conversation event end time must be after the start time.");
        }

        StartsAtUtc = normalizedStartsAtUtc;
        EndsAtUtc = normalizedEndsAtUtc;
    }

    [GeneratedRegex("^[a-z0-9]+(-[a-z0-9]+)*$", RegexOptions.Compiled)]
    private static partial Regex KeyPattern();
}

public sealed class ConversationEventLevel
{
    private ConversationEventLevel()
    {
    }

    internal ConversationEventLevel(Guid id, Guid conversationEventId, CefrLevel cefrLevel, int sortOrder, DateTime createdAtUtc)
    {
        Id = id == Guid.Empty ? throw new DomainRuleException("Conversation event level identifier cannot be empty.") : id;
        ConversationEventId = conversationEventId == Guid.Empty ? throw new DomainRuleException("Conversation event level event identifier cannot be empty.") : conversationEventId;
        CefrLevel = cefrLevel;
        SortOrder = ConversationEvent.NormalizeSortOrder(sortOrder);
        CreatedAtUtc = ConversationEvent.NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
    }

    public Guid Id { get; private set; }

    public Guid ConversationEventId { get; private set; }

    public CefrLevel CefrLevel { get; private set; }

    public int SortOrder { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }
}

public sealed class ConversationEventHelperLanguage
{
    private ConversationEventHelperLanguage()
    {
    }

    internal ConversationEventHelperLanguage(Guid id, Guid conversationEventId, string languageCode, int sortOrder, DateTime createdAtUtc)
    {
        Id = id == Guid.Empty ? throw new DomainRuleException("Conversation event helper language identifier cannot be empty.") : id;
        ConversationEventId = conversationEventId == Guid.Empty ? throw new DomainRuleException("Conversation event helper language event identifier cannot be empty.") : conversationEventId;
        LanguageCode = ConversationEvent.NormalizeKey(languageCode, "Conversation event helper language code");
        SortOrder = ConversationEvent.NormalizeSortOrder(sortOrder);
        CreatedAtUtc = ConversationEvent.NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
    }

    public Guid Id { get; private set; }

    public Guid ConversationEventId { get; private set; }

    public string LanguageCode { get; private set; } = string.Empty;

    public int SortOrder { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }
}

public sealed class ConversationEventPreparationPackLink
{
    private ConversationEventPreparationPackLink()
    {
    }

    internal ConversationEventPreparationPackLink(Guid id, Guid conversationEventId, string preparationPackSlug, int sortOrder, DateTime createdAtUtc)
    {
        Id = id == Guid.Empty ? throw new DomainRuleException("Conversation event preparation pack link identifier cannot be empty.") : id;
        ConversationEventId = conversationEventId == Guid.Empty ? throw new DomainRuleException("Conversation event preparation pack link event identifier cannot be empty.") : conversationEventId;
        PreparationPackSlug = ConversationEvent.NormalizeKey(preparationPackSlug, "Conversation event preparation pack slug");
        SortOrder = ConversationEvent.NormalizeSortOrder(sortOrder);
        CreatedAtUtc = ConversationEvent.NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
    }

    public Guid Id { get; private set; }

    public Guid ConversationEventId { get; private set; }

    public string PreparationPackSlug { get; private set; } = string.Empty;

    public int SortOrder { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }
}

public static class ConversationEventTaxonomy
{
    public static readonly IReadOnlySet<string> Categories = new HashSet<string>(StringComparer.Ordinal)
    {
        "conversation-cafe",
        "language-club",
        "online-meeting",
        "support-resource",
        "class",
        "workshop",
        "other",
    };

    public static readonly IReadOnlySet<string> PriceTypes = new HashSet<string>(StringComparer.Ordinal)
    {
        "free",
        "donation",
        "paid",
        "unknown",
    };

    public static readonly IReadOnlySet<string> VerificationStatuses = new HashSet<string>(StringComparer.Ordinal)
    {
        "unverified",
        "reviewed",
        "verified",
        "stale",
    };
}
