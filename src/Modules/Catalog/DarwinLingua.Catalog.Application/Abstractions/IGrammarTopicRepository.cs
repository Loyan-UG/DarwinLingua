using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Catalog.Application.Abstractions;

public interface IGrammarTopicRepository
{
    Task<IReadOnlyList<GrammarTopicListItemModel>> GetPublishedGrammarTopicsAsync(
        GrammarTopicListFilterModel filter,
        string targetLearningLanguageCode,
        CancellationToken cancellationToken);

    Task<GrammarTopicDetailModel?> GetPublishedGrammarTopicBySlugAsync(
        string slug,
        string targetLearningLanguageCode,
        string primaryMeaningLanguageCode,
        CancellationToken cancellationToken);
}
