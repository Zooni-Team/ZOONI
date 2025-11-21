-- Script para corregir la precisión de Latitud y Longitud en CartelMascota
-- El problema: DECIMAL(10,8) y DECIMAL(11,8) no son suficientes para coordenadas GPS
-- Solución: Cambiar a DECIMAL(11,8) para Latitud y DECIMAL(12,8) para Longitud

USE [Zooni]
GO

IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CartelMascota')
BEGIN
    PRINT 'Corrigiendo precisión de coordenadas en CartelMascota...';
END
GO

-- Corregir Latitud a DECIMAL(11,8)
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CartelMascota')
BEGIN
    BEGIN TRY
        ALTER TABLE CartelMascota ALTER COLUMN Latitud DECIMAL(11, 8) NOT NULL;
        PRINT '✅ Latitud corregida a DECIMAL(11,8)';
    END TRY
    BEGIN CATCH
        PRINT '⚠️ Intentando corrección de Latitud con paso intermedio...';
        BEGIN TRY
            ALTER TABLE CartelMascota ALTER COLUMN Latitud DECIMAL(11, 8) NULL;
            ALTER TABLE CartelMascota ALTER COLUMN Latitud DECIMAL(11, 8) NOT NULL;
            PRINT '✅ Latitud corregida a DECIMAL(11,8) (con paso intermedio)';
        END TRY
        BEGIN CATCH
            PRINT '❌ Error al corregir Latitud: ' + ERROR_MESSAGE();
        END CATCH
    END CATCH
END
GO

-- Corregir Longitud a DECIMAL(12,8)
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CartelMascota')
BEGIN
    BEGIN TRY
        ALTER TABLE CartelMascota ALTER COLUMN Longitud DECIMAL(12, 8) NOT NULL;
        PRINT '✅ Longitud corregida a DECIMAL(12,8)';
    END TRY
    BEGIN CATCH
        PRINT '⚠️ Intentando corrección de Longitud con paso intermedio...';
        BEGIN TRY
            ALTER TABLE CartelMascota ALTER COLUMN Longitud DECIMAL(12, 8) NULL;
            ALTER TABLE CartelMascota ALTER COLUMN Longitud DECIMAL(12, 8) NOT NULL;
            PRINT '✅ Longitud corregida a DECIMAL(12,8) (con paso intermedio)';
        END TRY
        BEGIN CATCH
            PRINT '❌ Error al corregir Longitud: ' + ERROR_MESSAGE();
        END CATCH
    END CATCH
END
GO

PRINT 'Corrección de coordenadas completada.';
GO
