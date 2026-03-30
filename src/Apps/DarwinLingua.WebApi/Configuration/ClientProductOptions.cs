namespace DarwinLingua.WebApi.Configuration;

/// <summary>
/// Describes one client product that can consume shared content manifests.
/// </summary>
public sealed class ClientProductOptions
{
    /// <summary>
    /// Gets or sets the stable product key.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name used for diagnostics.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the learning language code.
    /// </summary>
    public string LearningLanguageCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the default UI language code.
    /// </summary>
    public string DefaultUiLanguageCode { get; set; } = "en";

    /// <summary>
    /// Gets or sets a value indicating whether the product is active.
    /// </summary>
    public bool IsActive { get; set; } = true;
}
