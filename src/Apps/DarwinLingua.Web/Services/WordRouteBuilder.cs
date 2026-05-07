using System.Globalization;
using System.Text;

namespace DarwinLingua.Web.Services;

public static class WordRouteBuilder
{
    public static string CreateSlug(string? lemma)
    {
        if (string.IsNullOrWhiteSpace(lemma))
        {
            return "word";
        }

        string normalized = lemma.Trim()
            .Replace("ä", "ae", StringComparison.OrdinalIgnoreCase)
            .Replace("ö", "oe", StringComparison.OrdinalIgnoreCase)
            .Replace("ü", "ue", StringComparison.OrdinalIgnoreCase)
            .Replace("ß", "ss", StringComparison.OrdinalIgnoreCase)
            .Normalize(NormalizationForm.FormD);

        StringBuilder builder = new(normalized.Length);
        bool hasSeparator = false;

        foreach (char character in normalized)
        {
            UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(character);
            if (category == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

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

    public static string CreateRouteSlug(string? lemma, Guid publicId) =>
        $"{CreateSlug(lemma)}-{publicId:D}";
}
