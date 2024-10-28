self.addEventListener('fetch', (event) => {
    if (event.request.url.includes("/api/tasksequence")) {
        event.respondWith(
            caches.open("overblikplus-api-cache").then(cache => {
                return fetch(event.request)
                    .then(response => {
                        cache.put(event.request, response.clone());
                        return response;
                    })
                    .catch(() => cache.match(event.request));
            })
        );
    } else {
        event.respondWith(
            caches.match(event.request).then((response) => {
                return response || fetch(event.request);
            })
        );
    }
});
