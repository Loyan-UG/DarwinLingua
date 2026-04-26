namespace DarwinDeutsch.Maui.Services.Auth;

public interface IMobileAuthService
{
    Task<MobileAuthSession?> GetCurrentSessionAsync(CancellationToken cancellationToken);

    Task<MobileAuthSession> RegisterAsync(string email, string password, CancellationToken cancellationToken);

    Task<MobileAuthSession> SignInAsync(string email, string password, CancellationToken cancellationToken);

    Task<MobileAuthSession?> RefreshCurrentSessionAsync(CancellationToken cancellationToken);

    Task SignOutAsync(CancellationToken cancellationToken);
}
