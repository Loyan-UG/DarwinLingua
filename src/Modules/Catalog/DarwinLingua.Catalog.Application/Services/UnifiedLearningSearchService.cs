using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.SharedKernel.Exceptions;

namespace DarwinLingua.Catalog.Application.Services;

internal sealed class UnifiedLearningSearchService(IUnifiedLearningSearchRepository searchRepository) : IUnifiedLearningSearchService
{
    private const int MinimumQueryLength = 2;
    private const int MaximumQueryLength = 100;
    private const int MaximumFilterLength = 64;
    private static readonly HashSet<string> SupportedResultTypes = new(StringComparer.Ordinal)
    {
        "word",
        "grammar",
        "expression",
        "dialogue",
        "talk-topic",
        "exercise",
        "course-lesson",
        "exam-prep",
        "writing-template",
        "cultural-note",
        "event",
        "organizer",
    };

    public Task<IReadOnlyList<UnifiedLearningSearchResultModel>> SearchAsync(UnifiedLearningSearchFilterModel filter, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(filter);
        UnifiedLearningSearchFilterModel normalizedFilter = NormalizeFilter(filter);
        return normalizedFilter.Query is null
            ? Task.FromResult<IReadOnlyList<UnifiedLearningSearchResultModel>>([])
            : searchRepository.SearchAsync(normalizedFilter, cancellationToken);
    }

    private static UnifiedLearningSearchFilterModel NormalizeFilter(UnifiedLearningSearchFilterModel filter)
    {
        string? query = NormalizeSearchQuery(filter.Query);
        string? resultType = NormalizeResultType(filter.ResultType);
        string? cefrLevel = NormalizeOptionalFilter(filter.CefrLevel, nameof(filter.CefrLevel));
        string? category = NormalizeOptionalFilter(filter.Category, nameof(filter.Category));
        string? topicKey = NormalizeOptionalFilter(filter.TopicKey, nameof(filter.TopicKey));
        return new UnifiedLearningSearchFilterModel(query, cefrLevel, resultType, category, topicKey);
    }

    private static string? NormalizeSearchQuery(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        string trimmed = value.Trim();
        if (trimmed.Length > MaximumQueryLength)
        {
            throw new DomainRuleException($"Search query must be {MaximumQueryLength} characters or fewer.");
        }

        return trimmed.Length < MinimumQueryLength ? null : trimmed;
    }

    private static string? NormalizeResultType(string? value)
    {
        string? normalized = NormalizeOptionalFilter(value, "resultType");
        if (normalized is null)
        {
            return null;
        }

        if (!SupportedResultTypes.Contains(normalized))
        {
            throw new DomainRuleException($"Search result type '{normalized}' is not supported.");
        }

        return normalized;
    }

    private static string? NormalizeOptionalFilter(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        string normalized = value.Trim().ToLowerInvariant();
        if (normalized.Length > MaximumFilterLength)
        {
            throw new DomainRuleException($"{parameterName} must be {MaximumFilterLength} characters or fewer.");
        }

        return normalized;
    }
}
