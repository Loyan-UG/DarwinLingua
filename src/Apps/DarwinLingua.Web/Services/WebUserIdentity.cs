using System.Security.Claims;

namespace DarwinLingua.Web.Services;

internal static class WebUserIdentity
{
    public static string? TryGetEmail(ClaimsPrincipal user)
    {
        string? candidate = user.FindFirstValue(ClaimTypes.Email)
            ?? user.Identity?.Name;

        return !string.IsNullOrWhiteSpace(candidate) && candidate.Contains('@', StringComparison.Ordinal)
            ? candidate
            : null;
    }

    public static string GetRequiredEmail(ClaimsPrincipal user, string message) =>
        TryGetEmail(user) ?? throw new InvalidOperationException(message);
}
