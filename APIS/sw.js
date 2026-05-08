/* APIS Safe Service Worker v6
 * No intercepta fetch (evita errores de redirect / pagina en blanco).
 * Solo limpia caches antiguos y toma control.
 */
const SW_VERSION = 'apis-safe-v6';

self.addEventListener('install', function (event) {
  self.skipWaiting();
});

self.addEventListener('activate', function (event) {
  event.waitUntil(
    caches.keys().then(function (keys) {
      return Promise.all(keys.map(function (key) {
        return caches.delete(key);
      }));
    }).then(function () {
      return self.clients.claim();
    })
  );
});

// Intencionalmente SIN listener 'fetch':
// el navegador maneja todos los requests (incluyendo redirects de Forms Auth).
