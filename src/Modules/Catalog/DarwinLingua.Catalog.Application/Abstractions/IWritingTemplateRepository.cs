using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.SharedKernel.Globalization;

namespace DarwinLingua.Catalog.Application.Abstractions;

public interface IWritingTemplateRepository
{
    Task<IReadOnlyList<WritingTemplateListItemModel>> GetPublishedWritingTemplatesAsync(WritingTemplateListFilterModel filter, string? primaryMeaningLanguageCode, CancellationToken cancellationToken) =>
        GetPublishedWritingTemplatesAsync(filter, ContentLanguageRequirements.DefaultTargetLearningLanguageCode, primaryMeaningLanguageCode, cancellationToken);

    Task<IReadOnlyList<WritingTemplateListItemModel>> GetPublishedWritingTemplatesAsync(WritingTemplateListFilterModel filter, string targetLearningLanguageCode, string? primaryMeaningLanguageCode, CancellationToken cancellationToken);

    Task<WritingTemplateDetailModel?> GetPublishedWritingTemplateBySlugAsync(string slug, string? primaryMeaningLanguageCode, CancellationToken cancellationToken) =>
        GetPublishedWritingTemplateBySlugAsync(slug, ContentLanguageRequirements.DefaultTargetLearningLanguageCode, primaryMeaningLanguageCode, cancellationToken);

    Task<WritingTemplateDetailModel?> GetPublishedWritingTemplateBySlugAsync(string slug, string targetLearningLanguageCode, string? primaryMeaningLanguageCode, CancellationToken cancellationToken);
}
