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
    /// Gets or sets the target client-product key.
    /// </summary>
    public string ClientProductKey { get; set; } = "darwin-deutsch";

    /// <summary>
    /// Gets or sets the current mobile schema version.
    /// </summary>
    public int ClientSchemaVersion { get; set; } = 1;
}
