using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.SharedKernel.Content;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.WebApi.Services;

public interface IWebsiteCatalogQueryService
{
    Task<IReadOnlyList<TopicListItemModel>> GetTopicsAsync(string uiLanguageCode, CancellationToken cancellationToken);

    Task<IReadOnlyList<WordCollectionListItemModel>> GetCollectionsAsync(
        string meaningLanguageCode,
        CancellationToken cancellationToken);

    Task<WordCollectionDetailModel?> GetCollectionBySlugAsync(
        string slug,
        string meaningLanguageCode,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<WordListItemModel>> GetWordsByTopicPageAsync(
        string topicKey,
        string meaningLanguageCode,
        int skip,
        int take,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<WordListItemModel>> GetWordsByCefrPageAsync(
        string cefrLevel,
        string meaningLanguageCode,
        int skip,
        int take,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<WordListItemModel>> SearchWordsAsync(
        string query,
        string meaningLanguageCode,
        CancellationToken cancellationToken);

    Task<WordDetailModel?> GetWordDetailsAsync(
        Guid publicId,
        string primaryMeaningLanguageCode,
        string? secondaryMeaningLanguageCode,
        string uiLanguageCode,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<WordListItemModel>> GetWordsByIdsAsync(
        IReadOnlyCollection<Guid> wordIds,
        string meaningLanguageCode,
        CancellationToken cancellationToken);
}

internal sealed class WebsiteCatalogQueryService(
    ITopicQueryService topicQueryService,
    IWordCollectionQueryService wordCollectionQueryService,
    IWordQueryService wordQueryService,
    IWordDetailQueryService wordDetailQueryService,
    IDbContextFactory<DarwinLinguaDbContext> dbContextFactory) : IWebsiteCatalogQueryService
{
    public Task<IReadOnlyList<TopicListItemModel>> GetTopicsAsync(string uiLanguageCode, CancellationToken cancellationToken) =>
        topicQueryService.GetTopicsAsync(uiLanguageCode, cancellationToken);

    public Task<IReadOnlyList<WordCollectionListItemModel>> GetCollectionsAsync(
        string meaningLanguageCode,
        CancellationToken cancellationToken) =>
        wordCollectionQueryService.GetPublishedCollectionsAsync(meaningLanguageCode, cancellationToken);

    public Task<WordCollectionDetailModel?> GetCollectionBySlugAsync(
        string slug,
        string meaningLanguageCode,
        CancellationToken cancellationToken) =>
        wordCollectionQueryService.GetPublishedCollectionBySlugAsync(slug, meaningLanguageCode, cancellationToken);

    public Task<IReadOnlyList<WordListItemModel>> GetWordsByTopicPageAsync(
        string topicKey,
        string meaningLanguageCode,
        int skip,
        int take,
        CancellationToken cancellationToken) =>
        wordQueryService.GetWordsByTopicPageAsync(topicKey, meaningLanguageCode, skip, take, cancellationToken);

    public Task<IReadOnlyList<WordListItemModel>> GetWordsByCefrPageAsync(
        string cefrLevel,
        string meaningLanguageCode,
        int skip,
        int take,
        CancellationToken cancellationToken) =>
        wordQueryService.GetWordsByCefrPageAsync(cefrLevel, meaningLanguageCode, skip, take, cancellationToken);

    public Task<IReadOnlyList<WordListItemModel>> SearchWordsAsync(
        string query,
        string meaningLanguageCode,
        CancellationToken cancellationToken) =>
        wordQueryService.SearchWordsAsync(query, meaningLanguageCode, cancellationToken);

    public Task<WordDetailModel?> GetWordDetailsAsync(
        Guid publicId,
        string primaryMeaningLanguageCode,
        string? secondaryMeaningLanguageCode,
        string uiLanguageCode,
        CancellationToken cancellationToken) =>
        wordDetailQueryService.GetWordDetailsAsync(
            publicId,
            primaryMeaningLanguageCode,
            secondaryMeaningLanguageCode,
            uiLanguageCode,
            cancellationToken);

    public async Task<IReadOnlyList<WordListItemModel>> GetWordsByIdsAsync(
        IReadOnlyCollection<Guid> wordIds,
        string meaningLanguageCode,
        CancellationToken cancellationToken)
    {
        if (wordIds.Count == 0)
        {
            return [];
        }

        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        return await dbContext.WordEntries
            .AsNoTracking()
            .Where(word => wordIds.Contains(word.PublicId) && word.PublicationStatus == PublicationStatus.Active)
            .Select(word => new WordListItemModel(
                word.PublicId,
                word.Lemma,
                word.Article,
                word.PluralForm,
                word.PartOfSpeech.ToString(),
                word.PrimaryCefrLevel.ToString(),
                word.Senses
                    .OrderByDescending(sense => sense.IsPrimarySense)
                    .ThenBy(sense => sense.SenseOrder)
                    .SelectMany(sense => sense.Translations)
                    .Where(translation => translation.LanguageCode.Value == meaningLanguageCode)
                    .OrderByDescending(translation => translation.IsPrimary)
                    .Select(translation => translation.TranslationText)
                    .FirstOrDefault()))
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
