using System.ComponentModel.DataAnnotations;
using DarwinLingua.Web.Services;

namespace DarwinLingua.Web.Models;

public sealed record LearnerConversationProfilePageViewModel(
    LearnerConversationProfileInputModel Input,
    LearnerConversationProfileModel? SavedProfile,
    string? StatusMessage,
    string? ErrorMessage);

public sealed class LearnerConversationProfileInputModel
{
    [Required]
    [StringLength(128)]
    public string DisplayName { get; set; } = string.Empty;

    [StringLength(128)]
    public string? CityRegion { get; set; }

    [Required]
    [StringLength(32)]
    public string InteractionPreference { get; set; } = "both";

    [Required]
    [StringLength(8)]
    public string LearningLevel { get; set; } = "A1";

    [Required]
    [StringLength(256)]
    public string HelperLanguageCodesText { get; set; } = "en";

    [Required]
    [StringLength(1000)]
    public string ConversationGoals { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? AvailabilityNotes { get; set; }

    [Required]
    [StringLength(32)]
    public string Visibility { get; set; } = "private";

    public bool HasConfirmedAdult { get; set; }
}
