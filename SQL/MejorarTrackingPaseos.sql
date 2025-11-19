-- Script para mejorar el sistema de tracking de paseos
USE [Zooni]
GO

-- Agregar campos adicionales a UbicacionServicio para tracking completo
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[UbicacionServicio]') AND name = 'Distancia_Acumulada_Metros')
BEGIN
    ALTER TABLE [dbo].[UbicacionServicio] ADD [Distancia_Acumulada_Metros] [decimal](10,2) NULL DEFAULT 0
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[UbicacionServicio]') AND name = 'Tiempo_Transcurrido_Segundos')
BEGIN
    ALTER TABLE [dbo].[UbicacionServicio] ADD [Tiempo_Transcurrido_Segundos] [int] NULL DEFAULT 0
END
GO

-- Agregar campos a ReservaProveedor para resumen del paseo
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ReservaProveedor]') AND name = 'Distancia_Total_Metros')
BEGIN
    ALTER TABLE [dbo].[ReservaProveedor] ADD [Distancia_Total_Metros] [decimal](10,2) NULL
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ReservaProveedor]') AND name = 'Tiempo_Total_Segundos')
BEGIN
    ALTER TABLE [dbo].[ReservaProveedor] ADD [Tiempo_Total_Segundos] [int] NULL
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ReservaProveedor]') AND name = 'Ruta_GPS_JSON')
BEGIN
    ALTER TABLE [dbo].[ReservaProveedor] ADD [Ruta_GPS_JSON] [nvarchar](max) NULL -- JSON con todas las coordenadas
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ReservaProveedor]') AND name = 'Fecha_Hora_Inicio_Real')
BEGIN
    ALTER TABLE [dbo].[ReservaProveedor] ADD [Fecha_Hora_Inicio_Real] [datetime2](7) NULL -- Cuando realmente empezó el servicio
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ReservaProveedor]') AND name = 'Fecha_Hora_Fin_Real')
BEGIN
    ALTER TABLE [dbo].[ReservaProveedor] ADD [Fecha_Hora_Fin_Real] [datetime2](7) NULL -- Cuando realmente terminó el servicio
END
GO

-- Crear índice para búsquedas de servicios activos
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ReservaProveedor_EstadoActivo')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ReservaProveedor_EstadoActivo] ON [dbo].[ReservaProveedor]
    (
        [Id_EstadoReserva] ASC,
        [Id_Proveedor] ASC,
        [Fecha_Inicio] ASC
    )
    WHERE [Id_EstadoReserva] = 3 -- Solo servicios en curso
    WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
END
GO

-- Crear índice para tracking de ubicaciones
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_UbicacionServicio_Reserva')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_UbicacionServicio_Reserva] ON [dbo].[UbicacionServicio]
    (
        [Id_Reserva] ASC,
        [Fecha_Hora] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
END
GO

PRINT 'Sistema de tracking de paseos mejorado exitosamente'
GO

