using DarwinLingua.SharedKernel.Exceptions;
using DarwinLingua.SharedKernel.Lexicon;

namespace DarwinLingua.Catalog.Domain.Entities;

/// <summary>
/// Represents one lexical-role variant attached to a word entry.
/// </summary>
public sealed class WordLexicalForm
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WordLexicalForm"/> class for EF Core materialization.
    /// </summary>
    private WordLexicalForm()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WordLexicalForm"/> class.
    /// </summary>
    internal WordLexicalForm(
        Guid id,
        Guid wordEntryId,
        PartOfSpeech partOfSpeech,
        int sortOrder,
        bool isPrimary,
        DateTime createdAtUtc,
        string? article = null,
        string? pluralForm = null,
        string? infinitiveForm = null)
    {
        if (id == Guid.Empty)
        {
            throw new DomainRuleException("Word lexical-form identifier cannot be empty.");
        }

        if (wordEntryId == Guid.Empty)
        {
            throw new DomainRuleException("Word entry identifier cannot be empty for a lexical form.");
        }

        if (sortOrder <= 0)
        {
            throw new DomainRuleException("Lexical form sort order must be greater than zero.");
        }

        Id = id;
        WordEntryId = wordEntryId;
        PartOfSpeech = partOfSpeech;
        SortOrder = sortOrder;
        IsPrimary = isPrimary;
        Article = NormalizeOptionalText(article);
        PluralForm = NormalizeOptionalText(pluralForm);
        InfinitiveForm = NormalizeOptionalText(infinitiveForm);
        CreatedAtUtc = NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
        UpdatedAtUtc = CreatedAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid WordEntryId { get; private set; }

    public PartOfSpeech PartOfSpeech { get; private set; }

    public string? Article { get; private set; }

    public string? PluralForm { get; private set; }

    public string? InfinitiveForm { get; private set; }

    public int SortOrder { get; private set; }

    public bool IsPrimary { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    internal void SetPrimary(bool isPrimary, DateTime updatedAtUtc)
    {
        IsPrimary = isPrimary;
        UpdatedAtUtc = NormalizeUtc(updatedAtUtc, nameof(updatedAtUtc));
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static DateTime NormalizeUtc(DateTime value, string parameterName)
    {
        if (value == default)
        {
            throw new DomainRuleException($"{parameterName} cannot be empty.");
        }

        return value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime();
    }
}
