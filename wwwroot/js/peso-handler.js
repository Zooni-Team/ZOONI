function formatearPeso(input) {
    // Eliminar caracteres no numéricos excepto punto y coma
    let valor = input.value.replace(/[^\d.,]/g, '');
    
    // Reemplazar coma por punto para cálculos
    valor = valor.replace(',', '.');
    
    // Convertir a número y validar rango
    let numero = parseFloat(valor);
    if (isNaN(numero)) numero = 0;
    
    // Aplicar límites
    numero = Math.max(0.1, Math.min(300, numero));
    
    // Redondear a 2 decimales
    numero = Math.round(numero * 100) / 100;
    
    // Formatear para mostrar con coma decimal
    input.value = numero.toLocaleString('es-ES', {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    });
    
    // Guardar valor normalizado para el formulario
    input.setAttribute('data-peso', numero.toString());
}

// Aplicar a todos los inputs de peso
document.addEventListener('DOMContentLoaded', function() {
    document.querySelectorAll('input[data-tipo="peso"]').forEach(function(input) {
        input.addEventListener('input', function() {
            formatearPeso(this);
        });
        input.addEventListener('blur', function() {
            formatearPeso(this);
        });
    });
});