// Sistema de notificaciones push para Zooni
let notificacionesInterval = null;
let notificacionesNoLeidas = 0;

// Inicializar sistema de notificaciones
function initNotificaciones() {
    cargarNotificaciones();
    
    // Actualizar cada 30 segundos
    if (notificacionesInterval) {
        clearInterval(notificacionesInterval);
    }
    notificacionesInterval = setInterval(cargarNotificaciones, 30000);
}

// Cargar notificaciones del servidor
async function cargarNotificaciones() {
    try {
        const response = await fetch('/Notificacion/ObtenerNotificaciones');
        const data = await response.json();
        
        if (data.success) {
            notificacionesNoLeidas = data.noLeidas || 0;
            actualizarBadgeNotificaciones();
            mostrarNotificacionesEnPanel(data.notificaciones || []);
        }
    } catch (error) {
        console.error('Error al cargar notificaciones:', error);
    }
}

// Actualizar badge de notificaciones
function actualizarBadgeNotificaciones() {
    const badge = document.getElementById('notificacionesBadge');
    if (badge) {
        if (notificacionesNoLeidas > 0) {
            badge.textContent = notificacionesNoLeidas > 99 ? '99+' : notificacionesNoLeidas;
            badge.style.display = 'inline-block';
        } else {
            badge.style.display = 'none';
        }
    }
}

// Mostrar notificaciones en el panel
function mostrarNotificacionesEnPanel(notificaciones) {
    const container = document.getElementById('notificacionesList');
    if (!container) return;

    if (notificaciones.length === 0) {
        container.innerHTML = '<div class="notificacion-vacia">No tenés notificaciones</div>';
        return;
    }

    container.innerHTML = notificaciones.map(notif => {
        const icono = obtenerIconoPorTipo(notif.tipo);
        const fecha = formatearFecha(notif.fecha);
        const claseLeida = notif.leida ? 'leida' : '';
        
        return `
            <div class="notificacion-item ${claseLeida}" data-id="${notif.id}">
                <div class="notificacion-icono">${icono}</div>
                <div class="notificacion-contenido">
                    <div class="notificacion-titulo">${notif.titulo}</div>
                    <div class="notificacion-mensaje">${notif.mensaje}</div>
                    <div class="notificacion-fecha">${fecha}</div>
                </div>
                <div class="notificacion-acciones">
                    ${notif.url ? `<a href="${notif.url}" class="btn-notif-ver">Ver</a>` : ''}
                    <button class="btn-notif-eliminar" onclick="eliminarNotificacion(${notif.id})" title="Eliminar">✕</button>
                </div>
            </div>
        `;
    }).join('');

    // Agregar eventos de clic para marcar como leída
    container.querySelectorAll('.notificacion-item:not(.leida)').forEach(item => {
        item.addEventListener('click', function(e) {
            if (!e.target.classList.contains('btn-notif-eliminar') && !e.target.closest('.notificacion-acciones')) {
                marcarNotificacionLeida(parseInt(this.dataset.id));
            }
        });
    });
}

// Obtener icono según tipo de notificación
function obtenerIconoPorTipo(tipo) {
    const iconos = {
        'Mensaje': '💬',
        'SolicitudAmistad': '👥',
        'NuevaReserva': '📅',
        'CartelCercano': '📍',
        'ReservaConfirmada': '✅',
        'ReservaCancelada': '❌',
        'Resena': '⭐'
    };
    return iconos[tipo] || '🔔';
}

// Formatear fecha
function formatearFecha(fechaStr) {
    const fecha = new Date(fechaStr);
    const ahora = new Date();
    const diff = ahora - fecha;
    const minutos = Math.floor(diff / 60000);
    const horas = Math.floor(diff / 3600000);
    const dias = Math.floor(diff / 86400000);

    if (minutos < 1) return 'Ahora';
    if (minutos < 60) return `Hace ${minutos} min`;
    if (horas < 24) return `Hace ${horas} h`;
    if (dias < 7) return `Hace ${dias} d`;
    
    return fecha.toLocaleDateString('es-AR', { day: '2-digit', month: '2-digit' });
}

// Marcar notificación como leída
async function marcarNotificacionLeida(idNotificacion) {
    try {
        const response = await fetch('/Notificacion/MarcarLeida', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ idNotificacion: idNotificacion })
        });

        const data = await response.json();
        if (data.success) {
            const item = document.querySelector(`.notificacion-item[data-id="${idNotificacion}"]`);
            if (item) {
                item.classList.add('leida');
                notificacionesNoLeidas = Math.max(0, notificacionesNoLeidas - 1);
                actualizarBadgeNotificaciones();
            }
        }
    } catch (error) {
        console.error('Error al marcar notificación como leída:', error);
    }
}

// Eliminar notificación
async function eliminarNotificacion(idNotificacion) {
    if (!confirm('¿Eliminar esta notificación?')) return;

    try {
        const response = await fetch('/Notificacion/EliminarNotificacion', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ idNotificacion: idNotificacion })
        });

        const data = await response.json();
        if (data.success) {
            const item = document.querySelector(`.notificacion-item[data-id="${idNotificacion}"]`);
            if (item) {
                item.remove();
                if (!item.classList.contains('leida')) {
                    notificacionesNoLeidas = Math.max(0, notificacionesNoLeidas - 1);
                    actualizarBadgeNotificaciones();
                }
            }
        }
    } catch (error) {
        console.error('Error al eliminar notificación:', error);
    }
}

// Marcar todas como leídas
async function marcarTodasLeidas() {
    try {
        const response = await fetch('/Notificacion/MarcarTodasLeidas', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' }
        });

        const data = await response.json();
        if (data.success) {
            document.querySelectorAll('.notificacion-item').forEach(item => {
                item.classList.add('leida');
            });
            notificacionesNoLeidas = 0;
            actualizarBadgeNotificaciones();
        }
    } catch (error) {
        console.error('Error al marcar todas como leídas:', error);
    }
}

// Limpiar intervalo al salir
window.addEventListener('beforeunload', function() {
    if (notificacionesInterval) {
        clearInterval(notificacionesInterval);
    }
});

