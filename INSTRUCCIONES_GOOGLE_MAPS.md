# Instrucciones para Configurar Google Maps API

## Pasos para obtener tu API Key de Google Maps:

1. Ve a [Google Cloud Console](https://console.cloud.google.com/)
2. Crea un nuevo proyecto o selecciona uno existente
3. Habilita la **Maps JavaScript API**:
   - Ve a "APIs & Services" > "Library"
   - Busca "Maps JavaScript API"
   - Haz clic en "Enable"
4. Crea credenciales:
   - Ve a "APIs & Services" > "Credentials"
   - Haz clic en "Create Credentials" > "API Key"
   - Copia tu API Key
5. Reemplaza `YOUR_API_KEY` en `Views/Home/Comunidad.cshtml` línea 15 con tu API Key real

## Nota sobre el costo:
- Google Maps ofrece $200 USD de crédito gratuito por mes
- Esto cubre aproximadamente 28,000 cargas de mapas
- Para uso personal/desarrollo es completamente gratuito

## Alternativa sin API Key (OpenStreetMap):
Si prefieres no usar Google Maps, puedes usar Leaflet con OpenStreetMap que es completamente gratuito y no requiere API Key.

