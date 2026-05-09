using System.Text;

namespace DarwinLingua.SharedKernel.Lexicon;

/// <summary>
/// Creates stable URL slugs for lexical lemmas.
/// </summary>
public static class LemmaUrlSlug
{
    public static string FromLemma(string? lemma)
    {
        if (string.IsNullOrWhiteSpace(lemma))
        {
            return "word";
        }

        string normalized = lemma.Trim();
        StringBuilder builder = new(normalized.Length);
        bool hasSeparator = false;

        foreach (char character in normalized)
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(char.ToLowerInvariant(character));
                hasSeparator = false;
                continue;
            }

            if (!hasSeparator && builder.Length > 0)
            {
                builder.Append('-');
                hasSeparator = true;
            }
        }

        string slug = builder.ToString().Trim('-');
        return string.IsNullOrWhiteSpace(slug) ? "word" : slug;
    }

    public static string ToNormalizedLemmaCandidate(string slug) =>
        string.Join(' ', slug.Trim().Split('-', StringSplitOptions.RemoveEmptyEntries)).ToLowerInvariant();
}
