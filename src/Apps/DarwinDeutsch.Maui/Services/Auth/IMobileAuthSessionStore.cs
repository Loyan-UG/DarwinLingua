namespace DarwinDeutsch.Maui.Services.Auth;

public interface IMobileAuthSessionStore
{
    Task<MobileAuthSession?> GetAsync(CancellationToken cancellationToken);

    Task SaveAsync(MobileAuthSession session, CancellationToken cancellationToken);

    Task ClearAsync(CancellationToken cancellationToken);
}
