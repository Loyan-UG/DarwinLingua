using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DarwinLingua.Web.Models;

public sealed record SettingsPageViewModel(
    SettingsUpdateInputModel Input,
    IReadOnlyList<SelectListItem> UiLanguageOptions,
    IReadOnlyList<SelectListItem> MeaningLanguageOptions,
    IReadOnlyList<SelectListItem> SecondaryMeaningLanguageOptions,
    string? StatusMessage,
    bool IsAuthenticated = false);

public sealed class SettingsUpdateInputModel
{
    [Required]
    public string UiLanguageCode { get; init; } = "en";

    [Required]
    public string PrimaryMeaningLanguageCode { get; init; } = "en";

    public string? SecondaryMeaningLanguageCode { get; init; }
}
