namespace DarwinLingua.Web.Services;

internal static class WebRouteInput
{
    private const int MaxReturnUrlLength = 2048;

    public static string? NormalizeSlug(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        string trimmed = value.Trim();
        return IsSlug(trimmed) ? trimmed : null;
    }

    public static string? NormalizeLocalReturnUrl(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        string trimmed = value.Trim();
        return trimmed.Length <= MaxReturnUrlLength ? trimmed : null;
    }

    private static bool IsSlug(string value)
    {
        if (value.Length > 128)
        {
            return false;
        }

        bool previousWasDash = true;
        foreach (char character in value)
        {
            bool isAlphaNumeric = (character >= 'a' && character <= 'z') ||
                (character >= '0' && character <= '9');
            if (isAlphaNumeric)
            {
                previousWasDash = false;
                continue;
            }

            if (character != '-' || previousWasDash)
            {
                return false;
            }

            previousWasDash = true;
        }

        return !previousWasDash;
    }
}
