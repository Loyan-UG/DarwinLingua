namespace DarwinLingua.WebApi.Tests;

using System.Text.Json;
using Xunit;

public sealed class WebPwaInstallStructuralTests
{
    [Fact]
    public void WebPwaManifest_ShouldExposeDesktopInstallabilityBaseline()
    {
        string manifestPath = ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "wwwroot", "manifest.webmanifest");
        using JsonDocument manifest = JsonDocument.Parse(File.ReadAllText(manifestPath));
        JsonElement root = manifest.RootElement;

        Assert.Equal("Darwin Lingua", root.GetProperty("name").GetString());
        Assert.Equal("DarwinLingua", root.GetProperty("short_name").GetString());
        Assert.Equal("/", root.GetProperty("id").GetString());
        Assert.Equal("/", root.GetProperty("start_url").GetString());
        Assert.Equal("/", root.GetProperty("scope").GetString());
        Assert.Equal("standalone", root.GetProperty("display").GetString());
        Assert.Equal("#0f172a", root.GetProperty("theme_color").GetString());
        Assert.Equal("#08111f", root.GetProperty("background_color").GetString());

        JsonElement icons = root.GetProperty("icons");
        Assert.Contains(icons.EnumerateArray(), icon =>
            string.Equals(icon.GetProperty("src").GetString(), "/images/logo.png", StringComparison.Ordinal) &&
            string.Equals(icon.GetProperty("type").GetString(), "image/png", StringComparison.Ordinal) &&
            string.Equals(icon.GetProperty("sizes").GetString(), "1024x1024", StringComparison.Ordinal));
        Assert.Contains(icons.EnumerateArray(), icon =>
            string.Equals(icon.GetProperty("src").GetString(), "/icons/icon-192.svg", StringComparison.Ordinal) &&
            icon.GetProperty("purpose").GetString()!.Contains("maskable", StringComparison.Ordinal));
        Assert.Contains(icons.EnumerateArray(), icon =>
            string.Equals(icon.GetProperty("src").GetString(), "/icons/icon-maskable.svg", StringComparison.Ordinal) &&
            string.Equals(icon.GetProperty("purpose").GetString(), "maskable", StringComparison.Ordinal));
    }

    [Fact]
    public void WebPwaShell_ShouldRegisterServiceWorkerInstallPromptAndOfflineFallback()
    {
        string layoutSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Views", "Shared", "_Layout.cshtml"));
        string siteScript = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "wwwroot", "js", "site.js"));
        string serviceWorker = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "wwwroot", "sw.js"));
        string offlineShell = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "wwwroot", "offline.html"));
        string programSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Program.cs"));

        Assert.Contains("<link rel=\"manifest\" href=\"~/manifest.webmanifest\"", layoutSource, StringComparison.Ordinal);
        Assert.Contains("meta name=\"theme-color\"", layoutSource, StringComparison.Ordinal);
        Assert.Contains("id=\"install-banner\"", layoutSource, StringComparison.Ordinal);
        Assert.Contains("id=\"install-button\"", layoutSource, StringComparison.Ordinal);

        Assert.Contains("beforeinstallprompt", siteScript, StringComparison.Ordinal);
        Assert.Contains("event.preventDefault()", siteScript, StringComparison.Ordinal);
        Assert.Contains("deferredInstallPrompt.prompt()", siteScript, StringComparison.Ordinal);
        Assert.Contains("navigator.serviceWorker.register(\"/sw.js\")", siteScript, StringComparison.Ordinal);

        Assert.Contains("darwin-lingua-shell-v3", serviceWorker, StringComparison.Ordinal);
        Assert.Contains("const offlineShellUrl = \"/offline.html\"", serviceWorker, StringComparison.Ordinal);
        Assert.Contains("cache.addAll(shellAssets)", serviceWorker, StringComparison.Ordinal);
        Assert.Contains("event.request.mode === \"navigate\"", serviceWorker, StringComparison.Ordinal);
        Assert.Contains("fetch(event.request).catch", serviceWorker, StringComparison.Ordinal);
        Assert.Contains("caches.match(offlineShellUrl)", serviceWorker, StringComparison.Ordinal);
        Assert.Contains("requestUrl.origin === self.location.origin", serviceWorker, StringComparison.Ordinal);
        Assert.DoesNotContain("https://", serviceWorker, StringComparison.OrdinalIgnoreCase);

        Assert.Contains("Darwin Lingua is offline", offlineShell, StringComparison.Ordinal);
        Assert.Contains("Try the home page", offlineShell, StringComparison.Ordinal);
        Assert.Contains("worker-src 'self'", programSource, StringComparison.Ordinal);
        Assert.Contains("manifest-src 'self'", programSource, StringComparison.Ordinal);
    }

    private static string ResolveRepositoryPath(params string[] segments)
    {
        string? directory = AppContext.BaseDirectory;
        while (!string.IsNullOrWhiteSpace(directory))
        {
            string candidate = Path.Combine(new[] { directory }.Concat(segments).ToArray());
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = Directory.GetParent(directory)?.FullName;
        }

        throw new FileNotFoundException($"Could not resolve repository file '{string.Join(Path.DirectorySeparatorChar, segments)}'.");
    }
}
