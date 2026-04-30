using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DarwinLingua.Web.Models;

public sealed record SettingsPageViewModel(
    SettingsUpdateInputModel Input,
    IReadOnlyList<SelectListItem> UiLanguageOptions,
    IReadOnlyList<SelectListItem> MeaningLanguageOptions,
    IReadOnlyList<SelectListItem> SecondaryMeaningLanguageOptions,
    string? StatusMessage,
    bool CanUseDualMeaningLanguage,
    string? DualMeaningLanguageLockedMessage,
    bool IsAuthenticated = false);

public sealed class SettingsUpdateInputModel
{
    [Required]
    [StringLength(8)]
    public string UiLanguageCode { get; init; } = "en";

    [Required]
    [StringLength(8)]
    public string PrimaryMeaningLanguageCode { get; init; } = "en";

    [StringLength(8)]
    public string? SecondaryMeaningLanguageCode { get; init; }
}
