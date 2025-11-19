// Service Worker para tracking en background
self.addEventListener('install', event => {
    self.skipWaiting();
});

self.addEventListener('activate', event => {
    event.waitUntil(self.clients.claim());
});

self.addEventListener('sync', event => {
    if (event.tag === 'sync-ubicacion') {
        event.waitUntil(sincronizarUbicacion());
    }
});

async function sincronizarUbicacion() {
    // Obtener ubicaciones pendientes del IndexedDB y enviarlas al servidor
    // Esto permite que el tracking continúe aunque la app esté en background
    try {
        const ubicacionesPendientes = await obtenerUbicacionesPendientes();
        for (const ubicacion of ubicacionesPendientes) {
            await enviarUbicacion(ubicacion);
        }
    } catch (error) {
        console.error('Error sincronizando ubicación:', error);
    }
}

async function obtenerUbicacionesPendientes() {
    // Implementación con IndexedDB si es necesario
    return [];
}

async function enviarUbicacion(ubicacion) {
    // Enviar ubicación al servidor
    return fetch('/Paseo/GuardarUbicacion', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(ubicacion)
    });
}

