using System.Text.Json;

namespace DarwinDeutsch.Maui.Services.Auth;

internal sealed class SecureStorageMobileAuthSessionStore : IMobileAuthSessionStore
{
    private const string SessionKey = "mobile-auth-session";
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public async Task<MobileAuthSession?> GetAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        string? rawValue = await SecureStorage.Default.GetAsync(SessionKey).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<MobileAuthSession>(rawValue, SerializerOptions);
        }
        catch
        {
            await ClearAsync(cancellationToken).ConfigureAwait(false);
            return null;
        }
    }

    public Task SaveAsync(MobileAuthSession session, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(session);
        cancellationToken.ThrowIfCancellationRequested();
        string payload = JsonSerializer.Serialize(session, SerializerOptions);
        return SecureStorage.Default.SetAsync(SessionKey, payload);
    }

    public Task ClearAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        SecureStorage.Default.Remove(SessionKey);
        return Task.CompletedTask;
    }
}
