using DarwinLingua.SharedKernel.Lexicon;

namespace DarwinLingua.Web.Services;

public static class WordRouteBuilder
{
    public static string CreateSlug(string? lemma) => LemmaUrlSlug.FromLemma(lemma);

    public static string CreateRouteSlug(string? lemma) => CreateSlug(lemma);
}
