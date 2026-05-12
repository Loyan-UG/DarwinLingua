using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Catalog.Application.Services;

internal sealed class UnifiedLearningSearchService(IUnifiedLearningSearchRepository searchRepository) : IUnifiedLearningSearchService
{
    public Task<IReadOnlyList<UnifiedLearningSearchResultModel>> SearchAsync(UnifiedLearningSearchFilterModel filter, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(filter);
        return string.IsNullOrWhiteSpace(filter.Query)
            ? Task.FromResult<IReadOnlyList<UnifiedLearningSearchResultModel>>([])
            : searchRepository.SearchAsync(filter, cancellationToken);
    }
}
