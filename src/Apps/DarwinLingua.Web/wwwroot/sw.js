const shellCacheName = "darwin-lingua-shell-v3";
const offlineShellUrl = "/offline.html";
const shellAssets = [
    offlineShellUrl,
    "/manifest.webmanifest",
    "/icons/favicon.svg",
    "/icons/icon-192.svg",
    "/icons/icon-512.svg",
    "/icons/icon-maskable.svg"
];

self.addEventListener("install", (event) => {
    self.skipWaiting();
    event.waitUntil(caches.open(shellCacheName).then((cache) => cache.addAll(shellAssets)));
});

self.addEventListener("activate", (event) => {
    event.waitUntil(
        caches.keys()
            .then((keys) =>
                Promise.all(keys.filter((key) => key !== shellCacheName).map((key) => caches.delete(key))))
            .then(() => self.clients.claim())
    );
});

self.addEventListener("fetch", (event) => {
    if (event.request.method !== "GET") {
        return;
    }

    if (event.request.mode === "navigate") {
        event.respondWith(
            fetch(event.request).catch(() =>
                caches.match(offlineShellUrl).then((cachedResponse) => cachedResponse ?? Response.error()))
        );
        return;
    }

    const requestUrl = new URL(event.request.url);
    const isSameOrigin = requestUrl.origin === self.location.origin;
    const isCacheableAsset = isSameOrigin
        && (requestUrl.pathname.startsWith("/icons/")
            || requestUrl.pathname === "/manifest.webmanifest");

    if (!isCacheableAsset) {
        return;
    }

    event.respondWith(caches.match(event.request).then((cachedResponse) => cachedResponse ?? fetch(event.request)));
});
