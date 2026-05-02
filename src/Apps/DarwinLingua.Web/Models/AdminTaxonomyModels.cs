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

public sealed record AdminMergeTopicRequest(
    Guid TargetTopicId);

public sealed record AdminLabelsPageViewModel(
    IReadOnlyList<AdminLabelItemViewModel> Labels);

public sealed record AdminLabelItemViewModel(
    Guid LabelId,
    string Kind,
    string Key,
    string DisplayName,
    IReadOnlyList<AdminLabelLocalizationViewModel>? Localizations,
    int SortOrder,
    bool IsSystem,
    int WordCount,
    DateTime UpdatedAtUtc);

public sealed record AdminLabelLocalizationViewModel(
    string LanguageCode,
    string DisplayName);

public sealed class AdminLabelEditViewModel
{
    public Guid LabelId { get; set; }

    public bool IsNew => LabelId == Guid.Empty;

    [Required]
    public string Kind { get; set; } = "Usage";

    [Required]
    [StringLength(64)]
    [RegularExpression("^[a-z0-9]+(-[a-z0-9]+)*$")]
    public string Key { get; set; } = string.Empty;

    [Required]
    [StringLength(128)]
    public string DisplayName { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int SortOrder { get; set; }

    public bool IsSystem { get; set; }

    public static AdminLabelEditViewModel CreateNew() => new()
    {
        LabelId = Guid.Empty,
        Kind = "Usage",
    };

    public static AdminLabelEditViewModel FromItem(AdminLabelItemViewModel label) => new()
    {
        LabelId = label.LabelId,
        Kind = label.Kind,
        Key = label.Key,
        DisplayName = label.DisplayName,
        SortOrder = label.SortOrder,
        IsSystem = label.IsSystem,
    };
}

public sealed record AdminSaveLabelRequest(
    string Kind,
    string Key,
    string DisplayName,
    IReadOnlyList<AdminLabelLocalizationRequest>? Localizations,
    int SortOrder,
    bool IsSystem);

public sealed record AdminLabelLocalizationRequest(
    string LanguageCode,
    string DisplayName);

public sealed record AdminMergeLabelRequest(
    Guid TargetLabelId);
