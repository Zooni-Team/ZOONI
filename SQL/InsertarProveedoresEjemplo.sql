-- Script para insertar proveedores de ejemplo para preview
USE [Zooni]
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

-- Función auxiliar para crear usuarios y proveedores de ejemplo
-- Nota: Los datos están encriptados, así que necesitamos crear usuarios reales primero

-- Proveedor 1: Juan Pérez - Paseador en Buenos Aires
DECLARE @IdMail1 INT, @IdUser1 INT, @IdProveedor1 INT, @IdTipoServicio1 INT;

-- Crear mail
IF NOT EXISTS (SELECT * FROM Mail WHERE Correo = 'juan.paseador@zooni.com')
BEGIN
    INSERT INTO Mail (Correo) VALUES ('juan.paseador@zooni.com');
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
    
    INSERT INTO [User] (Id_Mail, Nombre, Apellido, Id_TipoUsuario, Estado)
    VALUES (@IdMail1, 'Juan', 'Pérez', @IdTipoProveedor, 1);
    SET @IdUser1 = SCOPE_IDENTITY();
END
ELSE
BEGIN
    SELECT @IdUser1 = Id_User FROM [User] WHERE Id_Mail = @IdMail1;
END

-- Crear proveedor si no existe
IF NOT EXISTS (SELECT * FROM ProveedorServicio WHERE Id_User = @IdUser1)
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
    
    -- Asignar tipo de servicio
    SELECT @IdTipoServicio1 = Id_TipoServicio FROM TipoServicio WHERE Descripcion = 'Paseador';
    INSERT INTO ProveedorServicio_TipoServicio (Id_Proveedor, Id_TipoServicio)
    VALUES (@IdProveedor1, @IdTipoServicio1);
    
    -- Asignar especies
    INSERT INTO ProveedorServicio_Especie (Id_Proveedor, Especie) VALUES (@IdProveedor1, 'Perro');
END
GO

-- Proveedor 2: María González - Cuidadora en Córdoba
DECLARE @IdMail2 INT, @IdUser2 INT, @IdProveedor2 INT, @IdTipoServicio2 INT;

IF NOT EXISTS (SELECT * FROM Mail WHERE Correo = 'maria.cuidadora@zooni.com')
BEGIN
    INSERT INTO Mail (Correo) VALUES ('maria.cuidadora@zooni.com');
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
    
    INSERT INTO [User] (Id_Mail, Nombre, Apellido, Id_TipoUsuario, Estado)
    VALUES (@IdMail2, 'María', 'González', @IdTipoProveedor2, 1);
    SET @IdUser2 = SCOPE_IDENTITY();
END
ELSE
BEGIN
    SELECT @IdUser2 = Id_User FROM [User] WHERE Id_Mail = @IdMail2;
END

IF NOT EXISTS (SELECT * FROM ProveedorServicio WHERE Id_User = @IdUser2)
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
    
    SELECT @IdTipoServicio2 = Id_TipoServicio FROM TipoServicio WHERE Descripcion = 'Cuidador';
    INSERT INTO ProveedorServicio_TipoServicio (Id_Proveedor, Id_TipoServicio)
    VALUES (@IdProveedor2, @IdTipoServicio2);
    
    INSERT INTO ProveedorServicio_Especie (Id_Proveedor, Especie) VALUES (@IdProveedor2, 'Perro');
    INSERT INTO ProveedorServicio_Especie (Id_Proveedor, Especie) VALUES (@IdProveedor2, 'Gato');
END
GO

-- Proveedor 3: Carlos Rodríguez - Paseador en Rosario
DECLARE @IdMail3 INT, @IdUser3 INT, @IdProveedor3 INT, @IdTipoServicio3 INT;

IF NOT EXISTS (SELECT * FROM Mail WHERE Correo = 'carlos.paseador@zooni.com')
BEGIN
    INSERT INTO Mail (Correo) VALUES ('carlos.paseador@zooni.com');
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
    
    INSERT INTO [User] (Id_Mail, Nombre, Apellido, Id_TipoUsuario, Estado)
    VALUES (@IdMail3, 'Carlos', 'Rodríguez', @IdTipoProveedor3, 1);
    SET @IdUser3 = SCOPE_IDENTITY();
END
ELSE
BEGIN
    SELECT @IdUser3 = Id_User FROM [User] WHERE Id_Mail = @IdMail3;
END

IF NOT EXISTS (SELECT * FROM ProveedorServicio WHERE Id_User = @IdUser3)
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
    
    SELECT @IdTipoServicio3 = Id_TipoServicio FROM TipoServicio WHERE Descripcion = 'Paseador';
    INSERT INTO ProveedorServicio_TipoServicio (Id_Proveedor, Id_TipoServicio)
    VALUES (@IdProveedor3, @IdTipoServicio3);
    
    INSERT INTO ProveedorServicio_Especie (Id_Proveedor, Especie) VALUES (@IdProveedor3, 'Perro');
END
GO

-- Proveedor 4: Ana Martínez - Peluquería en Mendoza
DECLARE @IdMail4 INT, @IdUser4 INT, @IdProveedor4 INT, @IdTipoServicio4 INT;

IF NOT EXISTS (SELECT * FROM Mail WHERE Correo = 'ana.peluqueria@zooni.com')
BEGIN
    INSERT INTO Mail (Correo) VALUES ('ana.peluqueria@zooni.com');
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
    
    INSERT INTO [User] (Id_Mail, Nombre, Apellido, Id_TipoUsuario, Estado)
    VALUES (@IdMail4, 'Ana', 'Martínez', @IdTipoProveedor4, 1);
    SET @IdUser4 = SCOPE_IDENTITY();
END
ELSE
BEGIN
    SELECT @IdUser4 = Id_User FROM [User] WHERE Id_Mail = @IdMail4;
END

IF NOT EXISTS (SELECT * FROM ProveedorServicio WHERE Id_User = @IdUser4)
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
    
    SELECT @IdTipoServicio4 = Id_TipoServicio FROM TipoServicio WHERE Descripcion = 'Peluquería';
    INSERT INTO ProveedorServicio_TipoServicio (Id_Proveedor, Id_TipoServicio)
    VALUES (@IdProveedor4, @IdTipoServicio4);
    
    INSERT INTO ProveedorServicio_Especie (Id_Proveedor, Especie) VALUES (@IdProveedor4, 'Perro');
    INSERT INTO ProveedorServicio_Especie (Id_Proveedor, Especie) VALUES (@IdProveedor4, 'Gato');
END
GO

-- Proveedor 5: Luis Fernández - Paseador y Cuidador en La Plata
DECLARE @IdMail5 INT, @IdUser5 INT, @IdProveedor5 INT, @IdTipoServicio5A INT, @IdTipoServicio5B INT;

IF NOT EXISTS (SELECT * FROM Mail WHERE Correo = 'luis.servicios@zooni.com')
BEGIN
    INSERT INTO Mail (Correo) VALUES ('luis.servicios@zooni.com');
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
    
    INSERT INTO [User] (Id_Mail, Nombre, Apellido, Id_TipoUsuario, Estado)
    VALUES (@IdMail5, 'Luis', 'Fernández', @IdTipoProveedor5, 1);
    SET @IdUser5 = SCOPE_IDENTITY();
END
ELSE
BEGIN
    SELECT @IdUser5 = Id_User FROM [User] WHERE Id_Mail = @IdMail5;
END

IF NOT EXISTS (SELECT * FROM ProveedorServicio WHERE Id_User = @IdUser5)
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
    
    SELECT @IdTipoServicio5A = Id_TipoServicio FROM TipoServicio WHERE Descripcion = 'Paseador';
    SELECT @IdTipoServicio5B = Id_TipoServicio FROM TipoServicio WHERE Descripcion = 'Cuidador';
    INSERT INTO ProveedorServicio_TipoServicio (Id_Proveedor, Id_TipoServicio)
    VALUES (@IdProveedor5, @IdTipoServicio5A);
    INSERT INTO ProveedorServicio_TipoServicio (Id_Proveedor, Id_TipoServicio)
    VALUES (@IdProveedor5, @IdTipoServicio5B);
    
    INSERT INTO ProveedorServicio_Especie (Id_Proveedor, Especie) VALUES (@IdProveedor5, 'Perro');
    INSERT INTO ProveedorServicio_Especie (Id_Proveedor, Especie) VALUES (@IdProveedor5, 'Gato');
END
GO

PRINT '✅ Proveedores de ejemplo insertados correctamente';
GO


