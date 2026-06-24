using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Catalog.Application.Services;

internal sealed class ExpressionQueryService(IExpressionRepository repository) : IExpressionQueryService
{
    public Task<IReadOnlyList<ExpressionListItemModel>> GetPublishedExpressionsAsync(
        ExpressionListFilterModel filter,
        string targetLearningLanguageCode,
        CancellationToken cancellationToken) =>
        repository.GetPublishedExpressionsAsync(filter, targetLearningLanguageCode, cancellationToken);

    public Task<ExpressionDetailModel?> GetPublishedExpressionBySlugAsync(
        string slug,
        string targetLearningLanguageCode,
        string primaryMeaningLanguageCode,
        CancellationToken cancellationToken) =>
        repository.GetPublishedExpressionBySlugAsync(slug, targetLearningLanguageCode, primaryMeaningLanguageCode, cancellationToken);

    public Task<ExpressionDetailModel?> GetPublishedExpressionBySlugAsync(
        string slug,
        string targetLearningLanguageCode,
        string primaryMeaningLanguageCode,
        bool includeSensitiveEducationalLanguage,
        CancellationToken cancellationToken) =>
        repository.GetPublishedExpressionBySlugAsync(slug, targetLearningLanguageCode, primaryMeaningLanguageCode, includeSensitiveEducationalLanguage, cancellationToken);
}
