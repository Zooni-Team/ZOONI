USE [master]
GO
/****** Object:  Database [Zooni]    Script Date: 30/10/2025 14:26:55 ******/
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
/****** Object:  User [alumno]    Script Date: 30/10/2025 14:26:55 ******/
CREATE USER [alumno] FOR LOGIN [alumno] WITH DEFAULT_SCHEMA=[dbo]
GO
/****** Object:  Table [dbo].[Calendario]    Script Date: 30/10/2025 14:26:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Calendario](
	[Id_Calendario] [int] IDENTITY(1,1) NOT NULL,
	[Id_User] [int] NOT NULL,
	[Nombre] [nvarchar](100) NOT NULL,
	[Descripcion] [nvarchar](500) NULL,
	[FechaCreacion] [datetime2](7) NOT NULL,
	[Activo] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id_Calendario] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CalendarioEvento]    Script Date: 30/10/2025 14:26:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CalendarioEvento](
	[Id_Evento] [int] IDENTITY(1,1) NOT NULL,
	[Id_Calendario] [int] NOT NULL,
	[Id_User] [int] NOT NULL,
	[Id_Mascota] [int] NULL,
	[Titulo] [nvarchar](150) NOT NULL,
	[Descripcion] [nvarchar](1000) NULL,
	[Fecha] [datetime2](7) NOT NULL,
	[Tipo] [nvarchar](50) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id_Evento] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Chat]    Script Date: 30/10/2025 14:26:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Chat](
	[Id_Chat] [int] IDENTITY(1,1) NOT NULL,
	[Nombre] [nvarchar](150) NULL,
	[EsGrupo] [bit] NOT NULL,
	[FechaCreacion] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id_Chat] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CirculoConfianza]    Script Date: 30/10/2025 14:26:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CirculoConfianza](
	[Id_Circulo] [int] IDENTITY(1,1) NOT NULL,
	[Id_User] [int] NOT NULL,
	[Id_Amigo] [int] NOT NULL,
	[Rol] [nvarchar](50) NOT NULL,
	[Latitud] [float] NOT NULL,
	[Longitud] [float] NOT NULL,
	[UltimaConexion] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id_Circulo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Clinica]    Script Date: 30/10/2025 14:26:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Clinica](
	[Id_Clinica] [int] IDENTITY(1,1) NOT NULL,
	[Nombre] [nvarchar](150) NOT NULL,
	[Direccion] [nvarchar](200) NULL,
	[Telefono] [nvarchar](50) NULL,
	[Ciudad] [nvarchar](100) NULL,
	[Provincia] [nvarchar](100) NULL,
	[Servicios] [nvarchar](500) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id_Clinica] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Comida]    Script Date: 30/10/2025 14:26:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Comida](
	[Id_Comida] [int] IDENTITY(1,1) NOT NULL,
	[Nombre] [nvarchar](150) NOT NULL,
	[Calorias] [decimal](10, 2) NOT NULL,
	[Proteina] [decimal](10, 2) NOT NULL,
	[Carbohidratos] [decimal](10, 2) NOT NULL,
	[Grasas] [decimal](10, 2) NOT NULL,
	[Tipo] [nvarchar](50) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id_Comida] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Comportamiento]    Script Date: 30/10/2025 14:26:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Comportamiento](
	[Id_Comportamiento] [int] IDENTITY(1,1) NOT NULL,
	[Id_Mascota] [int] NOT NULL,
	[Fecha] [date] NOT NULL,
	[Estado_Animo] [nvarchar](50) NOT NULL,
	[Actividad_Reciente] [nvarchar](200) NULL,
	[Notas] [nvarchar](1000) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id_Comportamiento] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Compra]    Script Date: 30/10/2025 14:26:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Compra](
	[Id_Compra] [int] IDENTITY(1,1) NOT NULL,
	[Id_User] [int] NOT NULL,
	[Id_Producto] [int] NOT NULL,
	[Cantidad] [int] NOT NULL,
	[Fecha] [datetime2](7) NOT NULL,
	[Id_Metodo_Pago] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id_Compra] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Comunidad]    Script Date: 30/10/2025 14:26:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Comunidad](
	[Id_Comunidad] [int] IDENTITY(1,1) NOT NULL,
	[Titulo] [nvarchar](150) NOT NULL,
	[Contenido] [nvarchar](max) NOT NULL,
	[Fecha] [datetime2](7) NOT NULL,
	[Tipo] [nvarchar](50) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id_Comunidad] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ComunidadXUsuario]    Script Date: 30/10/2025 14:26:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ComunidadXUsuario](
	[Id] [int] NOT NULL,
	[Id_User] [int] NOT NULL,
	[Id_Comunidad] [int] NOT NULL,
	[FechaIngreso] [datetime] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Consejo]    Script Date: 30/10/2025 14:26:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Consejo](
	[Id_Consejo] [int] IDENTITY(1,1) NOT NULL,
	[Titulo] [nvarchar](150) NOT NULL,
	[Descripcion] [nvarchar](max) NOT NULL,
	[Categoria] [nvarchar](50) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id_Consejo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CuidadoDiario]    Script Date: 30/10/2025 14:26:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CuidadoDiario](
	[Id_Cuidado] [int] IDENTITY(1,1) NOT NULL,
	[Id_Mascota] [int] NOT NULL,
	[Fecha] [date] NOT NULL,
	[Comio] [nvarchar](50) NULL,
	[Jugo] [nvarchar](50) NULL,
	[Animo] [nvarchar](50) NULL,
	[Comentario] [nvarchar](500) NULL,
	[FotoDia] [nvarchar](300) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id_Cuidado] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Dieta]    Script Date: 30/10/2025 14:26:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Dieta](
	[Id_Dieta] [int] IDENTITY(1,1) NOT NULL,
	[Id_Mascota] [int] NOT NULL,
	[Nombre] [nvarchar](150) NOT NULL,
	[Descripcion] [nvarchar](500) NULL,
	[FechaInicio] [date] NOT NULL,
	[FechaFin] [date] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id_Dieta] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Dieta_Comida]    Script Date: 30/10/2025 14:26:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Dieta_Comida](
	[Id_Dieta] [int] NOT NULL,
	[Id_Comida] [int] NOT NULL,
	[Porciones] [decimal](10, 2) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id_Dieta] ASC,
	[Id_Comida] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EstadoPago]    Script Date: 30/10/2025 14:26:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EstadoPago](
	[Id_EstadoPago] [int] IDENTITY(1,1) NOT NULL,
	[Descripcion] [nvarchar](50) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id_EstadoPago] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EstadoReserva]    Script Date: 30/10/2025 14:26:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EstadoReserva](
	[Id_EstadoReserva] [int] IDENTITY(1,1) NOT NULL,
	[Descripcion] [nvarchar](100) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id_EstadoReserva] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EventoMascota]    Script Date: 30/10/2025 14:26:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EventoMascota](
	[Id_Evento] [int] IDENTITY(1,1) NOT NULL,
	[Id_Mascota] [int] NOT NULL,
	[TipoEvento] [nvarchar](50) NOT NULL,
	[Fecha] [datetime2](7) NOT NULL,
	[Descripcion] [nvarchar](500) NULL,
	[Imagen] [nvarchar](300) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id_Evento] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[HistorialMedico]    Script Date: 30/10/2025 14:26:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HistorialMedico](
	[Id_Historial] [int] IDENTITY(1,1) NOT NULL,
	[Id_Mascota] [int] NOT NULL,
	[Id_Vet] [int] NOT NULL,
	[Fecha] [datetime2](7) NOT NULL,
	[Diagnostico] [nvarchar](500) NOT NULL,
	[Tratamiento] [nvarchar](1000) NULL,
	[Receta] [nvarchar](1000) NULL,
	[Archivo_Adjunto] [nvarchar](300) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id_Historial] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Invitacion]    Script Date: 30/10/2025 14:26:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Invitacion](
	[Id_Invitacion] [int] IDENTITY(1,1) NOT NULL,
	[Id_Mascota] [int] NOT NULL,
	[Id_Emisor] [int] NOT NULL,
	[Id_Receptor] [int] NOT NULL,
	[Rol] [nvarchar](50) NOT NULL,
	[Estado] [nvarchar](20) NOT NULL,
	[Fecha] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id_Invitacion] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Logro]    Script Date: 30/10/2025 14:26:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Logro](
	[Id_Logro] [int] IDENTITY(1,1) NOT NULL,
	[Id_User] [int] NOT NULL,
	[Id_Mascota] [int] NOT NULL,
	[Nombre] [nvarchar](150) NOT NULL,
	[Descripcion] [nvarchar](500) NULL,
	[Fecha_Obtenido] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id_Logro] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Mail]    Script Date: 30/10/2025 14:26:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Mail](
	[Id_Mail] [int] IDENTITY(1,1) NOT NULL,
	[Correo] [nvarchar](320) NOT NULL,
	[Contrasena] [nvarchar](200) NOT NULL,
	[Fecha_Creacion] [datetime2](7) NOT NULL,
	[Ultimo_Acceso] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id_Mail] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Mascota]    Script Date: 30/10/2025 14:26:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Mascota](
	[Id_Mascota] [int] IDENTITY(1,1) NOT NULL,
	[Id_User] [int] NOT NULL,
	[Nombre] [nvarchar](200) NULL,
	[Especie] [nvarchar](150) NULL,
	[Raza] [nvarchar](200) NULL,
	[Sexo] [nvarchar](50) NULL,
	[Edad] [int] NULL,
	[Fecha_Nacimiento] [date] NULL,
	[Peso] [decimal](6, 2) NULL,
	[Color] [nvarchar](150) NULL,
	[Esterilizado] [bit] NOT NULL,
	[Chip] [nvarchar](200) NULL,
	[Foto] [nvarchar](max) NULL,
	[PesoDisplay] [nvarchar](20) NULL,
 CONSTRAINT [PK__Mascota__C7A382FE0A959648] PRIMARY KEY CLUSTERED 
(
	[Id_Mascota] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MascotaXConsejo]    Script Date: 30/10/2025 14:26:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MascotaXConsejo](
	[Id] [int] NOT NULL,
	[Id_Macota] [int] NOT NULL,
	[Id_Consejo] [int] NOT NULL,
	[Fecha_Publicacion] [datetime] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MascotaXPrenda]    Script Date: 30/10/2025 14:26:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MascotaXPrenda](
	[Id] [int] NOT NULL,
	[Id_Mascota] [int] NOT NULL,
	[Id_Prenda] [int] NOT NULL,
	[CantidadPrendas] [int] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Mensaje]    Script Date: 30/10/2025 14:26:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Mensaje](
	[Id_Mensaje] [int] IDENTITY(1,1) NOT NULL,
	[Id_Chat] [int] NOT NULL,
	[Id_User] [int] NOT NULL,
	[Contenido] [nvarchar](max) NOT NULL,
	[Fecha] [datetime2](7) NOT NULL,
	[Leido] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id_Mensaje] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MetodoPago]    Script Date: 30/10/2025 14:26:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MetodoPago](
	[Id_MetodoPago] [int] IDENTITY(1,1) NOT NULL,
	[Descripcion] [nvarchar](100) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id_MetodoPago] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ModoViaje]    Script Date: 30/10/2025 14:26:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ModoViaje](
	[Id_ModoViaje] [int] IDENTITY(1,1) NOT NULL,
	[Id_User] [int] NOT NULL,
	[Id_Mascota] [int] NOT NULL,
	[Fecha_Inicio] [datetime2](7) NOT NULL,
	[Fecha_Fin] [datetime2](7) NULL,
	[Id_Paseador] [int] NULL,
	[Notas] [nvarchar](1000) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id_ModoViaje] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Notificacion]    Script Date: 30/10/2025 14:26:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Notificacion](
	[Id_Notificacion] [int] IDENTITY(1,1) NOT NULL,
	[Id_User] [int] NOT NULL,
	[Titulo] [nvarchar](150) NOT NULL,
	[Mensaje] [nvarchar](1000) NOT NULL,
	[Fecha] [datetime2](7) NOT NULL,
	[Leida] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id_Notificacion] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Pago]    Script Date: 30/10/2025 14:26:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Pago](
	[Id_Pago] [int] IDENTITY(1,1) NOT NULL,
	[Id_Reserva] [int] NOT NULL,
	[Id_MetodoPago] [int] NOT NULL,
	[Monto] [decimal](12, 2) NOT NULL,
	[Fecha_Pago] [datetime2](7) NOT NULL,
	[Id_EstadoPago] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id_Pago] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ParticipanteChat]    Script Date: 30/10/2025 14:26:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ParticipanteChat](
	[Id_Participante] [int] IDENTITY(1,1) NOT NULL,
	[Id_Chat] [int] NOT NULL,
	[Id_User] [int] NOT NULL,
	[Administrador] [bit] NOT NULL,
	[FechaIngreso] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id_Participante] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Paseador]    Script Date: 30/10/2025 14:26:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Paseador](
	[Id_Paseador] [int] IDENTITY(1,1) NOT NULL,
	[Id_User] [int] NOT NULL,
	[Experiencia_Anios] [int] NOT NULL,
	[Disponibilidad] [nvarchar](200) NULL,
	[Zona] [nvarchar](150) NULL,
	[Precio_Hora] [decimal](12, 2) NOT NULL,
	[Estrellas] [int] NOT NULL,
	[Cantidad_Paseos] [int] NOT NULL,
	[Licencia_Municipal] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id_Paseador] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Paseo]    Script Date: 30/10/2025 14:26:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Paseo](
	[Id_Paseo] [int] IDENTITY(1,1) NOT NULL,
	[Id_Paseador] [int] NOT NULL,
	[Id_Mascota] [int] NOT NULL,
	[Fecha] [date] NOT NULL,
	[Hora_Inicio] [datetime2](7) NOT NULL,
	[Hora_Fin] [datetime2](7) NULL,
	[Duracion] [int] NULL,
	[Ruta_GPS] [nvarchar](max) NULL,
	[Notas] [nvarchar](500) NULL,
	[Estado_Animo] [nvarchar](50) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id_Paseo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Peluquero]    Script Date: 30/10/2025 14:26:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Peluquero](
	[Id_Peluquero] [int] IDENTITY(1,1) NOT NULL,
	[Id_User] [int] NOT NULL,
	[Nombre] [nvarchar](150) NOT NULL,
	[Especialidad] [nvarchar](100) NULL,
	[Telefono] [nvarchar](30) NULL,
	[Direccion] [nvarchar](200) NULL,
	[Email] [nvarchar](320) NULL,
	[Descripcion] [nvarchar](500) NULL,
	[Calificacion_Promedio] [float] NULL,
	[ImagenUrl] [nvarchar](300) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id_Peluquero] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Perfil]    Script Date: 30/10/2025 14:26:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Perfil](
	[Id_Perfil] [int] IDENTITY(1,1) NOT NULL,
	[Id_Usuario] [int] NOT NULL,
	[FotoPerfil] [nvarchar](300) NULL,
	[Descripcion] [nvarchar](500) NULL,
	[AniosVigencia] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id_Perfil] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Prenda]    Script Date: 30/10/2025 14:26:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Prenda](
	[Id_Prenda] [int] IDENTITY(1,1) NOT NULL,
	[Color] [nvarchar](50) NOT NULL,
	[Tipo_Prenda] [nvarchar](100) NOT NULL,
	[Comprada] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id_Prenda] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Producto]    Script Date: 30/10/2025 14:26:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Producto](
	[Id_Producto] [int] IDENTITY(1,1) NOT NULL,
	[Nombre] [nvarchar](150) NOT NULL,
	[Categoria] [nvarchar](100) NOT NULL,
	[Descripcion] [nvarchar](1000) NULL,
	[Precio] [decimal](12, 2) NOT NULL,
	[ImagenUrl] [nvarchar](300) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id_Producto] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Resena]    Script Date: 30/10/2025 14:26:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Resena](
	[Id_Resena] [int] IDENTITY(1,1) NOT NULL,
	[Id_User] [int] NOT NULL,
	[Id_Servicio] [int] NOT NULL,
	[Calificacion] [int] NOT NULL,
	[Comentario] [nvarchar](1000) NULL,
	[Fecha] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id_Resena] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Reserva]    Script Date: 30/10/2025 14:26:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Reserva](
	[Id_Reserva] [int] IDENTITY(1,1) NOT NULL,
	[Id_User] [int] NOT NULL,
	[Id_Servicio] [int] NOT NULL,
	[Id_Mascota] [int] NOT NULL,
	[Fecha_Reserva] [date] NOT NULL,
	[Hora] [time](0) NOT NULL,
	[Id_EstadoReserva] [int] NOT NULL,
	[Precio_Final] [decimal](12, 2) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id_Reserva] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Servicio]    Script Date: 30/10/2025 14:26:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Servicio](
	[Id_Servicio] [int] IDENTITY(1,1) NOT NULL,
	[Nombre] [nvarchar](150) NOT NULL,
	[Descripcion] [nvarchar](500) NULL,
	[Precio_Base] [decimal](12, 2) NOT NULL,
	[Id_TipoServicio] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id_Servicio] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TipoServicio]    Script Date: 30/10/2025 14:26:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TipoServicio](
	[Id_TipoServicio] [int] IDENTITY(1,1) NOT NULL,
	[Descripcion] [nvarchar](100) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id_TipoServicio] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TipoUsuario]    Script Date: 30/10/2025 14:26:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TipoUsuario](
	[Id_TipoUsuario] [int] IDENTITY(1,1) NOT NULL,
	[Descripcion] [nvarchar](100) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id_TipoUsuario] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Tratamiento]    Script Date: 30/10/2025 14:26:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Tratamiento](
	[Id_Tratamiento] [int] IDENTITY(1,1) NOT NULL,
	[Id_Mascota] [int] NOT NULL,
	[Nombre] [nvarchar](150) NOT NULL,
	[Fecha_Inicio] [date] NOT NULL,
	[Proximo_Control] [date] NULL,
	[Veterinario] [nvarchar](150) NULL,
 CONSTRAINT [PK_Tratamiento] PRIMARY KEY CLUSTERED 
(
	[Id_Tratamiento] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Ubicacion]    Script Date: 30/10/2025 14:26:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Ubicacion](
	[Id_Ubicacion] [int] IDENTITY(1,1) NOT NULL,
	[Latitud] [decimal](10, 7) NOT NULL,
	[Longitud] [decimal](10, 7) NOT NULL,
	[Direccion] [nvarchar](200) NULL,
	[Tipo] [nvarchar](50) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id_Ubicacion] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[User]    Script Date: 30/10/2025 14:26:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[User](
	[Id_User] [int] IDENTITY(1,1) NOT NULL,
	[Id_Mail] [int] NOT NULL,
	[Nombre] [nvarchar](100) NOT NULL,
	[Apellido] [nvarchar](100) NOT NULL,
	[DNI] [nvarchar](20) NULL,
	[Telefono] [nvarchar](30) NULL,
	[Direccion] [nvarchar](200) NULL,
	[Ciudad] [nvarchar](100) NULL,
	[Provincia] [nvarchar](100) NULL,
	[Pais] [nvarchar](100) NULL,
	[Fecha_Nacimiento] [date] NULL,
	[Fecha_Registro] [datetime2](7) NOT NULL,
	[Id_Ubicacion] [int] NOT NULL,
	[Id_TipoUsuario] [int] NOT NULL,
	[Estado] [bit] NOT NULL,
 CONSTRAINT [PK__User__D03DEDCBA205EC98] PRIMARY KEY CLUSTERED 
(
	[Id_User] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Vacuna]    Script Date: 30/10/2025 14:26:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Vacuna](
	[Id_Vacuna] [int] IDENTITY(1,1) NOT NULL,
	[Id_Mascota] [int] NOT NULL,
	[Nombre] [nvarchar](150) NOT NULL,
	[Fecha_Aplicacion] [date] NULL,
	[Proxima_Dosis] [date] NULL,
	[Veterinario] [nvarchar](150) NULL,
	[Aplicada] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id_Vacuna] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Veterinario]    Script Date: 30/10/2025 14:26:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Veterinario](
	[Id_Vet] [int] IDENTITY(1,1) NOT NULL,
	[Id_Clínica] [int] NOT NULL,
	[Id_User] [int] NOT NULL,
	[Especialidad] [nvarchar](100) NULL,
	[Matricula] [nvarchar](50) NULL,
	[Clinica] [nvarchar](150) NULL,
	[Horario_Atencion] [nvarchar](200) NULL,
	[Valoracion_Promedio] [decimal](4, 2) NULL,
 CONSTRAINT [PK__Veterina__5263B1C533ECD740] PRIMARY KEY CLUSTERED 
(
	[Id_Vet] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET IDENTITY_INSERT [dbo].[Calendario] ON 

INSERT [dbo].[Calendario] ([Id_Calendario], [Id_User], [Nombre], [Descripcion], [FechaCreacion], [Activo]) VALUES (1, 2, N'Calendario de Cuidados', N'', CAST(N'2025-10-30T10:30:39.3922308' AS DateTime2), 1)
INSERT [dbo].[Calendario] ([Id_Calendario], [Id_User], [Nombre], [Descripcion], [FechaCreacion], [Activo]) VALUES (2, 9, N'Calendario de Cuidados', N'', CAST(N'2025-10-30T11:46:41.0548328' AS DateTime2), 1)
INSERT [dbo].[Calendario] ([Id_Calendario], [Id_User], [Nombre], [Descripcion], [FechaCreacion], [Activo]) VALUES (3, 16, N'Calendario de Cuidados', N'', CAST(N'2025-10-30T13:39:07.0253213' AS DateTime2), 1)
SET IDENTITY_INSERT [dbo].[Calendario] OFF
GO
SET IDENTITY_INSERT [dbo].[CalendarioEvento] ON 

INSERT [dbo].[CalendarioEvento] ([Id_Evento], [Id_Calendario], [Id_User], [Id_Mascota], [Titulo], [Descripcion], [Fecha], [Tipo]) VALUES (2, 1, 2, NULL, N'Baño', N'hola', CAST(N'2025-10-31T10:31:00.0000000' AS DateTime2), N'Paseo')
INSERT [dbo].[CalendarioEvento] ([Id_Evento], [Id_Calendario], [Id_User], [Id_Mascota], [Titulo], [Descripcion], [Fecha], [Tipo]) VALUES (5, 3, 16, NULL, N'vacuna', N'', CAST(N'2025-10-31T13:38:00.0000000' AS DateTime2), N'Vacuna')
SET IDENTITY_INSERT [dbo].[CalendarioEvento] OFF
GO
SET IDENTITY_INSERT [dbo].[Mail] ON 

INSERT [dbo].[Mail] ([Id_Mail], [Correo], [Contrasena], [Fecha_Creacion], [Ultimo_Acceso]) VALUES (1, N't@t', N't', CAST(N'2025-10-30T10:23:10.0826870' AS DateTime2), NULL)
INSERT [dbo].[Mail] ([Id_Mail], [Correo], [Contrasena], [Fecha_Creacion], [Ultimo_Acceso]) VALUES (2, N'wainerbrian@gmail.com', N'hola', CAST(N'2025-10-30T10:28:43.7803386' AS DateTime2), NULL)
INSERT [dbo].[Mail] ([Id_Mail], [Correo], [Contrasena], [Fecha_Creacion], [Ultimo_Acceso]) VALUES (3, N'temp_1761831623978@zooni.app', N'zooni@123', CAST(N'2025-10-30T10:40:23.9957649' AS DateTime2), NULL)
INSERT [dbo].[Mail] ([Id_Mail], [Correo], [Contrasena], [Fecha_Creacion], [Ultimo_Acceso]) VALUES (4, N'lu@gmail.com', N'hola', CAST(N'2025-10-30T10:40:29.4616063' AS DateTime2), NULL)
INSERT [dbo].[Mail] ([Id_Mail], [Correo], [Contrasena], [Fecha_Creacion], [Ultimo_Acceso]) VALUES (5, N'l@l', N'l', CAST(N'2025-10-30T10:43:06.9443973' AS DateTime2), NULL)
INSERT [dbo].[Mail] ([Id_Mail], [Correo], [Contrasena], [Fecha_Creacion], [Ultimo_Acceso]) VALUES (6, N's@s', N's', CAST(N'2025-10-30T10:54:11.5848244' AS DateTime2), NULL)
INSERT [dbo].[Mail] ([Id_Mail], [Correo], [Contrasena], [Fecha_Creacion], [Ultimo_Acceso]) VALUES (7, N'temp_1761832510722@zooni.app', N'zooni@123', CAST(N'2025-10-30T10:55:10.7235550' AS DateTime2), NULL)
INSERT [dbo].[Mail] ([Id_Mail], [Correo], [Contrasena], [Fecha_Creacion], [Ultimo_Acceso]) VALUES (8, N'ss@s', N'sss', CAST(N'2025-10-30T11:14:28.5797901' AS DateTime2), NULL)
INSERT [dbo].[Mail] ([Id_Mail], [Correo], [Contrasena], [Fecha_Creacion], [Ultimo_Acceso]) VALUES (9, N'lucasiandorin@gmail.com', N'2806', CAST(N'2025-10-30T11:42:36.2344835' AS DateTime2), NULL)
INSERT [dbo].[Mail] ([Id_Mail], [Correo], [Contrasena], [Fecha_Creacion], [Ultimo_Acceso]) VALUES (10, N'temp_1761836542873@zooni.app', N'zooni@123', CAST(N'2025-10-30T12:02:22.8768774' AS DateTime2), NULL)
INSERT [dbo].[Mail] ([Id_Mail], [Correo], [Contrasena], [Fecha_Creacion], [Ultimo_Acceso]) VALUES (11, N'd@d', N'd', CAST(N'2025-10-30T12:11:21.8421547' AS DateTime2), NULL)
INSERT [dbo].[Mail] ([Id_Mail], [Correo], [Contrasena], [Fecha_Creacion], [Ultimo_Acceso]) VALUES (12, N'temp_1761840724465@zooni.app', N'zooni@123', CAST(N'2025-10-30T13:12:04.4708083' AS DateTime2), NULL)
INSERT [dbo].[Mail] ([Id_Mail], [Correo], [Contrasena], [Fecha_Creacion], [Ultimo_Acceso]) VALUES (13, N'temp_1761841503788@zooni.app', N'zooni@123', CAST(N'2025-10-30T13:25:03.7936388' AS DateTime2), NULL)
INSERT [dbo].[Mail] ([Id_Mail], [Correo], [Contrasena], [Fecha_Creacion], [Ultimo_Acceso]) VALUES (14, N'e@e', N'a', CAST(N'2025-10-30T13:30:04.1593259' AS DateTime2), NULL)
INSERT [dbo].[Mail] ([Id_Mail], [Correo], [Contrasena], [Fecha_Creacion], [Ultimo_Acceso]) VALUES (15, N'temp_1761842129803@zooni.app', N'zooni@123', CAST(N'2025-10-30T13:35:29.8043037' AS DateTime2), NULL)
INSERT [dbo].[Mail] ([Id_Mail], [Correo], [Contrasena], [Fecha_Creacion], [Ultimo_Acceso]) VALUES (16, N'laila@gmail.com', N'aaa', CAST(N'2025-10-30T13:36:22.0210964' AS DateTime2), NULL)
INSERT [dbo].[Mail] ([Id_Mail], [Correo], [Contrasena], [Fecha_Creacion], [Ultimo_Acceso]) VALUES (17, N't@tt', N't', CAST(N'2025-10-30T13:40:33.4412787' AS DateTime2), NULL)
INSERT [dbo].[Mail] ([Id_Mail], [Correo], [Contrasena], [Fecha_Creacion], [Ultimo_Acceso]) VALUES (18, N't@ttt', N't', CAST(N'2025-10-30T13:55:08.4936833' AS DateTime2), NULL)
SET IDENTITY_INSERT [dbo].[Mail] OFF
GO
SET IDENTITY_INSERT [dbo].[Mascota] ON 

INSERT [dbo].[Mascota] ([Id_Mascota], [Id_User], [Nombre], [Especie], [Raza], [Sexo], [Edad], [Fecha_Nacimiento], [Peso], [Color], [Esterilizado], [Chip], [Foto]) VALUES (1, 1, N'tt', N'Gato', N'Persa', N'Macho', 80, CAST(N'2025-10-30' AS Date), CAST(5.70 AS Decimal(6, 2)), N'', 0, N'', N'')
INSERT [dbo].[Mascota] ([Id_Mascota], [Id_User], [Nombre], [Especie], [Raza], [Sexo], [Edad], [Fecha_Nacimiento], [Peso], [Color], [Esterilizado], [Chip], [Foto]) VALUES (3, 1, N'tt', N'Gato', N'Persa', N'No definido', 80, NULL, CAST(57.00 AS Decimal(6, 2)), NULL, 0, NULL, NULL)
INSERT [dbo].[Mascota] ([Id_Mascota], [Id_User], [Nombre], [Especie], [Raza], [Sexo], [Edad], [Fecha_Nacimiento], [Peso], [Color], [Esterilizado], [Chip], [Foto]) VALUES (4, 1, N'asdasd', N'Perro', N'Golden Retriever', N'Macho', 95, CAST(N'2025-10-30' AS Date), CAST(16.00 AS Decimal(6, 2)), N'', 0, N'', N'')
INSERT [dbo].[Mascota] ([Id_Mascota], [Id_User], [Nombre], [Especie], [Raza], [Sexo], [Edad], [Fecha_Nacimiento], [Peso], [Color], [Esterilizado], [Chip], [Foto]) VALUES (5, 2, N'Pocho', N'Gato', N'Siames', N'Macho', 59, CAST(N'2025-10-30' AS Date), CAST(5.80 AS Decimal(6, 2)), N'', 0, N'', N'')
INSERT [dbo].[Mascota] ([Id_Mascota], [Id_User], [Nombre], [Especie], [Raza], [Sexo], [Edad], [Fecha_Nacimiento], [Peso], [Color], [Esterilizado], [Chip], [Foto]) VALUES (6, 2, N'Pocho', N'Gato', N'Siames', N'No definido', 59, NULL, CAST(58.00 AS Decimal(6, 2)), NULL, 0, NULL, NULL)
INSERT [dbo].[Mascota] ([Id_Mascota], [Id_User], [Nombre], [Especie], [Raza], [Sexo], [Edad], [Fecha_Nacimiento], [Peso], [Color], [Esterilizado], [Chip], [Foto]) VALUES (7, 4, N'Gante', N'Gato', N'Maine Coon', N'Macho', 72, CAST(N'2025-10-30' AS Date), CAST(7.70 AS Decimal(6, 2)), N'', 0, N'', N'')
INSERT [dbo].[Mascota] ([Id_Mascota], [Id_User], [Nombre], [Especie], [Raza], [Sexo], [Edad], [Fecha_Nacimiento], [Peso], [Color], [Esterilizado], [Chip], [Foto]) VALUES (8, 4, N'Gante', N'Gato', N'Maine Coon', N'No definido', 72, NULL, CAST(77.00 AS Decimal(6, 2)), NULL, 0, NULL, NULL)
INSERT [dbo].[Mascota] ([Id_Mascota], [Id_User], [Nombre], [Especie], [Raza], [Sexo], [Edad], [Fecha_Nacimiento], [Peso], [Color], [Esterilizado], [Chip], [Foto]) VALUES (9, 5, N'asdasd', N'Perro', N'Pastor Alemán', N'Hembra', 113, CAST(N'2025-10-30' AS Date), CAST(32.30 AS Decimal(6, 2)), N'', 0, N'', N'')
INSERT [dbo].[Mascota] ([Id_Mascota], [Id_User], [Nombre], [Especie], [Raza], [Sexo], [Edad], [Fecha_Nacimiento], [Peso], [Color], [Esterilizado], [Chip], [Foto]) VALUES (10, 6, N'wswsws', N'Perro', N'Labrador Retriever', N'Macho', 128, CAST(N'2025-10-30' AS Date), CAST(41.70 AS Decimal(6, 2)), N'', 0, N'', N'')
INSERT [dbo].[Mascota] ([Id_Mascota], [Id_User], [Nombre], [Especie], [Raza], [Sexo], [Edad], [Fecha_Nacimiento], [Peso], [Color], [Esterilizado], [Chip], [Foto]) VALUES (11, 6, N'wswsws', N'Perro', N'Labrador Retriever', N'No definido', 128, NULL, CAST(417.00 AS Decimal(6, 2)), NULL, 0, NULL, NULL)
INSERT [dbo].[Mascota] ([Id_Mascota], [Id_User], [Nombre], [Especie], [Raza], [Sexo], [Edad], [Fecha_Nacimiento], [Peso], [Color], [Esterilizado], [Chip], [Foto]) VALUES (12, 8, N'y', N'Perro', N'Labrador Retriever', N'Macho', 97, CAST(N'2025-10-30' AS Date), CAST(28.00 AS Decimal(6, 2)), N'', 0, N'', N'')
INSERT [dbo].[Mascota] ([Id_Mascota], [Id_User], [Nombre], [Especie], [Raza], [Sexo], [Edad], [Fecha_Nacimiento], [Peso], [Color], [Esterilizado], [Chip], [Foto]) VALUES (13, 8, N'y', N'Perro', N'Labrador Retriever', N'No definido', 97, NULL, CAST(28.00 AS Decimal(6, 2)), NULL, 0, NULL, NULL)
INSERT [dbo].[Mascota] ([Id_Mascota], [Id_User], [Nombre], [Especie], [Raza], [Sexo], [Edad], [Fecha_Nacimiento], [Peso], [Color], [Esterilizado], [Chip], [Foto]) VALUES (14, 9, N'dobro', N'Perro', N'Caniche', N'Hembra', 209, CAST(N'2025-10-30' AS Date), CAST(44.80 AS Decimal(6, 2)), N'', 0, N'', N'')
INSERT [dbo].[Mascota] ([Id_Mascota], [Id_User], [Nombre], [Especie], [Raza], [Sexo], [Edad], [Fecha_Nacimiento], [Peso], [Color], [Esterilizado], [Chip], [Foto]) VALUES (15, 9, N'dobro', N'Perro', N'Caniche', N'No definido', 209, NULL, CAST(448.00 AS Decimal(6, 2)), NULL, 0, NULL, NULL)
INSERT [dbo].[Mascota] ([Id_Mascota], [Id_User], [Nombre], [Especie], [Raza], [Sexo], [Edad], [Fecha_Nacimiento], [Peso], [Color], [Esterilizado], [Chip], [Foto]) VALUES (16, 10, N'dedee', N'Perro', N'', N'', 0, CAST(N'2025-10-30' AS Date), CAST(0.00 AS Decimal(6, 2)), N'', 0, N'', N'')
INSERT [dbo].[Mascota] ([Id_Mascota], [Id_User], [Nombre], [Especie], [Raza], [Sexo], [Edad], [Fecha_Nacimiento], [Peso], [Color], [Esterilizado], [Chip], [Foto]) VALUES (17, 11, N'd', N'Perro', N'Golden Retriever', N'Hembra', 121, CAST(N'2025-10-30' AS Date), CAST(41.50 AS Decimal(6, 2)), N'', 0, N'', N'')
INSERT [dbo].[Mascota] ([Id_Mascota], [Id_User], [Nombre], [Especie], [Raza], [Sexo], [Edad], [Fecha_Nacimiento], [Peso], [Color], [Esterilizado], [Chip], [Foto]) VALUES (18, 11, N'd', N'Perro', N'Golden Retriever', N'No definido', 121, NULL, CAST(415.00 AS Decimal(6, 2)), NULL, 0, NULL, NULL)
INSERT [dbo].[Mascota] ([Id_Mascota], [Id_User], [Nombre], [Especie], [Raza], [Sexo], [Edad], [Fecha_Nacimiento], [Peso], [Color], [Esterilizado], [Chip], [Foto]) VALUES (19, 12, N'l', N'Perro', N'Pastor Alemán', N'Macho', 87, CAST(N'2025-10-30' AS Date), CAST(30.30 AS Decimal(6, 2)), N'', 0, N'', N'')
INSERT [dbo].[Mascota] ([Id_Mascota], [Id_User], [Nombre], [Especie], [Raza], [Sexo], [Edad], [Fecha_Nacimiento], [Peso], [Color], [Esterilizado], [Chip], [Foto]) VALUES (20, 13, N'e', N'Perro', N'Rottweiler', N'Macho', 97, CAST(N'2025-10-30' AS Date), CAST(44.10 AS Decimal(6, 2)), N'', 0, N'', N'')
INSERT [dbo].[Mascota] ([Id_Mascota], [Id_User], [Nombre], [Especie], [Raza], [Sexo], [Edad], [Fecha_Nacimiento], [Peso], [Color], [Esterilizado], [Chip], [Foto]) VALUES (21, 14, N'Pipi', N'Perro', N'Akita Inu', N'Macho', 53, CAST(N'2025-10-30' AS Date), CAST(28.80 AS Decimal(6, 2)), N'', 0, N'', N'')
INSERT [dbo].[Mascota] ([Id_Mascota], [Id_User], [Nombre], [Especie], [Raza], [Sexo], [Edad], [Fecha_Nacimiento], [Peso], [Color], [Esterilizado], [Chip], [Foto]) VALUES (22, 14, N'Pipi', N'Perro', N'Akita Inu', N'No definido', 53, NULL, CAST(288.00 AS Decimal(6, 2)), NULL, 0, NULL, NULL)
INSERT [dbo].[Mascota] ([Id_Mascota], [Id_User], [Nombre], [Especie], [Raza], [Sexo], [Edad], [Fecha_Nacimiento], [Peso], [Color], [Esterilizado], [Chip], [Foto]) VALUES (23, 16, N'perro', N'Perro', N'Golden Retriever', N'Hembra', 45, CAST(N'2025-10-30' AS Date), CAST(26.70 AS Decimal(6, 2)), N'', 0, N'', N'')
INSERT [dbo].[Mascota] ([Id_Mascota], [Id_User], [Nombre], [Especie], [Raza], [Sexo], [Edad], [Fecha_Nacimiento], [Peso], [Color], [Esterilizado], [Chip], [Foto]) VALUES (24, 16, N'perro', N'Perro', N'Golden Retriever', N'No definido', 45, NULL, CAST(267.00 AS Decimal(6, 2)), NULL, 0, NULL, NULL)
INSERT [dbo].[Mascota] ([Id_Mascota], [Id_User], [Nombre], [Especie], [Raza], [Sexo], [Edad], [Fecha_Nacimiento], [Peso], [Color], [Esterilizado], [Chip], [Foto]) VALUES (25, 17, N'asdasdasd', N'Gato', N'Maine Coon', N'Macho', 63, CAST(N'2025-10-30' AS Date), CAST(4.70 AS Decimal(6, 2)), N'', 0, N'', N'')
INSERT [dbo].[Mascota] ([Id_Mascota], [Id_User], [Nombre], [Especie], [Raza], [Sexo], [Edad], [Fecha_Nacimiento], [Peso], [Color], [Esterilizado], [Chip], [Foto]) VALUES (26, 17, N'asdasdasd', N'Gato', N'Maine Coon', N'No definido', 63, NULL, CAST(47.00 AS Decimal(6, 2)), NULL, 0, NULL, NULL)
INSERT [dbo].[Mascota] ([Id_Mascota], [Id_User], [Nombre], [Especie], [Raza], [Sexo], [Edad], [Fecha_Nacimiento], [Peso], [Color], [Esterilizado], [Chip], [Foto]) VALUES (27, 18, N'tt', N'Perro', N'Golden Retriever', N'Macho', 88, CAST(N'2025-10-30' AS Date), CAST(16.00 AS Decimal(6, 2)), N'', 0, N'', N'')
INSERT [dbo].[Mascota] ([Id_Mascota], [Id_User], [Nombre], [Especie], [Raza], [Sexo], [Edad], [Fecha_Nacimiento], [Peso], [Color], [Esterilizado], [Chip], [Foto]) VALUES (28, 18, N'tt', N'Perro', N'Golden Retriever', N'No definido', 88, NULL, CAST(16.00 AS Decimal(6, 2)), NULL, 0, NULL, NULL)
INSERT [dbo].[Mascota] ([Id_Mascota], [Id_User], [Nombre], [Especie], [Raza], [Sexo], [Edad], [Fecha_Nacimiento], [Peso], [Color], [Esterilizado], [Chip], [Foto]) VALUES (29, 18, N'sda', N'Gato', N'Bombay', N'Macho', 121, CAST(N'2025-10-30' AS Date), CAST(3.00 AS Decimal(6, 2)), N'', 0, N'', N'')
SET IDENTITY_INSERT [dbo].[Mascota] OFF
GO
SET IDENTITY_INSERT [dbo].[TipoUsuario] ON 

INSERT [dbo].[TipoUsuario] ([Id_TipoUsuario], [Descripcion]) VALUES (1, N'Dueño')
INSERT [dbo].[TipoUsuario] ([Id_TipoUsuario], [Descripcion]) VALUES (2, N'Usuario')
SET IDENTITY_INSERT [dbo].[TipoUsuario] OFF
GO
SET IDENTITY_INSERT [dbo].[Ubicacion] ON 

INSERT [dbo].[Ubicacion] ([Id_Ubicacion], [Latitud], [Longitud], [Direccion], [Tipo]) VALUES (1, CAST(-34.6037220 AS Decimal(10, 7)), CAST(-58.3815920 AS Decimal(10, 7)), N'Ubicación por defecto - CABA', N'Base')
INSERT [dbo].[Ubicacion] ([Id_Ubicacion], [Latitud], [Longitud], [Direccion], [Tipo]) VALUES (2, CAST(0.0000000 AS Decimal(10, 7)), CAST(0.0000000 AS Decimal(10, 7)), N'Sin especificar', N'Default')
SET IDENTITY_INSERT [dbo].[Ubicacion] OFF
GO
SET IDENTITY_INSERT [dbo].[User] ON 

INSERT [dbo].[User] ([Id_User], [Id_Mail], [Nombre], [Apellido], [DNI], [Telefono], [Direccion], [Ciudad], [Provincia], [Pais], [Fecha_Nacimiento], [Fecha_Registro], [Id_Ubicacion], [Id_TipoUsuario], [Estado]) VALUES (1, 1, N't', N't', NULL, N'+54 1', NULL, N'La Plata', N'Buenos Aires', N'Argentina', NULL, CAST(N'2025-10-30T10:23:10.0866587' AS DateTime2), 1, 1, 1)
INSERT [dbo].[User] ([Id_User], [Id_Mail], [Nombre], [Apellido], [DNI], [Telefono], [Direccion], [Ciudad], [Provincia], [Pais], [Fecha_Nacimiento], [Fecha_Registro], [Id_Ubicacion], [Id_TipoUsuario], [Estado]) VALUES (2, 2, N'Brian', N'Wainr', NULL, N'+54 1123457890', NULL, N'Palermo', N'Ciudad Autónoma de Buenos Aires', N'Argentina', NULL, CAST(N'2025-10-30T10:28:43.7803386' AS DateTime2), 1, 1, 1)
INSERT [dbo].[User] ([Id_User], [Id_Mail], [Nombre], [Apellido], [DNI], [Telefono], [Direccion], [Ciudad], [Provincia], [Pais], [Fecha_Nacimiento], [Fecha_Registro], [Id_Ubicacion], [Id_TipoUsuario], [Estado]) VALUES (3, 3, N'Nuevo', N'Usuario', NULL, NULL, NULL, NULL, NULL, NULL, NULL, CAST(N'2025-10-30T10:40:24.0152111' AS DateTime2), 1, 1, 1)
INSERT [dbo].[User] ([Id_User], [Id_Mail], [Nombre], [Apellido], [DNI], [Telefono], [Direccion], [Ciudad], [Provincia], [Pais], [Fecha_Nacimiento], [Fecha_Registro], [Id_Ubicacion], [Id_TipoUsuario], [Estado]) VALUES (4, 4, N'lu', N'kampel', NULL, N'+54 17367638732', NULL, N'Villa Crespo', N'Ciudad Autónoma de Buenos Aires', N'Argentina', NULL, CAST(N'2025-10-30T10:40:29.4616063' AS DateTime2), 1, 1, 1)
INSERT [dbo].[User] ([Id_User], [Id_Mail], [Nombre], [Apellido], [DNI], [Telefono], [Direccion], [Ciudad], [Provincia], [Pais], [Fecha_Nacimiento], [Fecha_Registro], [Id_Ubicacion], [Id_TipoUsuario], [Estado]) VALUES (5, 5, N'll', N'l', NULL, NULL, NULL, NULL, NULL, NULL, NULL, CAST(N'2025-10-30T10:43:06.9443973' AS DateTime2), 1, 1, 1)
INSERT [dbo].[User] ([Id_User], [Id_Mail], [Nombre], [Apellido], [DNI], [Telefono], [Direccion], [Ciudad], [Provincia], [Pais], [Fecha_Nacimiento], [Fecha_Registro], [Id_Ubicacion], [Id_TipoUsuario], [Estado]) VALUES (6, 6, N'swsws', N'sww', NULL, N'+54 1', NULL, N'Palermo', N'Ciudad Autónoma de Buenos Aires', N'Argentina', NULL, CAST(N'2025-10-30T10:54:11.5848244' AS DateTime2), 1, 1, 1)
INSERT [dbo].[User] ([Id_User], [Id_Mail], [Nombre], [Apellido], [DNI], [Telefono], [Direccion], [Ciudad], [Provincia], [Pais], [Fecha_Nacimiento], [Fecha_Registro], [Id_Ubicacion], [Id_TipoUsuario], [Estado]) VALUES (7, 7, N'Nuevo', N'Usuario', NULL, NULL, NULL, NULL, NULL, NULL, NULL, CAST(N'2025-10-30T10:55:10.7235550' AS DateTime2), 1, 1, 1)
INSERT [dbo].[User] ([Id_User], [Id_Mail], [Nombre], [Apellido], [DNI], [Telefono], [Direccion], [Ciudad], [Provincia], [Pais], [Fecha_Nacimiento], [Fecha_Registro], [Id_Ubicacion], [Id_TipoUsuario], [Estado]) VALUES (8, 8, N'ss', N'sssss', NULL, N'+598 s', NULL, N'Carmelo', N'Colonia', N'Uruguay', NULL, CAST(N'2025-10-30T11:14:28.5978173' AS DateTime2), 1, 1, 1)
INSERT [dbo].[User] ([Id_User], [Id_Mail], [Nombre], [Apellido], [DNI], [Telefono], [Direccion], [Ciudad], [Provincia], [Pais], [Fecha_Nacimiento], [Fecha_Registro], [Id_Ubicacion], [Id_TipoUsuario], [Estado]) VALUES (9, 9, N'Lucas', N'aiytv|', NULL, N'+54 1122629239', NULL, N'Villa Crespo', N'Ciudad Autónoma de Buenos Aires', N'Argentina', NULL, CAST(N'2025-10-30T11:42:36.2500292' AS DateTime2), 1, 1, 1)
INSERT [dbo].[User] ([Id_User], [Id_Mail], [Nombre], [Apellido], [DNI], [Telefono], [Direccion], [Ciudad], [Provincia], [Pais], [Fecha_Nacimiento], [Fecha_Registro], [Id_Ubicacion], [Id_TipoUsuario], [Estado]) VALUES (10, 10, N'Nuevo', N'Usuario', NULL, NULL, NULL, NULL, NULL, NULL, NULL, CAST(N'2025-10-30T12:02:22.8778776' AS DateTime2), 1, 1, 1)
INSERT [dbo].[User] ([Id_User], [Id_Mail], [Nombre], [Apellido], [DNI], [Telefono], [Direccion], [Ciudad], [Provincia], [Pais], [Fecha_Nacimiento], [Fecha_Registro], [Id_Ubicacion], [Id_TipoUsuario], [Estado]) VALUES (11, 11, N'd', N'd', NULL, N'+54 1', NULL, N'Santa Fe', N'Santa Fe', N'Argentina', NULL, CAST(N'2025-10-30T12:11:21.8421547' AS DateTime2), 1, 1, 1)
INSERT [dbo].[User] ([Id_User], [Id_Mail], [Nombre], [Apellido], [DNI], [Telefono], [Direccion], [Ciudad], [Provincia], [Pais], [Fecha_Nacimiento], [Fecha_Registro], [Id_Ubicacion], [Id_TipoUsuario], [Estado]) VALUES (12, 12, N'Nuevo', N'Usuario', NULL, NULL, NULL, NULL, NULL, NULL, NULL, CAST(N'2025-10-30T13:12:04.4715005' AS DateTime2), 1, 1, 1)
INSERT [dbo].[User] ([Id_User], [Id_Mail], [Nombre], [Apellido], [DNI], [Telefono], [Direccion], [Ciudad], [Provincia], [Pais], [Fecha_Nacimiento], [Fecha_Registro], [Id_Ubicacion], [Id_TipoUsuario], [Estado]) VALUES (13, 13, N'Nuevo', N'Usuario', NULL, NULL, NULL, NULL, NULL, NULL, NULL, CAST(N'2025-10-30T13:25:03.7946393' AS DateTime2), 1, 1, 1)
INSERT [dbo].[User] ([Id_User], [Id_Mail], [Nombre], [Apellido], [DNI], [Telefono], [Direccion], [Ciudad], [Provincia], [Pais], [Fecha_Nacimiento], [Fecha_Registro], [Id_Ubicacion], [Id_TipoUsuario], [Estado]) VALUES (14, 14, N'e', N'e', NULL, N'+54 3', NULL, N'Córdoba', N'Córdoba', N'Argentina', NULL, CAST(N'2025-10-30T13:30:04.1593259' AS DateTime2), 2, 2, 1)
INSERT [dbo].[User] ([Id_User], [Id_Mail], [Nombre], [Apellido], [DNI], [Telefono], [Direccion], [Ciudad], [Provincia], [Pais], [Fecha_Nacimiento], [Fecha_Registro], [Id_Ubicacion], [Id_TipoUsuario], [Estado]) VALUES (15, 15, N'Nuevo', N'Usuario', NULL, NULL, NULL, NULL, NULL, NULL, NULL, CAST(N'2025-10-30T13:35:29.8043037' AS DateTime2), 1, 1, 1)
INSERT [dbo].[User] ([Id_User], [Id_Mail], [Nombre], [Apellido], [DNI], [Telefono], [Direccion], [Ciudad], [Provincia], [Pais], [Fecha_Nacimiento], [Fecha_Registro], [Id_Ubicacion], [Id_TipoUsuario], [Estado]) VALUES (16, 16, N'lai', N'robb', NULL, N'+54 1', NULL, N'Palermo', N'Ciudad Autónoma de Buenos Aires', N'Argentina', NULL, CAST(N'2025-10-30T13:36:22.0220953' AS DateTime2), 1, 1, 1)
INSERT [dbo].[User] ([Id_User], [Id_Mail], [Nombre], [Apellido], [DNI], [Telefono], [Direccion], [Ciudad], [Provincia], [Pais], [Fecha_Nacimiento], [Fecha_Registro], [Id_Ubicacion], [Id_TipoUsuario], [Estado]) VALUES (17, 17, N't', N't', NULL, N'+54 1', NULL, N'Santa Fe', N'Santa Fe', N'Argentina', NULL, CAST(N'2025-10-30T13:40:33.4412787' AS DateTime2), 1, 1, 1)
INSERT [dbo].[User] ([Id_User], [Id_Mail], [Nombre], [Apellido], [DNI], [Telefono], [Direccion], [Ciudad], [Provincia], [Pais], [Fecha_Nacimiento], [Fecha_Registro], [Id_Ubicacion], [Id_TipoUsuario], [Estado]) VALUES (18, 18, N't', N't', NULL, N'+54 t', NULL, N'Santa Fe', N'Santa Fe', N'Argentina', NULL, CAST(N'2025-10-30T13:55:08.4936833' AS DateTime2), 1, 1, 1)
SET IDENTITY_INSERT [dbo].[User] OFF
GO
SET IDENTITY_INSERT [dbo].[Vacuna] ON 

INSERT [dbo].[Vacuna] ([Id_Vacuna], [Id_Mascota], [Nombre], [Fecha_Aplicacion], [Proxima_Dosis], [Veterinario], [Aplicada]) VALUES (1, 5, N'triple felina (v3: panleucopenia, calicivirus, rinotraqueítis)', CAST(N'2025-10-30' AS Date), CAST(N'2025-10-31' AS Date), N'asdsad', 1)
INSERT [dbo].[Vacuna] ([Id_Vacuna], [Id_Mascota], [Nombre], [Fecha_Aplicacion], [Proxima_Dosis], [Veterinario], [Aplicada]) VALUES (2, 5, N'Cuádruple Felina (V4: incluye Clamidiosis)', CAST(N'2025-10-30' AS Date), NULL, NULL, 1)
INSERT [dbo].[Vacuna] ([Id_Vacuna], [Id_Mascota], [Nombre], [Fecha_Aplicacion], [Proxima_Dosis], [Veterinario], [Aplicada]) VALUES (3, 5, N'Chequeo Geriátrico', CAST(N'2025-10-30' AS Date), NULL, NULL, 1)
INSERT [dbo].[Vacuna] ([Id_Vacuna], [Id_Mascota], [Nombre], [Fecha_Aplicacion], [Proxima_Dosis], [Veterinario], [Aplicada]) VALUES (4, 1, N'edeeded', CAST(N'2025-10-31' AS Date), NULL, N'3e33e3e', 0)
INSERT [dbo].[Vacuna] ([Id_Vacuna], [Id_Mascota], [Nombre], [Fecha_Aplicacion], [Proxima_Dosis], [Veterinario], [Aplicada]) VALUES (5, 1, N'e3ee3e3e3', CAST(N'2025-10-31' AS Date), NULL, N'3e232e32', 0)
INSERT [dbo].[Vacuna] ([Id_Vacuna], [Id_Mascota], [Nombre], [Fecha_Aplicacion], [Proxima_Dosis], [Veterinario], [Aplicada]) VALUES (6, 1, N'3e3e3e3e33e', CAST(N'2025-10-31' AS Date), CAST(N'2025-11-07' AS Date), N'no', 0)
INSERT [dbo].[Vacuna] ([Id_Vacuna], [Id_Mascota], [Nombre], [Fecha_Aplicacion], [Proxima_Dosis], [Veterinario], [Aplicada]) VALUES (7, 1, N'4t4t', CAST(N'4444-04-04' AS Date), CAST(N'4444-04-04' AS Date), N'4', 0)
INSERT [dbo].[Vacuna] ([Id_Vacuna], [Id_Mascota], [Nombre], [Fecha_Aplicacion], [Proxima_Dosis], [Veterinario], [Aplicada]) VALUES (10, 29, N'Triple Felina (V3: Panleucopenia, Calicivirus, Rinotraqueítis)', CAST(N'2025-10-30' AS Date), NULL, NULL, 1)
INSERT [dbo].[Vacuna] ([Id_Vacuna], [Id_Mascota], [Nombre], [Fecha_Aplicacion], [Proxima_Dosis], [Veterinario], [Aplicada]) VALUES (11, 29, N'Cuádruple Felina (V4: incluye Clamidiosis)', CAST(N'2025-10-30' AS Date), NULL, NULL, 1)
INSERT [dbo].[Vacuna] ([Id_Vacuna], [Id_Mascota], [Nombre], [Fecha_Aplicacion], [Proxima_Dosis], [Veterinario], [Aplicada]) VALUES (12, 29, N'Leucemia Felina (FeLV)', CAST(N'2025-10-30' AS Date), NULL, NULL, 1)
INSERT [dbo].[Vacuna] ([Id_Vacuna], [Id_Mascota], [Nombre], [Fecha_Aplicacion], [Proxima_Dosis], [Veterinario], [Aplicada]) VALUES (13, 29, N'Rabia Felina', CAST(N'2025-10-30' AS Date), NULL, NULL, 1)
INSERT [dbo].[Vacuna] ([Id_Vacuna], [Id_Mascota], [Nombre], [Fecha_Aplicacion], [Proxima_Dosis], [Veterinario], [Aplicada]) VALUES (14, 29, N'Rinotraqueítis Viral Felina', CAST(N'2025-10-30' AS Date), NULL, NULL, 1)
INSERT [dbo].[Vacuna] ([Id_Vacuna], [Id_Mascota], [Nombre], [Fecha_Aplicacion], [Proxima_Dosis], [Veterinario], [Aplicada]) VALUES (15, 29, N'Panleucopenia Felina', CAST(N'2025-10-30' AS Date), NULL, NULL, 1)
SET IDENTITY_INSERT [dbo].[Vacuna] OFF
GO
/****** Object:  Index [IX_CalendarioEvento_Calendario_Fecha]    Script Date: 30/10/2025 14:26:56 ******/
CREATE NONCLUSTERED INDEX [IX_CalendarioEvento_Calendario_Fecha] ON [dbo].[CalendarioEvento]
(
	[Id_Calendario] ASC,
	[Fecha] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ_CirculoConfianza]    Script Date: 30/10/2025 14:26:56 ******/
ALTER TABLE [dbo].[CirculoConfianza] ADD  CONSTRAINT [UQ_CirculoConfianza] UNIQUE NONCLUSTERED 
(
	[Id_User] ASC,
	[Id_Amigo] ASC,
	[Rol] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_Comportamiento_Mascota_Fecha]    Script Date: 30/10/2025 14:26:56 ******/
CREATE NONCLUSTERED INDEX [IX_Comportamiento_Mascota_Fecha] ON [dbo].[Comportamiento]
(
	[Id_Mascota] ASC,
	[Fecha] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [UQ_CuidadoDiario_Mascota_Fecha]    Script Date: 30/10/2025 14:26:56 ******/
ALTER TABLE [dbo].[CuidadoDiario] ADD  CONSTRAINT [UQ_CuidadoDiario_Mascota_Fecha] UNIQUE NONCLUSTERED 
(
	[Id_Mascota] ASC,
	[Fecha] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__EstadoPa__92C53B6C1EE77986]    Script Date: 30/10/2025 14:26:56 ******/
ALTER TABLE [dbo].[EstadoPago] ADD UNIQUE NONCLUSTERED 
(
	[Descripcion] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__EstadoPa__92C53B6C801F0A2F]    Script Date: 30/10/2025 14:26:56 ******/
ALTER TABLE [dbo].[EstadoPago] ADD UNIQUE NONCLUSTERED 
(
	[Descripcion] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__EstadoRe__92C53B6CBA19B547]    Script Date: 30/10/2025 14:26:56 ******/
ALTER TABLE [dbo].[EstadoReserva] ADD UNIQUE NONCLUSTERED 
(
	[Descripcion] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__EstadoRe__92C53B6CC1D43127]    Script Date: 30/10/2025 14:26:56 ******/
ALTER TABLE [dbo].[EstadoReserva] ADD UNIQUE NONCLUSTERED 
(
	[Descripcion] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_EventoMascota_Mascota_Fecha]    Script Date: 30/10/2025 14:26:56 ******/
CREATE NONCLUSTERED INDEX [IX_EventoMascota_Mascota_Fecha] ON [dbo].[EventoMascota]
(
	[Id_Mascota] ASC,
	[Fecha] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_HistorialMedico_Mascota_Fecha]    Script Date: 30/10/2025 14:26:56 ******/
CREATE NONCLUSTERED INDEX [IX_HistorialMedico_Mascota_Fecha] ON [dbo].[HistorialMedico]
(
	[Id_Mascota] ASC,
	[Fecha] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Invitacion_Receptor_Estado]    Script Date: 30/10/2025 14:26:56 ******/
CREATE NONCLUSTERED INDEX [IX_Invitacion_Receptor_Estado] ON [dbo].[Invitacion]
(
	[Id_Receptor] ASC,
	[Estado] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_Logro_User_Mascota]    Script Date: 30/10/2025 14:26:56 ******/
CREATE NONCLUSTERED INDEX [IX_Logro_User_Mascota] ON [dbo].[Logro]
(
	[Id_User] ASC,
	[Id_Mascota] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__Mail__60695A194FD5863D]    Script Date: 30/10/2025 14:26:56 ******/
ALTER TABLE [dbo].[Mail] ADD UNIQUE NONCLUSTERED 
(
	[Correo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__Mail__60695A19915951E8]    Script Date: 30/10/2025 14:26:56 ******/
ALTER TABLE [dbo].[Mail] ADD UNIQUE NONCLUSTERED 
(
	[Correo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_Mascota_User]    Script Date: 30/10/2025 14:26:56 ******/
CREATE NONCLUSTERED INDEX [IX_Mascota_User] ON [dbo].[Mascota]
(
	[Id_User] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_Mensaje_Chat_Fecha]    Script Date: 30/10/2025 14:26:56 ******/
CREATE NONCLUSTERED INDEX [IX_Mensaje_Chat_Fecha] ON [dbo].[Mensaje]
(
	[Id_Chat] ASC,
	[Fecha] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__MetodoPa__92C53B6C940DF2ED]    Script Date: 30/10/2025 14:26:56 ******/
ALTER TABLE [dbo].[MetodoPago] ADD UNIQUE NONCLUSTERED 
(
	[Descripcion] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__MetodoPa__92C53B6CAC4DE94E]    Script Date: 30/10/2025 14:26:56 ******/
ALTER TABLE [dbo].[MetodoPago] ADD UNIQUE NONCLUSTERED 
(
	[Descripcion] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_ModoViaje_User]    Script Date: 30/10/2025 14:26:56 ******/
CREATE NONCLUSTERED INDEX [IX_ModoViaje_User] ON [dbo].[ModoViaje]
(
	[Id_User] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_Notificacion_User_Leida_Fecha]    Script Date: 30/10/2025 14:26:56 ******/
CREATE NONCLUSTERED INDEX [IX_Notificacion_User_Leida_Fecha] ON [dbo].[Notificacion]
(
	[Id_User] ASC,
	[Leida] ASC,
	[Fecha] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [UQ_ParticipanteChat_Chat_User]    Script Date: 30/10/2025 14:26:56 ******/
CREATE UNIQUE NONCLUSTERED INDEX [UQ_ParticipanteChat_Chat_User] ON [dbo].[ParticipanteChat]
(
	[Id_Chat] ASC,
	[Id_User] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [UQ_Paseador_User]    Script Date: 30/10/2025 14:26:56 ******/
CREATE UNIQUE NONCLUSTERED INDEX [UQ_Paseador_User] ON [dbo].[Paseador]
(
	[Id_User] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_Paseo_Paseador_Fecha]    Script Date: 30/10/2025 14:26:56 ******/
CREATE NONCLUSTERED INDEX [IX_Paseo_Paseador_Fecha] ON [dbo].[Paseo]
(
	[Id_Paseador] ASC,
	[Fecha] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_Resena_Servicio_Fecha]    Script Date: 30/10/2025 14:26:56 ******/
CREATE NONCLUSTERED INDEX [IX_Resena_Servicio_Fecha] ON [dbo].[Resena]
(
	[Id_Servicio] ASC,
	[Fecha] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_Reserva_User_Fecha]    Script Date: 30/10/2025 14:26:56 ******/
CREATE NONCLUSTERED INDEX [IX_Reserva_User_Fecha] ON [dbo].[Reserva]
(
	[Id_User] ASC,
	[Fecha_Reserva] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__TipoServ__92C53B6C385A46B3]    Script Date: 30/10/2025 14:26:56 ******/
ALTER TABLE [dbo].[TipoServicio] ADD UNIQUE NONCLUSTERED 
(
	[Descripcion] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__TipoServ__92C53B6CE0117946]    Script Date: 30/10/2025 14:26:56 ******/
ALTER TABLE [dbo].[TipoServicio] ADD UNIQUE NONCLUSTERED 
(
	[Descripcion] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__TipoUsua__92C53B6C2667A93D]    Script Date: 30/10/2025 14:26:56 ******/
ALTER TABLE [dbo].[TipoUsuario] ADD UNIQUE NONCLUSTERED 
(
	[Descripcion] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__TipoUsua__92C53B6CBEBF37C6]    Script Date: 30/10/2025 14:26:56 ******/
ALTER TABLE [dbo].[TipoUsuario] ADD UNIQUE NONCLUSTERED 
(
	[Descripcion] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_Tratamiento_Mascota_FechaInicio]    Script Date: 30/10/2025 14:26:56 ******/
CREATE NONCLUSTERED INDEX [IX_Tratamiento_Mascota_FechaInicio] ON [dbo].[Tratamiento]
(
	[Id_Mascota] ASC,
	[Fecha_Inicio] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ_User_DNI]    Script Date: 30/10/2025 14:26:56 ******/
CREATE UNIQUE NONCLUSTERED INDEX [UQ_User_DNI] ON [dbo].[User]
(
	[DNI] ASC
)
WHERE ([DNI] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_Vacuna_Mascota_Fecha]    Script Date: 30/10/2025 14:26:56 ******/
CREATE NONCLUSTERED INDEX [IX_Vacuna_Mascota_Fecha] ON [dbo].[Vacuna]
(
	[Id_Mascota] ASC,
	[Fecha_Aplicacion] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [UQ_Veterinario_User]    Script Date: 30/10/2025 14:26:56 ******/
CREATE UNIQUE NONCLUSTERED INDEX [UQ_Veterinario_User] ON [dbo].[Veterinario]
(
	[Id_User] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Calendario] ADD  DEFAULT (sysutcdatetime()) FOR [FechaCreacion]
GO
ALTER TABLE [dbo].[Calendario] ADD  DEFAULT ((1)) FOR [Activo]
GO
ALTER TABLE [dbo].[Chat] ADD  DEFAULT (sysutcdatetime()) FOR [FechaCreacion]
GO
ALTER TABLE [dbo].[Comunidad] ADD  DEFAULT (sysutcdatetime()) FOR [Fecha]
GO
ALTER TABLE [dbo].[Invitacion] ADD  DEFAULT (sysutcdatetime()) FOR [Fecha]
GO
ALTER TABLE [dbo].[Logro] ADD  DEFAULT (sysutcdatetime()) FOR [Fecha_Obtenido]
GO
ALTER TABLE [dbo].[Mail] ADD  DEFAULT (sysutcdatetime()) FOR [Fecha_Creacion]
GO
ALTER TABLE [dbo].[Mascota] ADD  CONSTRAINT [DF__Mascota__Esteril__14270015]  DEFAULT ((0)) FOR [Esterilizado]
GO
ALTER TABLE [dbo].[Mensaje] ADD  DEFAULT (sysutcdatetime()) FOR [Fecha]
GO
ALTER TABLE [dbo].[Mensaje] ADD  DEFAULT ((0)) FOR [Leido]
GO
ALTER TABLE [dbo].[Notificacion] ADD  DEFAULT (sysutcdatetime()) FOR [Fecha]
GO
ALTER TABLE [dbo].[Notificacion] ADD  DEFAULT ((0)) FOR [Leida]
GO
ALTER TABLE [dbo].[Pago] ADD  DEFAULT (sysutcdatetime()) FOR [Fecha_Pago]
GO
ALTER TABLE [dbo].[ParticipanteChat] ADD  DEFAULT ((0)) FOR [Administrador]
GO
ALTER TABLE [dbo].[ParticipanteChat] ADD  DEFAULT (sysutcdatetime()) FOR [FechaIngreso]
GO
ALTER TABLE [dbo].[Paseador] ADD  DEFAULT ((0)) FOR [Experiencia_Anios]
GO
ALTER TABLE [dbo].[Paseador] ADD  DEFAULT ((0)) FOR [Precio_Hora]
GO
ALTER TABLE [dbo].[Paseador] ADD  DEFAULT ((0)) FOR [Estrellas]
GO
ALTER TABLE [dbo].[Paseador] ADD  DEFAULT ((0)) FOR [Cantidad_Paseos]
GO
ALTER TABLE [dbo].[Paseador] ADD  DEFAULT ((0)) FOR [Licencia_Municipal]
GO
ALTER TABLE [dbo].[Prenda] ADD  DEFAULT ((0)) FOR [Comprada]
GO
ALTER TABLE [dbo].[Resena] ADD  DEFAULT (sysutcdatetime()) FOR [Fecha]
GO
ALTER TABLE [dbo].[Servicio] ADD  DEFAULT ((0)) FOR [Precio_Base]
GO
ALTER TABLE [dbo].[User] ADD  CONSTRAINT [DF__User__Fecha_Regi__14270015]  DEFAULT (sysutcdatetime()) FOR [Fecha_Registro]
GO
ALTER TABLE [dbo].[User] ADD  CONSTRAINT [DF__User__Estado__151B244E]  DEFAULT ((1)) FOR [Estado]
GO
ALTER TABLE [dbo].[Vacuna] ADD  DEFAULT ((0)) FOR [Aplicada]
GO
ALTER TABLE [dbo].[Calendario]  WITH CHECK ADD  CONSTRAINT [FK_Calendario_User] FOREIGN KEY([Id_User])
REFERENCES [dbo].[User] ([Id_User])
GO
ALTER TABLE [dbo].[Calendario] CHECK CONSTRAINT [FK_Calendario_User]
GO
ALTER TABLE [dbo].[CalendarioEvento]  WITH CHECK ADD  CONSTRAINT [FK_CalendarioEvento_Calendario] FOREIGN KEY([Id_Calendario])
REFERENCES [dbo].[Calendario] ([Id_Calendario])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CalendarioEvento] CHECK CONSTRAINT [FK_CalendarioEvento_Calendario]
GO
ALTER TABLE [dbo].[CalendarioEvento]  WITH CHECK ADD  CONSTRAINT [FK_CalendarioEvento_User] FOREIGN KEY([Id_User])
REFERENCES [dbo].[User] ([Id_User])
GO
ALTER TABLE [dbo].[CalendarioEvento] CHECK CONSTRAINT [FK_CalendarioEvento_User]
GO
ALTER TABLE [dbo].[CirculoConfianza]  WITH CHECK ADD  CONSTRAINT [FK_Circulo_Amigo] FOREIGN KEY([Id_Amigo])
REFERENCES [dbo].[User] ([Id_User])
GO
ALTER TABLE [dbo].[CirculoConfianza] CHECK CONSTRAINT [FK_Circulo_Amigo]
GO
ALTER TABLE [dbo].[CirculoConfianza]  WITH CHECK ADD  CONSTRAINT [FK_Circulo_User] FOREIGN KEY([Id_User])
REFERENCES [dbo].[User] ([Id_User])
GO
ALTER TABLE [dbo].[CirculoConfianza] CHECK CONSTRAINT [FK_Circulo_User]
GO
ALTER TABLE [dbo].[Comportamiento]  WITH CHECK ADD  CONSTRAINT [FK_Comportamiento_Mascota] FOREIGN KEY([Id_Mascota])
REFERENCES [dbo].[Mascota] ([Id_Mascota])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Comportamiento] CHECK CONSTRAINT [FK_Comportamiento_Mascota]
GO
ALTER TABLE [dbo].[Compra]  WITH CHECK ADD  CONSTRAINT [FK_Compra_Producto] FOREIGN KEY([Id_Producto])
REFERENCES [dbo].[Producto] ([Id_Producto])
GO
ALTER TABLE [dbo].[Compra] CHECK CONSTRAINT [FK_Compra_Producto]
GO
ALTER TABLE [dbo].[Compra]  WITH CHECK ADD  CONSTRAINT [FK_Compra_User] FOREIGN KEY([Id_User])
REFERENCES [dbo].[User] ([Id_User])
GO
ALTER TABLE [dbo].[Compra] CHECK CONSTRAINT [FK_Compra_User]
GO
ALTER TABLE [dbo].[ComunidadXUsuario]  WITH CHECK ADD  CONSTRAINT [FK_ComunidadXUsuario_Comunidad] FOREIGN KEY([Id_Comunidad])
REFERENCES [dbo].[Comunidad] ([Id_Comunidad])
GO
ALTER TABLE [dbo].[ComunidadXUsuario] CHECK CONSTRAINT [FK_ComunidadXUsuario_Comunidad]
GO
ALTER TABLE [dbo].[ComunidadXUsuario]  WITH CHECK ADD  CONSTRAINT [FK_ComunidadXUsuario_User] FOREIGN KEY([Id_User])
REFERENCES [dbo].[User] ([Id_User])
GO
ALTER TABLE [dbo].[ComunidadXUsuario] CHECK CONSTRAINT [FK_ComunidadXUsuario_User]
GO
ALTER TABLE [dbo].[CuidadoDiario]  WITH CHECK ADD  CONSTRAINT [FK_CuidadoDiario_Mascota] FOREIGN KEY([Id_Mascota])
REFERENCES [dbo].[Mascota] ([Id_Mascota])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CuidadoDiario] CHECK CONSTRAINT [FK_CuidadoDiario_Mascota]
GO
ALTER TABLE [dbo].[Dieta]  WITH CHECK ADD  CONSTRAINT [FK_Dieta_Mascota] FOREIGN KEY([Id_Mascota])
REFERENCES [dbo].[Mascota] ([Id_Mascota])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Dieta] CHECK CONSTRAINT [FK_Dieta_Mascota]
GO
ALTER TABLE [dbo].[Dieta_Comida]  WITH CHECK ADD  CONSTRAINT [FK_DietaComida_Comida] FOREIGN KEY([Id_Comida])
REFERENCES [dbo].[Comida] ([Id_Comida])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Dieta_Comida] CHECK CONSTRAINT [FK_DietaComida_Comida]
GO
ALTER TABLE [dbo].[Dieta_Comida]  WITH CHECK ADD  CONSTRAINT [FK_DietaComida_Dieta] FOREIGN KEY([Id_Dieta])
REFERENCES [dbo].[Dieta] ([Id_Dieta])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Dieta_Comida] CHECK CONSTRAINT [FK_DietaComida_Dieta]
GO
ALTER TABLE [dbo].[EventoMascota]  WITH CHECK ADD  CONSTRAINT [FK_EventoMascota_Mascota] FOREIGN KEY([Id_Mascota])
REFERENCES [dbo].[Mascota] ([Id_Mascota])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[EventoMascota] CHECK CONSTRAINT [FK_EventoMascota_Mascota]
GO
ALTER TABLE [dbo].[HistorialMedico]  WITH CHECK ADD  CONSTRAINT [FK_HistorialMedico_Mascota] FOREIGN KEY([Id_Mascota])
REFERENCES [dbo].[Mascota] ([Id_Mascota])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[HistorialMedico] CHECK CONSTRAINT [FK_HistorialMedico_Mascota]
GO
ALTER TABLE [dbo].[HistorialMedico]  WITH CHECK ADD  CONSTRAINT [FK_HistorialMedico_Vet] FOREIGN KEY([Id_Vet])
REFERENCES [dbo].[Veterinario] ([Id_Vet])
GO
ALTER TABLE [dbo].[HistorialMedico] CHECK CONSTRAINT [FK_HistorialMedico_Vet]
GO
ALTER TABLE [dbo].[Invitacion]  WITH CHECK ADD  CONSTRAINT [FK_Invitacion_Emisor] FOREIGN KEY([Id_Emisor])
REFERENCES [dbo].[User] ([Id_User])
GO
ALTER TABLE [dbo].[Invitacion] CHECK CONSTRAINT [FK_Invitacion_Emisor]
GO
ALTER TABLE [dbo].[Invitacion]  WITH CHECK ADD  CONSTRAINT [FK_Invitacion_Mascota] FOREIGN KEY([Id_Mascota])
REFERENCES [dbo].[Mascota] ([Id_Mascota])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Invitacion] CHECK CONSTRAINT [FK_Invitacion_Mascota]
GO
ALTER TABLE [dbo].[Invitacion]  WITH CHECK ADD  CONSTRAINT [FK_Invitacion_Receptor] FOREIGN KEY([Id_Receptor])
REFERENCES [dbo].[User] ([Id_User])
GO
ALTER TABLE [dbo].[Invitacion] CHECK CONSTRAINT [FK_Invitacion_Receptor]
GO
ALTER TABLE [dbo].[Logro]  WITH CHECK ADD  CONSTRAINT [FK_Logro_Mascota] FOREIGN KEY([Id_Mascota])
REFERENCES [dbo].[Mascota] ([Id_Mascota])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Logro] CHECK CONSTRAINT [FK_Logro_Mascota]
GO
ALTER TABLE [dbo].[Logro]  WITH CHECK ADD  CONSTRAINT [FK_Logro_User] FOREIGN KEY([Id_User])
REFERENCES [dbo].[User] ([Id_User])
GO
ALTER TABLE [dbo].[Logro] CHECK CONSTRAINT [FK_Logro_User]
GO
ALTER TABLE [dbo].[MascotaXConsejo]  WITH CHECK ADD  CONSTRAINT [FK_MascotaXConsejo_Consejo] FOREIGN KEY([Id_Consejo])
REFERENCES [dbo].[Consejo] ([Id_Consejo])
GO
ALTER TABLE [dbo].[MascotaXConsejo] CHECK CONSTRAINT [FK_MascotaXConsejo_Consejo]
GO
ALTER TABLE [dbo].[MascotaXConsejo]  WITH CHECK ADD  CONSTRAINT [FK_MascotaXConsejo_Mascota] FOREIGN KEY([Id_Macota])
REFERENCES [dbo].[Mascota] ([Id_Mascota])
GO
ALTER TABLE [dbo].[MascotaXConsejo] CHECK CONSTRAINT [FK_MascotaXConsejo_Mascota]
GO
ALTER TABLE [dbo].[MascotaXPrenda]  WITH CHECK ADD  CONSTRAINT [FK_MascotaXPrenda_Mascota] FOREIGN KEY([Id_Mascota])
REFERENCES [dbo].[Mascota] ([Id_Mascota])
GO
ALTER TABLE [dbo].[MascotaXPrenda] CHECK CONSTRAINT [FK_MascotaXPrenda_Mascota]
GO
ALTER TABLE [dbo].[MascotaXPrenda]  WITH CHECK ADD  CONSTRAINT [FK_MascotaXPrenda_Prenda] FOREIGN KEY([Id_Prenda])
REFERENCES [dbo].[Prenda] ([Id_Prenda])
GO
ALTER TABLE [dbo].[MascotaXPrenda] CHECK CONSTRAINT [FK_MascotaXPrenda_Prenda]
GO
ALTER TABLE [dbo].[Mensaje]  WITH CHECK ADD  CONSTRAINT [FK_Mensaje_Chat] FOREIGN KEY([Id_Chat])
REFERENCES [dbo].[Chat] ([Id_Chat])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Mensaje] CHECK CONSTRAINT [FK_Mensaje_Chat]
GO
ALTER TABLE [dbo].[Mensaje]  WITH CHECK ADD  CONSTRAINT [FK_Mensaje_User] FOREIGN KEY([Id_User])
REFERENCES [dbo].[User] ([Id_User])
GO
ALTER TABLE [dbo].[Mensaje] CHECK CONSTRAINT [FK_Mensaje_User]
GO
ALTER TABLE [dbo].[ModoViaje]  WITH CHECK ADD  CONSTRAINT [FK_ModoViaje_Mascota] FOREIGN KEY([Id_Mascota])
REFERENCES [dbo].[Mascota] ([Id_Mascota])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ModoViaje] CHECK CONSTRAINT [FK_ModoViaje_Mascota]
GO
ALTER TABLE [dbo].[ModoViaje]  WITH CHECK ADD  CONSTRAINT [FK_ModoViaje_Paseador] FOREIGN KEY([Id_Paseador])
REFERENCES [dbo].[Paseador] ([Id_Paseador])
GO
ALTER TABLE [dbo].[ModoViaje] CHECK CONSTRAINT [FK_ModoViaje_Paseador]
GO
ALTER TABLE [dbo].[ModoViaje]  WITH CHECK ADD  CONSTRAINT [FK_ModoViaje_User] FOREIGN KEY([Id_User])
REFERENCES [dbo].[User] ([Id_User])
GO
ALTER TABLE [dbo].[ModoViaje] CHECK CONSTRAINT [FK_ModoViaje_User]
GO
ALTER TABLE [dbo].[Notificacion]  WITH CHECK ADD  CONSTRAINT [FK_Notificacion_User] FOREIGN KEY([Id_User])
REFERENCES [dbo].[User] ([Id_User])
GO
ALTER TABLE [dbo].[Notificacion] CHECK CONSTRAINT [FK_Notificacion_User]
GO
ALTER TABLE [dbo].[Pago]  WITH CHECK ADD  CONSTRAINT [FK_Pago_EstadoPago] FOREIGN KEY([Id_EstadoPago])
REFERENCES [dbo].[EstadoPago] ([Id_EstadoPago])
GO
ALTER TABLE [dbo].[Pago] CHECK CONSTRAINT [FK_Pago_EstadoPago]
GO
ALTER TABLE [dbo].[Pago]  WITH CHECK ADD  CONSTRAINT [FK_Pago_MetodoPago] FOREIGN KEY([Id_MetodoPago])
REFERENCES [dbo].[MetodoPago] ([Id_MetodoPago])
GO
ALTER TABLE [dbo].[Pago] CHECK CONSTRAINT [FK_Pago_MetodoPago]
GO
ALTER TABLE [dbo].[Pago]  WITH CHECK ADD  CONSTRAINT [FK_Pago_Reserva] FOREIGN KEY([Id_Reserva])
REFERENCES [dbo].[Reserva] ([Id_Reserva])
GO
ALTER TABLE [dbo].[Pago] CHECK CONSTRAINT [FK_Pago_Reserva]
GO
ALTER TABLE [dbo].[ParticipanteChat]  WITH CHECK ADD  CONSTRAINT [FK_ParticipanteChat_Chat] FOREIGN KEY([Id_Chat])
REFERENCES [dbo].[Chat] ([Id_Chat])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ParticipanteChat] CHECK CONSTRAINT [FK_ParticipanteChat_Chat]
GO
ALTER TABLE [dbo].[ParticipanteChat]  WITH CHECK ADD  CONSTRAINT [FK_ParticipanteChat_User] FOREIGN KEY([Id_User])
REFERENCES [dbo].[User] ([Id_User])
GO
ALTER TABLE [dbo].[ParticipanteChat] CHECK CONSTRAINT [FK_ParticipanteChat_User]
GO
ALTER TABLE [dbo].[Paseador]  WITH CHECK ADD  CONSTRAINT [FK_Paseador_User] FOREIGN KEY([Id_User])
REFERENCES [dbo].[User] ([Id_User])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Paseador] CHECK CONSTRAINT [FK_Paseador_User]
GO
ALTER TABLE [dbo].[Paseo]  WITH CHECK ADD  CONSTRAINT [FK_Paseo_Mascota] FOREIGN KEY([Id_Mascota])
REFERENCES [dbo].[Mascota] ([Id_Mascota])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Paseo] CHECK CONSTRAINT [FK_Paseo_Mascota]
GO
ALTER TABLE [dbo].[Paseo]  WITH CHECK ADD  CONSTRAINT [FK_Paseo_Paseador] FOREIGN KEY([Id_Paseador])
REFERENCES [dbo].[Paseador] ([Id_Paseador])
GO
ALTER TABLE [dbo].[Paseo] CHECK CONSTRAINT [FK_Paseo_Paseador]
GO
ALTER TABLE [dbo].[Peluquero]  WITH CHECK ADD  CONSTRAINT [FK_Peluquero_User] FOREIGN KEY([Id_User])
REFERENCES [dbo].[User] ([Id_User])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Peluquero] CHECK CONSTRAINT [FK_Peluquero_User]
GO
ALTER TABLE [dbo].[Perfil]  WITH CHECK ADD  CONSTRAINT [FK_Perfil_User] FOREIGN KEY([Id_Usuario])
REFERENCES [dbo].[User] ([Id_User])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Perfil] CHECK CONSTRAINT [FK_Perfil_User]
GO
ALTER TABLE [dbo].[Resena]  WITH CHECK ADD  CONSTRAINT [FK_Resena_Servicio] FOREIGN KEY([Id_Servicio])
REFERENCES [dbo].[Servicio] ([Id_Servicio])
GO
ALTER TABLE [dbo].[Resena] CHECK CONSTRAINT [FK_Resena_Servicio]
GO
ALTER TABLE [dbo].[Resena]  WITH CHECK ADD  CONSTRAINT [FK_Resena_User] FOREIGN KEY([Id_User])
REFERENCES [dbo].[User] ([Id_User])
GO
ALTER TABLE [dbo].[Resena] CHECK CONSTRAINT [FK_Resena_User]
GO
ALTER TABLE [dbo].[Reserva]  WITH CHECK ADD  CONSTRAINT [FK_Reserva_EstadoReserva] FOREIGN KEY([Id_EstadoReserva])
REFERENCES [dbo].[EstadoReserva] ([Id_EstadoReserva])
GO
ALTER TABLE [dbo].[Reserva] CHECK CONSTRAINT [FK_Reserva_EstadoReserva]
GO
ALTER TABLE [dbo].[Reserva]  WITH CHECK ADD  CONSTRAINT [FK_Reserva_Mascota] FOREIGN KEY([Id_Mascota])
REFERENCES [dbo].[Mascota] ([Id_Mascota])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Reserva] CHECK CONSTRAINT [FK_Reserva_Mascota]
GO
ALTER TABLE [dbo].[Reserva]  WITH CHECK ADD  CONSTRAINT [FK_Reserva_Servicio] FOREIGN KEY([Id_Servicio])
REFERENCES [dbo].[Servicio] ([Id_Servicio])
GO
ALTER TABLE [dbo].[Reserva] CHECK CONSTRAINT [FK_Reserva_Servicio]
GO
ALTER TABLE [dbo].[Reserva]  WITH CHECK ADD  CONSTRAINT [FK_Reserva_User] FOREIGN KEY([Id_User])
REFERENCES [dbo].[User] ([Id_User])
GO
ALTER TABLE [dbo].[Reserva] CHECK CONSTRAINT [FK_Reserva_User]
GO
ALTER TABLE [dbo].[Servicio]  WITH CHECK ADD  CONSTRAINT [FK_Servicio_TipoServicio] FOREIGN KEY([Id_TipoServicio])
REFERENCES [dbo].[TipoServicio] ([Id_TipoServicio])
GO
ALTER TABLE [dbo].[Servicio] CHECK CONSTRAINT [FK_Servicio_TipoServicio]
GO
ALTER TABLE [dbo].[Tratamiento]  WITH CHECK ADD  CONSTRAINT [FK_Tratamiento_Mascota] FOREIGN KEY([Id_Mascota])
REFERENCES [dbo].[Mascota] ([Id_Mascota])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Tratamiento] CHECK CONSTRAINT [FK_Tratamiento_Mascota]
GO
ALTER TABLE [dbo].[User]  WITH CHECK ADD  CONSTRAINT [FK_User_Mail] FOREIGN KEY([Id_Mail])
REFERENCES [dbo].[Mail] ([Id_Mail])
GO
ALTER TABLE [dbo].[User] CHECK CONSTRAINT [FK_User_Mail]
GO
ALTER TABLE [dbo].[User]  WITH CHECK ADD  CONSTRAINT [FK_User_TipoUsuario] FOREIGN KEY([Id_TipoUsuario])
REFERENCES [dbo].[TipoUsuario] ([Id_TipoUsuario])
GO
ALTER TABLE [dbo].[User] CHECK CONSTRAINT [FK_User_TipoUsuario]
GO
ALTER TABLE [dbo].[User]  WITH CHECK ADD  CONSTRAINT [FK_User_Ubicacion] FOREIGN KEY([Id_Ubicacion])
REFERENCES [dbo].[Ubicacion] ([Id_Ubicacion])
GO
ALTER TABLE [dbo].[User] CHECK CONSTRAINT [FK_User_Ubicacion]
GO
ALTER TABLE [dbo].[Vacuna]  WITH CHECK ADD  CONSTRAINT [FK_Vacuna_Mascota] FOREIGN KEY([Id_Mascota])
REFERENCES [dbo].[Mascota] ([Id_Mascota])
GO
ALTER TABLE [dbo].[Vacuna] CHECK CONSTRAINT [FK_Vacuna_Mascota]
GO
ALTER TABLE [dbo].[Veterinario]  WITH CHECK ADD  CONSTRAINT [FK_Veterinario_Clinica] FOREIGN KEY([Id_Clínica])
REFERENCES [dbo].[Clinica] ([Id_Clinica])
GO
ALTER TABLE [dbo].[Veterinario] CHECK CONSTRAINT [FK_Veterinario_Clinica]
GO
ALTER TABLE [dbo].[Veterinario]  WITH CHECK ADD  CONSTRAINT [FK_Veterinario_User] FOREIGN KEY([Id_User])
REFERENCES [dbo].[User] ([Id_User])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Veterinario] CHECK CONSTRAINT [FK_Veterinario_User]
GO
USE [Zooni]
GO
ALTER DATABASE [Zooni] SET  READ_WRITE 
GO
ALTER TABLE Mascota ALTER COLUMN Peso DECIMAL(10,2);
ALTER TABLE Mascota ADD Archivada BIT DEFAULT 0;
ALTER TABLE Mascota ADD TagColor NVARCHAR(20) NULL;
