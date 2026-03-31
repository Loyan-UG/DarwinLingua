using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.SharedKernel.Globalization;

namespace DarwinLingua.Catalog.Application.Abstractions;

/// <summary>
/// Defines persistence access for catalog topic aggregates.
/// </summary>
public interface ITopicRepository
{
    /// <summary>
    /// Returns the stored topics with their localizations.
    /// </summary>
    Task<IReadOnlyList<Topic>> GetAllAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Returns localized display names for the requested topic identifiers.
    /// </summary>
    Task<IReadOnlyDictionary<Guid, string>> GetDisplayNamesByIdsAsync(
        IReadOnlyCollection<Guid> topicIds,
        LanguageCode preferredLanguageCode,
        LanguageCode fallbackLanguageCode,
        CancellationToken cancellationToken);
}
