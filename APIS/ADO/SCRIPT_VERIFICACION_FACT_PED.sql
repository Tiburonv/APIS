-- ========================================
-- SCRIPT DE VERIFICACIÓN - buscarPorDocumCC1
-- Fecha: 09/10/2025
-- Propósito: Verificar que el SP incluya FACT y PED
-- ========================================

USE [A_ZULIA_12]
GO

PRINT '========================================';
PRINT 'VERIFICACIÓN DEL SP buscarPorDocumCC1';
PRINT 'Fecha: ' + CONVERT(VARCHAR(20), GETDATE(), 120);
PRINT '========================================';
PRINT '';

-- 1. Verificar la definición del SP
PRINT '1. VERIFICANDO DEFINICIÓN DEL SP...';
PRINT '';

IF OBJECT_DEFINITION(OBJECT_ID('dbo.buscarPorDocumCC1')) LIKE '%''PED''%'
BEGIN
    PRINT '✅ CORRECTO: El SP incluye ''PED'' en el filtro';
END
ELSE
BEGIN
    PRINT '❌ ERROR: El SP NO incluye ''PED'' en el filtro';
    PRINT 'Por favor, ejecute el script de actualización primero.';
END
PRINT '';

-- 2. Probar el SP con diferentes usuarios
PRINT '2. PROBANDO EL SP CON USUARIO 04...';
PRINT '';

-- Crear tabla temporal para almacenar resultados
CREATE TABLE #Resultados (
    co_tipo_doc NVARCHAR(10),
    cantidad INT
);

-- Ejecutar el SP y agrupar por tipo de documento
INSERT INTO #Resultados (co_tipo_doc, cantidad)
SELECT 
    co_tipo_doc,
    COUNT(*) as cantidad
FROM (
    -- Obtener una muestra de 1000 registros
    SELECT co_tipo_doc
    FROM dbo.saDocumentoVenta
    WHERE saldo <> 0
        AND anulado = 0
        AND co_tipo_doc IN ('FACT', 'N/CR', 'PED')
) AS Documentos
GROUP BY co_tipo_doc;

-- Mostrar resultados
PRINT 'TIPOS DE DOCUMENTOS ENCONTRADOS:';
PRINT '';

SELECT 
    co_tipo_doc AS [Tipo Documento],
    cantidad AS [Cantidad],
    CASE 
        WHEN co_tipo_doc = 'FACT' THEN '✅ Facturas'
        WHEN co_tipo_doc = 'PED' THEN '✅ Pedidos'
        WHEN co_tipo_doc = 'N/CR' THEN '✅ Notas de Crédito'
        ELSE '⚠️ Otro tipo'
    END AS [Descripción]
FROM #Resultados
ORDER BY co_tipo_doc;

PRINT '';
PRINT 'TOTAL DE DOCUMENTOS: ' + CAST((SELECT SUM(cantidad) FROM #Resultados) AS VARCHAR(10));
PRINT '';

-- 3. Verificar que hay documentos PED
IF EXISTS (SELECT 1 FROM #Resultados WHERE co_tipo_doc = 'PED')
BEGIN
    PRINT '✅ CORRECTO: Se encontraron documentos tipo PED';
    PRINT '   Cantidad de PED: ' + CAST((SELECT cantidad FROM #Resultados WHERE co_tipo_doc = 'PED') AS VARCHAR(10));
END
ELSE
BEGIN
    PRINT '⚠️ ADVERTENCIA: No se encontraron documentos tipo PED en la base de datos';
    PRINT '   Esto puede ser normal si no hay pedidos con saldo pendiente.';
END
PRINT '';

-- 4. Verificar que hay documentos FACT
IF EXISTS (SELECT 1 FROM #Resultados WHERE co_tipo_doc = 'FACT')
BEGIN
    PRINT '✅ CORRECTO: Se encontraron documentos tipo FACT';
    PRINT '   Cantidad de FACT: ' + CAST((SELECT cantidad FROM #Resultados WHERE co_tipo_doc = 'FACT') AS VARCHAR(10));
END
ELSE
BEGIN
    PRINT '⚠️ ADVERTENCIA: No se encontraron documentos tipo FACT en la base de datos';
END
PRINT '';

-- 5. Prueba real del SP
PRINT '3. EJECUTANDO PRUEBA REAL DEL SP...';
PRINT '';

-- Ejecutar el SP buscarPorDocumCC1 con consulta vacía
DECLARE @TotalRegistros INT;

SELECT @TotalRegistros = COUNT(*)
FROM (
    EXEC buscarPorDocumCC1 @consulta = '', @cantidad = 100, @usuario = '04'
) AS Resultado;

PRINT 'Total de registros devueltos por el SP: ' + CAST(@TotalRegistros AS VARCHAR(10));
PRINT '';

-- 6. Resumen final
PRINT '========================================';
PRINT 'RESUMEN DE LA VERIFICACIÓN';
PRINT '========================================';
PRINT '';

-- Verificación del filtro en el SP
IF OBJECT_DEFINITION(OBJECT_ID('dbo.buscarPorDocumCC1')) LIKE '%''PED''%'
    PRINT '✅ Filtro del SP: CORRECTO (incluye PED)';
ELSE
    PRINT '❌ Filtro del SP: INCORRECTO (NO incluye PED)';

-- Verificación de documentos encontrados
IF EXISTS (SELECT 1 FROM #Resultados WHERE co_tipo_doc IN ('FACT', 'PED'))
    PRINT '✅ Documentos: Se encontraron FACT y/o PED';
ELSE
    PRINT '⚠️ Documentos: No se encontraron documentos';

PRINT '';
PRINT '========================================';
PRINT 'FIN DE LA VERIFICACIÓN';
PRINT '========================================';

-- Limpiar tabla temporal
DROP TABLE #Resultados;

GO

-- ========================================
-- CONSULTAS ADICIONALES PARA PRUEBAS
-- ========================================

-- Consulta 1: Ver una muestra de documentos FACT y PED
PRINT '';
PRINT 'MUESTRA DE DOCUMENTOS FACT Y PED:';
PRINT '';

SELECT TOP 10
    co_tipo_doc AS [Tipo],
    nro_doc AS [Número],
    co_cli AS [Cliente],
    FORMAT(fec_emis, 'dd/MM/yyyy') AS [Fecha],
    saldo AS [Saldo]
FROM dbo.saDocumentoVenta
WHERE saldo <> 0
    AND anulado = 0
    AND co_tipo_doc IN ('FACT', 'PED')
ORDER BY fec_emis DESC;

GO

-- Consulta 2: Contar documentos por tipo para un cliente específico
PRINT '';
PRINT 'PARA PROBAR CON UN CLIENTE ESPECÍFICO, EJECUTA:';
PRINT '';
PRINT '-- Reemplaza ''CODIGO_CLIENTE'' con un código real:';
PRINT 'EXEC buscarPorDocumCC1 @consulta = ''CODIGO_CLIENTE'', @cantidad = 100, @usuario = ''04''';
PRINT '';

GO

