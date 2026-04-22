let deferredInstallPrompt = null;
const wordNavigationStorageKey = "darwinlingua.web.word-navigation";

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

function canUseSpeechSynthesis() {
    return typeof window !== "undefined"
        && "speechSynthesis" in window
        && typeof window.SpeechSynthesisUtterance === "function";
}

function stopCurrentSpeech() {
    if (!canUseSpeechSynthesis()) {
        return;
    }

    window.speechSynthesis.cancel();
}

function resolveSpeechVoice(languageCode) {
    if (!canUseSpeechSynthesis()) {
        return null;
    }

    const normalizedLanguageCode = (languageCode || "de-DE").toLowerCase();
    const voices = window.speechSynthesis.getVoices();

    return voices.find((voice) => voice.lang && voice.lang.toLowerCase() === normalizedLanguageCode)
        ?? voices.find((voice) => voice.lang && voice.lang.toLowerCase().startsWith("de"))
        ?? null;
}

function speakText(text, languageCode) {
    if (!canUseSpeechSynthesis()) {
        return;
    }

    const normalizedText = (text || "").trim();
    if (!normalizedText) {
        return;
    }

    stopCurrentSpeech();

    const utterance = new window.SpeechSynthesisUtterance(normalizedText);
    utterance.lang = languageCode || "de-DE";

    const voice = resolveSpeechVoice(utterance.lang);
    if (voice) {
        utterance.voice = voice;
    }

    window.speechSynthesis.speak(utterance);
}

function getWordListContainer(element) {
    return element ? element.closest("[data-word-list-container]") : null;
}

function captureWordNavigationContext(triggerElement) {
    const container = getWordListContainer(triggerElement);
    if (!container) {
        return;
    }

    const ids = Array.from(container.querySelectorAll("[data-word-link][data-word-id]"))
        .map((element) => element.getAttribute("data-word-id"))
        .filter((value) => !!value);

    if (ids.length === 0) {
        return;
    }

    window.sessionStorage.setItem(wordNavigationStorageKey, JSON.stringify({
        ids,
        sourcePath: `${window.location.pathname}${window.location.search}`
    }));
}

function getStoredWordNavigationContext() {
    const rawValue = window.sessionStorage.getItem(wordNavigationStorageKey);
    if (!rawValue) {
        return null;
    }

    try {
        const parsed = JSON.parse(rawValue);
        if (!parsed || !Array.isArray(parsed.ids)) {
            return null;
        }

        return parsed;
    } catch {
        return null;
    }
}

function configureWordNavigation(detailElement) {
    if (!detailElement) {
        return;
    }

    const currentWordId = detailElement.getAttribute("data-word-id");
    if (!currentWordId) {
        return;
    }

    const context = getStoredWordNavigationContext();
    if (!context || !Array.isArray(context.ids) || context.ids.length === 0) {
        return;
    }

    const currentIndex = context.ids.indexOf(currentWordId);
    if (currentIndex < 0) {
        return;
    }

    const previousId = currentIndex > 0 ? context.ids[currentIndex - 1] : null;
    const nextId = currentIndex < context.ids.length - 1 ? context.ids[currentIndex + 1] : null;
    const previousButton = detailElement.querySelector("[data-word-nav='prev']");
    const nextButton = detailElement.querySelector("[data-word-nav='next']");

    const navigateToWord = (targetId) => {
        if (!targetId) {
            return;
        }

        window.location.assign(`/words/${targetId}`);
    };

    if (previousButton) {
        previousButton.disabled = !previousId;
        previousButton.addEventListener("click", () => navigateToWord(previousId));
    }

    if (nextButton) {
        nextButton.disabled = !nextId;
        nextButton.addEventListener("click", () => navigateToWord(nextId));
    }

    document.addEventListener("keydown", (event) => {
        const targetTagName = event.target && event.target.tagName ? event.target.tagName.toLowerCase() : "";
        if (targetTagName === "input" || targetTagName === "textarea" || targetTagName === "select") {
            return;
        }

        if (event.key === "ArrowLeft" && previousId) {
            event.preventDefault();
            navigateToWord(previousId);
        }

        if (event.key === "ArrowRight" && nextId) {
            event.preventDefault();
            navigateToWord(nextId);
        }
    });

    let touchStartX = 0;
    let touchStartY = 0;

    detailElement.addEventListener("touchstart", (event) => {
        const touch = event.changedTouches && event.changedTouches[0];
        if (!touch) {
            return;
        }

        touchStartX = touch.clientX;
        touchStartY = touch.clientY;
    }, { passive: true });

    detailElement.addEventListener("touchend", (event) => {
        const touch = event.changedTouches && event.changedTouches[0];
        if (!touch) {
            return;
        }

        const deltaX = touch.clientX - touchStartX;
        const deltaY = touch.clientY - touchStartY;

        if (Math.abs(deltaX) < 70 || Math.abs(deltaX) < Math.abs(deltaY)) {
            return;
        }

        if (deltaX < 0 && nextId) {
            navigateToWord(nextId);
        } else if (deltaX > 0 && previousId) {
            navigateToWord(previousId);
        }
    }, { passive: true });
}

function configureWordSpeech(detailElement) {
    if (!detailElement) {
        return;
    }

    const speakButtons = detailElement.querySelectorAll("[data-speak-text]");
    speakButtons.forEach((button) => {
        button.addEventListener("click", () => {
            speakText(button.getAttribute("data-speak-text"), button.getAttribute("data-speak-lang") || "de-DE");
        });
    });

    const lemma = detailElement.getAttribute("data-word-lemma");
    if (lemma && canUseSpeechSynthesis()) {
        window.setTimeout(() => {
            speakText(lemma, "de-DE");
        }, 1000);
    }
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

    document.body.addEventListener("click", (event) => {
        const trigger = event.target.closest("[data-word-link]");
        if (!trigger) {
            return;
        }

        captureWordNavigationContext(trigger);
    });

    const wordDetailPage = document.querySelector("[data-word-detail-page]");
    if (wordDetailPage) {
        configureWordNavigation(wordDetailPage);
        configureWordSpeech(wordDetailPage);
    }
});
