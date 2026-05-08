-- ========================================
-- VERIFICACIÓN RÁPIDA - Orden del SP
-- Fecha: 09/10/2025
-- ========================================

USE [A_ZULIA_12]
GO

PRINT '========================================';
PRINT 'VERIFICANDO EL FIX DEL ORDEN DEL SP';
PRINT '========================================';
PRINT '';

-- 1. Verificar la definición actual del SP
DECLARE @Definicion NVARCHAR(MAX);
SET @Definicion = OBJECT_DEFINITION(OBJECT_ID('dbo.buscarPorDocumCC1'));

PRINT '1. VERIFICANDO LA DEFINICIÓN DEL SP...';
PRINT '';

IF @Definicion LIKE '%ORDER BY Dias DESC%'
BEGIN
    PRINT '✅ CORRECTO: El SP ordena por "Dias DESC"';
    PRINT '   Esto mezclará FACT y PED por antigüedad';
END
ELSE IF @Definicion LIKE '%ORDER BY Doc ASC%'
BEGIN
    PRINT '❌ PROBLEMA: El SP aún ordena por "Doc ASC"';
    PRINT '   Esto hace que solo se muestren FACT primero';
    PRINT '';
    PRINT '⚠️ SOLUCIÓN: Ejecuta el script FIX_ORDEN_SP_buscarPorDocumCC1.sql';
END
ELSE
BEGIN
    PRINT '⚠️ ADVERTENCIA: No se pudo detectar el ORDER BY';
    PRINT '   Revisa manualmente el SP';
END

PRINT '';
PRINT '========================================';

-- 2. Prueba rápida del SP (primeros 20 registros)
PRINT '';
PRINT '2. EJECUTANDO PRUEBA CON @consulta = ''''...';
PRINT '   (Primeros 20 registros)';
PRINT '';

SELECT TOP 20
    co_tipo_doc AS [Tipo],
    nro_doc AS [Número],
    cli_des AS [Cliente],
    fec_emis1 AS [Fecha],
    Dias AS [Días],
    saldo AS [Saldo]
FROM (
    EXEC buscarPorDocumCC1 @consulta = '', @cantidad = 100, @usuario = '04'
) AS Resultado
ORDER BY Dias DESC;

PRINT '';
PRINT '========================================';
PRINT 'Si ves FACT y PED mezclados, ✅ FUNCIONA';
PRINT 'Si solo ves FACT, ❌ ejecuta el fix';
PRINT '========================================';

GO

