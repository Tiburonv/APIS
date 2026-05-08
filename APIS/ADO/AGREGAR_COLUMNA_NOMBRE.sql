-- Script para agregar la columna 'Nombre' a la tabla Usuarios
-- Este script NO elimina ningún dato existente

-- Verificar si la columna ya existe antes de agregarla
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Usuarios' 
    AND COLUMN_NAME = 'Nombre'
)
BEGIN
    -- Agregar la columna Nombre a la tabla Usuarios
    ALTER TABLE Usuarios
    ADD Nombre NVARCHAR(100) NULL;
    
    PRINT 'Columna Nombre agregada exitosamente a la tabla Usuarios';
END
ELSE
BEGIN
    PRINT 'La columna Nombre ya existe en la tabla Usuarios';
END
GO

-- Verificar que la columna fue agregada correctamente
SELECT * FROM Usuarios;
GO

