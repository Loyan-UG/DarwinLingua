namespace DarwinDeutsch.Maui.Services.Updates;

/// <summary>
/// Defines the mobile client's remote content-update configuration.
/// </summary>
public sealed class RemoteContentUpdateOptions
{
    /// <summary>
    /// Gets or sets the Web API base URL.
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether TLS certificate errors should be ignored for the configured remote-update host.
    /// </summary>
    public bool IgnoreTlsCertificateErrors { get; set; }

    /// <summary>
    /// Gets or sets the optional request-header name used to bypass tunnel/browser warning pages.
    /// </summary>
    public string BrowserWarningBypassHeaderName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional request-header value used to bypass tunnel/browser warning pages.
    /// </summary>
    public string BrowserWarningBypassHeaderValue { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the target client-product key.
    /// </summary>
    public string ClientProductKey { get; set; } = "darwin-deutsch";

    /// <summary>
    /// Gets or sets the current mobile schema version.
    /// </summary>
    public int ClientSchemaVersion { get; set; } = 1;

    /// <summary>
    /// Gets or sets the timeout in seconds for remote status checks.
    /// </summary>
    public int StatusRequestTimeoutSeconds { get; set; } = 1;

    /// <summary>
    /// Gets or sets the timeout in seconds for remote manifest requests.
    /// </summary>
    public int ManifestRequestTimeoutSeconds { get; set; } = 4;
}
