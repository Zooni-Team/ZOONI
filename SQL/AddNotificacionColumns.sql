-- Script para agregar columnas faltantes a la tabla Notificacion
-- Ejecutar este script para actualizar la estructura de la tabla

-- Verificar si la columna Tipo existe antes de agregarla
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Notificacion]') AND name = 'Tipo')
BEGIN
    ALTER TABLE [dbo].[Notificacion]
    ADD [Tipo] [nvarchar](50) NOT NULL DEFAULT 'General';
    PRINT 'Columna Tipo agregada';
END
GO

-- Verificar si la columna Id_Referencia existe antes de agregarla
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Notificacion]') AND name = 'Id_Referencia')
BEGIN
    ALTER TABLE [dbo].[Notificacion]
    ADD [Id_Referencia] [int] NULL;
    PRINT 'Columna Id_Referencia agregada';
END
GO

-- Verificar si la columna Url existe antes de agregarla
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Notificacion]') AND name = 'Url')
BEGIN
    ALTER TABLE [dbo].[Notificacion]
    ADD [Url] [nvarchar](500) NULL;
    PRINT 'Columna Url agregada';
END
GO

-- Verificar si la columna Eliminada existe antes de agregarla
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Notificacion]') AND name = 'Eliminada')
BEGIN
    ALTER TABLE [dbo].[Notificacion]
    ADD [Eliminada] [bit] NOT NULL DEFAULT 0;
    PRINT 'Columna Eliminada agregada';
END
GO

PRINT 'Actualización de tabla Notificacion completada';

