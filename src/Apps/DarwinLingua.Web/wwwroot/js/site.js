let deferredInstallPrompt = null;
const wordNavigationStorageKey = "darwinlingua.web.word-navigation";
const manualSpeechRates = [1, 0.75, 0.5];

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

function speakText(text, languageCode, rate) {
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
    utterance.rate = Number.isFinite(rate) && rate > 0 ? rate : 1;

    const voice = resolveSpeechVoice(utterance.lang);
    if (voice) {
        utterance.voice = voice;
    }

    window.speechSynthesis.speak(utterance);
}

function getNextManualSpeechRate(button) {
    if (!button) {
        return 1;
    }

    const currentIndex = Number.parseInt(button.dataset.speechRateIndex || "0", 10);
    const normalizedIndex = Number.isInteger(currentIndex) && currentIndex >= 0
        ? currentIndex % manualSpeechRates.length
        : 0;
    const rate = manualSpeechRates[normalizedIndex];
    button.dataset.speechRateIndex = ((normalizedIndex + 1) % manualSpeechRates.length).toString();
    return rate;
}

function isTypingTarget(target) {
    const element = target instanceof Element ? target : null;
    if (!element) {
        return false;
    }

    const tagName = element.tagName ? element.tagName.toLowerCase() : "";
    return tagName === "input"
        || tagName === "textarea"
        || tagName === "select"
        || element.isContentEditable;
}

function getWordListContainer(element) {
    return element ? element.closest("[data-word-list-container]") : null;
}

function captureWordNavigationContext(triggerElement) {
    const container = getWordListContainer(triggerElement);
    if (!container) {
        return;
    }

    const entries = Array.from(container.querySelectorAll("[data-word-link][data-word-id]"))
        .map((element) => ({
            id: element.getAttribute("data-word-id"),
            href: element.getAttribute("href")
        }))
        .filter((entry) => !!entry.id);
    const ids = entries.map((entry) => entry.id);

    if (ids.length === 0) {
        return;
    }

    window.sessionStorage.setItem(wordNavigationStorageKey, JSON.stringify({
        ids,
        entries,
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

        const entry = Array.isArray(context.entries)
            ? context.entries.find((item) => item && item.id === targetId)
            : null;
        window.location.assign(entry && entry.href ? entry.href : `/words/${targetId}`);
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
        if (isTypingTarget(event.target)) {
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

    const lemma = detailElement.getAttribute("data-word-lemma");
    if (lemma && canUseSpeechSynthesis()) {
        window.setTimeout(() => {
            speakText(lemma, "de-DE", 1);
        }, 1000);
    }
}

function configureGlobalSpeech(rootElement) {
    const root = rootElement || document;
    root.querySelectorAll("[data-speak-text]").forEach((button) => {
        if (button.dataset.speechBound === "true") {
            return;
        }

        button.dataset.speechBound = "true";
        button.addEventListener("click", () => {
            speakText(
                button.getAttribute("data-speak-text"),
                button.getAttribute("data-speak-lang") || "de-DE",
                getNextManualSpeechRate(button));
        });
    });
}

function configureSpeechShortcut() {
    document.addEventListener("keydown", (event) => {
        if (!event.ctrlKey || event.altKey || event.metaKey || event.shiftKey || event.code !== "Space") {
            return;
        }

        if (isTypingTarget(event.target)) {
            return;
        }

        const speechButton = document.querySelector("[data-keyboard-speak-target='true']");
        if (!speechButton) {
            return;
        }

        event.preventDefault();
        speechButton.click();
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
        navigator.serviceWorker.register("/sw.js").then((registration) => {
            registration.update().catch(() => {
            });
        }).catch(() => {
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

    document.body.addEventListener("submit", (event) => {
        const form = event.target instanceof HTMLFormElement ? event.target : null;
        const message = form ? form.getAttribute("data-confirm-submit") : null;
        if (!message) {
            return;
        }

        if (!window.confirm(message)) {
            event.preventDefault();
            event.stopPropagation();
        }
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

    document.body.addEventListener("htmx:afterSwap", (event) => {
        configureGlobalSpeech(event.target);
    });

    const wordDetailPage = document.querySelector("[data-word-detail-page]");
    if (wordDetailPage) {
        configureWordNavigation(wordDetailPage);
        configureWordSpeech(wordDetailPage);
    }

    configureGlobalSpeech(document);
    configureSpeechShortcut();
});
