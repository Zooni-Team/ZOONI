-- Agregar campos para rastrear estado online/offline
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('User') AND name = 'UltimaActividad')
BEGIN
    ALTER TABLE [dbo].[User]
    ADD [UltimaActividad] [datetime2](7) NULL DEFAULT GETDATE();
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('User') AND name = 'EstadoOnline')
BEGIN
    ALTER TABLE [dbo].[User]
    ADD [EstadoOnline] [bit] NOT NULL DEFAULT 0;
END
GO

-- Crear índice para mejorar consultas de estado online
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_User_UltimaActividad')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_User_UltimaActividad] 
    ON [dbo].[User] ([UltimaActividad])
    INCLUDE ([EstadoOnline], [Id_User]);
END
GO

-- Actualizar todos los usuarios existentes para que tengan una última actividad
UPDATE [dbo].[User]
SET UltimaActividad = GETDATE(), EstadoOnline = 0
WHERE UltimaActividad IS NULL;
GO

