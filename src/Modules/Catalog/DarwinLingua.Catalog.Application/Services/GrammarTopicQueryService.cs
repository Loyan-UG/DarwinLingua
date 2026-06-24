using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Catalog.Application.Services;

internal sealed class GrammarTopicQueryService(IGrammarTopicRepository repository) : IGrammarTopicQueryService
{
    public Task<IReadOnlyList<GrammarTopicListItemModel>> GetPublishedGrammarTopicsAsync(
        GrammarTopicListFilterModel filter,
        string targetLearningLanguageCode,
        CancellationToken cancellationToken) =>
        repository.GetPublishedGrammarTopicsAsync(filter, targetLearningLanguageCode, cancellationToken);

    public Task<GrammarTopicDetailModel?> GetPublishedGrammarTopicBySlugAsync(
        string slug,
        string targetLearningLanguageCode,
        string primaryMeaningLanguageCode,
        CancellationToken cancellationToken) =>
        repository.GetPublishedGrammarTopicBySlugAsync(slug, targetLearningLanguageCode, primaryMeaningLanguageCode, cancellationToken);
}
