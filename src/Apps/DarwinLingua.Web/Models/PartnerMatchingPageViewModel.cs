using System.ComponentModel.DataAnnotations;
using DarwinLingua.Web.Services;

namespace DarwinLingua.Web.Models;

public sealed record PartnerMatchingPageViewModel(
    PartnerMatchSearchInputModel Search,
    IReadOnlyList<PartnerMatchProfileModel> Matches,
    IReadOnlyList<PartnerRequestModel> Requests,
    string? StatusMessage,
    string? ErrorMessage);

public sealed class PartnerMatchSearchInputModel
{
    [StringLength(128)]
    public string? CityRegion { get; set; }

    public string? InteractionPreference { get; set; }

    public string? LearningLevel { get; set; }

    [StringLength(32)]
    public string? HelperLanguageCode { get; set; }

    [StringLength(128)]
    public string? GoalKeyword { get; set; }
}

public sealed class PartnerRequestInputModel
{
    [Required]
    public Guid TargetLearnerProfileId { get; set; }

    [Required]
    [StringLength(64)]
    public string OpenerTemplateKey { get; set; } = "practice-goals";

    [StringLength(500)]
    public string? Note { get; set; }
}
