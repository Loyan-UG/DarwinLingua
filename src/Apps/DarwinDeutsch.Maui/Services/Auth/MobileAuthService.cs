using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace DarwinDeutsch.Maui.Services.Auth;

internal sealed class MobileAuthService(
    HttpClient httpClient,
    IMobileAuthSessionStore sessionStore) : IMobileAuthService
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public async Task<MobileAuthSession?> GetCurrentSessionAsync(CancellationToken cancellationToken)
    {
        MobileAuthSession? session = await sessionStore.GetAsync(cancellationToken).ConfigureAwait(false);
        if (session is null)
        {
            return null;
        }

        if (session.IsExpired(DateTimeOffset.UtcNow) && !string.IsNullOrWhiteSpace(session.RefreshToken))
        {
            session = await RefreshCurrentSessionAsync(cancellationToken).ConfigureAwait(false);
            if (session is null)
            {
                return null;
            }
        }

        AuthenticatedUserResponse user = await GetCurrentUserAsync(session.AccessToken, cancellationToken).ConfigureAwait(false);
        MobileAuthSession hydrated = session with
        {
            UserId = user.UserId,
            Email = user.Email ?? session.Email,
            Roles = user.Roles,
            EntitlementTier = user.EntitlementTier,
            TrialEndsAtUtc = user.TrialEndsAtUtc,
            PremiumEndsAtUtc = user.PremiumEndsAtUtc,
            EnabledFeatures = user.EnabledFeatures,
        };

        await sessionStore.SaveAsync(hydrated, cancellationToken).ConfigureAwait(false);
        return hydrated;
    }

    public async Task<MobileAuthSession> RegisterAsync(string email, string password, CancellationToken cancellationToken)
    {
        RegisterRequest request = new(email.Trim(), password);
        using HttpResponseMessage response = await httpClient
            .PostAsJsonAsync("/api/auth/register", request, SerializerOptions, cancellationToken)
            .ConfigureAwait(false);

        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
        return await SignInAsync(email, password, cancellationToken).ConfigureAwait(false);
    }

    public async Task<MobileAuthSession> SignInAsync(string email, string password, CancellationToken cancellationToken)
    {
        LoginRequest request = new(email.Trim(), password);
        using HttpResponseMessage response = await httpClient
            .PostAsJsonAsync("/api/auth/login", request, SerializerOptions, cancellationToken)
            .ConfigureAwait(false);

        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
        AccessTokenResponse? accessToken = await response.Content
            .ReadFromJsonAsync<AccessTokenResponse>(SerializerOptions, cancellationToken)
            .ConfigureAwait(false);

        if (accessToken is null || string.IsNullOrWhiteSpace(accessToken.AccessToken))
        {
            throw new InvalidOperationException("The authentication response did not include a usable access token.");
        }

        AuthenticatedUserResponse user = await GetCurrentUserAsync(accessToken.AccessToken, cancellationToken).ConfigureAwait(false);
        MobileAuthSession session = CreateSession(user, accessToken);
        await sessionStore.SaveAsync(session, cancellationToken).ConfigureAwait(false);
        return session;
    }

    public async Task<MobileAuthSession?> RefreshCurrentSessionAsync(CancellationToken cancellationToken)
    {
        MobileAuthSession? current = await sessionStore.GetAsync(cancellationToken).ConfigureAwait(false);
        if (current is null || string.IsNullOrWhiteSpace(current.RefreshToken))
        {
            return null;
        }

        RefreshRequest request = new(current.RefreshToken);
        using HttpResponseMessage response = await httpClient
            .PostAsJsonAsync("/api/auth/refresh", request, SerializerOptions, cancellationToken)
            .ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            await sessionStore.ClearAsync(cancellationToken).ConfigureAwait(false);
            return null;
        }

        AccessTokenResponse? accessToken = await response.Content
            .ReadFromJsonAsync<AccessTokenResponse>(SerializerOptions, cancellationToken)
            .ConfigureAwait(false);

        if (accessToken is null || string.IsNullOrWhiteSpace(accessToken.AccessToken))
        {
            await sessionStore.ClearAsync(cancellationToken).ConfigureAwait(false);
            return null;
        }

        AuthenticatedUserResponse user = await GetCurrentUserAsync(accessToken.AccessToken, cancellationToken).ConfigureAwait(false);
        MobileAuthSession session = CreateSession(user, accessToken);
        await sessionStore.SaveAsync(session, cancellationToken).ConfigureAwait(false);
        return session;
    }

    public Task SignOutAsync(CancellationToken cancellationToken)
    {
        return sessionStore.ClearAsync(cancellationToken);
    }

    private async Task<AuthenticatedUserResponse> GetCurrentUserAsync(string accessToken, CancellationToken cancellationToken)
    {
        using HttpRequestMessage request = new(HttpMethod.Get, "/api/auth/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);

        AuthenticatedUserResponse? user = await response.Content
            .ReadFromJsonAsync<AuthenticatedUserResponse>(SerializerOptions, cancellationToken)
            .ConfigureAwait(false);

        return user ?? throw new InvalidOperationException("The authenticated user response was empty.");
    }

    private static MobileAuthSession CreateSession(AuthenticatedUserResponse user, AccessTokenResponse accessToken)
    {
        return new MobileAuthSession(
            user.UserId,
            user.Email ?? string.Empty,
            accessToken.AccessToken,
            accessToken.RefreshToken,
            DateTimeOffset.UtcNow.AddSeconds(accessToken.ExpiresIn),
            user.Roles,
            user.EntitlementTier,
            user.TrialEndsAtUtc,
            user.PremiumEndsAtUtc,
            user.EnabledFeatures);
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        string body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(body))
        {
            response.EnsureSuccessStatusCode();
        }

        throw new InvalidOperationException(body);
    }

    private sealed record RegisterRequest(string Email, string Password);

    private sealed record LoginRequest(string Email, string Password);

    private sealed record RefreshRequest(string RefreshToken);

    private sealed record AccessTokenResponse(string TokenType, string AccessToken, int ExpiresIn, string? RefreshToken);

    private sealed record AuthenticatedUserResponse(
        string UserId,
        string? Email,
        bool IsAuthenticated,
        IReadOnlyList<string> Roles,
        string EntitlementTier,
        DateTimeOffset? TrialEndsAtUtc,
        DateTimeOffset? PremiumEndsAtUtc,
        IReadOnlyList<string> EnabledFeatures);
}
