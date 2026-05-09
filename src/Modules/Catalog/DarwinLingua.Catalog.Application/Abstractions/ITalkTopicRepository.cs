using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Catalog.Application.Abstractions;

public interface ITalkTopicRepository
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
