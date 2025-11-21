    -- Script para corregir el tipo de dato del campo TelefonoContacto en CartelMascota
    -- Este script convierte el campo de numérico a NVARCHAR para permitir números de teléfono como texto

    -- Verificar si la tabla existe
    IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CartelMascota')
    BEGIN
        -- Verificar si el campo TelefonoContacto es numérico
        IF EXISTS (
            SELECT 1 FROM sys.columns c
            INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
            WHERE c.object_id = OBJECT_ID('CartelMascota') 
            AND c.name = 'TelefonoContacto'
            AND (t.name IN ('numeric', 'int', 'bigint', 'decimal', 'float', 'real', 'smallint', 'tinyint'))
        )
        BEGIN
            PRINT 'Convirtiendo TelefonoContacto de numérico a NVARCHAR...';
            
            -- Paso 1: Crear columna temporal si no existe
            IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('CartelMascota') AND name = 'TelefonoContacto_Temp')
            BEGIN
                ALTER TABLE CartelMascota ADD TelefonoContacto_Temp NVARCHAR(50) NULL;
                PRINT 'Columna temporal creada.';
            END
        END
    END
    GO

    -- Paso 2: Copiar y convertir datos existentes
    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('CartelMascota') AND name = 'TelefonoContacto_Temp')
    AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('CartelMascota') AND name = 'TelefonoContacto')
    BEGIN
        DECLARE @sqlUpdate NVARCHAR(MAX);
        SET @sqlUpdate = N'
            UPDATE CartelMascota 
            SET TelefonoContacto_Temp = CONVERT(NVARCHAR(50), TelefonoContacto)
            WHERE TelefonoContacto_Temp IS NULL';
        EXEC sp_executesql @sqlUpdate;
        PRINT 'Datos copiados a columna temporal.';
    END
    GO

    -- Paso 3: Eliminar la columna antigua
    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('CartelMascota') AND name = 'TelefonoContacto')
    AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('CartelMascota') AND name = 'TelefonoContacto_Temp')
    BEGIN
        ALTER TABLE CartelMascota DROP COLUMN TelefonoContacto;
        PRINT 'Columna antigua eliminada.';
    END
    GO

    -- Paso 4: Renombrar la columna temporal
    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('CartelMascota') AND name = 'TelefonoContacto_Temp')
    BEGIN
        EXEC sp_rename 'CartelMascota.TelefonoContacto_Temp', 'TelefonoContacto', 'COLUMN';
        PRINT 'Columna temporal renombrada.';
    END
    GO

    -- Paso 5: Establecer valores por defecto para NULL si existen
    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('CartelMascota') AND name = 'TelefonoContacto')
    BEGIN
        UPDATE CartelMascota SET TelefonoContacto = '0000000000' WHERE TelefonoContacto IS NULL;
        PRINT 'Valores NULL actualizados.';
    END
    GO

    -- Paso 6: Establecer como NOT NULL
    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('CartelMascota') AND name = 'TelefonoContacto')
    BEGIN
        BEGIN TRY
            ALTER TABLE CartelMascota ALTER COLUMN TelefonoContacto NVARCHAR(50) NOT NULL;
            PRINT 'Conversión completada exitosamente.';
        END TRY
        BEGIN CATCH
            PRINT 'Error al establecer NOT NULL: ' + ERROR_MESSAGE();
            PRINT 'Intentando establecer valores por defecto primero...';
            UPDATE CartelMascota SET TelefonoContacto = '0000000000' WHERE TelefonoContacto IS NULL;
            ALTER TABLE CartelMascota ALTER COLUMN TelefonoContacto NVARCHAR(50) NOT NULL;
            PRINT 'Conversión completada exitosamente después de corregir valores NULL.';
        END CATCH
    END
    GO
