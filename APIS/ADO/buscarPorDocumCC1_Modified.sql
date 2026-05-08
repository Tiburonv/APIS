USE [A_ZULIA_12]
GO
/****** Object:  StoredProcedure [dbo].[buscarPorDocumCC1]    Script Date: 09/10/2025 17:45:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- SP MODIFICADO PARA INCLUIR:
-- 1. Campo tiene_imagen (indica si la factura tiene imagen)
-- 2. UNION ALL entre DocumentoCalculado (FACT, N/CR) y PedidoCalculado (PED)
-- 3. Orden por Dias DESC para mezclar todos los tipos de documentos

-- NOTA: Para listados rapidos use ADO\buscarPorDocumCC1_incluirImagen.sql (@incluirImagen BIT)

ALTER PROCEDURE [dbo].[buscarPorDocumCC1]
    @consulta NVARCHAR(100),
    @cantidad INT,
    @usuario NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH DocumentoCalculado AS (
        SELECT  
            a.co_ven,
            a.co_cli,
            a.tasa,
            b.cli_des,
            a.co_tipo_doc,
            a.co_tipo_doc AS Doc,
            iif(a.co_tipo_doc='N/CR', a.nro_orig, a.nro_doc) AS nro_doc,
            a.doc_orig,
            a.fec_emis,
            FORMAT(a.fec_emis, 'dd/MM/yyyy') AS fec_emis1,
            DATEDIFF(DAY, a.fec_emis, GETDATE()) AS Dias,
            ROUND(iif(a.co_tipo_doc='N/CR',(a.saldo/a.tasa)*-1,(a.saldo/a.tasa)),2) AS saldo,
            ROUND((a.total_bruto / a.tasa) + (a.otros1 / a.tasa), 2) AS monto_bru,
            ROUND(a.total_neto / a.tasa, 2) AS monto_net,
            ROUND(iif(a.co_tipo_doc in ('ADEL','N/CR'),(a.monto_imp / a.tasa)*-1,(a.monto_imp / a.tasa)),2) AS monto_imp,
            ROUND(iif(a.co_tipo_doc in ('ADEL','N/CR'),((a.monto_imp / a.tasa) / 4)*-1,((a.monto_imp / a.tasa) / 4)),2) AS IVA25,
            ROUND(((a.monto_imp / a.tasa) / 4) * 3, 2) AS IVA75,
            IIF(ROUND(a.saldo, 2) = ROUND(a.monto_imp, 2) / 4, a.saldo, 0) AS Ret25,
            IIF(ROUND(a.saldo / a.tasa, 2) = ROUND(((a.monto_imp / a.tasa) / 4) * 3, 2),
                ROUND(a.saldo / a.tasa, 2), 0) AS Ret75,
            ROUND(((a.monto_imp / a.tasa) / 4) * 3, 2) AS esperado_ret75,
            ROUND(ISNULL(c.monto_ret_imp / a.tasa, 2), 2) AS monto_ret_imp_convertido,
            CASE
                WHEN a.monto_imp = 0 THEN NULL
                WHEN ROUND(ISNULL(c.monto_ret_imp / a.tasa, 2), 2) = ROUND(((a.monto_imp / a.tasa) / 4) * 3, 2)
                    THEN 'Proce'
                WHEN a.co_tipo_doc in ('ADEL', 'N/CR')
                    THEN FORMAT(ROUND(((a.monto_imp / a.tasa) / 4) * 3, 2)*-1, 'N2')
                ELSE FORMAT(ROUND(((a.monto_imp / a.tasa) / 4) * 3, 2), 'N2')
            END AS estado_retencion,
            f.picture AS Imagen,
            -- NUEVO CAMPO: tiene_imagen devuelve 1 si hay imagen, 0 si no hay
            CASE 
                WHEN f.picture IS NOT NULL AND DATALENGTH(f.picture) > 0 THEN 1
                ELSE 0
            END AS tiene_imagen
        FROM saDocumentoVenta AS a
        INNER JOIN saCliente AS b ON a.co_cli = b.co_cli
        LEFT JOIN saCobroRetenIvaReng AS c ON a.nro_doc = c.numero_documento
        LEFT JOIN dbo.fnFactImagen() AS f ON a.nro_doc = f.doc_num
        WHERE 
            a.saldo <> 0
            AND a.anulado = 0
            AND a.co_tipo_doc IN ('FACT', 'N/CR')
            AND (b.cli_des LIKE '%' + @consulta + '%' OR a.co_cli LIKE '%' + @consulta + '%')
            AND (@usuario IN ('04', '01', '03') OR a.co_ven = @usuario)
    ),

    PedidoCalculado AS (
        SELECT  
            a.co_ven,
            a.co_cli,
            a.tasa,
            b.cli_des,
            'PED' AS co_tipo_doc,
            'PED' AS Doc,
            CAST(dbo.fn_LimpiarEspacios(a.doc_num) AS VARCHAR(10)) AS nro_doc,
            CAST(0 AS varchar (1)) AS doc_orig,
            a.fec_emis,
            FORMAT(a.fec_emis, 'dd/MM/yyyy') AS fec_emis1,
            DATEDIFF(DAY, a.fec_emis, GETDATE()) AS Dias,
            ROUND((a.saldo/a.tasa),2) AS saldo,
            ROUND(a.total_bruto/a.tasa, 2) AS monto_bru,
            ROUND(a.total_neto/a.tasa, 2) AS monto_net,
            ROUND(a.monto_imp/a.tasa, 2) AS monto_imp,
            CAST(0 AS INT) AS IVA25,
            CAST(0 AS INT) AS IVA75,
            CAST(0 AS INT) AS Ret25,
            CAST(0 AS INT) AS Ret75,
            CAST(0 AS INT) AS esperado_ret75,
            CAST(0 AS INT) AS monto_ret_imp_convertido,
            CAST(0 AS varchar (1)) AS estado_retencion,
            NULL AS Imagen,
            CAST(0 AS INT) AS tiene_imagen
        FROM saPedidoVenta AS a
        INNER JOIN saCliente AS b ON a.co_cli = b.co_cli
        WHERE 
            a.anulado = 0 
            AND a.status <> 2
            AND (b.cli_des LIKE '%' + @consulta + '%' OR a.co_cli LIKE '%' + @consulta + '%')
            AND (@usuario IN ('04','01', '03') OR a.co_ven = @usuario)
    )

    SELECT TOP (@cantidad) *
    FROM (
        SELECT * FROM DocumentoCalculado
        UNION ALL
        SELECT * FROM PedidoCalculado
    ) AS ResultadoUnificado
    ORDER BY Dias DESC;  -- ✅ CORREGIDO: Ordena por días (más antiguos primero) para mezclar FACT y PED
END

GO

-- ========================================
-- COMENTARIOS SOBRE LAS MODIFICACIONES:
-- ========================================
--
-- 1. Campo tiene_imagen:
--    - Devuelve 1 cuando la factura tiene imagen (f.picture IS NOT NULL AND DATALENGTH(f.picture) > 0)
--    - Devuelve 0 cuando no hay imagen
--    - Permite a la aplicación mostrar el icono de imagen sin procesar los datos completos
--
-- 2. UNION ALL entre DocumentoCalculado y PedidoCalculado:
--    - DocumentoCalculado: Incluye FACT y N/CR de la tabla saDocumentoVenta
--    - PedidoCalculado: Incluye PED de la tabla saPedidoVenta
--    - Se unifica para mostrar todos los tipos de documentos en una sola consulta
--
-- 3. ORDER BY Dias DESC (CRÍTICO):
--    - ANTES: ORDER BY Doc ASC → Ordenaba alfabéticamente (FACT primero, PED después)
--    - DESPUÉS: ORDER BY Dias DESC → Ordena por antigüedad (mezclando FACT y PED)
--    - PROBLEMA RESUELTO: El botón "Todos" ahora muestra tanto FACT como PED
--
-- 4. Filtros de usuario:
--    - Usuarios '04', '01', '03' pueden ver todos los documentos
--    - Otros usuarios solo ven sus propios documentos (a.co_ven = @usuario)
--
-- ========================================
-- HISTORIAL DE CAMBIOS:
-- ========================================
-- Fecha: 09/10/2025
-- Cambio: Se corrigió ORDER BY Doc ASC → ORDER BY Dias DESC
-- Motivo: El botón "Todos" solo mostraba FACT porque el TOP(@cantidad) 
--         tomaba solo los primeros registros ordenados alfabéticamente
-- ========================================
