using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Catalog.Application.Abstractions;

public interface IGrammarTopicQueryService
{
    Task<IReadOnlyList<GrammarTopicListItemModel>> GetPublishedGrammarTopicsAsync(
        GrammarTopicListFilterModel filter,
        CancellationToken cancellationToken);

    Task<GrammarTopicDetailModel?> GetPublishedGrammarTopicBySlugAsync(
        string slug,
        string primaryMeaningLanguageCode,
        CancellationToken cancellationToken);
}
