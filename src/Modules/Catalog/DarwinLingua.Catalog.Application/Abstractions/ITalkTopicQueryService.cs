using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.SharedKernel.Globalization;

namespace DarwinLingua.Catalog.Application.Abstractions;

public interface ITalkTopicQueryService
{
    Task<IReadOnlyList<TalkTopicListItemModel>> GetPublishedTalkTopicsAsync(
        TalkTopicListFilterModel filter,
        CancellationToken cancellationToken) =>
        GetPublishedTalkTopicsAsync(filter, ContentLanguageRequirements.DefaultTargetLearningLanguageCode, cancellationToken);

    Task<IReadOnlyList<TalkTopicListItemModel>> GetPublishedTalkTopicsAsync(
        TalkTopicListFilterModel filter,
        string targetLearningLanguageCode,
        CancellationToken cancellationToken);

    Task<TalkTopicDetailModel?> GetPublishedTalkTopicBySlugAsync(
        string slug,
        string primaryMeaningLanguageCode,
        string? secondaryMeaningLanguageCode,
        CancellationToken cancellationToken) =>
        GetPublishedTalkTopicBySlugAsync(slug, ContentLanguageRequirements.DefaultTargetLearningLanguageCode, primaryMeaningLanguageCode, secondaryMeaningLanguageCode, cancellationToken);

    Task<TalkTopicDetailModel?> GetPublishedTalkTopicBySlugAsync(
        string slug,
        string targetLearningLanguageCode,
        string primaryMeaningLanguageCode,
        string? secondaryMeaningLanguageCode,
        CancellationToken cancellationToken);
}
