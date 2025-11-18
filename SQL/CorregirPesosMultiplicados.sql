-- Script para corregir pesos que están multiplicados por 10 en la base de datos
-- Este script divide por 10 los pesos que son >= 10 y actualiza tanto Peso como PesoDisplay
USE [Zooni]
GO

-- Corregir pesos que están multiplicados por 10 (>= 10 kg y <= 3000 kg)
-- Incluye el caso de 300kg que debería ser 30kg
UPDATE Mascota
SET 
    Peso = ROUND(Peso / 10.0, 2),
    PesoDisplay = CAST(ROUND(Peso / 10.0, 2) AS NVARCHAR(20)) + ' kg'
WHERE 
    Peso >= 10.0 
    AND Peso <= 3000.0  -- Solo corregir pesos razonables (hasta 300 kg * 10)
    AND (
        PesoDisplay IS NULL 
        OR PesoDisplay = '' 
        OR PesoDisplay LIKE '%' + CAST(Peso AS NVARCHAR(20)) + '%'
        OR PesoDisplay LIKE '%' + CAST(CAST(Peso AS INT) AS NVARCHAR(20)) + '%'
    )
GO

-- Corregir pesos muy altos (multiplicados por 100)
UPDATE Mascota
SET 
    Peso = ROUND(Peso / 100.0, 2),
    PesoDisplay = CAST(ROUND(Peso / 100.0, 2) AS NVARCHAR(20)) + ' kg'
WHERE 
    Peso > 3000.0 
    AND Peso <= 30000.0  -- Solo corregir pesos razonables (hasta 300 kg * 100)
    AND (
        PesoDisplay IS NULL 
        OR PesoDisplay = '' 
        OR PesoDisplay LIKE '%' + CAST(Peso AS NVARCHAR(20)) + '%'
    )
GO

-- Asegurar que todos los pesos estén en el rango válido (0.1 - 300 kg)
UPDATE Mascota
SET Peso = 0.1
WHERE Peso < 0.1
GO

UPDATE Mascota
SET Peso = 300.0
WHERE Peso > 300.0
GO

-- Redondear todos los pesos a 2 decimales
UPDATE Mascota
SET Peso = ROUND(Peso, 2)
WHERE Peso IS NOT NULL
GO

-- Actualizar PesoDisplay para todos los registros que no lo tengan o esté incorrecto
UPDATE Mascota
SET PesoDisplay = CAST(ROUND(Peso, 2) AS NVARCHAR(20)) + ' kg'
WHERE PesoDisplay IS NULL 
   OR PesoDisplay = ''
   OR PesoDisplay NOT LIKE CAST(ROUND(Peso, 2) AS NVARCHAR(20)) + '%'
GO

PRINT 'Pesos corregidos exitosamente'
GO

