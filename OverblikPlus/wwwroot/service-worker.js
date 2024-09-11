self.addEventListener('install', (event) => {
    // Tvinger service worker til at opdatere med det samme
    self.skipWaiting();
});

self.addEventListener('activate', (event) => {
    // Ryd gammel cache og gør den nyeste version aktiv
    event.waitUntil(
        caches.keys().then(cacheNames => {
            return Promise.all(
                cacheNames.map(cache => {
                    return caches.delete(cache);
                })
            );
        }).then(() => self.clients.claim())
    );
});
