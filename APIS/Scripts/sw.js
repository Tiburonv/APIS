const CACHE_NAME = 'apis-cache-v1';
const urlsToCache = [
  '/',
  '/Content/Site.css',
  '/Content/bootstrap.min.css',
  '/Scripts/jquery-3.7.0.min.js',
  '/Scripts/bootstrap.bundle.min.js',
  '/Content/Imagen/logoConfig.js'
];

// Instalación del Service Worker
self.addEventListener('install', function(event) {
  event.waitUntil(
    caches.open(CACHE_NAME)
      .then(function(cache) {
        console.log('Cache abierto');
        return cache.addAll(urlsToCache);
      })
  );
});

// Activación del Service Worker
self.addEventListener('activate', function(event) {
  event.waitUntil(
    caches.keys().then(function(cacheNames) {
      return Promise.all(
        cacheNames.map(function(cacheName) {
          if (cacheName !== CACHE_NAME) {
            console.log('Eliminando cache antiguo:', cacheName);
            return caches.delete(cacheName);
          }
        })
      );
    })
  );
});

// Interceptar peticiones
self.addEventListener('fetch', function(event) {
  event.respondWith(
    caches.match(event.request)
      .then(function(response) {
        // Devolver desde cache si está disponible
        if (response) {
          return response;
        }
        
        // Si no está en cache, hacer petición a la red
        return fetch(event.request).then(
          function(response) {
            // Verificar que la respuesta sea válida
            if(!response || response.status !== 200 || response.type !== 'basic') {
              return response;
            }

            // Clonar la respuesta
            var responseToCache = response.clone();

            caches.open(CACHE_NAME)
              .then(function(cache) {
                cache.put(event.request, responseToCache);
              });

            return response;
          }
        );
      })
  );
});

// Manejo de notificaciones push
self.addEventListener('push', function(event) {
  const options = {
    body: event.data ? event.data.text() : 'Nueva notificación de APIS',
    icon: '/Content/Imagen/logo-192x192.png',
    badge: '/Content/Imagen/logo-72x72.png',
    vibrate: [100, 50, 100],
    data: {
      dateOfArrival: Date.now(),
      primaryKey: 1
    },
    actions: [
      {
        action: 'explore',
        title: 'Ver',
        icon: '/Content/Imagen/logo-72x72.png'
      },
      {
        action: 'close',
        title: 'Cerrar',
        icon: '/Content/Imagen/logo-72x72.png'
      }
    ]
  };

  event.waitUntil(
    self.registration.showNotification('APIS - Sistema de Gestión', options)
  );
});

// Manejo de clics en notificaciones
self.addEventListener('notificationclick', function(event) {
  event.notification.close();

  if (event.action === 'explore') {
    event.waitUntil(
      clients.openWindow('/')
    );
  }
});
