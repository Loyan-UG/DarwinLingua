using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Catalog.Application.Services;

internal sealed class TalkTopicQueryService(ITalkTopicRepository talkTopicRepository) : ITalkTopicQueryService
{
    public Task<IReadOnlyList<TalkTopicListItemModel>> GetPublishedTalkTopicsAsync(
        TalkTopicListFilterModel filter,
        CancellationToken cancellationToken) =>
        talkTopicRepository.GetPublishedTalkTopicsAsync(filter, cancellationToken);

    public Task<TalkTopicDetailModel?> GetPublishedTalkTopicBySlugAsync(
        string slug,
        string primaryMeaningLanguageCode,
        string? secondaryMeaningLanguageCode,
        CancellationToken cancellationToken) =>
        talkTopicRepository.GetPublishedTalkTopicBySlugAsync(
            slug,
            primaryMeaningLanguageCode,
            secondaryMeaningLanguageCode,
            cancellationToken);
}
