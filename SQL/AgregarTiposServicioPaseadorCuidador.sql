-- Script para agregar tipos de servicio "Paseador" y "Cuidador" si no existen
USE [Zooni]
GO

-- Agregar "Paseador" si no existe
IF NOT EXISTS (SELECT * FROM TipoServicio WHERE Descripcion = 'Paseador')
BEGIN
    INSERT INTO TipoServicio (Descripcion) VALUES ('Paseador');
    PRINT 'Tipo de servicio "Paseador" agregado';
END
ELSE
BEGIN
    PRINT 'Tipo de servicio "Paseador" ya existe';
END
GO

-- Agregar "Cuidador" si no existe
IF NOT EXISTS (SELECT * FROM TipoServicio WHERE Descripcion = 'Cuidador')
BEGIN
    INSERT INTO TipoServicio (Descripcion) VALUES ('Cuidador');
    PRINT 'Tipo de servicio "Cuidador" agregado';
END
ELSE
BEGIN
    PRINT 'Tipo de servicio "Cuidador" ya existe';
END
GO

PRINT 'Verificación de tipos de servicio completada'
GO

