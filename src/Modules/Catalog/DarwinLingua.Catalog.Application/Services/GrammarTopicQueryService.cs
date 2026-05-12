using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Catalog.Application.Services;

internal sealed class GrammarTopicQueryService(IGrammarTopicRepository repository) : IGrammarTopicQueryService
{
    public Task<IReadOnlyList<GrammarTopicListItemModel>> GetPublishedGrammarTopicsAsync(
        GrammarTopicListFilterModel filter,
        CancellationToken cancellationToken) =>
        repository.GetPublishedGrammarTopicsAsync(filter, cancellationToken);

    public Task<GrammarTopicDetailModel?> GetPublishedGrammarTopicBySlugAsync(
        string slug,
        string primaryMeaningLanguageCode,
        CancellationToken cancellationToken) =>
        repository.GetPublishedGrammarTopicBySlugAsync(slug, primaryMeaningLanguageCode, cancellationToken);
}
