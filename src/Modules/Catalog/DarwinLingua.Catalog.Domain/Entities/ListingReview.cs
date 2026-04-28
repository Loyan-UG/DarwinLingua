using DarwinLingua.SharedKernel.Exceptions;

namespace DarwinLingua.Catalog.Domain.Entities;

public sealed class ListingReview
{
    private ListingReview()
    {
    }

    public ListingReview(Guid id, string listingType, string listingKey, string status, DateTime createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new DomainRuleException("Listing review identifier cannot be empty.");
        }

        Id = id;
        ListingType = ConversationEvent.NormalizeTaxonomyKey(listingType, ListingReviewTaxonomy.ListingTypes, "Listing review type");
        ListingKey = ConversationEvent.NormalizeRequiredText(listingKey, nameof(listingKey), 256);
        Status = ConversationEvent.NormalizeTaxonomyKey(status, ListingReviewStatuses.All, "Listing review status");
        CreatedAtUtc = ConversationEvent.NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
        UpdatedAtUtc = CreatedAtUtc;
    }

    public Guid Id { get; private set; }

    public string ListingType { get; private set; } = string.Empty;

    public string ListingKey { get; private set; } = string.Empty;

    public string Status { get; private set; } = string.Empty;

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }
}

public static class ListingReviewTaxonomy
{
    public static readonly IReadOnlySet<string> ListingTypes = new HashSet<string>(StringComparer.Ordinal)
    {
        "conversation-event",
        "organizer-profile",
    };
}

public static class ListingReviewStatuses
{
    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        "pending",
        "approved",
        "rejected",
        "needs-changes",
    };
}
