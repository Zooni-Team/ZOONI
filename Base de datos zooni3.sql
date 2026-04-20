SET NOCOUNT ON;
SET XACT_ABORT ON;

------------------------------------------------------------
-- 0) DATABASE SAFE START
------------------------------------------------------------
IF DB_ID(N'Zooni') IS NULL
    CREATE DATABASE Zooni;
GO

USE Zooni;
GO

------------------------------------------------------------
-- 1) MIGRATION CONTROL
------------------------------------------------------------
IF OBJECT_ID('dbo.__MigrationLog','U') IS NULL
BEGIN
    CREATE TABLE dbo.__MigrationLog(
        Name NVARCHAR(200) PRIMARY KEY,
        ExecutedAt DATETIME2 DEFAULT SYSUTCDATETIME()
    );
END
GO

------------------------------------------------------------
-- 2) ROLES (SISTEMA MODERNO)
------------------------------------------------------------
IF OBJECT_ID('dbo.Role','U') IS NULL
BEGIN
    CREATE TABLE dbo.Role(
        Id_Role INT IDENTITY(1,1) PRIMARY KEY,
        Nombre NVARCHAR(50) UNIQUE NOT NULL
    );

    INSERT INTO dbo.Role (Nombre)
    VALUES ('OWNER'), ('WALKER'), ('VET'), ('CLINIC'), ('ADMIN');
END
GO

------------------------------------------------------------
-- 3) USERS
------------------------------------------------------------
IF OBJECT_ID('dbo.[User]','U') IS NULL
BEGIN
    CREATE TABLE dbo.[User](
        Id_User INT IDENTITY(1,1) PRIMARY KEY,
        Nombre NVARCHAR(100),
        Apellido NVARCHAR(100),
        Mail NVARCHAR(320) UNIQUE NOT NULL,
        Contrasena NVARCHAR(255) NOT NULL,
        Telefono NVARCHAR(30),
        FechaRegistro DATETIME2 DEFAULT SYSUTCDATETIME(),
        FotoPerfil NVARCHAR(500),
        Ubicacion NVARCHAR(300),
        Estado BIT DEFAULT 1
    );
END
GO

------------------------------------------------------------
-- 4) USER ROLE (N:M)
------------------------------------------------------------
IF OBJECT_ID('dbo.UserRole','U') IS NULL
BEGIN
    CREATE TABLE dbo.UserRole(
        Id_User INT,
        Id_Role INT,
        PRIMARY KEY (Id_User, Id_Role),
        FOREIGN KEY (Id_User) REFERENCES dbo.[User](Id_User),
        FOREIGN KEY (Id_Role) REFERENCES dbo.Role(Id_Role)
    );
END
GO

------------------------------------------------------------
-- 5) MASCOTAS
------------------------------------------------------------
IF OBJECT_ID('dbo.Mascota','U') IS NULL
BEGIN
    CREATE TABLE dbo.Mascota(
        Id_Mascota INT IDENTITY(1,1) PRIMARY KEY,
        Id_User INT NOT NULL,
        Nombre NVARCHAR(100),
        Especie NVARCHAR(50),
        Raza NVARCHAR(100),
        FechaNacimiento DATE,
        Peso DECIMAL(5,2),
        Foto NVARCHAR(500),
        EstadoSalud NVARCHAR(200),
        ChipId NVARCHAR(100),
        FOREIGN KEY (Id_User) REFERENCES dbo.[User](Id_User)
    );
END
GO

------------------------------------------------------------
-- 6) HISTORIAL UNIFICADO (CORE ZOONI)
------------------------------------------------------------
IF OBJECT_ID('dbo.HistorialEvento','U') IS NULL
BEGIN
    CREATE TABLE dbo.HistorialEvento(
        Id_Evento INT IDENTITY(1,1) PRIMARY KEY,
        Id_Mascota INT NOT NULL,
        Tipo NVARCHAR(50), -- VACUNA | CONSULTA | PASEO | TRATAMIENTO
        Fecha DATETIME2 DEFAULT SYSUTCDATETIME(),
        Descripcion NVARCHAR(1000),
        Visibilidad BIT DEFAULT 1,
        FOREIGN KEY (Id_Mascota) REFERENCES dbo.Mascota(Id_Mascota)
    );
END
GO

------------------------------------------------------------
-- 7) VACUNAS
------------------------------------------------------------
IF OBJECT_ID('dbo.Vacuna','U') IS NULL
BEGIN
    CREATE TABLE dbo.Vacuna(
        Id_Vacuna INT IDENTITY(1,1) PRIMARY KEY,
        Nombre NVARCHAR(150),
        Dosis NVARCHAR(100)
    );
END
GO

------------------------------------------------------------
-- 8) MASCOTA VACUNA
------------------------------------------------------------
IF OBJECT_ID('dbo.MascotaVacuna','U') IS NULL
BEGIN
    CREATE TABLE dbo.MascotaVacuna(
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Id_Mascota INT NOT NULL,
        Id_Vacuna INT NOT NULL,
        FechaAplicacion DATE,
        ProximaDosis DATE,
        AplicadaPor NVARCHAR(150),
        FOREIGN KEY (Id_Mascota) REFERENCES dbo.Mascota(Id_Mascota),
        FOREIGN KEY (Id_Vacuna) REFERENCES dbo.Vacuna(Id_Vacuna)
    );
END
GO

------------------------------------------------------------
-- 9) TRATAMIENTOS
------------------------------------------------------------
IF OBJECT_ID('dbo.Tratamiento','U') IS NULL
BEGIN
    CREATE TABLE dbo.Tratamiento(
        Id_Tratamiento INT IDENTITY(1,1) PRIMARY KEY,
        Id_Mascota INT NOT NULL,
        Nombre NVARCHAR(150),
        Veterinario NVARCHAR(150),
        FechaInicio DATE,
        FechaFin DATE,
        Observaciones NVARCHAR(1000),
        FOREIGN KEY (Id_Mascota) REFERENCES dbo.Mascota(Id_Mascota)
    );
END
GO

------------------------------------------------------------
-- 10) PASEOS
------------------------------------------------------------
IF OBJECT_ID('dbo.Paseo','U') IS NULL
BEGIN
    CREATE TABLE dbo.Paseo(
        Id_Paseo INT IDENTITY(1,1) PRIMARY KEY,
        Id_Mascota INT NOT NULL,
        Id_Walker INT NULL,
        HoraInicio DATETIME2,
        HoraFin DATETIME2,
        UbicacionInicio NVARCHAR(300),
        UbicacionFin NVARCHAR(300),
        Estado NVARCHAR(50),
        Rating INT,
        FOREIGN KEY (Id_Mascota) REFERENCES dbo.Mascota(Id_Mascota),
        FOREIGN KEY (Id_Walker) REFERENCES dbo.[User](Id_User)
    );
END
GO

------------------------------------------------------------
-- 11) TRACKING PASEO
------------------------------------------------------------
IF OBJECT_ID('dbo.PaseoTrack','U') IS NULL
BEGIN
    CREATE TABLE dbo.PaseoTrack(
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Id_Paseo INT NOT NULL,
        Lat DECIMAL(10,7),
        Lng DECIMAL(10,7),
        Timestamp DATETIME2,
        FOREIGN KEY (Id_Paseo) REFERENCES dbo.Paseo(Id_Paseo)
    );
END
GO

------------------------------------------------------------
-- 12) CLINICAS
------------------------------------------------------------
IF OBJECT_ID('dbo.Clinica','U') IS NULL
BEGIN
    CREATE TABLE dbo.Clinica(
        Id_Clinica INT IDENTITY(1,1) PRIMARY KEY,
        Nombre NVARCHAR(150),
        Direccion NVARCHAR(300),
        Telefono NVARCHAR(50),
        Mail NVARCHAR(320),
        Rating DECIMAL(3,2)
    );
END
GO

------------------------------------------------------------
-- 13) VETERINARIOS
------------------------------------------------------------
IF OBJECT_ID('dbo.Veterinario','U') IS NULL
BEGIN
    CREATE TABLE dbo.Veterinario(
        Id_Vet INT IDENTITY(1,1) PRIMARY KEY,
        Id_Clinica INT,
        Nombre NVARCHAR(150),
        Matricula NVARCHAR(100),
        Especialidad NVARCHAR(150),
        FOREIGN KEY (Id_Clinica) REFERENCES dbo.Clinica(Id_Clinica)
    );
END
GO

------------------------------------------------------------
-- 14) PUBLICACIONES
------------------------------------------------------------
IF OBJECT_ID('dbo.Publicacion','U') IS NULL
BEGIN
    CREATE TABLE dbo.Publicacion(
        Id_Publicacion INT IDENTITY(1,1) PRIMARY KEY,
        Id_User INT NOT NULL,
        Id_Mascota INT NULL,
        ImagenUrl NVARCHAR(500),
        Descripcion NVARCHAR(2000),
        Fecha DATETIME2 DEFAULT SYSUTCDATETIME(),
        FOREIGN KEY (Id_User) REFERENCES dbo.[User](Id_User),
        FOREIGN KEY (Id_Mascota) REFERENCES dbo.Mascota(Id_Mascota)
    );
END
GO

------------------------------------------------------------
-- 15) COMENTARIOS
------------------------------------------------------------
IF OBJECT_ID('dbo.Comentario','U') IS NULL
BEGIN
    CREATE TABLE dbo.Comentario(
        Id_Comentario INT IDENTITY(1,1) PRIMARY KEY,
        Id_Publicacion INT NOT NULL,
        Id_User INT NOT NULL,
        Contenido NVARCHAR(1000),
        Fecha DATETIME2 DEFAULT SYSUTCDATETIME(),
        FOREIGN KEY (Id_Publicacion) REFERENCES dbo.Publicacion(Id_Publicacion),
        FOREIGN KEY (Id_User) REFERENCES dbo.[User](Id_User)
    );
END
GO

------------------------------------------------------------
-- 16) NOTIFICACIONES
------------------------------------------------------------
IF OBJECT_ID('dbo.Notificacion','U') IS NULL
BEGIN
    CREATE TABLE dbo.Notificacion(
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Id_User INT NOT NULL,
        Titulo NVARCHAR(150),
        Mensaje NVARCHAR(500),
        Tipo NVARCHAR(50),
        Leido BIT DEFAULT 0,
        Fecha DATETIME2 DEFAULT SYSUTCDATETIME(),
        FOREIGN KEY (Id_User) REFERENCES dbo.[User](Id_User)
    );
END
GO

------------------------------------------------------------
-- 17) CLEAN LEGACY TABLES (SAFE DROP)
------------------------------------------------------------
DECLARE @t NVARCHAR(200);

DECLARE cur CURSOR FOR
SELECT name FROM (VALUES
('dbo.Mencion'),
('dbo.Reserva'),
('dbo.EstadoReserva'),
('dbo.CompartirPublicacion'),
('dbo.Historia'),
('dbo.HistoriaDestacada'),
('dbo.ParticipanteChat'),
('dbo.MascotaXConsejo'),
('dbo.MascotaXPrenda')
) x(name);

OPEN cur;
FETCH NEXT FROM cur INTO @t;

WHILE @@FETCH_STATUS = 0
BEGIN
    IF OBJECT_ID(@t,'U') IS NOT NULL
        EXEC('DROP TABLE ' + @t);

    FETCH NEXT FROM cur INTO @t;
END

CLOSE cur;
DEALLOCATE cur;
GO
