namespace DarwinLingua.WebApi.Models;

public sealed record AdminTopicsResponse(
    IReadOnlyList<AdminTopicItemResponse> Topics);

public sealed record AdminTopicItemResponse(
    Guid TopicId,
    string Key,
    int SortOrder,
    bool IsSystem,
    int WordCount,
    DateTime UpdatedAtUtc,
    IReadOnlyList<AdminTopicLocalizationResponse> Localizations);

public sealed record AdminTopicLocalizationResponse(
    string LanguageCode,
    string DisplayName);

public sealed record AdminSaveTopicRequest(
    string Key,
    int SortOrder,
    bool IsSystem,
    IReadOnlyList<AdminTopicLocalizationRequest>? Localizations);

public sealed record AdminTopicLocalizationRequest(
    string LanguageCode,
    string DisplayName);

public sealed record AdminLabelsResponse(
    IReadOnlyList<AdminLabelItemResponse> Labels);

public sealed record AdminLabelItemResponse(
    string Kind,
    string Key,
    int WordCount,
    int FirstSortOrder);
