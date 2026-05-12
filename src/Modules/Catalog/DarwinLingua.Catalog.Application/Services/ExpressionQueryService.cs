using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Catalog.Application.Services;

internal sealed class ExpressionQueryService(IExpressionRepository repository) : IExpressionQueryService
{
    public Task<IReadOnlyList<ExpressionListItemModel>> GetPublishedExpressionsAsync(
        ExpressionListFilterModel filter,
        CancellationToken cancellationToken) =>
        repository.GetPublishedExpressionsAsync(filter, cancellationToken);

    public Task<ExpressionDetailModel?> GetPublishedExpressionBySlugAsync(
        string slug,
        string primaryMeaningLanguageCode,
        CancellationToken cancellationToken) =>
        repository.GetPublishedExpressionBySlugAsync(slug, primaryMeaningLanguageCode, cancellationToken);
}
