using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Catalog.Application.Abstractions;

public interface IExpressionQueryService
{
    Task<IReadOnlyList<ExpressionListItemModel>> GetPublishedExpressionsAsync(
        ExpressionListFilterModel filter,
        CancellationToken cancellationToken);

    Task<ExpressionDetailModel?> GetPublishedExpressionBySlugAsync(
        string slug,
        string primaryMeaningLanguageCode,
        CancellationToken cancellationToken);

    Task<ExpressionDetailModel?> GetPublishedExpressionBySlugAsync(
        string slug,
        string primaryMeaningLanguageCode,
        bool includeSensitiveEducationalLanguage,
        CancellationToken cancellationToken);
}
