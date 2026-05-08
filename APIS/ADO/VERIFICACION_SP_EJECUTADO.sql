-- SCRIPT DE VERIFICACIÓN - SP MODIFICADO
-- Ejecutar DESPUÉS de aplicar buscarPorDocumCC1_Modified.sql

USE [A_ZULIA_12]
GO

PRINT '=== VERIFICANDO QUE EL SP SE HAYA MODIFICADO ==='

-- 1. Verificar que el SP tenga el campo tiene_imagen
EXEC sp_helptext 'buscarPorDocumCC1'
GO

PRINT '=== PROBANDO EL SP MODIFICADO ==='

-- 2. Ejecutar el SP y verificar campos
SELECT TOP 5 
    co_tipo_doc,
    nro_doc,
    cli_des,
    tiene_imagen,
    CASE 
        WHEN tiene_imagen = 1 THEN 'CON IMAGEN'
        ELSE 'SIN IMAGEN'
    END as EstadoImagen,
    CASE 
        WHEN Imagen IS NOT NULL THEN 'IMAGEN PRESENTE'
        ELSE 'IMAGEN NULL'
    END as CampoImagen
FROM buscarPorDocumCC1('', 5, '01')
ORDER BY tiene_imagen DESC
GO

PRINT '=== VERIFICACIÓN COMPLETADA ==='
PRINT 'Si ves el campo tiene_imagen y Imagen, el SP está funcionando correctamente.'
PRINT 'Si no ves estos campos, el SP NO se ha modificado.'
