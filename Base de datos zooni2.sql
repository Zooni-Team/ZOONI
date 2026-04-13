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
-- 0.1 MIGRATION CONTROL TABLE
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
-- 1) TIPO USUARIO
------------------------------------------------------------
IF OBJECT_ID('dbo.TipoUsuario','U') IS NULL
BEGIN
    CREATE TABLE dbo.TipoUsuario(
        Id_TipoUsuario INT IDENTITY(1,1) PRIMARY KEY,
        Descripcion NVARCHAR(200) NOT NULL UNIQUE
    );
END
GO

IF OBJECT_ID('dbo.[User]','U') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM dbo.__MigrationLog WHERE Name='TipoUsuarioMigration')
BEGIN

    -- Insert distinct types
    IF COL_LENGTH('dbo.[User]','TipoUsuario') IS NOT NULL
    BEGIN
        INSERT INTO dbo.TipoUsuario (Descripcion)
        SELECT DISTINCT LTRIM(RTRIM(TipoUsuario))
        FROM dbo.[User]
        WHERE TipoUsuario IS NOT NULL
          AND LTRIM(RTRIM(TipoUsuario)) <> ''
          AND NOT EXISTS (
              SELECT 1 FROM dbo.TipoUsuario t
              WHERE t.Descripcion = LTRIM(RTRIM(TipoUsuario))
          );

        IF COL_LENGTH('dbo.[User]','Id_TipoUsuario') IS NULL
            ALTER TABLE dbo.[User] ADD Id_TipoUsuario INT;

        UPDATE u
        SET Id_TipoUsuario = t.Id_TipoUsuario
        FROM dbo.[User] u
        JOIN dbo.TipoUsuario t
          ON t.Descripcion = LTRIM(RTRIM(u.TipoUsuario));

        ALTER TABLE dbo.[User] DROP COLUMN TipoUsuario;
    END

    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name='FK_User_TipoUsuario')
    BEGIN
        ALTER TABLE dbo.[User]
        ADD CONSTRAINT FK_User_TipoUsuario
        FOREIGN KEY (Id_TipoUsuario)
        REFERENCES dbo.TipoUsuario(Id_TipoUsuario);
    END

    INSERT INTO dbo.__MigrationLog(Name) VALUES('TipoUsuarioMigration');
END
GO

------------------------------------------------------------
-- 2) USER AUTH (MAIL + PASSWORD)
------------------------------------------------------------
IF OBJECT_ID('dbo.[User]','U') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM dbo.__MigrationLog WHERE Name='UserAuthMigration')
BEGIN

    IF COL_LENGTH('dbo.[User]','Mail') IS NULL
        ALTER TABLE dbo.[User] ADD Mail NVARCHAR(320);

    IF COL_LENGTH('dbo.[User]','Contrasena') IS NULL
        ALTER TABLE dbo.[User] ADD Contrasena NVARCHAR(200);

    -- Migration from Mail table if exists
    IF OBJECT_ID('dbo.Mail','U') IS NOT NULL
    AND COL_LENGTH('dbo.[User]','Id_Mail') IS NOT NULL
    BEGIN
        UPDATE u
        SET Mail = m.Correo,
            Contrasena = m.Contrasena
        FROM dbo.[User] u
        JOIN dbo.Mail m ON m.Id_Mail = u.Id_Mail;

        ALTER TABLE dbo.[User] DROP COLUMN Id_Mail;
        DROP TABLE dbo.Mail;
    END

    INSERT INTO dbo.__MigrationLog(Name) VALUES('UserAuthMigration');
END
GO

------------------------------------------------------------
-- 3) HISTORIAL BASE
------------------------------------------------------------
IF OBJECT_ID('dbo.HistorialMedico','U') IS NULL
BEGIN
    CREATE TABLE dbo.HistorialMedico(
        Id_Historial INT IDENTITY(1,1) PRIMARY KEY,
        Id_Mascota INT NOT NULL,
        Fecha DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        Notas NVARCHAR(1000)
    );
END
GO

------------------------------------------------------------
-- 4) HISTORIAL VACUNA
------------------------------------------------------------
IF OBJECT_ID('dbo.HistorialVacuna','U') IS NULL
BEGIN
    CREATE TABLE dbo.HistorialVacuna(
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Id_Historial INT NOT NULL,
        Nombre NVARCHAR(150),
        Fecha_Aplicacion DATE,
        Proxima_Dosis DATE,
        Id_Proveedor INT,
        Aplicada BIT DEFAULT 0
    );
END
GO

------------------------------------------------------------
-- 5) HISTORIAL TRATAMIENTO
------------------------------------------------------------
IF OBJECT_ID('dbo.HistorialTratamiento','U') IS NULL
BEGIN
    CREATE TABLE dbo.HistorialTratamiento(
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Id_Historial INT NOT NULL,
        Nombre NVARCHAR(150),
        Fecha_Inicio DATE,
        Fecha_Fin DATE,
        Veterinario NVARCHAR(150),
        Observaciones NVARCHAR(1000)
    );
END
GO

------------------------------------------------------------
-- 6) FK HISTORIAL -> MASCOTA
------------------------------------------------------------
IF OBJECT_ID('dbo.Mascota','U') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name='FK_Historial_Mascota')
BEGIN
    ALTER TABLE dbo.HistorialMedico
    ADD CONSTRAINT FK_Historial_Mascota
    FOREIGN KEY (Id_Mascota)
    REFERENCES dbo.Mascota(Id_Mascota)
    ON DELETE CASCADE;
END
GO

------------------------------------------------------------
-- 7) FK VACUNA
------------------------------------------------------------
IF OBJECT_ID('dbo.HistorialVacuna','U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name='FK_HV_Historial')
    BEGIN
        ALTER TABLE dbo.HistorialVacuna
        ADD CONSTRAINT FK_HV_Historial
        FOREIGN KEY (Id_Historial)
        REFERENCES dbo.HistorialMedico(Id_Historial)
        ON DELETE CASCADE;
    END

    IF OBJECT_ID('dbo.Proveedor','U') IS NOT NULL
    AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name='FK_HV_Proveedor')
    BEGIN
        ALTER TABLE dbo.HistorialVacuna
        ADD CONSTRAINT FK_HV_Proveedor
        FOREIGN KEY (Id_Proveedor)
        REFERENCES dbo.Proveedor(Id_Proveedor);
    END
END
GO

------------------------------------------------------------
-- 8) FK TRATAMIENTO
------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name='FK_HT_Historial')
BEGIN
    ALTER TABLE dbo.HistorialTratamiento
    ADD CONSTRAINT FK_HT_Historial
    FOREIGN KEY (Id_Historial)
    REFERENCES dbo.HistorialMedico(Id_Historial)
    ON DELETE CASCADE;
END
GO

------------------------------------------------------------
-- 9) MIGRACIÓN VACUNA (VERSIÓN CORRECTA Y SEGURA)
------------------------------------------------------------
IF OBJECT_ID('dbo.Vacuna','U') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM dbo.__MigrationLog WHERE Name='VacunaMigration')
BEGIN

    DECLARE @map TABLE(
        Id_Vacuna INT,
        Id_Historial INT
    );

    INSERT INTO dbo.HistorialMedico (Id_Mascota, Fecha, Notas)
    OUTPUT v.Id_Vacuna, INSERTED.Id_Historial
    INTO @map(Id_Vacuna, Id_Historial)
    SELECT
        v.Id_Mascota,
        ISNULL(v.Fecha_Aplicacion, SYSUTCDATETIME()),
        'Migrado desde Vacuna'
    FROM dbo.Vacuna v;

    INSERT INTO dbo.HistorialVacuna(
        Id_Historial, Nombre, Fecha_Aplicacion, Proxima_Dosis, Id_Proveedor, Aplicada
    )
    SELECT
        m.Id_Historial,
        v.Nombre,
        v.Fecha_Aplicacion,
        v.Proxima_Dosis,
        v.Id_Proveedor,
        v.Aplicada
    FROM dbo.Vacuna v
    JOIN @map m ON m.Id_Vacuna = v.Id_Vacuna;

    INSERT INTO dbo.__MigrationLog(Name) VALUES('VacunaMigration');
END
GO

------------------------------------------------------------
-- 10) PUBLICACIONES
------------------------------------------------------------
IF OBJECT_ID('dbo.Publicacion','U') IS NULL
BEGIN
    CREATE TABLE dbo.Publicacion(
        Id_Publicacion INT IDENTITY PRIMARY KEY,
        Id_User INT NOT NULL,
        Id_Mascota INT NULL,
        ImagenUrl NVARCHAR(500),
        Descripcion NVARCHAR(2000),
        Fecha DATETIME2 DEFAULT SYSUTCDATETIME(),
        Anclada BIT DEFAULT 0,
        FechaAnclada DATETIME2 NULL,
        Eliminada BIT DEFAULT 0
    );
END
GO

IF OBJECT_ID('dbo.ComentarioPublicacion','U') IS NULL
BEGIN
    CREATE TABLE dbo.ComentarioPublicacion(
        Id_Comentario INT IDENTITY PRIMARY KEY,
        Id_Publicacion INT NOT NULL,
        Id_User INT NOT NULL,
        Contenido NVARCHAR(1000),
        Fecha DATETIME2 DEFAULT SYSUTCDATETIME(),
        Eliminado BIT DEFAULT 0
    );
END
GO

------------------------------------------------------------
-- 11) FKs PUBLICACIONES
------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name='FK_Comentario_Publicacion')
BEGIN
    ALTER TABLE dbo.ComentarioPublicacion
    ADD CONSTRAINT FK_Comentario_Publicacion
    FOREIGN KEY (Id_Publicacion)
    REFERENCES dbo.Publicacion(Id_Publicacion)
    ON DELETE CASCADE;
END
GO

IF OBJECT_ID('dbo.[User]','U') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name='FK_Comentario_User')
BEGIN
    ALTER TABLE dbo.ComentarioPublicacion
    ADD CONSTRAINT FK_Comentario_User
    FOREIGN KEY (Id_User)
    REFERENCES dbo.[User](Id_User);
END
GO

------------------------------------------------------------
-- 12) CLEANUP SEGURO
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
