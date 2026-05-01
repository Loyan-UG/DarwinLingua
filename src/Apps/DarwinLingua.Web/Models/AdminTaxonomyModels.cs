using System.ComponentModel.DataAnnotations;

namespace DarwinLingua.Web.Models;

public sealed record AdminTopicsPageViewModel(
    IReadOnlyList<AdminTopicItemViewModel> Topics);

public sealed record AdminTopicItemViewModel(
    Guid TopicId,
    string Key,
    int SortOrder,
    bool IsSystem,
    int WordCount,
    DateTime UpdatedAtUtc,
    IReadOnlyList<AdminTopicLocalizationViewModel> Localizations);

public sealed record AdminTopicLocalizationViewModel(
    string LanguageCode,
    string DisplayName);

public sealed class AdminTopicEditViewModel
{
    public Guid TopicId { get; set; }

    public bool IsNew => TopicId == Guid.Empty;

    [Required]
    [StringLength(96)]
    public string Key { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int SortOrder { get; set; }

    public bool IsSystem { get; set; }

    [StringLength(16)]
    public string? PrimaryLanguageCode { get; set; } = "en";

    [StringLength(128)]
    public string? PrimaryDisplayName { get; set; }

    [StringLength(16)]
    public string? SecondaryLanguageCode { get; set; }

    [StringLength(128)]
    public string? SecondaryDisplayName { get; set; }

    public static AdminTopicEditViewModel CreateNew() => new()
    {
        TopicId = Guid.Empty,
        PrimaryLanguageCode = "en",
    };

    public static AdminTopicEditViewModel FromItem(AdminTopicItemViewModel topic)
    {
        AdminTopicLocalizationViewModel? primary = topic.Localizations.FirstOrDefault();
        AdminTopicLocalizationViewModel? secondary = topic.Localizations.Skip(1).FirstOrDefault();

        return new()
        {
            TopicId = topic.TopicId,
            Key = topic.Key,
            SortOrder = topic.SortOrder,
            IsSystem = topic.IsSystem,
            PrimaryLanguageCode = primary?.LanguageCode ?? "en",
            PrimaryDisplayName = primary?.DisplayName,
            SecondaryLanguageCode = secondary?.LanguageCode,
            SecondaryDisplayName = secondary?.DisplayName,
        };
    }
}

public sealed record AdminSaveTopicRequest(
    string Key,
    int SortOrder,
    bool IsSystem,
    IReadOnlyList<AdminTopicLocalizationRequest>? Localizations);

public sealed record AdminTopicLocalizationRequest(
    string LanguageCode,
    string DisplayName);

public sealed record AdminLabelsPageViewModel(
    IReadOnlyList<AdminLabelItemViewModel> Labels);

public sealed record AdminLabelItemViewModel(
    string Kind,
    string Key,
    int WordCount,
    int FirstSortOrder);
