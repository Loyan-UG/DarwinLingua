using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Catalog.Application.Abstractions;

public interface ITalkTopicQueryService
{
    Task<IReadOnlyList<TalkTopicListItemModel>> GetPublishedTalkTopicsAsync(
        TalkTopicListFilterModel filter,
        CancellationToken cancellationToken);

    Task<TalkTopicDetailModel?> GetPublishedTalkTopicBySlugAsync(
        string slug,
        string primaryMeaningLanguageCode,
        string? secondaryMeaningLanguageCode,
        CancellationToken cancellationToken);
}
