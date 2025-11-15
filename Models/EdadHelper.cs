using System;

namespace Zooni.Models
{
    public static class EdadHelper
    {
        /// <summary>
        /// Calcula la edad en meses desde la fecha de nacimiento hasta hoy
        /// </summary>
        public static int CalcularEdadEnMeses(DateTime? fechaNacimiento)
        {
            if (!fechaNacimiento.HasValue || fechaNacimiento.Value == DateTime.MinValue)
                return 0;

            var hoy = DateTime.Today;
            var fechaNac = fechaNacimiento.Value.Date;

            // Calcular diferencia total en meses
            int años = hoy.Year - fechaNac.Year;
            int meses = hoy.Month - fechaNac.Month;

            // Ajustar si aún no cumplió años este año
            if (hoy.Day < fechaNac.Day)
            {
                meses--;
            }

            // Si los meses son negativos, ajustar
            if (meses < 0)
            {
                años--;
                meses += 12;
            }

            // Retornar total en meses
            return (años * 12) + meses;
        }

        /// <summary>
        /// Actualiza la edad en la base de datos calculándola desde Fecha_Nacimiento
        /// </summary>
        public static int ActualizarEdadDesdeFechaNacimiento(DateTime? fechaNacimiento, int edadActual)
        {
            if (fechaNacimiento.HasValue && fechaNacimiento.Value != DateTime.MinValue)
            {
                return CalcularEdadEnMeses(fechaNacimiento);
            }
            return edadActual; // Si no hay fecha de nacimiento, mantener la edad actual
        }
    }
}






