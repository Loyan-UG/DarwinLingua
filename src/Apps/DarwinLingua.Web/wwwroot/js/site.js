let deferredInstallPrompt = null;

window.addEventListener("beforeinstallprompt", (event) => {
    event.preventDefault();
    deferredInstallPrompt = event;

    const banner = document.getElementById("install-banner");
    if (banner) {
        banner.hidden = false;
    }
});

window.addEventListener("appinstalled", () => {
    deferredInstallPrompt = null;

    const banner = document.getElementById("install-banner");
    if (banner) {
        banner.hidden = true;
    }
});

document.addEventListener("DOMContentLoaded", () => {
    const installButton = document.getElementById("install-button");
    if (installButton) {
        installButton.addEventListener("click", async () => {
            if (!deferredInstallPrompt) {
                return;
            }

            await deferredInstallPrompt.prompt();
            deferredInstallPrompt = null;

            const banner = document.getElementById("install-banner");
            if (banner) {
                banner.hidden = true;
            }
        });
    }

    if ("serviceWorker" in navigator) {
        navigator.serviceWorker.register("/sw.js").catch(() => {
        });
    }
});
