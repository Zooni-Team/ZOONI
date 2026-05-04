USE [master]
GO
/****** Object:  Database [Zooni]    Script Date: 4/5/2026 09:58:07 ******/
CREATE DATABASE [Zooni]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'Zooni', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL14.MSSQLSERVER\MSSQL\DATA\Zooni.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'Zooni_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL14.MSSQLSERVER\MSSQL\DATA\Zooni_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
GO
ALTER DATABASE [Zooni] SET COMPATIBILITY_LEVEL = 140
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [Zooni].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [Zooni] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [Zooni] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [Zooni] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [Zooni] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [Zooni] SET ARITHABORT OFF 
GO
ALTER DATABASE [Zooni] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [Zooni] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [Zooni] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [Zooni] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [Zooni] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [Zooni] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [Zooni] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [Zooni] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [Zooni] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [Zooni] SET  ENABLE_BROKER 
GO
ALTER DATABASE [Zooni] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [Zooni] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [Zooni] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [Zooni] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [Zooni] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [Zooni] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [Zooni] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [Zooni] SET RECOVERY FULL 
GO
ALTER DATABASE [Zooni] SET  MULTI_USER 
GO
ALTER DATABASE [Zooni] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [Zooni] SET DB_CHAINING OFF 
GO
ALTER DATABASE [Zooni] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [Zooni] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [Zooni] SET DELAYED_DURABILITY = DISABLED 
GO
EXEC sys.sp_db_vardecimal_storage_format N'Zooni', N'ON'
GO
ALTER DATABASE [Zooni] SET QUERY_STORE = OFF
GO
USE [Zooni]
GO
/****** Object:  User [alumno]    Script Date: 4/5/2026 09:58:07 ******/
CREATE USER [alumno] FOR LOGIN [alumno] WITH DEFAULT_SCHEMA=[dbo]
GO
/****** Object:  Table [dbo].[__MigrationLog]    Script Date: 4/5/2026 09:58:07 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[__MigrationLog](
	[Name] [nvarchar](200) NOT NULL,
	[ExecutedAt] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Clinica]    Script Date: 4/5/2026 09:58:07 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Clinica](
	[Id_Clinica] [int] IDENTITY(1,1) NOT NULL,
	[Nombre] [nvarchar](150) NULL,
	[Direccion] [nvarchar](300) NULL,
	[Telefono] [nvarchar](50) NULL,
	[Mail] [nvarchar](320) NULL,
	[Rating] [decimal](3, 2) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id_Clinica] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Comentario]    Script Date: 4/5/2026 09:58:07 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Comentario](
	[Id_Comentario] [int] IDENTITY(1,1) NOT NULL,
	[Id_Publicacion] [int] NOT NULL,
	[Id_User] [int] NOT NULL,
	[Contenido] [nvarchar](1000) NULL,
	[Fecha] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id_Comentario] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[HistorialEvento]    Script Date: 4/5/2026 09:58:07 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HistorialEvento](
	[Id_Evento] [int] IDENTITY(1,1) NOT NULL,
	[Id_Mascota] [int] NOT NULL,
	[Tipo] [nvarchar](50) NULL,
	[Fecha] [datetime2](7) NULL,
	[Descripcion] [nvarchar](1000) NULL,
	[Visibilidad] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id_Evento] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Mascota]    Script Date: 4/5/2026 09:58:07 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Mascota](
	[Id_Mascota] [int] IDENTITY(1,1) NOT NULL,
	[Id_User] [int] NOT NULL,
	[Nombre] [nvarchar](100) NULL,
	[Especie] [nvarchar](50) NULL,
	[Raza] [nvarchar](100) NULL,
	[FechaNacimiento] [date] NULL,
	[Peso] [decimal](5, 2) NULL,
	[Foto] [nvarchar](500) NULL,
	[EstadoSalud] [nvarchar](200) NULL,
	[ChipId] [nvarchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id_Mascota] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MascotaVacuna]    Script Date: 4/5/2026 09:58:07 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MascotaVacuna](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Id_Mascota] [int] NOT NULL,
	[Id_Vacuna] [int] NOT NULL,
	[FechaAplicacion] [date] NULL,
	[ProximaDosis] [date] NULL,
	[AplicadaPor] [nvarchar](150) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Match]    Script Date: 4/5/2026 09:58:07 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Match](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[idMascotaUno] [int] NOT NULL,
	[idMascotaDos] [int] NOT NULL,
	[idUsuarioUno] [int] NOT NULL,
	[idUsuarioDos] [int] NOT NULL,
	[fecha] [datetime2](7) NULL,
	[activo] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Mensaje]    Script Date: 4/5/2026 09:58:07 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Mensaje](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[idMatch] [int] NOT NULL,
	[idUsuario] [int] NOT NULL,
	[texto] [nvarchar](2000) NULL,
	[fecha] [datetime2](7) NULL,
	[leido] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Notificacion]    Script Date: 4/5/2026 09:58:07 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Notificacion](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Id_User] [int] NOT NULL,
	[Titulo] [nvarchar](150) NULL,
	[Mensaje] [nvarchar](500) NULL,
	[Tipo] [nvarchar](50) NULL,
	[Leido] [bit] NULL,
	[Fecha] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Paseo]    Script Date: 4/5/2026 09:58:07 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Paseo](
	[Id_Paseo] [int] IDENTITY(1,1) NOT NULL,
	[Id_Mascota] [int] NOT NULL,
	[Id_Walker] [int] NULL,
	[HoraInicio] [datetime2](7) NULL,
	[HoraFin] [datetime2](7) NULL,
	[UbicacionInicio] [nvarchar](300) NULL,
	[UbicacionFin] [nvarchar](300) NULL,
	[Estado] [nvarchar](50) NULL,
	[Rating] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id_Paseo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PaseoTrack]    Script Date: 4/5/2026 09:58:07 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PaseoTrack](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Id_Paseo] [int] NOT NULL,
	[Lat] [decimal](10, 7) NULL,
	[Lng] [decimal](10, 7) NULL,
	[Timestamp] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Publicacion]    Script Date: 4/5/2026 09:58:07 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Publicacion](
	[Id_Publicacion] [int] IDENTITY(1,1) NOT NULL,
	[Id_User] [int] NOT NULL,
	[Id_Mascota] [int] NULL,
	[ImagenUrl] [nvarchar](500) NULL,
	[Descripcion] [nvarchar](2000) NULL,
	[Fecha] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id_Publicacion] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Role]    Script Date: 4/5/2026 09:58:07 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Role](
	[Id_Role] [int] IDENTITY(1,1) NOT NULL,
	[Nombre] [nvarchar](50) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id_Role] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Tratamiento]    Script Date: 4/5/2026 09:58:07 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Tratamiento](
	[Id_Tratamiento] [int] IDENTITY(1,1) NOT NULL,
	[Id_Mascota] [int] NOT NULL,
	[Nombre] [nvarchar](150) NULL,
	[Veterinario] [nvarchar](150) NULL,
	[FechaInicio] [date] NULL,
	[FechaFin] [date] NULL,
	[Observaciones] [nvarchar](1000) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id_Tratamiento] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[User]    Script Date: 4/5/2026 09:58:07 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[User](
	[Id_User] [int] IDENTITY(1,1) NOT NULL,
	[Nombre] [nvarchar](100) NULL,
	[Apellido] [nvarchar](100) NULL,
	[Mail] [nvarchar](320) NOT NULL,
	[Contrasena] [nvarchar](255) NOT NULL,
	[Telefono] [nvarchar](30) NULL,
	[FechaRegistro] [datetime2](7) NULL,
	[FotoPerfil] [nvarchar](500) NULL,
	[Ubicacion] [nvarchar](300) NULL,
	[Estado] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id_User] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UserRole]    Script Date: 4/5/2026 09:58:07 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserRole](
	[Id_User] [int] NOT NULL,
	[Id_Role] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id_User] ASC,
	[Id_Role] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Vacuna]    Script Date: 4/5/2026 09:58:07 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Vacuna](
	[Id_Vacuna] [int] IDENTITY(1,1) NOT NULL,
	[Nombre] [nvarchar](150) NULL,
	[Dosis] [nvarchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id_Vacuna] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Veterinario]    Script Date: 4/5/2026 09:58:07 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Veterinario](
	[Id_Vet] [int] IDENTITY(1,1) NOT NULL,
	[Id_Clinica] [int] NULL,
	[Nombre] [nvarchar](150) NULL,
	[Matricula] [nvarchar](100) NULL,
	[Especialidad] [nvarchar](150) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id_Vet] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Voto]    Script Date: 4/5/2026 09:58:07 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Voto](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[idMascotaOrigen] [int] NOT NULL,
	[idMascotaDestino] [int] NOT NULL,
	[leGusta] [bit] NOT NULL,
	[fecha] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET IDENTITY_INSERT [dbo].[Role] ON 

INSERT [dbo].[Role] ([Id_Role], [Nombre]) VALUES (5, N'ADMIN')
INSERT [dbo].[Role] ([Id_Role], [Nombre]) VALUES (4, N'CLINIC')
INSERT [dbo].[Role] ([Id_Role], [Nombre]) VALUES (1, N'OWNER')
INSERT [dbo].[Role] ([Id_Role], [Nombre]) VALUES (3, N'VET')
INSERT [dbo].[Role] ([Id_Role], [Nombre]) VALUES (2, N'WALKER')
SET IDENTITY_INSERT [dbo].[Role] OFF
GO
/****** Object:  Index [UQ__Match__7BC3F4A6F26976E7]    Script Date: 4/5/2026 09:58:07 ******/
ALTER TABLE [dbo].[Match] ADD UNIQUE NONCLUSTERED 
(
	[idMascotaUno] ASC,
	[idMascotaDos] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_Match_MascotaDos]    Script Date: 4/5/2026 09:58:07 ******/
CREATE NONCLUSTERED INDEX [IX_Match_MascotaDos] ON [dbo].[Match]
(
	[idMascotaDos] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_Match_MascotaUno]    Script Date: 4/5/2026 09:58:07 ******/
CREATE NONCLUSTERED INDEX [IX_Match_MascotaUno] ON [dbo].[Match]
(
	[idMascotaUno] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_Mensaje_Match]    Script Date: 4/5/2026 09:58:07 ******/
CREATE NONCLUSTERED INDEX [IX_Mensaje_Match] ON [dbo].[Mensaje]
(
	[idMatch] ASC,
	[fecha] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__Role__75E3EFCF8684379B]    Script Date: 4/5/2026 09:58:07 ******/
ALTER TABLE [dbo].[Role] ADD UNIQUE NONCLUSTERED 
(
	[Nombre] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__User__2724B2D17584FD00]    Script Date: 4/5/2026 09:58:07 ******/
ALTER TABLE [dbo].[User] ADD UNIQUE NONCLUSTERED 
(
	[Mail] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [UQ__Voto__4669983786296C11]    Script Date: 4/5/2026 09:58:07 ******/
ALTER TABLE [dbo].[Voto] ADD UNIQUE NONCLUSTERED 
(
	[idMascotaOrigen] ASC,
	[idMascotaDestino] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_Voto_Destino]    Script Date: 4/5/2026 09:58:07 ******/
CREATE NONCLUSTERED INDEX [IX_Voto_Destino] ON [dbo].[Voto]
(
	[idMascotaDestino] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[__MigrationLog] ADD  DEFAULT (sysutcdatetime()) FOR [ExecutedAt]
GO
ALTER TABLE [dbo].[Comentario] ADD  DEFAULT (sysutcdatetime()) FOR [Fecha]
GO
ALTER TABLE [dbo].[HistorialEvento] ADD  DEFAULT (sysutcdatetime()) FOR [Fecha]
GO
ALTER TABLE [dbo].[HistorialEvento] ADD  DEFAULT ((1)) FOR [Visibilidad]
GO
ALTER TABLE [dbo].[Match] ADD  DEFAULT (sysutcdatetime()) FOR [fecha]
GO
ALTER TABLE [dbo].[Match] ADD  DEFAULT ((1)) FOR [activo]
GO
ALTER TABLE [dbo].[Mensaje] ADD  DEFAULT (sysutcdatetime()) FOR [fecha]
GO
ALTER TABLE [dbo].[Mensaje] ADD  DEFAULT ((0)) FOR [leido]
GO
ALTER TABLE [dbo].[Notificacion] ADD  DEFAULT ((0)) FOR [Leido]
GO
ALTER TABLE [dbo].[Notificacion] ADD  DEFAULT (sysutcdatetime()) FOR [Fecha]
GO
ALTER TABLE [dbo].[Publicacion] ADD  DEFAULT (sysutcdatetime()) FOR [Fecha]
GO
ALTER TABLE [dbo].[User] ADD  DEFAULT (sysutcdatetime()) FOR [FechaRegistro]
GO
ALTER TABLE [dbo].[User] ADD  DEFAULT ((1)) FOR [Estado]
GO
ALTER TABLE [dbo].[Voto] ADD  DEFAULT (sysutcdatetime()) FOR [fecha]
GO
ALTER TABLE [dbo].[Comentario]  WITH CHECK ADD FOREIGN KEY([Id_Publicacion])
REFERENCES [dbo].[Publicacion] ([Id_Publicacion])
GO
ALTER TABLE [dbo].[Comentario]  WITH CHECK ADD FOREIGN KEY([Id_User])
REFERENCES [dbo].[User] ([Id_User])
GO
ALTER TABLE [dbo].[HistorialEvento]  WITH CHECK ADD FOREIGN KEY([Id_Mascota])
REFERENCES [dbo].[Mascota] ([Id_Mascota])
GO
ALTER TABLE [dbo].[Mascota]  WITH CHECK ADD FOREIGN KEY([Id_User])
REFERENCES [dbo].[User] ([Id_User])
GO
ALTER TABLE [dbo].[MascotaVacuna]  WITH CHECK ADD FOREIGN KEY([Id_Mascota])
REFERENCES [dbo].[Mascota] ([Id_Mascota])
GO
ALTER TABLE [dbo].[MascotaVacuna]  WITH CHECK ADD FOREIGN KEY([Id_Vacuna])
REFERENCES [dbo].[Vacuna] ([Id_Vacuna])
GO
ALTER TABLE [dbo].[Match]  WITH CHECK ADD FOREIGN KEY([idMascotaUno])
REFERENCES [dbo].[Mascota] ([Id_Mascota])
GO
ALTER TABLE [dbo].[Match]  WITH CHECK ADD FOREIGN KEY([idMascotaDos])
REFERENCES [dbo].[Mascota] ([Id_Mascota])
GO
ALTER TABLE [dbo].[Match]  WITH CHECK ADD FOREIGN KEY([idUsuarioUno])
REFERENCES [dbo].[User] ([Id_User])
GO
ALTER TABLE [dbo].[Match]  WITH CHECK ADD FOREIGN KEY([idUsuarioDos])
REFERENCES [dbo].[User] ([Id_User])
GO
ALTER TABLE [dbo].[Mensaje]  WITH CHECK ADD FOREIGN KEY([idMatch])
REFERENCES [dbo].[Match] ([id])
GO
ALTER TABLE [dbo].[Mensaje]  WITH CHECK ADD FOREIGN KEY([idUsuario])
REFERENCES [dbo].[User] ([Id_User])
GO
ALTER TABLE [dbo].[Notificacion]  WITH CHECK ADD FOREIGN KEY([Id_User])
REFERENCES [dbo].[User] ([Id_User])
GO
ALTER TABLE [dbo].[Paseo]  WITH CHECK ADD FOREIGN KEY([Id_Mascota])
REFERENCES [dbo].[Mascota] ([Id_Mascota])
GO
ALTER TABLE [dbo].[Paseo]  WITH CHECK ADD FOREIGN KEY([Id_Walker])
REFERENCES [dbo].[User] ([Id_User])
GO
ALTER TABLE [dbo].[PaseoTrack]  WITH CHECK ADD FOREIGN KEY([Id_Paseo])
REFERENCES [dbo].[Paseo] ([Id_Paseo])
GO
ALTER TABLE [dbo].[Publicacion]  WITH CHECK ADD FOREIGN KEY([Id_Mascota])
REFERENCES [dbo].[Mascota] ([Id_Mascota])
GO
ALTER TABLE [dbo].[Publicacion]  WITH CHECK ADD FOREIGN KEY([Id_User])
REFERENCES [dbo].[User] ([Id_User])
GO
ALTER TABLE [dbo].[Tratamiento]  WITH CHECK ADD FOREIGN KEY([Id_Mascota])
REFERENCES [dbo].[Mascota] ([Id_Mascota])
GO
ALTER TABLE [dbo].[UserRole]  WITH CHECK ADD FOREIGN KEY([Id_Role])
REFERENCES [dbo].[Role] ([Id_Role])
GO
ALTER TABLE [dbo].[UserRole]  WITH CHECK ADD FOREIGN KEY([Id_User])
REFERENCES [dbo].[User] ([Id_User])
GO
ALTER TABLE [dbo].[Veterinario]  WITH CHECK ADD FOREIGN KEY([Id_Clinica])
REFERENCES [dbo].[Clinica] ([Id_Clinica])
GO
ALTER TABLE [dbo].[Voto]  WITH CHECK ADD FOREIGN KEY([idMascotaDestino])
REFERENCES [dbo].[Mascota] ([Id_Mascota])
GO
ALTER TABLE [dbo].[Voto]  WITH CHECK ADD FOREIGN KEY([idMascotaOrigen])
REFERENCES [dbo].[Mascota] ([Id_Mascota])
GO
USE [master]
GO
ALTER DATABASE [Zooni] SET  READ_WRITE 
GO
