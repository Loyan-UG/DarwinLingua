using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Catalog.Application.Services;

internal sealed class WritingTemplateQueryService(IWritingTemplateRepository repository) : IWritingTemplateQueryService
{
    public Task<IReadOnlyList<WritingTemplateListItemModel>> GetPublishedWritingTemplatesAsync(WritingTemplateListFilterModel filter, CancellationToken cancellationToken) =>
        repository.GetPublishedWritingTemplatesAsync(filter, cancellationToken);

    public Task<WritingTemplateDetailModel?> GetPublishedWritingTemplateBySlugAsync(string slug, CancellationToken cancellationToken) =>
        repository.GetPublishedWritingTemplateBySlugAsync(slug, cancellationToken);
}
