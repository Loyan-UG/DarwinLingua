using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.SharedKernel.Globalization;

namespace DarwinLingua.Catalog.Application.Abstractions;

public interface IGrammarTopicQueryService
{
    Task<IReadOnlyList<GrammarTopicListItemModel>> GetPublishedGrammarTopicsAsync(
        GrammarTopicListFilterModel filter,
        CancellationToken cancellationToken) =>
        GetPublishedGrammarTopicsAsync(
            filter,
            ContentLanguageRequirements.DefaultTargetLearningLanguageCode,
            cancellationToken);

    Task<IReadOnlyList<GrammarTopicListItemModel>> GetPublishedGrammarTopicsAsync(
        GrammarTopicListFilterModel filter,
        string targetLearningLanguageCode,
        CancellationToken cancellationToken);

    Task<GrammarTopicDetailModel?> GetPublishedGrammarTopicBySlugAsync(
        string slug,
        string primaryMeaningLanguageCode,
        CancellationToken cancellationToken) =>
        GetPublishedGrammarTopicBySlugAsync(
            slug,
            ContentLanguageRequirements.DefaultTargetLearningLanguageCode,
            primaryMeaningLanguageCode,
            cancellationToken);

    Task<GrammarTopicDetailModel?> GetPublishedGrammarTopicBySlugAsync(
        string slug,
        string targetLearningLanguageCode,
        string primaryMeaningLanguageCode,
        CancellationToken cancellationToken);
}
