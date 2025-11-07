-- Actualizar la columna Peso en la tabla Mascota para asegurar precisión y rango
ALTER TABLE Mascota ALTER COLUMN Peso decimal(6,2) NOT NULL DEFAULT 0.1;

-- Agregar constraint para el rango de peso
ALTER TABLE Mascota ADD CONSTRAINT CHK_Peso CHECK (Peso >= 0.1 AND Peso <= 300.0);

-- Agregar columna PesoDisplay para mantener el valor ingresado por el usuario
ALTER TABLE Mascota ADD PesoDisplay nvarchar(20) NULL;

-- Migrar datos existentes
UPDATE Mascota SET PesoDisplay = CAST(Peso as nvarchar(20)) + ' kg';

-- Corregir datos existentes
UPDATE Mascota SET Peso = 0.1 WHERE Peso < 0.1;
UPDATE Mascota SET Peso = 300.0 WHERE Peso > 300.0;
UPDATE Mascota SET Peso = ROUND(Peso, 2) WHERE Peso IS NOT NULL;