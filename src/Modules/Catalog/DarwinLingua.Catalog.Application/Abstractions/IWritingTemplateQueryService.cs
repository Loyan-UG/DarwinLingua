using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Catalog.Application.Abstractions;

public interface IWritingTemplateQueryService
{
    Task<IReadOnlyList<WritingTemplateListItemModel>> GetPublishedWritingTemplatesAsync(WritingTemplateListFilterModel filter, CancellationToken cancellationToken);

    Task<WritingTemplateDetailModel?> GetPublishedWritingTemplateBySlugAsync(string slug, CancellationToken cancellationToken);
}
