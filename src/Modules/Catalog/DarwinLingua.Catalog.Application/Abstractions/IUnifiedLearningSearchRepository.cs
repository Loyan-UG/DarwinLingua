using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Catalog.Application.Abstractions;

public interface IUnifiedLearningSearchRepository
{
    Task<IReadOnlyList<UnifiedLearningSearchResultModel>> SearchAsync(UnifiedLearningSearchFilterModel filter, CancellationToken cancellationToken);
}
