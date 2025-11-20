-- Script para insertar usuarios fake de paseadores y cuidadores para testeo
-- Contraseña para todos: "test123"
-- Hash SHA-256 con salt (Base64): generado por PasswordHelper.HashPassword("test123")
USE [Zooni]
GO

-- Hash válido para "test123" (SHA-256 con salt, Base64)
-- Este hash se genera dinámicamente, pero usamos uno fijo para testing
-- Formato: 16 bytes salt + 32 bytes hash = 48 bytes total en Base64
-- Para generar un hash válido, ejecutar en C#: PasswordHelper.HashPassword("test123")
-- Hash de ejemplo (puede variar por el salt aleatorio):
DECLARE @ContrasenaHash NVARCHAR(255) = 'dGVzdDEyM0FCT0RFRkdISUpLTE1OT1BRUlNUVVZXWFlaW1xdXl9gYWJjZGVmZ2hpams=';

-- Verificar que existan los tipos de servicio
IF NOT EXISTS (SELECT * FROM TipoServicio WHERE Descripcion = 'Paseador')
BEGIN
    INSERT INTO TipoServicio (Descripcion) VALUES ('Paseador');
END

IF NOT EXISTS (SELECT * FROM TipoServicio WHERE Descripcion = 'Cuidador')
BEGIN
    INSERT INTO TipoServicio (Descripcion) VALUES ('Cuidador');
END

DECLARE @TipoServicioPaseador INT;
DECLARE @TipoServicioCuidador INT;
SELECT @TipoServicioPaseador = Id_TipoServicio FROM TipoServicio WHERE Descripcion = 'Paseador';
SELECT @TipoServicioCuidador = Id_TipoServicio FROM TipoServicio WHERE Descripcion = 'Cuidador';

-- ============================================
-- PROVEEDORES CERCA DE VETERINARIAS
-- ============================================

-- 1. Paseador cerca de Veterinaria Palermo (-34.5895, -58.3974)
DECLARE @UserId1 INT;
IF NOT EXISTS (SELECT 1 FROM [User] WHERE Nombre = 'Juan' AND Apellido = 'Paseador')
BEGIN
    DECLARE @MailId1 INT;
    INSERT INTO Mail (Correo, Contrasena, Fecha_Creacion) 
    VALUES ('juan.paseador@test.com', @ContrasenaHash, GETDATE());
    SET @MailId1 = SCOPE_IDENTITY();
    
    INSERT INTO [User] (Nombre, Apellido, Id_Mail, Estado, EstadoOnline, UltimaActividad)
    VALUES ('Juan', 'Paseador', @MailId1, 1, 1, GETDATE());
    SET @UserId1 = SCOPE_IDENTITY();
    
    INSERT INTO Perfil (Id_Usuario, FotoPerfil) VALUES (@UserId1, '/img/perfil/default.png');
    
    DECLARE @UbicacionId1 INT;
    INSERT INTO Ubicacion (Latitud, Longitud, Tipo) VALUES (-34.5900, -58.3980, 'Casa');
    SET @UbicacionId1 = SCOPE_IDENTITY();
    UPDATE [User] SET Id_Ubicacion = @UbicacionId1 WHERE Id_User = @UserId1;
    
    DECLARE @ProveedorId1 INT;
    INSERT INTO ProveedorServicio (
        Id_User, DNI, NombreCompleto, Experiencia_Anios, Descripcion, 
        FotoPerfil, Telefono, Direccion, Ciudad, Provincia, Pais,
        Latitud, Longitud, Radio_Atencion_Km, Tipo_Ubicacion, Precio_Hora, Estado
    ) VALUES (
        @UserId1, '12345678', 'Juan Paseador', 5, 
        'Paseador profesional con 5 años de experiencia. Especializado en perros grandes y activos.',
        '/img/perfil/default.png', '+54 11 1234-5678', 'Av. Santa Fe 4500', 'Buenos Aires', 'CABA', 'Argentina',
        -34.5900, -58.3980, 3.0, 'Cobertura', 2500.00, 1
    );
    SET @ProveedorId1 = SCOPE_IDENTITY();
    
    IF @TipoServicioPaseador IS NOT NULL
        INSERT INTO ProveedorServicio_TipoServicio (Id_Proveedor, Id_TipoServicio) VALUES (@ProveedorId1, @TipoServicioPaseador);
    INSERT INTO ProveedorServicio_Especie (Id_Proveedor, Especie) VALUES (@ProveedorId1, 'Perro');
END

-- 2. Cuidador cerca de Veterinaria Belgrano (-34.5714, -58.4432)
DECLARE @UserId2 INT;
IF NOT EXISTS (SELECT 1 FROM [User] WHERE Nombre = 'María' AND Apellido = 'Cuidadora')
BEGIN
    DECLARE @MailId2 INT;
    INSERT INTO Mail (Correo, Contrasena, Fecha_Creacion) VALUES ('maria.cuidadora@test.com', @ContrasenaHash, GETDATE());
    SET @MailId2 = SCOPE_IDENTITY();
    
    INSERT INTO [User] (Nombre, Apellido, Id_Mail, Estado, EstadoOnline, UltimaActividad)
    VALUES ('María', 'Cuidadora', @MailId2, 1, 1, GETDATE());
    SET @UserId2 = SCOPE_IDENTITY();
    
    INSERT INTO Perfil (Id_Usuario, FotoPerfil) VALUES (@UserId2, '/img/perfil/default.png');
    
    DECLARE @UbicacionId2 INT;
    INSERT INTO Ubicacion (Latitud, Longitud, Tipo) VALUES (-34.5720, -58.4440, 'Casa');
    SET @UbicacionId2 = SCOPE_IDENTITY();
    UPDATE [User] SET Id_Ubicacion = @UbicacionId2 WHERE Id_User = @UserId2;
    
    DECLARE @ProveedorId2 INT;
    INSERT INTO ProveedorServicio (
        Id_User, DNI, NombreCompleto, Experiencia_Anios, Descripcion, 
        FotoPerfil, Telefono, Direccion, Ciudad, Provincia, Pais,
        Latitud, Longitud, Radio_Atencion_Km, Tipo_Ubicacion, Precio_Hora, Estado
    ) VALUES (
        @UserId2, '87654321', 'María Cuidadora', 8, 
        'Cuidadora profesional de gatos con amplia experiencia. Servicio a domicilio.',
        '/img/perfil/default.png', '+54 11 9876-5432', 'Av. Cabildo 2000', 'Buenos Aires', 'CABA', 'Argentina',
        -34.5720, -58.4440, NULL, 'Precisa', 3500.00, 1
    );
    SET @ProveedorId2 = SCOPE_IDENTITY();
    
    IF @TipoServicioCuidador IS NOT NULL
        INSERT INTO ProveedorServicio_TipoServicio (Id_Proveedor, Id_TipoServicio) VALUES (@ProveedorId2, @TipoServicioCuidador);
    INSERT INTO ProveedorServicio_Especie (Id_Proveedor, Especie) VALUES (@ProveedorId2, 'Gato');
END

-- 3. Paseador cerca de Centro (-34.6037, -58.3816)
DECLARE @UserId3 INT;
IF NOT EXISTS (SELECT 1 FROM [User] WHERE Nombre = 'Carlos' AND Apellido = 'Paseador')
BEGIN
    DECLARE @MailId3 INT;
    INSERT INTO Mail (Correo, Contrasena, Fecha_Creacion) VALUES ('carlos.paseador@test.com', @ContrasenaHash, GETDATE());
    SET @MailId3 = SCOPE_IDENTITY();
    
    INSERT INTO [User] (Nombre, Apellido, Id_Mail, Estado, EstadoOnline, UltimaActividad)
    VALUES ('Carlos', 'Paseador', @MailId3, 1, 1, GETDATE());
    SET @UserId3 = SCOPE_IDENTITY();
    
    INSERT INTO Perfil (Id_Usuario, FotoPerfil) VALUES (@UserId3, '/img/perfil/default.png');
    
    DECLARE @UbicacionId3 INT;
    INSERT INTO Ubicacion (Latitud, Longitud, Tipo) VALUES (-34.6040, -58.3820, 'Casa');
    SET @UbicacionId3 = SCOPE_IDENTITY();
    UPDATE [User] SET Id_Ubicacion = @UbicacionId3 WHERE Id_User = @UserId3;
    
    DECLARE @ProveedorId3 INT;
    INSERT INTO ProveedorServicio (
        Id_User, DNI, NombreCompleto, Experiencia_Anios, Descripcion, 
        FotoPerfil, Telefono, Direccion, Ciudad, Provincia, Pais,
        Latitud, Longitud, Radio_Atencion_Km, Tipo_Ubicacion, Precio_Hora, Estado
    ) VALUES (
        @UserId3, '11223344', 'Carlos Paseador', 3, 
        'Paseador joven y enérgico. Perfecto para perros que necesitan ejercicio.',
        '/img/perfil/default.png', '+54 11 5555-6666', 'Av. Corrientes 1200', 'Buenos Aires', 'CABA', 'Argentina',
        -34.6040, -58.3820, 5.0, 'Cobertura', 2000.00, 1
    );
    SET @ProveedorId3 = SCOPE_IDENTITY();
    
    IF @TipoServicioPaseador IS NOT NULL
        INSERT INTO ProveedorServicio_TipoServicio (Id_Proveedor, Id_TipoServicio) VALUES (@ProveedorId3, @TipoServicioPaseador);
    INSERT INTO ProveedorServicio_Especie (Id_Proveedor, Especie) VALUES (@ProveedorId3, 'Perro');
END

-- 4. Cuidador cerca de Caballito (-34.6118, -58.3960)
DECLARE @UserId4 INT;
IF NOT EXISTS (SELECT 1 FROM [User] WHERE Nombre = 'Ana' AND Apellido = 'Cuidadora')
BEGIN
    DECLARE @MailId4 INT;
    INSERT INTO Mail (Correo, Contrasena, Fecha_Creacion) VALUES ('ana.cuidadora@test.com', @ContrasenaHash, GETDATE());
    SET @MailId4 = SCOPE_IDENTITY();
    
    INSERT INTO [User] (Nombre, Apellido, Id_Mail, Estado, EstadoOnline, UltimaActividad)
    VALUES ('Ana', 'Cuidadora', @MailId4, 1, 1, GETDATE());
    SET @UserId4 = SCOPE_IDENTITY();
    
    INSERT INTO Perfil (Id_Usuario, FotoPerfil) VALUES (@UserId4, '/img/perfil/default.png');
    
    DECLARE @UbicacionId4 INT;
    INSERT INTO Ubicacion (Latitud, Longitud, Tipo) VALUES (-34.6120, -58.3965, 'Casa');
    SET @UbicacionId4 = SCOPE_IDENTITY();
    UPDATE [User] SET Id_Ubicacion = @UbicacionId4 WHERE Id_User = @UserId4;
    
    DECLARE @ProveedorId4 INT;
    INSERT INTO ProveedorServicio (
        Id_User, DNI, NombreCompleto, Experiencia_Anios, Descripcion, 
        FotoPerfil, Telefono, Direccion, Ciudad, Provincia, Pais,
        Latitud, Longitud, Radio_Atencion_Km, Tipo_Ubicacion, Precio_Hora, Estado
    ) VALUES (
        @UserId4, '55667788', 'Ana Cuidadora', 6, 
        'Cuidadora especializada en perros. Servicio de guardería y cuidado diurno.',
        '/img/perfil/default.png', '+54 11 7777-8888', 'Av. Rivadavia 4500', 'Buenos Aires', 'CABA', 'Argentina',
        -34.6120, -58.3965, NULL, 'Precisa', 4000.00, 1
    );
    SET @ProveedorId4 = SCOPE_IDENTITY();
    
    IF @TipoServicioCuidador IS NOT NULL
        INSERT INTO ProveedorServicio_TipoServicio (Id_Proveedor, Id_TipoServicio) VALUES (@ProveedorId4, @TipoServicioCuidador);
    INSERT INTO ProveedorServicio_Especie (Id_Proveedor, Especie) VALUES (@ProveedorId4, 'Perro');
END

-- 5. Paseador cerca de Palermo Oftalmológica (-34.5889, -58.4208)
DECLARE @UserId5 INT;
IF NOT EXISTS (SELECT 1 FROM [User] WHERE Nombre = 'Pedro' AND Apellido = 'Paseador')
BEGIN
    DECLARE @MailId5 INT;
    INSERT INTO Mail (Correo, Contrasena, Fecha_Creacion) VALUES ('pedro.paseador@test.com', @ContrasenaHash, GETDATE());
    SET @MailId5 = SCOPE_IDENTITY();
    
    INSERT INTO [User] (Nombre, Apellido, Id_Mail, Estado, EstadoOnline, UltimaActividad)
    VALUES ('Pedro', 'Paseador', @MailId5, 1, 1, GETDATE());
    SET @UserId5 = SCOPE_IDENTITY();
    
    INSERT INTO Perfil (Id_Usuario, FotoPerfil) VALUES (@UserId5, '/img/perfil/default.png');
    
    DECLARE @UbicacionId5 INT;
    INSERT INTO Ubicacion (Latitud, Longitud, Tipo) VALUES (-34.5895, -58.4215, 'Casa');
    SET @UbicacionId5 = SCOPE_IDENTITY();
    UPDATE [User] SET Id_Ubicacion = @UbicacionId5 WHERE Id_User = @UserId5;
    
    DECLARE @ProveedorId5 INT;
    INSERT INTO ProveedorServicio (
        Id_User, DNI, NombreCompleto, Experiencia_Anios, Descripcion, 
        FotoPerfil, Telefono, Direccion, Ciudad, Provincia, Pais,
        Latitud, Longitud, Radio_Atencion_Km, Tipo_Ubicacion, Precio_Hora, Estado
    ) VALUES (
        @UserId5, '99887766', 'Pedro Paseador', 4, 
        'Paseador experimentado. Disponible mañanas y tardes.',
        '/img/perfil/default.png', '+54 11 4444-5555', 'Av. Scalabrini Ortiz 200', 'Buenos Aires', 'CABA', 'Argentina',
        -34.5895, -58.4215, 4.0, 'Cobertura', 2200.00, 1
    );
    SET @ProveedorId5 = SCOPE_IDENTITY();
    
    IF @TipoServicioPaseador IS NOT NULL
        INSERT INTO ProveedorServicio_TipoServicio (Id_Proveedor, Id_TipoServicio) VALUES (@ProveedorId5, @TipoServicioPaseador);
    INSERT INTO ProveedorServicio_Especie (Id_Proveedor, Especie) VALUES (@ProveedorId5, 'Perro');
END

-- 6. Cuidador cerca de San Telmo (-34.6200, -58.3700)
DECLARE @UserId6 INT;
IF NOT EXISTS (SELECT 1 FROM [User] WHERE Nombre = 'Laura' AND Apellido = 'Cuidadora')
BEGIN
    DECLARE @MailId6 INT;
    INSERT INTO Mail (Correo, Contrasena, Fecha_Creacion) VALUES ('laura.cuidadora@test.com', @ContrasenaHash, GETDATE());
    SET @MailId6 = SCOPE_IDENTITY();
    
    INSERT INTO [User] (Nombre, Apellido, Id_Mail, Estado, EstadoOnline, UltimaActividad)
    VALUES ('Laura', 'Cuidadora', @MailId6, 1, 1, GETDATE());
    SET @UserId6 = SCOPE_IDENTITY();
    
    INSERT INTO Perfil (Id_Usuario, FotoPerfil) VALUES (@UserId6, '/img/perfil/default.png');
    
    DECLARE @UbicacionId6 INT;
    INSERT INTO Ubicacion (Latitud, Longitud, Tipo) VALUES (-34.6205, -58.3705, 'Casa');
    SET @UbicacionId6 = SCOPE_IDENTITY();
    UPDATE [User] SET Id_Ubicacion = @UbicacionId6 WHERE Id_User = @UserId6;
    
    DECLARE @ProveedorId6 INT;
    INSERT INTO ProveedorServicio (
        Id_User, DNI, NombreCompleto, Experiencia_Anios, Descripcion, 
        FotoPerfil, Telefono, Direccion, Ciudad, Provincia, Pais,
        Latitud, Longitud, Radio_Atencion_Km, Tipo_Ubicacion, Precio_Hora, Estado
    ) VALUES (
        @UserId6, '33445566', 'Laura Cuidadora', 7, 
        'Cuidadora de gatos y perros pequeños. Servicio a domicilio con experiencia.',
        '/img/perfil/default.png', '+54 11 3333-4444', 'Av. San Juan 1300', 'Buenos Aires', 'CABA', 'Argentina',
        -34.6205, -58.3705, NULL, 'Precisa', 3800.00, 1
    );
    SET @ProveedorId6 = SCOPE_IDENTITY();
    
    IF @TipoServicioCuidador IS NOT NULL
        INSERT INTO ProveedorServicio_TipoServicio (Id_Proveedor, Id_TipoServicio) VALUES (@ProveedorId6, @TipoServicioCuidador);
    INSERT INTO ProveedorServicio_Especie (Id_Proveedor, Especie) VALUES (@ProveedorId6, 'Gato');
    INSERT INTO ProveedorServicio_Especie (Id_Proveedor, Especie) VALUES (@ProveedorId6, 'Perro');
END

-- 7. Paseador cerca de Centro (-34.6037, -58.3816) - Segundo
DECLARE @UserId7 INT;
IF NOT EXISTS (SELECT 1 FROM [User] WHERE Nombre = 'Roberto' AND Apellido = 'Paseador')
BEGIN
    DECLARE @MailId7 INT;
    INSERT INTO Mail (Correo, Contrasena, Fecha_Creacion) VALUES ('roberto.paseador@test.com', @ContrasenaHash, GETDATE());
    SET @MailId7 = SCOPE_IDENTITY();
    
    INSERT INTO [User] (Nombre, Apellido, Id_Mail, Estado, EstadoOnline, UltimaActividad)
    VALUES ('Roberto', 'Paseador', @MailId7, 1, 1, GETDATE());
    SET @UserId7 = SCOPE_IDENTITY();
    
    INSERT INTO Perfil (Id_Usuario, FotoPerfil) VALUES (@UserId7, '/img/perfil/default.png');
    
    DECLARE @UbicacionId7 INT;
    INSERT INTO Ubicacion (Latitud, Longitud, Tipo) VALUES (-34.6030, -58.3810, 'Casa');
    SET @UbicacionId7 = SCOPE_IDENTITY();
    UPDATE [User] SET Id_Ubicacion = @UbicacionId7 WHERE Id_User = @UserId7;
    
    DECLARE @ProveedorId7 INT;
    INSERT INTO ProveedorServicio (
        Id_User, DNI, NombreCompleto, Experiencia_Anios, Descripcion, 
        FotoPerfil, Telefono, Direccion, Ciudad, Provincia, Pais,
        Latitud, Longitud, Radio_Atencion_Km, Tipo_Ubicacion, Precio_Hora, Estado
    ) VALUES (
        @UserId7, '22334455', 'Roberto Paseador', 6, 
        'Paseador profesional. Disponible fines de semana y feriados.',
        '/img/perfil/default.png', '+54 11 2222-3333', 'Av. Corrientes 1500', 'Buenos Aires', 'CABA', 'Argentina',
        -34.6030, -58.3810, 3.5, 'Cobertura', 2300.00, 1
    );
    SET @ProveedorId7 = SCOPE_IDENTITY();
    
    IF @TipoServicioPaseador IS NOT NULL
        INSERT INTO ProveedorServicio_TipoServicio (Id_Proveedor, Id_TipoServicio) VALUES (@ProveedorId7, @TipoServicioPaseador);
    INSERT INTO ProveedorServicio_Especie (Id_Proveedor, Especie) VALUES (@ProveedorId7, 'Perro');
END

-- 8. Cuidador cerca de Palermo (-34.5895, -58.3974)
DECLARE @UserId8 INT;
IF NOT EXISTS (SELECT 1 FROM [User] WHERE Nombre = 'Sofía' AND Apellido = 'Cuidadora')
BEGIN
    DECLARE @MailId8 INT;
    INSERT INTO Mail (Correo, Contrasena, Fecha_Creacion) VALUES ('sofia.cuidadora@test.com', @ContrasenaHash, GETDATE());
    SET @MailId8 = SCOPE_IDENTITY();
    
    INSERT INTO [User] (Nombre, Apellido, Id_Mail, Estado, EstadoOnline, UltimaActividad)
    VALUES ('Sofía', 'Cuidadora', @MailId8, 1, 1, GETDATE());
    SET @UserId8 = SCOPE_IDENTITY();
    
    INSERT INTO Perfil (Id_Usuario, FotoPerfil) VALUES (@UserId8, '/img/perfil/default.png');
    
    DECLARE @UbicacionId8 INT;
    INSERT INTO Ubicacion (Latitud, Longitud, Tipo) VALUES (-34.5900, -58.3980, 'Casa');
    SET @UbicacionId8 = SCOPE_IDENTITY();
    UPDATE [User] SET Id_Ubicacion = @UbicacionId8 WHERE Id_User = @UserId8;
    
    DECLARE @ProveedorId8 INT;
    INSERT INTO ProveedorServicio (
        Id_User, DNI, NombreCompleto, Experiencia_Anios, Descripcion, 
        FotoPerfil, Telefono, Direccion, Ciudad, Provincia, Pais,
        Latitud, Longitud, Radio_Atencion_Km, Tipo_Ubicacion, Precio_Hora, Estado
    ) VALUES (
        @UserId8, '44556677', 'Sofía Cuidadora', 5, 
        'Cuidadora de perros y gatos. Servicio de guardería diurna.',
        '/img/perfil/default.png', '+54 11 1111-2222', 'Av. Santa Fe 2400', 'Buenos Aires', 'CABA', 'Argentina',
        -34.5900, -58.3980, NULL, 'Precisa', 3600.00, 1
    );
    SET @ProveedorId8 = SCOPE_IDENTITY();
    
    IF @TipoServicioCuidador IS NOT NULL
        INSERT INTO ProveedorServicio_TipoServicio (Id_Proveedor, Id_TipoServicio) VALUES (@ProveedorId8, @TipoServicioCuidador);
    INSERT INTO ProveedorServicio_Especie (Id_Proveedor, Especie) VALUES (@ProveedorId8, 'Perro');
    INSERT INTO ProveedorServicio_Especie (Id_Proveedor, Especie) VALUES (@ProveedorId8, 'Gato');
END

PRINT 'Usuarios fake de proveedores insertados correctamente'
GO
