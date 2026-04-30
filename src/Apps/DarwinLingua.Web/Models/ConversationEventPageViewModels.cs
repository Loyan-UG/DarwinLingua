using System.ComponentModel.DataAnnotations;
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
    [Required]
    [StringLength(128)]
    public string ParticipantName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(320)]
    public string ParticipantEmail { get; set; } = string.Empty;

    [Required]
    [StringLength(32)]
    public string Status { get; set; } = "interested";
}
