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

// Variable global para almacenar notificaciones
let notificacionesGlobales = [];

// Mostrar notificaciones en el panel
function mostrarNotificacionesEnPanel(notificaciones) {
    const container = document.getElementById('notificacionesList');
    if (!container) return;

    // Guardar notificaciones globalmente para acceso en eventos
    notificacionesGlobales = notificaciones;

    if (notificaciones.length === 0) {
        container.innerHTML = '<div class="notificacion-vacia">No tenés notificaciones</div>';
        return;
    }

    container.innerHTML = notificaciones.map(notif => {
        const icono = obtenerIconoPorTipo(notif.tipo);
        const fecha = formatearFecha(notif.fecha);
        const claseLeida = notif.leida ? 'leida' : '';
        
        // Determinar URL para el botón Ver
        let urlVer = '';
        if (notif.tipo === 'Mensaje' && notif.idReferencia) {
            urlVer = `/Home/Mensajes?amigoId=${notif.idReferencia}`;
        } else if (notif.tipo === 'SolicitudAmistad') {
            urlVer = '/Home/Comunidad';
        } else if (notif.url) {
            urlVer = notif.url;
        }
        
        return `
            <div class="notificacion-item ${claseLeida}" data-id="${notif.id}" data-tipo="${notif.tipo}" data-id-referencia="${notif.idReferencia || ''}" data-url="${notif.url || ''}">
                <div class="notificacion-icono">${icono}</div>
                <div class="notificacion-contenido">
                    <div class="notificacion-titulo">${notif.titulo}</div>
                    <div class="notificacion-mensaje">${notif.mensaje}</div>
                    <div class="notificacion-fecha">${fecha}</div>
                </div>
                <div class="notificacion-acciones">
                    ${urlVer ? `<a href="${urlVer}" class="btn-notif-ver" onclick="event.stopPropagation();">Ver</a>` : ''}
                    <button class="btn-notif-eliminar" onclick="event.stopPropagation(); eliminarNotificacion(${notif.id})" title="Eliminar">✕</button>
                </div>
            </div>
        `;
    }).join('');

    // Agregar eventos de clic para marcar como leída y redirigir
    container.querySelectorAll('.notificacion-item').forEach(item => {
        item.addEventListener('click', function(e) {
            // No hacer nada si se hizo clic en los botones de acción
            if (e.target.classList.contains('btn-notif-eliminar') || 
                e.target.closest('.notificacion-acciones') || 
                e.target.classList.contains('btn-notif-ver') ||
                e.target.closest('.btn-notif-ver')) {
                return;
            }
            
            const notifId = parseInt(this.dataset.id);
            const tipo = this.dataset.tipo;
            const idReferencia = this.dataset.idReferencia ? parseInt(this.dataset.idReferencia) : null;
            const url = this.dataset.url || '';
            
            // Marcar como leída si no está leída
            const notif = notificacionesGlobales.find(n => n.id === notifId);
            if (notif && !notif.leida) {
                marcarNotificacionLeida(notifId);
            }
            
            // Redirigir según el tipo de notificación
            if (tipo === 'Mensaje' && idReferencia) {
                // Redirigir al chat específico
                window.location.href = `/Home/Mensajes?amigoId=${idReferencia}`;
            } else if (tipo === 'SolicitudAmistad') {
                // Redirigir a Comunidad para ver solicitudes
                window.location.href = '/Home/Comunidad';
            } else if (url) {
                // Usar la URL si está disponible
                window.location.href = url;
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
                // Verificar si estaba leída antes de remover
                const estabaLeida = item.classList.contains('leida');
                item.remove();
                
                // Actualizar contador solo si no estaba leída
                if (!estabaLeida) {
                    notificacionesNoLeidas = Math.max(0, notificacionesNoLeidas - 1);
                    actualizarBadgeNotificaciones();
                }
                
                // Actualizar lista global
                notificacionesGlobales = notificacionesGlobales.filter(n => n.id !== idNotificacion);
                
                // Si no quedan notificaciones, mostrar mensaje
                if (notificacionesGlobales.length === 0) {
                    const container = document.getElementById('notificacionesList');
                    if (container) {
                        container.innerHTML = '<div class="notificacion-vacia">No tenés notificaciones</div>';
                    }
                }
            }
        } else {
            alert(data.message || 'Error al eliminar la notificación');
        }
    } catch (error) {
        console.error('Error al eliminar notificación:', error);
        alert('Error al eliminar la notificación');
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

