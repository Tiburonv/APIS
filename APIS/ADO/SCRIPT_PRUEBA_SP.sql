-- SCRIPT DE PRUEBA PARA VERIFICAR EL SP MODIFICADO
-- Ejecutar después de aplicar la modificación al SP

USE [A_ZULIA_12]
GO

-- 1. VERIFICAR QUE EL SP SE ACTUALIZÓ CORRECTAMENTE
PRINT '=== VERIFICANDO ACTUALIZACIÓN DEL SP ==='
EXEC sp_helptext 'buscarPorDocumCC1'
GO

-- 2. PRUEBA BÁSICA DEL SP
PRINT '=== PRUEBA BÁSICA DEL SP ==='
EXEC buscarPorDocumCC1 @consulta = '', @cantidad = 5, @usuario = '01'
GO

-- 3. VERIFICAR QUE EL CAMPO tiene_imagen EXISTA Y TENGA VALORES CORRECTOS
PRINT '=== VERIFICANDO CAMPO tiene_imagen ==='
SELECT 
    co_tipo_doc,
    nro_doc,
    cli_des,
    tiene_imagen,
    CASE 
        WHEN tiene_imagen = 1 THEN 'CON IMAGEN'
        ELSE 'SIN IMAGEN'
    END as EstadoImagen
FROM buscarPorDocumCC1('', 20, '01')
ORDER BY tiene_imagen DESC, Dias DESC
GO

-- 4. CONTAR FACTURAS CON Y SIN IMAGEN
PRINT '=== CONTANDO FACTURAS CON Y SIN IMAGEN ==='
SELECT 
    COUNT(*) as TotalFacturas,
    SUM(tiene_imagen) as ConImagen,
    COUNT(*) - SUM(tiene_imagen) as SinImagen,
    ROUND((SUM(tiene_imagen) * 100.0 / COUNT(*)), 2) as PorcentajeConImagen
FROM buscarPorDocumCC1('', 1000, '01')
GO

-- 5. VERIFICAR QUE NO HAYA VALORES NULL EN tiene_imagen
PRINT '=== VERIFICANDO QUE NO HAY VALORES NULL ==='
SELECT 
    COUNT(*) as TotalRegistros,
    SUM(CASE WHEN tiene_imagen IS NULL THEN 1 ELSE 0 END) as RegistrosNull,
    SUM(CASE WHEN tiene_imagen NOT IN (0,1) THEN 1 ELSE 0 END) as ValoresInvalidos
FROM buscarPorDocumCC1('', 1000, '01')
GO

-- 6. PRUEBA DE RENDIMIENTO
PRINT '=== PRUEBA DE RENDIMIENTO ==='
SET STATISTICS TIME ON
EXEC buscarPorDocumCC1 @consulta = '', @cantidad = 100, @usuario = '01'
SET STATISTICS TIME OFF
GO

-- 7. VERIFICAR QUE EL CAMPO Imagen SIGA FUNCIONANDO
PRINT '=== VERIFICANDO CAMPO Imagen ==='
SELECT 
    co_tipo_doc,
    nro_doc,
    tiene_imagen,
    CASE 
        WHEN Imagen IS NOT NULL THEN 'IMAGEN PRESENTE'
        ELSE 'IMAGEN NULL'
    END as EstadoImagen,
    CASE 
        WHEN Imagen IS NOT NULL AND DATALENGTH(Imagen) > 0 THEN 'IMAGEN CON CONTENIDO'
        ELSE 'IMAGEN SIN CONTENIDO'
    END as ContenidoImagen
FROM buscarPorDocumCC1('', 10, '01')
ORDER BY tiene_imagen DESC
GO

PRINT '=== PRUEBAS COMPLETADAS ==='
PRINT 'Si todas las consultas se ejecutaron sin errores, el SP está funcionando correctamente.'
PRINT 'Verificar que:'
PRINT '1. El campo tiene_imagen aparezca en los resultados'
PRINT '2. Los valores sean solo 0 o 1'
PRINT '3. No haya valores NULL'
PRINT '4. El conteo de imágenes sea correcto'
