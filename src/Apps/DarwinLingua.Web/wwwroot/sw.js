const shellCacheName = "darwin-lingua-shell-v1";
const shellAssets = [
    "/",
    "/manifest.webmanifest",
    "/css/site.css",
    "/js/site.js",
    "/icons/icon-192.svg",
    "/icons/icon-512.svg"
];

self.addEventListener("install", (event) => {
    event.waitUntil(caches.open(shellCacheName).then((cache) => cache.addAll(shellAssets)));
});

self.addEventListener("activate", (event) => {
    event.waitUntil(
        caches.keys().then((keys) =>
            Promise.all(keys.filter((key) => key !== shellCacheName).map((key) => caches.delete(key))))
    );
});

self.addEventListener("fetch", (event) => {
    if (event.request.method !== "GET") {
        return;
    }

    event.respondWith(
        caches.match(event.request).then((cachedResponse) => cachedResponse ?? fetch(event.request))
    );
});
