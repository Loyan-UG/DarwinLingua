namespace DarwinDeutsch.Maui.Services.Auth;

public sealed class MobileAuthOptions
{
    public string BaseUrl { get; init; } = string.Empty;

    public bool IgnoreTlsCertificateErrors { get; init; }

    public string? BrowserWarningBypassHeaderName { get; init; }

    public string? BrowserWarningBypassHeaderValue { get; init; }
}
