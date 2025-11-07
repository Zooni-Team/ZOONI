using System;
using System.Globalization;

namespace Zooni.Models
{
    public static class PesoHelper
    {
        public const decimal MIN_PESO = 0.1M;
        public const decimal MAX_PESO = 300M;

        public static (decimal peso, string display) NormalizarPeso(string pesoInput)
        {
            if (string.IsNullOrWhiteSpace(pesoInput))
                return (MIN_PESO, $"{MIN_PESO:F2}".Replace('.', ',') + " kg");

            // Limpiar la entrada y reemplazar coma por punto
            pesoInput = pesoInput.Trim();
            string pesoDisplay = pesoInput;
            pesoInput = pesoInput.Replace(',', '.');
            
            if (decimal.TryParse(pesoInput, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal peso))
            {
                peso = Math.Round(peso, 2);
                
                // Para almacenamiento interno, validamos rango
                if (peso < MIN_PESO) peso = MIN_PESO;
                if (peso > MAX_PESO) peso = MAX_PESO;
                
                // Pero mantenemos el valor original para mostrar
                return (peso, pesoDisplay + " kg");
            }
            
            return (MIN_PESO, $"{MIN_PESO:F2}".Replace('.', ',') + " kg");
        }

        public static string FormatearPeso(decimal? peso, string displayOverride = null)
        {
            if (!string.IsNullOrEmpty(displayOverride))
                return displayOverride.EndsWith(" kg") ? displayOverride : displayOverride + " kg";

            if (!peso.HasValue)
                return $"{MIN_PESO:F2}".Replace('.', ',') + " kg";

            // Usamos el valor almacenado normalizado
            var pesoNormalizado = Math.Max(MIN_PESO, Math.Min(MAX_PESO, peso.Value));
            return $"{Math.Round(pesoNormalizado, 2):F2}".Replace('.', ',') + " kg";
        }

        public static decimal NormalizarPesoDecimal(decimal? peso)
        {
            if (!peso.HasValue) return MIN_PESO;
            return Math.Max(MIN_PESO, Math.Min(MAX_PESO, Math.Round(peso.Value, 2)));
        }

        public static bool ValidarPesoParaEspecie(decimal peso, string especie)
        {
            var maxPorEspecie = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
            {
                {"perro", 80M}, {"gato", 15M}, {"conejo", 5M}, 
                {"ave", 3M}, {"pez", 2M}, {"reptil", 20M},
                {"hamster", 0.25M}, {"raton", 0.6M}
            };

            if (maxPorEspecie.TryGetValue(especie, out decimal maxPeso))
            {
                return peso <= maxPeso;
            }

            return peso <= MAX_PESO;
        }
    }
}
