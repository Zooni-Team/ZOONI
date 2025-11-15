-- Crear tabla ApodoAmigo si no existe (soluciona el error actual)
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ApodoAmigo')
BEGIN
    CREATE TABLE [dbo].[ApodoAmigo](
        [Id_Apodo] [int] IDENTITY(1,1) NOT NULL,
        [Id_User] [int] NOT NULL,
        [Id_Amigo] [int] NOT NULL,
        [Apodo] [nvarchar](100) NULL,
        CONSTRAINT [PK_ApodoAmigo] PRIMARY KEY CLUSTERED ([Id_Apodo] ASC),
        CONSTRAINT [FK_ApodoAmigo_User] FOREIGN KEY ([Id_User]) REFERENCES [dbo].[User]([Id_User]),
        CONSTRAINT [FK_ApodoAmigo_Amigo] FOREIGN KEY ([Id_Amigo]) REFERENCES [dbo].[User]([Id_User]),
        CONSTRAINT [UQ_ApodoAmigo_User_Amigo] UNIQUE ([Id_User], [Id_Amigo])
    ) ON [PRIMARY]
END
GO

-- Tabla para solicitudes de compartir mascota
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'SolicitudMascotaCompartida')
BEGIN
    CREATE TABLE [dbo].[SolicitudMascotaCompartida](
        [Id_Solicitud] [int] IDENTITY(1,1) NOT NULL,
        [Id_Mascota] [int] NOT NULL,
        [Id_Propietario] [int] NOT NULL,  -- Dueño original de la mascota
        [Id_Solicitante] [int] NOT NULL,  -- Usuario que solicita compartir
        [Estado] [nvarchar](20) NOT NULL DEFAULT 'Pendiente',  -- Pendiente, Aceptada, Rechazada
        [Fecha_Solicitud] [datetime2](7) NOT NULL DEFAULT GETDATE(),
        [Fecha_Respuesta] [datetime2](7) NULL,
        [Mensaje] [nvarchar](500) NULL,
        CONSTRAINT [PK_SolicitudMascotaCompartida] PRIMARY KEY CLUSTERED ([Id_Solicitud] ASC),
        CONSTRAINT [FK_SolicitudMascota_Propietario] FOREIGN KEY ([Id_Propietario]) REFERENCES [dbo].[User]([Id_User]),
        CONSTRAINT [FK_SolicitudMascota_Solicitante] FOREIGN KEY ([Id_Solicitante]) REFERENCES [dbo].[User]([Id_User]),
        CONSTRAINT [FK_SolicitudMascota_Mascota] FOREIGN KEY ([Id_Mascota]) REFERENCES [dbo].[Mascota]([Id_Mascota])
    ) ON [PRIMARY]
END
GO

-- Tabla para mascotas compartidas (cuando la solicitud es aceptada)
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'MascotaCompartida')
BEGIN
    CREATE TABLE [dbo].[MascotaCompartida](
        [Id_Compartida] [int] IDENTITY(1,1) NOT NULL,
        [Id_Mascota] [int] NOT NULL,
        [Id_Propietario] [int] NOT NULL,  -- Dueño original
        [Id_UsuarioCompartido] [int] NOT NULL,  -- Usuario con quien se comparte
        [Permiso_Edicion] [bit] NOT NULL DEFAULT 1,  -- Si puede editar o solo ver
        [Fecha_Compartida] [datetime2](7) NOT NULL DEFAULT GETDATE(),
        [Activo] [bit] NOT NULL DEFAULT 1,
        CONSTRAINT [PK_MascotaCompartida] PRIMARY KEY CLUSTERED ([Id_Compartida] ASC),
        CONSTRAINT [FK_MascotaCompartida_Propietario] FOREIGN KEY ([Id_Propietario]) REFERENCES [dbo].[User]([Id_User]),
        CONSTRAINT [FK_MascotaCompartida_UsuarioCompartido] FOREIGN KEY ([Id_UsuarioCompartido]) REFERENCES [dbo].[User]([Id_User]),
        CONSTRAINT [FK_MascotaCompartida_Mascota] FOREIGN KEY ([Id_Mascota]) REFERENCES [dbo].[Mascota]([Id_Mascota]),
        CONSTRAINT [UQ_MascotaCompartida] UNIQUE ([Id_Mascota], [Id_UsuarioCompartido])
    ) ON [PRIMARY]
END
GO

-- Índices para mejorar el rendimiento
CREATE NONCLUSTERED INDEX [IX_SolicitudMascota_Estado] 
ON [dbo].[SolicitudMascotaCompartida] ([Estado])
INCLUDE ([Id_Solicitante], [Id_Propietario], [Id_Mascota])
GO

CREATE NONCLUSTERED INDEX [IX_MascotaCompartida_Usuario] 
ON [dbo].[MascotaCompartida] ([Id_UsuarioCompartido], [Activo])
INCLUDE ([Id_Mascota], [Permiso_Edicion])
GO

