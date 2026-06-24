using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Catalog.Application.Abstractions;

public interface IExpressionRepository
{
    Task<IReadOnlyList<ExpressionListItemModel>> GetPublishedExpressionsAsync(
        ExpressionListFilterModel filter,
        string targetLearningLanguageCode,
        CancellationToken cancellationToken);

    Task<ExpressionDetailModel?> GetPublishedExpressionBySlugAsync(
        string slug,
        string targetLearningLanguageCode,
        string primaryMeaningLanguageCode,
        CancellationToken cancellationToken);

    Task<ExpressionDetailModel?> GetPublishedExpressionBySlugAsync(
        string slug,
        string targetLearningLanguageCode,
        string primaryMeaningLanguageCode,
        bool includeSensitiveEducationalLanguage,
        CancellationToken cancellationToken);
}
