using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Web.Models;

public sealed record TalkTopicIndexPageViewModel(
    IReadOnlyList<TalkTopicListItemModel> TalkTopics,
    IReadOnlyList<string> CefrLevels,
    IReadOnlyList<string> Categories,
    IReadOnlyList<string> TopicKeys,
    IReadOnlyList<string> ContentTypes,
    IReadOnlyList<string> SpeakingGoals,
    string? SelectedCefrLevel,
    string? SelectedCategory,
    string? SelectedTopicKey,
    string? SelectedContentType,
    string? SelectedSpeakingGoal);

public sealed record TalkTopicDetailPageViewModel(
    TalkTopicDetailModel TalkTopic,
    string PrimaryMeaningLanguageCode,
    string? SecondaryMeaningLanguageCode);
