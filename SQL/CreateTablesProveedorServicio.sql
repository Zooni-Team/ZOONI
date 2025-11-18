-- Script para crear tabla de Proveedores de Servicios
USE [Zooni]
GO

-- Agregar tipo de usuario "Proveedor" si no existe
IF NOT EXISTS (SELECT * FROM TipoUsuario WHERE Descripcion = 'Proveedor')
BEGIN
    INSERT INTO TipoUsuario (Descripcion) VALUES ('Proveedor');
END
GO

-- Insertar tipos de servicio básicos si no existen
-- Tipos principales: Paseador y Cuidador
IF NOT EXISTS (SELECT * FROM TipoServicio WHERE Descripcion = 'Paseador')
BEGIN
    INSERT INTO TipoServicio (Descripcion) VALUES ('Paseador');
END
GO

IF NOT EXISTS (SELECT * FROM TipoServicio WHERE Descripcion = 'Cuidador')
BEGIN
    INSERT INTO TipoServicio (Descripcion) VALUES ('Cuidador');
END
GO

-- Tipos adicionales (opcionales)
IF NOT EXISTS (SELECT * FROM TipoServicio WHERE Descripcion = 'Paseo')
BEGIN
    INSERT INTO TipoServicio (Descripcion) VALUES ('Paseo');
END
GO

IF NOT EXISTS (SELECT * FROM TipoServicio WHERE Descripcion = 'Cuidado')
BEGIN
    INSERT INTO TipoServicio (Descripcion) VALUES ('Cuidado');
END
GO

IF NOT EXISTS (SELECT * FROM TipoServicio WHERE Descripcion = 'Guardería')
BEGIN
    INSERT INTO TipoServicio (Descripcion) VALUES ('Guardería');
END
GO

IF NOT EXISTS (SELECT * FROM TipoServicio WHERE Descripcion = 'Peluquería')
BEGIN
    INSERT INTO TipoServicio (Descripcion) VALUES ('Peluquería');
END
GO

-- Crear tabla ProveedorServicio
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ProveedorServicio]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ProveedorServicio](
        [Id_Proveedor] [int] IDENTITY(1,1) NOT NULL,
        [Id_User] [int] NOT NULL,
        [DNI] [nvarchar](20) NOT NULL,
        [NombreCompleto] [nvarchar](200) NOT NULL,
        [Experiencia_Anios] [int] NOT NULL DEFAULT 0,
        [Descripcion] [nvarchar](1000) NULL,
        [FotoPerfil] [nvarchar](500) NULL,
        [Telefono] [nvarchar](30) NULL,
        [Direccion] [nvarchar](200) NULL,
        [Ciudad] [nvarchar](100) NULL,
        [Provincia] [nvarchar](100) NULL,
        [Pais] [nvarchar](100) NULL,
        [Precio_Hora] [decimal](12, 2) NULL,
        [Calificacion_Promedio] [decimal](4, 2) NULL DEFAULT 0,
        [Cantidad_Resenas] [int] NOT NULL DEFAULT 0,
        [Estado] [bit] NOT NULL DEFAULT 1,
        [Fecha_Registro] [datetime2](7) NOT NULL DEFAULT GETDATE(),
        [Verificado] [bit] NOT NULL DEFAULT 0,
        CONSTRAINT [PK_ProveedorServicio] PRIMARY KEY CLUSTERED 
        (
            [Id_Proveedor] ASC
        )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
        CONSTRAINT [UQ_ProveedorServicio_User] UNIQUE NONCLUSTERED 
        (
            [Id_User] ASC
        )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
        CONSTRAINT [UQ_ProveedorServicio_DNI] UNIQUE NONCLUSTERED 
        (
            [DNI] ASC
        )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY]
END
GO

-- Crear tabla ProveedorServicio_TipoServicio (servicios que ofrece)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ProveedorServicio_TipoServicio]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ProveedorServicio_TipoServicio](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Id_Proveedor] [int] NOT NULL,
        [Id_TipoServicio] [int] NOT NULL,
        CONSTRAINT [PK_ProveedorServicio_TipoServicio] PRIMARY KEY CLUSTERED 
        (
            [Id] ASC
        )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
        CONSTRAINT [UQ_Proveedor_TipoServicio] UNIQUE NONCLUSTERED 
        (
            [Id_Proveedor] ASC,
            [Id_TipoServicio] ASC
        )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY]
END
GO

-- Crear tabla ProveedorServicio_Especie (especies que atiende)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ProveedorServicio_Especie]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ProveedorServicio_Especie](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Id_Proveedor] [int] NOT NULL,
        [Especie] [nvarchar](50) NOT NULL,
        CONSTRAINT [PK_ProveedorServicio_Especie] PRIMARY KEY CLUSTERED 
        (
            [Id] ASC
        )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
        CONSTRAINT [UQ_Proveedor_Especie] UNIQUE NONCLUSTERED 
        (
            [Id_Proveedor] ASC,
            [Especie] ASC
        )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY]
END
GO

-- Agregar Foreign Keys
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_ProveedorServicio_User')
BEGIN
    ALTER TABLE [dbo].[ProveedorServicio] WITH CHECK ADD CONSTRAINT [FK_ProveedorServicio_User] 
    FOREIGN KEY([Id_User]) REFERENCES [dbo].[User] ([Id_User]) ON DELETE CASCADE
    ALTER TABLE [dbo].[ProveedorServicio] CHECK CONSTRAINT [FK_ProveedorServicio_User]
END
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Proveedor_TipoServicio_Proveedor')
BEGIN
    ALTER TABLE [dbo].[ProveedorServicio_TipoServicio] WITH CHECK ADD CONSTRAINT [FK_Proveedor_TipoServicio_Proveedor] 
    FOREIGN KEY([Id_Proveedor]) REFERENCES [dbo].[ProveedorServicio] ([Id_Proveedor]) ON DELETE CASCADE
    ALTER TABLE [dbo].[ProveedorServicio_TipoServicio] CHECK CONSTRAINT [FK_Proveedor_TipoServicio_Proveedor]
END
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Proveedor_TipoServicio_TipoServicio')
BEGIN
    ALTER TABLE [dbo].[ProveedorServicio_TipoServicio] WITH CHECK ADD CONSTRAINT [FK_Proveedor_TipoServicio_TipoServicio] 
    FOREIGN KEY([Id_TipoServicio]) REFERENCES [dbo].[TipoServicio] ([Id_TipoServicio])
    ALTER TABLE [dbo].[ProveedorServicio_TipoServicio] CHECK CONSTRAINT [FK_Proveedor_TipoServicio_TipoServicio]
END
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Proveedor_Especie_Proveedor')
BEGIN
    ALTER TABLE [dbo].[ProveedorServicio_Especie] WITH CHECK ADD CONSTRAINT [FK_Proveedor_Especie_Proveedor] 
    FOREIGN KEY([Id_Proveedor]) REFERENCES [dbo].[ProveedorServicio] ([Id_Proveedor]) ON DELETE CASCADE
    ALTER TABLE [dbo].[ProveedorServicio_Especie] CHECK CONSTRAINT [FK_Proveedor_Especie_Proveedor]
END
GO

-- Modificar tabla Resena para soportar proveedores
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Resena]') AND name = 'Id_Proveedor')
BEGIN
    ALTER TABLE [dbo].[Resena] ADD [Id_Proveedor] [int] NULL
END
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Resena_Proveedor')
BEGIN
    ALTER TABLE [dbo].[Resena] WITH CHECK ADD CONSTRAINT [FK_Resena_Proveedor] 
    FOREIGN KEY([Id_Proveedor]) REFERENCES [dbo].[ProveedorServicio] ([Id_Proveedor]) ON DELETE CASCADE
    ALTER TABLE [dbo].[Resena] CHECK CONSTRAINT [FK_Resena_Proveedor]
END
GO

-- Crear índices para mejorar rendimiento
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ProveedorServicio_Estado')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ProveedorServicio_Estado] ON [dbo].[ProveedorServicio]
    (
        [Estado] ASC,
        [Calificacion_Promedio] DESC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ProveedorServicio_Ciudad')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ProveedorServicio_Ciudad] ON [dbo].[ProveedorServicio]
    (
        [Ciudad] ASC,
        [Provincia] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
END
GO

PRINT 'Tablas de Proveedores de Servicios creadas exitosamente'
GO

