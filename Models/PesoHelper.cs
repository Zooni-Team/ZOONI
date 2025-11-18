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
                
                // ✅ CORRECCIÓN SOLO PARA PESOS CLARAMENTE INCORRECTOS
                // Solo dividir si el peso es claramente demasiado alto (ej: 473 kg, 300 kg sin decimales)
                // NO dividir pesos válidos como 47.3 kg, 30.5 kg, etc.
                
                // Si el peso es >= 100 kg (muy raro para mascotas), podría estar multiplicado
                // Pero solo si no tiene decimales significativos (sugiere error de entrada)
                if (peso >= 100.0M && peso <= 3000.0M)
                {
                    // Solo corregir si parece un número redondo multiplicado (ej: 300, 473, 500)
                    // No corregir si tiene decimales razonables (ej: 47.3, 123.5)
                    decimal parteDecimal = peso - Math.Floor(peso);
                    // Si es un número muy redondo (sin decimales o con .0) y está en rango sospechoso
                    if (parteDecimal < 0.1M && peso > 80.0M) // Solo números redondos > 80 kg
                    {
                        decimal pesoCorregido = peso / 10.0M;
                        // Validar que el peso corregido sea razonable
                        if (pesoCorregido >= MIN_PESO && pesoCorregido <= MAX_PESO)
                        {
                            peso = pesoCorregido;
                            pesoDisplay = peso.ToString("F2", CultureInfo.InvariantCulture).Replace('.', ',');
                        }
                    }
                }
                // Si es extremadamente alto (multiplicado por 100)
                else if (peso > 3000.0M && peso <= 30000.0M)
                {
                    decimal pesoCorregido = peso / 100.0M;
                    if (pesoCorregido >= MIN_PESO && pesoCorregido <= MAX_PESO)
                    {
                        peso = pesoCorregido;
                        pesoDisplay = peso.ToString("F2", CultureInfo.InvariantCulture).Replace('.', ',');
                    }
                }
                
                // Para almacenamiento interno, validamos rango
                if (peso < MIN_PESO) peso = MIN_PESO;
                if (peso > MAX_PESO) peso = MAX_PESO;
                
                // Formatear display
                if (!pesoDisplay.Contains("kg"))
                {
                    pesoDisplay = peso.ToString("F2", CultureInfo.InvariantCulture).Replace('.', ',') + " kg";
                }
                else if (!pesoDisplay.EndsWith(" kg"))
                {
                    pesoDisplay = pesoDisplay.Replace("kg", "").Trim() + " kg";
                }
                
                return (peso, pesoDisplay);
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
