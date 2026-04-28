using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Web.Models;

public sealed record ConversationEventIndexPageViewModel(
    IReadOnlyList<ConversationEventListItemModel> Events,
    ConversationEventListFilterModel Filter);

public sealed record ConversationEventDetailPageViewModel(
    ConversationEventDetailModel Event,
    IReadOnlyList<EventPreparationPackListItemModel> PreparationPacks,
    DarwinLingua.Web.Services.EventRsvpSummaryModel RsvpSummary,
    EventRsvpInputModel Input,
    string? StatusMessage,
    string? ErrorMessage);

public sealed class EventRsvpInputModel
{
    [System.ComponentModel.DataAnnotations.Required]
    public string ParticipantName { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.EmailAddress]
    public string ParticipantEmail { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required]
    public string Status { get; set; } = "interested";
}
