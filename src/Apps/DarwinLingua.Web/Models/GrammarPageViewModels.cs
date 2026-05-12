using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Web.Models;

public sealed record GrammarIndexPageViewModel(
    IReadOnlyList<GrammarTopicListItemModel> GrammarTopics,
    IReadOnlyList<string> CefrLevels,
    IReadOnlyList<string> GrammarCategories,
    IReadOnlyList<string> TopicKeys,
    string? SelectedCefrLevel,
    string? SelectedGrammarCategory,
    string? SelectedTopicKey,
    string? Query);

public sealed record GrammarDetailPageViewModel(
    GrammarTopicDetailModel GrammarTopic,
    string PrimaryMeaningLanguageCode);
