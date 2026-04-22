using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace DarwinLingua.Web.Services;

public sealed record WebActorContext(
    string ActorId,
    bool IsAuthenticated,
    string? UserId,
    string? Email);

public interface IWebActorContextAccessor
{
    WebActorContext GetCurrentActor();
}

internal sealed class WebActorContextAccessor(IHttpContextAccessor httpContextAccessor) : IWebActorContextAccessor
{
    private const string GuestCookieName = "darwinlingua_guest";

    public WebActorContext GetCurrentActor()
    {
        HttpContext httpContext = httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("No active HttpContext is available.");

        ClaimsPrincipal user = httpContext.User;
        string? userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (user.Identity?.IsAuthenticated == true && !string.IsNullOrWhiteSpace(userId))
        {
            return new WebActorContext(
                userId,
                true,
                userId,
                user.FindFirstValue(ClaimTypes.Email));
        }

        string? guestId = httpContext.Request.Cookies[GuestCookieName];

        if (string.IsNullOrWhiteSpace(guestId))
        {
            guestId = Guid.NewGuid().ToString("N");
            httpContext.Response.Cookies.Append(
                GuestCookieName,
                guestId,
                new CookieOptions
                {
                    IsEssential = true,
                    HttpOnly = true,
                    SameSite = SameSiteMode.Lax,
                    Secure = httpContext.Request.IsHttps,
                    Expires = DateTimeOffset.UtcNow.AddDays(180)
                });
        }

        return new WebActorContext($"guest:{guestId}", false, null, null);
    }
}
