let deferredInstallPrompt = null;

function sendTelemetry(payload) {
    if (!payload || !payload.eventName) {
        return;
    }

    const body = JSON.stringify({
        eventName: payload.eventName,
        pagePath: window.location.pathname,
        durationMs: payload.durationMs,
        isFailure: payload.isFailure === true
    });

    if (navigator.sendBeacon) {
        const blob = new Blob([body], { type: "application/json" });
        navigator.sendBeacon("/telemetry/client-event", blob);
        return;
    }

    fetch("/telemetry/client-event", {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body,
        keepalive: true
    }).catch(() => {
    });
}

window.addEventListener("beforeinstallprompt", (event) => {
    event.preventDefault();
    deferredInstallPrompt = event;
    sendTelemetry({ eventName: "install.prompt.available" });

    const banner = document.getElementById("install-banner");
    if (banner) {
        banner.hidden = false;
    }
});

window.addEventListener("appinstalled", () => {
    deferredInstallPrompt = null;
    sendTelemetry({ eventName: "install.completed" });

    const banner = document.getElementById("install-banner");
    if (banner) {
        banner.hidden = true;
    }
});

document.addEventListener("DOMContentLoaded", () => {
    window.addEventListener("load", () => {
        if (!window.performance || !performance.timing) {
            return;
        }

        const navigationStart = performance.timing.navigationStart;
        const loadEventEnd = performance.timing.loadEventEnd;
        const durationMs = loadEventEnd > navigationStart ? loadEventEnd - navigationStart : 0;

        if (durationMs >= 2000) {
            sendTelemetry({ eventName: "page.load.slow", durationMs });
        }
    }, { once: true });

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

    document.querySelectorAll("[data-modal-open]").forEach((button) => {
        button.addEventListener("click", () => {
            const modalId = button.getAttribute("data-modal-open");
            const modal = modalId ? document.getElementById(modalId) : null;
            if (modal && typeof modal.showModal === "function") {
                modal.showModal();
            }
        });
    });

    document.querySelectorAll("[data-modal-close]").forEach((button) => {
        button.addEventListener("click", () => {
            const modalId = button.getAttribute("data-modal-close");
            const modal = modalId ? document.getElementById(modalId) : null;
            if (modal && typeof modal.close === "function") {
                modal.close();
            }
        });
    });

    document.body.addEventListener("htmx:responseError", () => {
        sendTelemetry({ eventName: "htmx.response.error", isFailure: true });
    });

    document.body.addEventListener("htmx:sendError", () => {
        sendTelemetry({ eventName: "htmx.network.error", isFailure: true });
    });
});
