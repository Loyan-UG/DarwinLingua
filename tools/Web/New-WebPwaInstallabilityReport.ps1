param(
    [string]$WebUrl = "https://localhost:7501",
    [string]$OutputPath = "artifacts/installability-report.json",
    [string]$ChromePath = ""
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..")
$webRoot = Join-Path $repoRoot "src\Apps\DarwinLingua.Web"
$outputFullPath = Join-Path $repoRoot $OutputPath
$outputDirectory = Split-Path -Parent $outputFullPath

if (-not (Test-Path $outputDirectory)) {
    New-Item -ItemType Directory -Path $outputDirectory | Out-Null
}

if ([string]::IsNullOrWhiteSpace($ChromePath)) {
    $candidateChromePaths = @(
        "C:\Program Files\Google\Chrome\Application\chrome.exe",
        "C:\Program Files (x86)\Google\Chrome\Application\chrome.exe",
        "C:\Program Files\Microsoft\Edge\Application\msedge.exe",
        "C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe"
    )

    $ChromePath = $candidateChromePaths | Where-Object { Test-Path $_ } | Select-Object -First 1
}

if ([string]::IsNullOrWhiteSpace($ChromePath) -or -not (Test-Path $ChromePath)) {
    throw "Chrome or Edge executable was not found. Pass -ChromePath explicitly."
}

if (-not (Test-Path (Join-Path $webRoot "node_modules\puppeteer-core"))) {
    throw "puppeteer-core is not installed under $webRoot. Run npm install in the Web project before this report."
}

$nodeScript = @'
const fs = require("fs");
const path = require("path");
const puppeteer = require("puppeteer-core");

const webUrl = process.argv[2];
const outputPath = process.argv[3];
const chromePath = process.argv[4];

const checks = [];
const manualChecks = [];
const consoleMessages = [];

function addCheck(name, passed, details = null) {
  checks.push({ name, passed: Boolean(passed), details });
}

function normalizeUrl(base, value) {
  try {
    return new URL(value, base).toString();
  } catch {
    return null;
  }
}

(async () => {
  const browser = await puppeteer.launch({
    executablePath: chromePath,
    headless: "new",
    ignoreHTTPSErrors: true,
    args: [
      "--ignore-certificate-errors",
      "--allow-insecure-localhost",
      "--no-first-run",
      "--no-default-browser-check",
      "--disable-background-networking"
    ]
  });

  try {
    const page = await browser.newPage();
    await page.evaluateOnNewDocument(() => {
      window.__darwinLinguaBeforeInstallPromptSeen = false;
      window.addEventListener("beforeinstallprompt", () => {
        window.__darwinLinguaBeforeInstallPromptSeen = true;
      });
    });

    page.on("console", (message) => {
      consoleMessages.push({ type: message.type(), text: message.text() });
    });

    const response = await page.goto(webUrl, { waitUntil: "networkidle0", timeout: 30000 });
    const status = response ? response.status() : 0;
    const pageUrl = page.url();
    const pageOrigin = new URL(pageUrl).origin;

    const result = await page.evaluate(async () => {
      const manifestElement = document.querySelector("link[rel='manifest']");
      const manifestHref = manifestElement ? manifestElement.getAttribute("href") : null;
      const manifestUrl = manifestHref ? new URL(manifestHref, location.href).toString() : null;
      let manifestStatus = null;
      let manifestContentType = null;
      let manifestJson = null;
      let manifestError = null;

      if (manifestUrl) {
        try {
          const manifestResponse = await fetch(manifestUrl);
          manifestStatus = manifestResponse.status;
          manifestContentType = manifestResponse.headers.get("content-type");
          manifestJson = await manifestResponse.json();
        } catch (error) {
          manifestError = error.message;
        }
      }

      const iconResults = [];
      if (manifestJson && Array.isArray(manifestJson.icons)) {
        for (const icon of manifestJson.icons) {
          const iconUrl = new URL(icon.src, location.href).toString();
          try {
            const iconResponse = await fetch(iconUrl);
            iconResults.push({
              src: icon.src,
              sizes: icon.sizes || "",
              type: icon.type || "",
              purpose: icon.purpose || "",
              status: iconResponse.status,
              contentType: iconResponse.headers.get("content-type")
            });
          } catch (error) {
            iconResults.push({
              src: icon.src,
              sizes: icon.sizes || "",
              type: icon.type || "",
              purpose: icon.purpose || "",
              error: error.message
            });
          }
        }
      }

      let serviceWorker = {
        supported: "serviceWorker" in navigator,
        ready: false,
        scope: null,
        registrationCount: 0,
        controller: Boolean(navigator.serviceWorker && navigator.serviceWorker.controller)
      };

      if (serviceWorker.supported) {
        const registration = await navigator.serviceWorker.ready;
        const registrations = await navigator.serviceWorker.getRegistrations();
        serviceWorker = {
          supported: true,
          ready: Boolean(registration),
          scope: registration ? registration.scope : null,
          registrationCount: registrations.length,
          controller: Boolean(navigator.serviceWorker.controller)
        };
      }

      const cacheKeys = "caches" in window ? await caches.keys() : [];
      const cachedRequests = [];
      for (const key of cacheKeys) {
        const cache = await caches.open(key);
        const requests = await cache.keys();
        cachedRequests.push({
          cache: key,
          urls: requests.map((request) => request.url)
        });
      }

      let offlineResponseStatus = null;
      let offlineResponseContentType = null;
      let offlineResponseText = "";
      try {
        const offlineResponse = await fetch("/offline.html");
        offlineResponseStatus = offlineResponse.status;
        offlineResponseContentType = offlineResponse.headers.get("content-type");
        offlineResponseText = await offlineResponse.text();
      } catch {
        offlineResponseStatus = null;
      }

      return {
        title: document.title,
        htmlLang: document.documentElement.lang,
        manifestHref,
        manifestUrl,
        manifestStatus,
        manifestContentType,
        manifestJson,
        manifestError,
        iconResults,
        serviceWorker,
        cacheKeys,
        cachedRequests,
        offlineResponseStatus,
        offlineResponseContentType,
        offlineResponseHasShellCopy: offlineResponseText.includes("Darwin Lingua is offline"),
        beforeInstallPromptSeen: Boolean(window.__darwinLinguaBeforeInstallPromptSeen)
      };
    });

    await page.reload({ waitUntil: "networkidle0", timeout: 30000 });
    const controllerAfterReload = await page.evaluate(() => Boolean(navigator.serviceWorker && navigator.serviceWorker.controller));

    const manifest = result.manifestJson || {};
    const icons = Array.isArray(manifest.icons) ? manifest.icons : [];
    const displayOverride = Array.isArray(manifest.display_override) ? manifest.display_override : [];
    const fetchedIcons = result.iconResults || [];
    const cachedUrls = (result.cachedRequests || []).flatMap((entry) => entry.urls || []);

    addCheck("main page returns HTTP 200", status === 200, { status, pageUrl });
    addCheck("site is served from a secure local origin", pageUrl.startsWith("https://") || pageOrigin.includes("localhost"), { pageUrl });
    addCheck("manifest link is present", Boolean(result.manifestHref), { manifestHref: result.manifestHref });
    addCheck("manifest fetch returns HTTP 200", result.manifestStatus === 200, { status: result.manifestStatus, contentType: result.manifestContentType, error: result.manifestError });
    addCheck("manifest declares name and short_name", Boolean(manifest.name && manifest.short_name), { name: manifest.name || null, shortName: manifest.short_name || null });
    addCheck("manifest declares id, start_url, and scope", Boolean(manifest.id && manifest.start_url && manifest.scope), { id: manifest.id || null, startUrl: manifest.start_url || null, scope: manifest.scope || null });
    addCheck("manifest display supports standalone", manifest.display === "standalone" || displayOverride.includes("standalone"), { display: manifest.display || null, displayOverride });
    addCheck("manifest has theme and background colors", Boolean(manifest.theme_color && manifest.background_color), { themeColor: manifest.theme_color || null, backgroundColor: manifest.background_color || null });
    addCheck("manifest includes a PNG icon at least 512px", icons.some((icon) => (icon.type || "").includes("png") && /(^|\\s)(512|1024)x(512|1024)(\\s|$)/.test(icon.sizes || "")), { icons });
    addCheck("manifest includes a maskable icon", icons.some((icon) => (icon.purpose || "").includes("maskable")), { icons });
    addCheck("all manifest icons are fetchable", fetchedIcons.length > 0 && fetchedIcons.every((icon) => icon.status === 200), { fetchedIcons });
    addCheck("service worker API is supported", result.serviceWorker.supported, result.serviceWorker);
    addCheck("service worker becomes ready", result.serviceWorker.ready && result.serviceWorker.registrationCount > 0, result.serviceWorker);
    addCheck("service worker controls page after reload", controllerAfterReload, { controllerAfterReload });
    addCheck("shell cache is created", result.cacheKeys.includes("darwin-lingua-shell-v3"), { cacheKeys: result.cacheKeys });
    addCheck("offline shell page is cached", cachedUrls.some((url) => url.endsWith("/offline.html")), { cachedUrls });
    addCheck("offline shell route returns HTTP 200", result.offlineResponseStatus === 200 && result.offlineResponseHasShellCopy, { status: result.offlineResponseStatus, contentType: result.offlineResponseContentType });

    manualChecks.push({
      name: "desktop browser install prompt acceptance",
      status: result.beforeInstallPromptSeen ? "prompt-event-observed-but-browser-acceptance-still-manual" : "manual-required",
      details: "Headless Chromium cannot complete the real browser install prompt. Validate install prompt and installed-window behavior manually on target desktop Chromium."
    });
    manualChecks.push({
      name: "Android Chrome install flow",
      status: "manual-required",
      details: "Validate on Android Chrome or emulator with remote DevTools."
    });

    const failedChecks = checks.filter((check) => !check.passed);
    const report = {
      generatedAtUtc: new Date().toISOString(),
      url: webUrl,
      finalUrl: pageUrl,
      chromePath,
      title: result.title,
      htmlLang: result.htmlLang,
      checks,
      failedChecks,
      manualChecks,
      consoleMessages
    };

    fs.mkdirSync(path.dirname(outputPath), { recursive: true });
    fs.writeFileSync(outputPath, JSON.stringify(report, null, 2), "utf8");

    if (failedChecks.length > 0) {
      console.error(JSON.stringify({ outputPath, failedChecks }, null, 2));
      process.exit(1);
    }

    console.log(JSON.stringify({
      outputPath,
      passedChecks: checks.length,
      manualChecks: manualChecks.length
    }, null, 2));
  } finally {
    await browser.close();
  }
})().catch((error) => {
  console.error(error);
  process.exit(1);
});
'@

$scriptPath = Join-Path $webRoot ("darwinlingua-pwa-installability-" + [Guid]::NewGuid().ToString("N") + ".cjs")
Set-Content -LiteralPath $scriptPath -Value $nodeScript -Encoding UTF8

try {
    Push-Location $webRoot
    node $scriptPath $WebUrl $outputFullPath $ChromePath
    if ($LASTEXITCODE -ne 0) {
        throw "PWA installability report failed with exit code $LASTEXITCODE."
    }
}
finally {
    Pop-Location
    Remove-Item -LiteralPath $scriptPath -ErrorAction SilentlyContinue
}
