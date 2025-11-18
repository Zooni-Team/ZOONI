-- Script para agregar campos de ubicación y radio de atención a ProveedorServicio
USE [Zooni]
GO

-- Agregar campos de ubicación (latitud, longitud) para ubicación precisa
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ProveedorServicio]') AND name = 'Latitud')
BEGIN
    ALTER TABLE [dbo].[ProveedorServicio] ADD [Latitud] [decimal](10, 8) NULL
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ProveedorServicio]') AND name = 'Longitud')
BEGIN
    ALTER TABLE [dbo].[ProveedorServicio] ADD [Longitud] [decimal](11, 8) NULL
END
GO

-- Agregar radio de atención en kilómetros (para paseadores - zona de cobertura)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ProveedorServicio]') AND name = 'Radio_Atencion_Km')
BEGIN
    ALTER TABLE [dbo].[ProveedorServicio] ADD [Radio_Atencion_Km] [decimal](10, 2) NULL DEFAULT 5.00
END
GO

-- Agregar campo para indicar si es ubicación precisa (cuidadores) o zona de cobertura (paseadores)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ProveedorServicio]') AND name = 'Tipo_Ubicacion')
BEGIN
    ALTER TABLE [dbo].[ProveedorServicio] ADD [Tipo_Ubicacion] [nvarchar](20) NULL DEFAULT 'Cobertura'
    -- Valores posibles: 'Cobertura' (paseadores - zona), 'Precisa' (cuidadores - ubicación exacta)
END
GO

-- Crear índice para búsquedas por ubicación
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ProveedorServicio_Ubicacion')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ProveedorServicio_Ubicacion] ON [dbo].[ProveedorServicio]
    (
        [Latitud] ASC,
        [Longitud] ASC,
        [Estado] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
END
GO

PRINT 'Campos de ubicación agregados a ProveedorServicio exitosamente'
GO

