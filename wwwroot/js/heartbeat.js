// 🟢 Script global para mantener el estado online del usuario
(function() {
    'use strict';
    
    let heartbeatInterval = null;
    let isPageVisible = true;
    
    // Función para enviar heartbeat
    function enviarHeartbeat() {
        fetch('/Home/Heartbeat', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            }
        })
        .then(response => response.json())
        .then(data => {
            if (!data.success) {
                console.warn('⚠️ Error en heartbeat:', data.message);
            }
        })
        .catch(error => {
            console.error('❌ Error al enviar heartbeat:', error);
        });
    }
    
    // Detectar cuando la página se oculta/muestra
    document.addEventListener('visibilitychange', function() {
        isPageVisible = !document.hidden;
        if (isPageVisible) {
            // Si la página vuelve a ser visible, enviar heartbeat inmediatamente
            enviarHeartbeat();
        }
    });
    
    // Detectar cuando la ventana pierde el foco
    window.addEventListener('blur', function() {
        // Opcional: podrías pausar el heartbeat cuando la ventana pierde el foco
        // pero por ahora lo mantenemos activo
    });
    
    // Detectar cuando la ventana recupera el foco
    window.addEventListener('focus', function() {
        enviarHeartbeat();
    });
    
    // Iniciar heartbeat cuando se carga la página
    function iniciarHeartbeat() {
        // Enviar heartbeat inmediatamente
        enviarHeartbeat();
        
        // Luego enviar cada 2 minutos (120 segundos)
        heartbeatInterval = setInterval(enviarHeartbeat, 120000);
    }
    
    // Detener heartbeat cuando se cierra la página
    window.addEventListener('beforeunload', function() {
        if (heartbeatInterval) {
            clearInterval(heartbeatInterval);
        }
        
        // Intentar marcar como offline (puede que no siempre se ejecute)
        navigator.sendBeacon('/Home/Heartbeat', JSON.stringify({ offline: true }));
    });
    
    // Iniciar cuando el DOM esté listo
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', iniciarHeartbeat);
    } else {
        iniciarHeartbeat();
    }
})();

