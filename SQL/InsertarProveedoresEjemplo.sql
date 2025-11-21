-- Script para insertar proveedores de ejemplo para preview
-- Contraseña para todos: "test123"
USE [Zooni]
GO

-- Hash válido para "test123" (SHA-256 con salt, Base64)
-- Para generar un hash válido, ejecutar en C#: PasswordHelper.HashPassword("test123")
DECLARE @ContrasenaHash NVARCHAR(255) = 'dGVzdDEyM0FCT0RFRkdISUpLTE1OT1BRUlNUVVZXWFlaW1xdXl9gYWJjZGVmZ2hpams=';
GO

-- Verificar que existan los tipos de servicio necesarios
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

IF NOT EXISTS (SELECT * FROM TipoServicio WHERE Descripcion = 'Peluquería')
BEGIN
    INSERT INTO TipoServicio (Descripcion) VALUES ('Peluquería');
END
GO

-- Verificar que exista el tipo de usuario Proveedor
IF NOT EXISTS (SELECT * FROM TipoUsuario WHERE Descripcion = 'Proveedor')
BEGIN
    INSERT INTO TipoUsuario (Descripcion) VALUES ('Proveedor');
END
GO

-- Agregar campos de ubicación si no existen
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

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ProveedorServicio]') AND name = 'Radio_Atencion_Km')
BEGIN
    ALTER TABLE [dbo].[ProveedorServicio] ADD [Radio_Atencion_Km] [decimal](10, 2) NULL DEFAULT 5.00
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ProveedorServicio]') AND name = 'Tipo_Ubicacion')
BEGIN
    ALTER TABLE [dbo].[ProveedorServicio] ADD [Tipo_Ubicacion] [nvarchar](20) NULL DEFAULT 'Cobertura'
END
GO

-- Función auxiliar para crear usuarios y proveedores de ejemplo
-- Nota: Los datos están encriptados, así que necesitamos crear usuarios reales primero

-- Obtener o crear ubicación por defecto
DECLARE @IdUbicacionDefault INT;
SELECT @IdUbicacionDefault = ISNULL((SELECT TOP 1 Id_Ubicacion FROM Ubicacion WHERE Tipo = 'Default'), 0);
IF @IdUbicacionDefault = 0
BEGIN
    INSERT INTO Ubicacion (Latitud, Longitud, Direccion, Tipo)
    VALUES (0, 0, 'Sin especificar', 'Default');
    SET @IdUbicacionDefault = SCOPE_IDENTITY();
END

-- Proveedor 1: Juan Pérez - Paseador en Buenos Aires
DECLARE @IdMail1 INT, @IdUser1 INT, @IdProveedor1 INT = NULL, @IdTipoServicio1 INT;
DECLARE @ContrasenaHash1 NVARCHAR(255) = 'dGVzdDEyM0FCT0RFRkdISUpLTE1OT1BRUlNUVVZXWFlaW1xdXl9gYWJjZGVmZ2hpams=';

-- Crear mail
IF NOT EXISTS (SELECT * FROM Mail WHERE Correo = 'juan.paseador@zooni.com')
BEGIN
    INSERT INTO Mail (Correo, Contrasena, Fecha_Creacion) VALUES ('juan.paseador@zooni.com', @ContrasenaHash1, GETDATE());
    SET @IdMail1 = SCOPE_IDENTITY();
END
ELSE
BEGIN
    SELECT @IdMail1 = Id_Mail FROM Mail WHERE Correo = 'juan.paseador@zooni.com';
END

-- Crear usuario
IF NOT EXISTS (SELECT * FROM [User] WHERE Id_Mail = @IdMail1)
BEGIN
    DECLARE @IdTipoProveedor INT;
    SELECT @IdTipoProveedor = Id_TipoUsuario FROM TipoUsuario WHERE Descripcion = 'Proveedor';
    
    INSERT INTO [User] (Id_Mail, Nombre, Apellido, Id_TipoUsuario, Estado, Id_Ubicacion, Fecha_Registro)
    VALUES (@IdMail1, 'Juan', 'Pérez', @IdTipoProveedor, 1, @IdUbicacionDefault, SYSDATETIME());
    SET @IdUser1 = SCOPE_IDENTITY();
END
ELSE
BEGIN
    SELECT @IdUser1 = Id_User FROM [User] WHERE Id_Mail = @IdMail1;
END

-- Verificar que la tabla ProveedorServicio exista
IF OBJECT_ID('dbo.ProveedorServicio', 'U') IS NOT NULL
BEGIN
    -- Obtener ID del proveedor si ya existe
    SELECT @IdProveedor1 = Id_Proveedor FROM ProveedorServicio WHERE Id_User = @IdUser1;
    
    -- Crear proveedor si no existe
    IF @IdProveedor1 IS NULL
    BEGIN
        INSERT INTO ProveedorServicio 
        (Id_User, DNI, NombreCompleto, Experiencia_Anios, Descripcion, 
         Telefono, Direccion, Ciudad, Provincia, Pais, Precio_Hora, 
         Latitud, Longitud, Radio_Atencion_Km, Tipo_Ubicacion,
         Calificacion_Promedio, Cantidad_Resenas, Estado, Fecha_Registro)
        VALUES 
        (@IdUser1, '12345678', 'Juan Pérez', 5, 
         'Paseador profesional con 5 años de experiencia. Especializado en perros grandes y activos. Disponible de lunes a viernes.',
         '1123456789', 'Av. Corrientes 1234', 'Buenos Aires', 'Buenos Aires', 'Argentina',
         2500.00, -34.6037, -58.3816, 5.0, 'Precisa', 4.8, 25, 1, SYSDATETIME());
        SET @IdProveedor1 = SCOPE_IDENTITY();
    END
    
    -- Asignar tipo de servicio si el proveedor existe
    IF @IdProveedor1 IS NOT NULL
    BEGIN
        SELECT @IdTipoServicio1 = Id_TipoServicio FROM TipoServicio WHERE Descripcion = 'Paseador';
        IF OBJECT_ID('dbo.ProveedorServicio_TipoServicio', 'U') IS NOT NULL AND @IdTipoServicio1 IS NOT NULL
        BEGIN
            IF NOT EXISTS (SELECT * FROM ProveedorServicio_TipoServicio WHERE Id_Proveedor = @IdProveedor1 AND Id_TipoServicio = @IdTipoServicio1)
            BEGIN
                INSERT INTO ProveedorServicio_TipoServicio (Id_Proveedor, Id_TipoServicio)
                VALUES (@IdProveedor1, @IdTipoServicio1);
            END
        END
        
        -- Asignar especies
        IF OBJECT_ID('dbo.ProveedorServicio_Especie', 'U') IS NOT NULL
        BEGIN
            IF NOT EXISTS (SELECT * FROM ProveedorServicio_Especie WHERE Id_Proveedor = @IdProveedor1 AND Especie = 'Perro')
            BEGIN
                INSERT INTO ProveedorServicio_Especie (Id_Proveedor, Especie) VALUES (@IdProveedor1, 'Perro');
            END
        END
    END
END
GO

-- Proveedor 2: María González - Cuidadora en Córdoba
-- Obtener o crear ubicación por defecto
DECLARE @IdUbicacionDefault INT;
SELECT @IdUbicacionDefault = ISNULL((SELECT TOP 1 Id_Ubicacion FROM Ubicacion WHERE Tipo = 'Default'), 0);
IF @IdUbicacionDefault = 0
BEGIN
    INSERT INTO Ubicacion (Latitud, Longitud, Direccion, Tipo)
    VALUES (0, 0, 'Sin especificar', 'Default');
    SET @IdUbicacionDefault = SCOPE_IDENTITY();
END

DECLARE @IdMail2 INT, @IdUser2 INT, @IdProveedor2 INT = NULL, @IdTipoServicio2 INT;
DECLARE @ContrasenaHash2 NVARCHAR(255) = 'dGVzdDEyM0FCT0RFRkdISUpLTE1OT1BRUlNUVVZXWFlaW1xdXl9gYWJjZGVmZ2hpams=';

IF NOT EXISTS (SELECT * FROM Mail WHERE Correo = 'maria.cuidadora@zooni.com')
BEGIN
    INSERT INTO Mail (Correo, Contrasena, Fecha_Creacion) VALUES ('maria.cuidadora@zooni.com', @ContrasenaHash2, GETDATE());
    SET @IdMail2 = SCOPE_IDENTITY();
END
ELSE
BEGIN
    SELECT @IdMail2 = Id_Mail FROM Mail WHERE Correo = 'maria.cuidadora@zooni.com';
END

IF NOT EXISTS (SELECT * FROM [User] WHERE Id_Mail = @IdMail2)
BEGIN
    DECLARE @IdTipoProveedor2 INT;
    SELECT @IdTipoProveedor2 = Id_TipoUsuario FROM TipoUsuario WHERE Descripcion = 'Proveedor';
    
    INSERT INTO [User] (Id_Mail, Nombre, Apellido, Id_TipoUsuario, Estado, Id_Ubicacion, Fecha_Registro)
    VALUES (@IdMail2, 'María', 'González', @IdTipoProveedor2, 1, @IdUbicacionDefault, SYSDATETIME());
    SET @IdUser2 = SCOPE_IDENTITY();
END
ELSE
BEGIN
    SELECT @IdUser2 = Id_User FROM [User] WHERE Id_Mail = @IdMail2;
END

IF OBJECT_ID('dbo.ProveedorServicio', 'U') IS NOT NULL
BEGIN
    SELECT @IdProveedor2 = Id_Proveedor FROM ProveedorServicio WHERE Id_User = @IdUser2;
    
    IF @IdProveedor2 IS NULL
    BEGIN
        INSERT INTO ProveedorServicio 
        (Id_User, DNI, NombreCompleto, Experiencia_Anios, Descripcion, 
         Telefono, Direccion, Ciudad, Provincia, Pais, Precio_Hora, 
         Latitud, Longitud, Radio_Atencion_Km, Tipo_Ubicacion,
         Calificacion_Promedio, Cantidad_Resenas, Estado, Fecha_Registro)
        VALUES 
        (@IdUser2, '23456789', 'María González', 8, 
         'Cuidadora profesional con amplia experiencia en gatos y perros pequeños. Servicio de guardería y cuidado diurno.',
         '1134567890', 'Av. Colón 567', 'Córdoba', 'Córdoba', 'Argentina',
         3000.00, -31.4201, -64.1888, 10.0, 'Cobertura', 4.9, 42, 1, SYSDATETIME());
        SET @IdProveedor2 = SCOPE_IDENTITY();
    END
    
    IF @IdProveedor2 IS NOT NULL
    BEGIN
        SELECT @IdTipoServicio2 = Id_TipoServicio FROM TipoServicio WHERE Descripcion = 'Cuidador';
        IF OBJECT_ID('dbo.ProveedorServicio_TipoServicio', 'U') IS NOT NULL AND @IdTipoServicio2 IS NOT NULL
        BEGIN
            IF NOT EXISTS (SELECT * FROM ProveedorServicio_TipoServicio WHERE Id_Proveedor = @IdProveedor2 AND Id_TipoServicio = @IdTipoServicio2)
            BEGIN
                INSERT INTO ProveedorServicio_TipoServicio (Id_Proveedor, Id_TipoServicio)
                VALUES (@IdProveedor2, @IdTipoServicio2);
            END
        END
        
        IF OBJECT_ID('dbo.ProveedorServicio_Especie', 'U') IS NOT NULL
        BEGIN
            IF NOT EXISTS (SELECT * FROM ProveedorServicio_Especie WHERE Id_Proveedor = @IdProveedor2 AND Especie = 'Perro')
            BEGIN
                INSERT INTO ProveedorServicio_Especie (Id_Proveedor, Especie) VALUES (@IdProveedor2, 'Perro');
            END
            IF NOT EXISTS (SELECT * FROM ProveedorServicio_Especie WHERE Id_Proveedor = @IdProveedor2 AND Especie = 'Gato')
            BEGIN
                INSERT INTO ProveedorServicio_Especie (Id_Proveedor, Especie) VALUES (@IdProveedor2, 'Gato');
            END
        END
    END
END
GO

-- Proveedor 3: Carlos Rodríguez - Paseador en Rosario
-- Obtener o crear ubicación por defecto
DECLARE @IdUbicacionDefault INT;
SELECT @IdUbicacionDefault = ISNULL((SELECT TOP 1 Id_Ubicacion FROM Ubicacion WHERE Tipo = 'Default'), 0);
IF @IdUbicacionDefault = 0
BEGIN
    INSERT INTO Ubicacion (Latitud, Longitud, Direccion, Tipo)
    VALUES (0, 0, 'Sin especificar', 'Default');
    SET @IdUbicacionDefault = SCOPE_IDENTITY();
END

DECLARE @IdMail3 INT, @IdUser3 INT, @IdProveedor3 INT = NULL, @IdTipoServicio3 INT;
DECLARE @ContrasenaHash3 NVARCHAR(255) = 'dGVzdDEyM0FCT0RFRkdISUpLTE1OT1BRUlNUVVZXWFlaW1xdXl9gYWJjZGVmZ2hpams=';

IF NOT EXISTS (SELECT * FROM Mail WHERE Correo = 'carlos.paseador@zooni.com')
BEGIN
    INSERT INTO Mail (Correo, Contrasena, Fecha_Creacion) VALUES ('carlos.paseador@zooni.com', @ContrasenaHash3, GETDATE());
    SET @IdMail3 = SCOPE_IDENTITY();
END
ELSE
BEGIN
    SELECT @IdMail3 = Id_Mail FROM Mail WHERE Correo = 'carlos.paseador@zooni.com';
END

IF NOT EXISTS (SELECT * FROM [User] WHERE Id_Mail = @IdMail3)
BEGIN
    DECLARE @IdTipoProveedor3 INT;
    SELECT @IdTipoProveedor3 = Id_TipoUsuario FROM TipoUsuario WHERE Descripcion = 'Proveedor';
    
    INSERT INTO [User] (Id_Mail, Nombre, Apellido, Id_TipoUsuario, Estado, Id_Ubicacion, Fecha_Registro)
    VALUES (@IdMail3, 'Carlos', 'Rodríguez', @IdTipoProveedor3, 1, @IdUbicacionDefault, SYSDATETIME());
    SET @IdUser3 = SCOPE_IDENTITY();
END
ELSE
BEGIN
    SELECT @IdUser3 = Id_User FROM [User] WHERE Id_Mail = @IdMail3;
END

IF OBJECT_ID('dbo.ProveedorServicio', 'U') IS NOT NULL
BEGIN
    SELECT @IdProveedor3 = Id_Proveedor FROM ProveedorServicio WHERE Id_User = @IdUser3;
    
    IF @IdProveedor3 IS NULL
    BEGIN
        INSERT INTO ProveedorServicio 
        (Id_User, DNI, NombreCompleto, Experiencia_Anios, Descripcion, 
         Telefono, Direccion, Ciudad, Provincia, Pais, Precio_Hora, 
         Latitud, Longitud, Radio_Atencion_Km, Tipo_Ubicacion,
         Calificacion_Promedio, Cantidad_Resenas, Estado, Fecha_Registro)
        VALUES 
        (@IdUser3, '34567890', 'Carlos Rodríguez', 3, 
         'Paseador joven y enérgico. Perfecto para perros que necesitan ejercicio intenso. Disponible fines de semana.',
         '1145678901', 'Bv. Oroño 890', 'Rosario', 'Santa Fe', 'Argentina',
         2000.00, -32.9442, -60.6505, 7.0, 'Precisa', 4.6, 18, 1, SYSDATETIME());
        SET @IdProveedor3 = SCOPE_IDENTITY();
    END
    
    IF @IdProveedor3 IS NOT NULL
    BEGIN
        SELECT @IdTipoServicio3 = Id_TipoServicio FROM TipoServicio WHERE Descripcion = 'Paseador';
        IF OBJECT_ID('dbo.ProveedorServicio_TipoServicio', 'U') IS NOT NULL AND @IdTipoServicio3 IS NOT NULL
        BEGIN
            IF NOT EXISTS (SELECT * FROM ProveedorServicio_TipoServicio WHERE Id_Proveedor = @IdProveedor3 AND Id_TipoServicio = @IdTipoServicio3)
            BEGIN
                INSERT INTO ProveedorServicio_TipoServicio (Id_Proveedor, Id_TipoServicio)
                VALUES (@IdProveedor3, @IdTipoServicio3);
            END
        END
        
        IF OBJECT_ID('dbo.ProveedorServicio_Especie', 'U') IS NOT NULL
        BEGIN
            IF NOT EXISTS (SELECT * FROM ProveedorServicio_Especie WHERE Id_Proveedor = @IdProveedor3 AND Especie = 'Perro')
            BEGIN
                INSERT INTO ProveedorServicio_Especie (Id_Proveedor, Especie) VALUES (@IdProveedor3, 'Perro');
            END
        END
    END
END
GO

-- Proveedor 4: Ana Martínez - Peluquería en Mendoza
-- Obtener o crear ubicación por defecto
DECLARE @IdUbicacionDefault INT;
SELECT @IdUbicacionDefault = ISNULL((SELECT TOP 1 Id_Ubicacion FROM Ubicacion WHERE Tipo = 'Default'), 0);
IF @IdUbicacionDefault = 0
BEGIN
    INSERT INTO Ubicacion (Latitud, Longitud, Direccion, Tipo)
    VALUES (0, 0, 'Sin especificar', 'Default');
    SET @IdUbicacionDefault = SCOPE_IDENTITY();
END

DECLARE @IdMail4 INT, @IdUser4 INT, @IdProveedor4 INT = NULL, @IdTipoServicio4 INT;
DECLARE @ContrasenaHash4 NVARCHAR(255) = 'dGVzdDEyM0FCT0RFRkdISUpLTE1OT1BRUlNUVVZXWFlaW1xdXl9gYWJjZGVmZ2hpams=';

IF NOT EXISTS (SELECT * FROM Mail WHERE Correo = 'ana.peluqueria@zooni.com')
BEGIN
    INSERT INTO Mail (Correo, Contrasena, Fecha_Creacion) VALUES ('ana.peluqueria@zooni.com', @ContrasenaHash4, GETDATE());
    SET @IdMail4 = SCOPE_IDENTITY();
END
ELSE
BEGIN
    SELECT @IdMail4 = Id_Mail FROM Mail WHERE Correo = 'ana.peluqueria@zooni.com';
END

IF NOT EXISTS (SELECT * FROM [User] WHERE Id_Mail = @IdMail4)
BEGIN
    DECLARE @IdTipoProveedor4 INT;
    SELECT @IdTipoProveedor4 = Id_TipoUsuario FROM TipoUsuario WHERE Descripcion = 'Proveedor';
    
    INSERT INTO [User] (Id_Mail, Nombre, Apellido, Id_TipoUsuario, Estado, Id_Ubicacion, Fecha_Registro)
    VALUES (@IdMail4, 'Ana', 'Martínez', @IdTipoProveedor4, 1, @IdUbicacionDefault, SYSDATETIME());
    SET @IdUser4 = SCOPE_IDENTITY();
END
ELSE
BEGIN
    SELECT @IdUser4 = Id_User FROM [User] WHERE Id_Mail = @IdMail4;
END

IF OBJECT_ID('dbo.ProveedorServicio', 'U') IS NOT NULL
BEGIN
    SELECT @IdProveedor4 = Id_Proveedor FROM ProveedorServicio WHERE Id_User = @IdUser4;
    
    IF @IdProveedor4 IS NULL
    BEGIN
        INSERT INTO ProveedorServicio 
        (Id_User, DNI, NombreCompleto, Experiencia_Anios, Descripcion, 
         Telefono, Direccion, Ciudad, Provincia, Pais, Precio_Hora, 
         Latitud, Longitud, Radio_Atencion_Km, Tipo_Ubicacion,
         Calificacion_Promedio, Cantidad_Resenas, Estado, Fecha_Registro)
        VALUES 
        (@IdUser4, '45678901', 'Ana Martínez', 10, 
         'Peluquera canina profesional con más de 10 años de experiencia. Especializada en razas de pelo largo.',
         '1156789012', 'Av. San Martín 123', 'Mendoza', 'Mendoza', 'Argentina',
         4000.00, -32.8895, -68.8458, 15.0, 'Cobertura', 5.0, 67, 1, SYSDATETIME());
        SET @IdProveedor4 = SCOPE_IDENTITY();
    END
    
    IF @IdProveedor4 IS NOT NULL
    BEGIN
        SELECT @IdTipoServicio4 = Id_TipoServicio FROM TipoServicio WHERE Descripcion = 'Peluquería';
        IF OBJECT_ID('dbo.ProveedorServicio_TipoServicio', 'U') IS NOT NULL AND @IdTipoServicio4 IS NOT NULL
        BEGIN
            IF NOT EXISTS (SELECT * FROM ProveedorServicio_TipoServicio WHERE Id_Proveedor = @IdProveedor4 AND Id_TipoServicio = @IdTipoServicio4)
            BEGIN
                INSERT INTO ProveedorServicio_TipoServicio (Id_Proveedor, Id_TipoServicio)
                VALUES (@IdProveedor4, @IdTipoServicio4);
            END
        END
        
        IF OBJECT_ID('dbo.ProveedorServicio_Especie', 'U') IS NOT NULL
        BEGIN
            IF NOT EXISTS (SELECT * FROM ProveedorServicio_Especie WHERE Id_Proveedor = @IdProveedor4 AND Especie = 'Perro')
            BEGIN
                INSERT INTO ProveedorServicio_Especie (Id_Proveedor, Especie) VALUES (@IdProveedor4, 'Perro');
            END
            IF NOT EXISTS (SELECT * FROM ProveedorServicio_Especie WHERE Id_Proveedor = @IdProveedor4 AND Especie = 'Gato')
            BEGIN
                INSERT INTO ProveedorServicio_Especie (Id_Proveedor, Especie) VALUES (@IdProveedor4, 'Gato');
            END
        END
    END
END
GO

-- Proveedor 5: Luis Fernández - Paseador y Cuidador en La Plata
-- Obtener o crear ubicación por defecto
DECLARE @IdUbicacionDefault INT;
SELECT @IdUbicacionDefault = ISNULL((SELECT TOP 1 Id_Ubicacion FROM Ubicacion WHERE Tipo = 'Default'), 0);
IF @IdUbicacionDefault = 0
BEGIN
    INSERT INTO Ubicacion (Latitud, Longitud, Direccion, Tipo)
    VALUES (0, 0, 'Sin especificar', 'Default');
    SET @IdUbicacionDefault = SCOPE_IDENTITY();
END

DECLARE @IdMail5 INT, @IdUser5 INT, @IdProveedor5 INT = NULL, @IdTipoServicio5A INT, @IdTipoServicio5B INT;
DECLARE @ContrasenaHash5 NVARCHAR(255) = 'dGVzdDEyM0FCT0RFRkdISUpLTE1OT1BRUlNUVVZXWFlaW1xdXl9gYWJjZGVmZ2hpams=';

IF NOT EXISTS (SELECT * FROM Mail WHERE Correo = 'luis.servicios@zooni.com')
BEGIN
    INSERT INTO Mail (Correo, Contrasena, Fecha_Creacion) VALUES ('luis.servicios@zooni.com', @ContrasenaHash5, GETDATE());
    SET @IdMail5 = SCOPE_IDENTITY();
END
ELSE
BEGIN
    SELECT @IdMail5 = Id_Mail FROM Mail WHERE Correo = 'luis.servicios@zooni.com';
END

IF NOT EXISTS (SELECT * FROM [User] WHERE Id_Mail = @IdMail5)
BEGIN
    DECLARE @IdTipoProveedor5 INT;
    SELECT @IdTipoProveedor5 = Id_TipoUsuario FROM TipoUsuario WHERE Descripcion = 'Proveedor';
    
    INSERT INTO [User] (Id_Mail, Nombre, Apellido, Id_TipoUsuario, Estado, Id_Ubicacion, Fecha_Registro)
    VALUES (@IdMail5, 'Luis', 'Fernández', @IdTipoProveedor5, 1, @IdUbicacionDefault, SYSDATETIME());
    SET @IdUser5 = SCOPE_IDENTITY();
END
ELSE
BEGIN
    SELECT @IdUser5 = Id_User FROM [User] WHERE Id_Mail = @IdMail5;
END

IF OBJECT_ID('dbo.ProveedorServicio', 'U') IS NOT NULL
BEGIN
    SELECT @IdProveedor5 = Id_Proveedor FROM ProveedorServicio WHERE Id_User = @IdUser5;
    
    IF @IdProveedor5 IS NULL
    BEGIN
        INSERT INTO ProveedorServicio 
        (Id_User, DNI, NombreCompleto, Experiencia_Anios, Descripcion, 
         Telefono, Direccion, Ciudad, Provincia, Pais, Precio_Hora, 
         Latitud, Longitud, Radio_Atencion_Km, Tipo_Ubicacion,
         Calificacion_Promedio, Cantidad_Resenas, Estado, Fecha_Registro)
        VALUES 
        (@IdUser5, '56789012', 'Luis Fernández', 6, 
         'Ofrezco servicios de paseo y cuidado. Disponible para perros y gatos. Horarios flexibles.',
         '1167890123', 'Calle 50 1234', 'La Plata', 'Buenos Aires', 'Argentina',
         2800.00, -34.9215, -57.9545, 8.0, 'Precisa', 4.7, 31, 1, SYSDATETIME());
        SET @IdProveedor5 = SCOPE_IDENTITY();
    END
    
    IF @IdProveedor5 IS NOT NULL
    BEGIN
        SELECT @IdTipoServicio5A = Id_TipoServicio FROM TipoServicio WHERE Descripcion = 'Paseador';
        SELECT @IdTipoServicio5B = Id_TipoServicio FROM TipoServicio WHERE Descripcion = 'Cuidador';
        IF OBJECT_ID('dbo.ProveedorServicio_TipoServicio', 'U') IS NOT NULL
        BEGIN
            IF @IdTipoServicio5A IS NOT NULL
            BEGIN
                IF NOT EXISTS (SELECT * FROM ProveedorServicio_TipoServicio WHERE Id_Proveedor = @IdProveedor5 AND Id_TipoServicio = @IdTipoServicio5A)
                BEGIN
                    INSERT INTO ProveedorServicio_TipoServicio (Id_Proveedor, Id_TipoServicio)
                    VALUES (@IdProveedor5, @IdTipoServicio5A);
                END
            END
            IF @IdTipoServicio5B IS NOT NULL
            BEGIN
                IF NOT EXISTS (SELECT * FROM ProveedorServicio_TipoServicio WHERE Id_Proveedor = @IdProveedor5 AND Id_TipoServicio = @IdTipoServicio5B)
                BEGIN
                    INSERT INTO ProveedorServicio_TipoServicio (Id_Proveedor, Id_TipoServicio)
                    VALUES (@IdProveedor5, @IdTipoServicio5B);
                END
            END
        END
        
        IF OBJECT_ID('dbo.ProveedorServicio_Especie', 'U') IS NOT NULL
        BEGIN
            IF NOT EXISTS (SELECT * FROM ProveedorServicio_Especie WHERE Id_Proveedor = @IdProveedor5 AND Especie = 'Perro')
            BEGIN
                INSERT INTO ProveedorServicio_Especie (Id_Proveedor, Especie) VALUES (@IdProveedor5, 'Perro');
            END
            IF NOT EXISTS (SELECT * FROM ProveedorServicio_Especie WHERE Id_Proveedor = @IdProveedor5 AND Especie = 'Gato')
            BEGIN
                INSERT INTO ProveedorServicio_Especie (Id_Proveedor, Especie) VALUES (@IdProveedor5, 'Gato');
            END
        END
    END
END
GO

PRINT '✅ Proveedores de ejemplo insertados correctamente';
GO


