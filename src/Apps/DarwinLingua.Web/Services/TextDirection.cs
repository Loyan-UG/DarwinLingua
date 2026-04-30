namespace DarwinLingua.Web.Services;

public static class TextDirection
{
    private static readonly HashSet<string> RtlLanguageCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        "ar",
        "arc",
        "ckb",
        "dv",
        "fa",
        "he",
        "ku",
        "ps",
        "sd",
        "ug",
        "ur",
        "yi"
    };

    public static string FromLanguageCode(string? languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
        {
            return "ltr";
        }

        string normalizedCode = languageCode.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)[0];
        return RtlLanguageCodes.Contains(normalizedCode) ? "rtl" : "ltr";
    }
}
