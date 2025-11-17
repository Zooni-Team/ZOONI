-- Tablas para sistema de perfil social tipo Instagram

-- Tabla Publicacion
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Publicacion')
BEGIN
    CREATE TABLE [dbo].[Publicacion](
        [Id_Publicacion] [int] IDENTITY(1,1) NOT NULL,
        [Id_User] [int] NOT NULL,
        [Id_Mascota] [int] NULL, -- Opcional: puede ser publicación del usuario o de su mascota
        [ImagenUrl] [nvarchar](500) NULL,
        [Descripcion] [nvarchar](2000) NULL,
        [Fecha] [datetime2](7) NOT NULL DEFAULT GETDATE(),
        [Anclada] [bit] NOT NULL DEFAULT 0,
        [FechaAnclada] [datetime2](7) NULL,
        [Eliminada] [bit] NOT NULL DEFAULT 0,
        CONSTRAINT [PK_Publicacion] PRIMARY KEY CLUSTERED ([Id_Publicacion] ASC),
        CONSTRAINT [FK_Publicacion_User] FOREIGN KEY ([Id_User]) REFERENCES [dbo].[User]([Id_User]),
        CONSTRAINT [FK_Publicacion_Mascota] FOREIGN KEY ([Id_Mascota]) REFERENCES [dbo].[Mascota]([Id_Mascota])
    ) ON [PRIMARY]
END
GO

-- Tabla Like (me gusta en publicaciones)
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'LikePublicacion')
BEGIN
    CREATE TABLE [dbo].[LikePublicacion](
        [Id_Like] [int] IDENTITY(1,1) NOT NULL,
        [Id_Publicacion] [int] NOT NULL,
        [Id_User] [int] NOT NULL,
        [Fecha] [datetime2](7) NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [PK_LikePublicacion] PRIMARY KEY CLUSTERED ([Id_Like] ASC),
        CONSTRAINT [FK_LikePublicacion_Publicacion] FOREIGN KEY ([Id_Publicacion]) REFERENCES [dbo].[Publicacion]([Id_Publicacion]) ON DELETE CASCADE,
        CONSTRAINT [FK_LikePublicacion_User] FOREIGN KEY ([Id_User]) REFERENCES [dbo].[User]([Id_User]),
        CONSTRAINT [UQ_LikePublicacion_User_Publicacion] UNIQUE ([Id_User], [Id_Publicacion])
    ) ON [PRIMARY]
END
GO

-- Tabla Comentario
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ComentarioPublicacion')
BEGIN
    CREATE TABLE [dbo].[ComentarioPublicacion](
        [Id_Comentario] [int] IDENTITY(1,1) NOT NULL,
        [Id_Publicacion] [int] NOT NULL,
        [Id_User] [int] NOT NULL,
        [Contenido] [nvarchar](1000) NOT NULL,
        [Fecha] [datetime2](7) NOT NULL DEFAULT GETDATE(),
        [Eliminado] [bit] NOT NULL DEFAULT 0,
        CONSTRAINT [PK_ComentarioPublicacion] PRIMARY KEY CLUSTERED ([Id_Comentario] ASC),
        CONSTRAINT [FK_ComentarioPublicacion_Publicacion] FOREIGN KEY ([Id_Publicacion]) REFERENCES [dbo].[Publicacion]([Id_Publicacion]) ON DELETE CASCADE,
        CONSTRAINT [FK_ComentarioPublicacion_User] FOREIGN KEY ([Id_User]) REFERENCES [dbo].[User]([Id_User])
    ) ON [PRIMARY]
END
GO

-- Tabla Compartir (compartir publicaciones)
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CompartirPublicacion')
BEGIN
    CREATE TABLE [dbo].[CompartirPublicacion](
        [Id_Compartir] [int] IDENTITY(1,1) NOT NULL,
        [Id_Publicacion] [int] NOT NULL,
        [Id_User] [int] NOT NULL, -- Usuario que comparte
        [Fecha] [datetime2](7) NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [PK_CompartirPublicacion] PRIMARY KEY CLUSTERED ([Id_Compartir] ASC),
        CONSTRAINT [FK_CompartirPublicacion_Publicacion] FOREIGN KEY ([Id_Publicacion]) REFERENCES [dbo].[Publicacion]([Id_Publicacion]) ON DELETE CASCADE,
        CONSTRAINT [FK_CompartirPublicacion_User] FOREIGN KEY ([Id_User]) REFERENCES [dbo].[User]([Id_User]),
        CONSTRAINT [UQ_CompartirPublicacion_User_Publicacion] UNIQUE ([Id_User], [Id_Publicacion])
    ) ON [PRIMARY]
END
GO

-- Tabla Historia
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Historia')
BEGIN
    CREATE TABLE [dbo].[Historia](
        [Id_Historia] [int] IDENTITY(1,1) NOT NULL,
        [Id_User] [int] NOT NULL,
        [Id_Mascota] [int] NULL,
        [ImagenUrl] [nvarchar](500) NOT NULL,
        [Texto] [nvarchar](500) NULL,
        [Fecha] [datetime2](7) NOT NULL DEFAULT GETDATE(),
        [Expiracion] [datetime2](7) NOT NULL, -- 24 horas después de la creación
        [Eliminada] [bit] NOT NULL DEFAULT 0,
        CONSTRAINT [PK_Historia] PRIMARY KEY CLUSTERED ([Id_Historia] ASC),
        CONSTRAINT [FK_Historia_User] FOREIGN KEY ([Id_User]) REFERENCES [dbo].[User]([Id_User]),
        CONSTRAINT [FK_Historia_Mascota] FOREIGN KEY ([Id_Mascota]) REFERENCES [dbo].[Mascota]([Id_Mascota])
    ) ON [PRIMARY]
END
GO

-- Tabla HistoriaDestacada (historias destacadas)
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'HistoriaDestacada')
BEGIN
    CREATE TABLE [dbo].[HistoriaDestacada](
        [Id_Destacada] [int] IDENTITY(1,1) NOT NULL,
        [Id_User] [int] NOT NULL,
        [Id_Historia] [int] NOT NULL,
        [Titulo] [nvarchar](100) NULL,
        [Fecha] [datetime2](7) NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [PK_HistoriaDestacada] PRIMARY KEY CLUSTERED ([Id_Destacada] ASC),
        CONSTRAINT [FK_HistoriaDestacada_User] FOREIGN KEY ([Id_User]) REFERENCES [dbo].[User]([Id_User]),
        CONSTRAINT [FK_HistoriaDestacada_Historia] FOREIGN KEY ([Id_Historia]) REFERENCES [dbo].[Historia]([Id_Historia]) ON DELETE CASCADE
    ) ON [PRIMARY]
END
GO

-- Tabla Mencion (menciones en publicaciones e historias)
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Mencion')
BEGIN
    CREATE TABLE [dbo].[Mencion](
        [Id_Mencion] [int] IDENTITY(1,1) NOT NULL,
        [Id_User_Mencionado] [int] NOT NULL, -- Usuario mencionado
        [Id_Publicacion] [int] NULL, -- Puede ser en publicación
        [Id_Historia] [int] NULL, -- O en historia
        [Id_User_Menciona] [int] NOT NULL, -- Usuario que menciona
        [Fecha] [datetime2](7) NOT NULL DEFAULT GETDATE(),
        [Vista] [bit] NOT NULL DEFAULT 0,
        [Reposteada] [bit] NOT NULL DEFAULT 0, -- Si el mencionado reposteó
        CONSTRAINT [PK_Mencion] PRIMARY KEY CLUSTERED ([Id_Mencion] ASC),
        CONSTRAINT [FK_Mencion_User_Mencionado] FOREIGN KEY ([Id_User_Mencionado]) REFERENCES [dbo].[User]([Id_User]),
        CONSTRAINT [FK_Mencion_User_Menciona] FOREIGN KEY ([Id_User_Menciona]) REFERENCES [dbo].[User]([Id_User]),
        CONSTRAINT [FK_Mencion_Publicacion] FOREIGN KEY ([Id_Publicacion]) REFERENCES [dbo].[Publicacion]([Id_Publicacion]) ON DELETE CASCADE,
        CONSTRAINT [FK_Mencion_Historia] FOREIGN KEY ([Id_Historia]) REFERENCES [dbo].[Historia]([Id_Historia]) ON DELETE CASCADE,
        CONSTRAINT [CK_Mencion_Tipo] CHECK (([Id_Publicacion] IS NOT NULL AND [Id_Historia] IS NULL) OR ([Id_Publicacion] IS NULL AND [Id_Historia] IS NOT NULL))
    ) ON [PRIMARY]
END
GO

-- Índices para mejorar rendimiento
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Publicacion_User_Fecha')
BEGIN
    CREATE INDEX [IX_Publicacion_User_Fecha] ON [dbo].[Publicacion]([Id_User], [Fecha] DESC)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Historia_User_Expiracion')
BEGIN
    CREATE INDEX [IX_Historia_User_Expiracion] ON [dbo].[Historia]([Id_User], [Expiracion] DESC)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Mencion_User_Mencionado')
BEGIN
    CREATE INDEX [IX_Mencion_User_Mencionado] ON [dbo].[Mencion]([Id_User_Mencionado], [Vista])
END
GO

