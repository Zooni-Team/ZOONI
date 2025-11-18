-- Script para crear tabla de Reservas de Proveedores de Servicios
USE [Zooni]
GO

-- Crear tabla ReservaProveedor para coordinar servicios con proveedores
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ReservaProveedor]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ReservaProveedor](
        [Id_Reserva] [int] IDENTITY(1,1) NOT NULL,
        [Id_User] [int] NOT NULL, -- Dueño de mascota que contrata
        [Id_Proveedor] [int] NOT NULL, -- Proveedor de servicio
        [Id_Mascota] [int] NOT NULL,
        [Id_TipoServicio] [int] NOT NULL, -- Paseo, Cuidado, etc.
        [Fecha_Inicio] [datetime2](7) NOT NULL,
        [Fecha_Fin] [datetime2](7) NULL, -- Para servicios de cuidado extendido
        [Hora_Inicio] [time](0) NOT NULL,
        [Hora_Fin] [time](0) NULL,
        [Duracion_Horas] [decimal](5,2) NULL,
        [Precio_Total] [decimal](12, 2) NOT NULL,
        [Id_EstadoReserva] [int] NOT NULL DEFAULT 1, -- 1=Pendiente, 2=Confirmada, 3=EnCurso, 4=Completada, 5=Cancelada
        [Notas] [nvarchar](1000) NULL,
        [Direccion_Servicio] [nvarchar](500) NULL, -- Dirección donde se realizará el servicio
        [Latitud_Servicio] [decimal](10, 8) NULL,
        [Longitud_Servicio] [decimal](11, 8) NULL,
        [Compartir_Ubicacion] [bit] NOT NULL DEFAULT 0, -- Si el dueño quiere compartir ubicación durante el servicio
        [Fecha_Creacion] [datetime2](7) NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [PK_ReservaProveedor] PRIMARY KEY CLUSTERED 
        (
            [Id_Reserva] ASC
        )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY]
END
GO

-- Agregar Foreign Keys
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_ReservaProveedor_User')
BEGIN
    ALTER TABLE [dbo].[ReservaProveedor] WITH CHECK ADD CONSTRAINT [FK_ReservaProveedor_User] 
    FOREIGN KEY([Id_User]) REFERENCES [dbo].[User] ([Id_User]) ON DELETE CASCADE
    ALTER TABLE [dbo].[ReservaProveedor] CHECK CONSTRAINT [FK_ReservaProveedor_User]
END
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_ReservaProveedor_Proveedor')
BEGIN
    ALTER TABLE [dbo].[ReservaProveedor] WITH CHECK ADD CONSTRAINT [FK_ReservaProveedor_Proveedor] 
    FOREIGN KEY([Id_Proveedor]) REFERENCES [dbo].[ProveedorServicio] ([Id_Proveedor]) ON DELETE CASCADE
    ALTER TABLE [dbo].[ReservaProveedor] CHECK CONSTRAINT [FK_ReservaProveedor_Proveedor]
END
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_ReservaProveedor_Mascota')
BEGIN
    ALTER TABLE [dbo].[ReservaProveedor] WITH CHECK ADD CONSTRAINT [FK_ReservaProveedor_Mascota] 
    FOREIGN KEY([Id_Mascota]) REFERENCES [dbo].[Mascota] ([Id_Mascota]) ON DELETE CASCADE
    ALTER TABLE [dbo].[ReservaProveedor] CHECK CONSTRAINT [FK_ReservaProveedor_Mascota]
END
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_ReservaProveedor_TipoServicio')
BEGIN
    ALTER TABLE [dbo].[ReservaProveedor] WITH CHECK ADD CONSTRAINT [FK_ReservaProveedor_TipoServicio] 
    FOREIGN KEY([Id_TipoServicio]) REFERENCES [dbo].[TipoServicio] ([Id_TipoServicio])
    ALTER TABLE [dbo].[ReservaProveedor] CHECK CONSTRAINT [FK_ReservaProveedor_TipoServicio]
END
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_ReservaProveedor_EstadoReserva')
BEGIN
    ALTER TABLE [dbo].[ReservaProveedor] WITH CHECK ADD CONSTRAINT [FK_ReservaProveedor_EstadoReserva] 
    FOREIGN KEY([Id_EstadoReserva]) REFERENCES [dbo].[EstadoReserva] ([Id_EstadoReserva])
    ALTER TABLE [dbo].[ReservaProveedor] CHECK CONSTRAINT [FK_ReservaProveedor_EstadoReserva]
END
GO

-- Crear tabla para seguimiento de ubicación en tiempo real durante el servicio
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UbicacionServicio]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[UbicacionServicio](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Id_Reserva] [int] NOT NULL,
        [Id_Proveedor] [int] NOT NULL,
        [Latitud] [decimal](10, 8) NOT NULL,
        [Longitud] [decimal](11, 8) NOT NULL,
        [Fecha_Hora] [datetime2](7) NOT NULL DEFAULT GETDATE(),
        [Tipo] [nvarchar](20) NOT NULL DEFAULT 'Proveedor', -- 'Proveedor' o 'Mascota'
        CONSTRAINT [PK_UbicacionServicio] PRIMARY KEY CLUSTERED 
        (
            [Id] ASC
        )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY]
END
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_UbicacionServicio_Reserva')
BEGIN
    ALTER TABLE [dbo].[UbicacionServicio] WITH CHECK ADD CONSTRAINT [FK_UbicacionServicio_Reserva] 
    FOREIGN KEY([Id_Reserva]) REFERENCES [dbo].[ReservaProveedor] ([Id_Reserva]) ON DELETE CASCADE
    ALTER TABLE [dbo].[UbicacionServicio] CHECK CONSTRAINT [FK_UbicacionServicio_Reserva]
END
GO

-- Crear índices para mejorar rendimiento
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ReservaProveedor_Proveedor')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ReservaProveedor_Proveedor] ON [dbo].[ReservaProveedor]
    (
        [Id_Proveedor] ASC,
        [Id_EstadoReserva] ASC,
        [Fecha_Inicio] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ReservaProveedor_User')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ReservaProveedor_User] ON [dbo].[ReservaProveedor]
    (
        [Id_User] ASC,
        [Id_EstadoReserva] ASC,
        [Fecha_Inicio] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
END
GO

PRINT 'Tabla ReservaProveedor creada exitosamente'
GO

