# INSTRUCCIONES PARA ACTUALIZAR EL SP buscarPorDocumCC1

## Fecha: 09/10/2025

## CAMBIO REALIZADO

Se modificó el procedimiento almacenado `buscarPorDocumCC1` para que incluya documentos tipo **'PED'** (pedidos) además de **'FACT'** y **'N/CR'**.

### Línea modificada:
**Antes:**
```sql
AND a.co_tipo_doc IN ('FACT', 'N/CR')
```

**Después:**
```sql
AND a.co_tipo_doc IN ('FACT', 'N/CR', 'PED')
```

## PASOS PARA APLICAR EL CAMBIO

### Opción 1: Ejecutar todo el script
1. Abre **SQL Server Management Studio (SSMS)**
2. Conéctate a tu servidor de base de datos
3. Abre el archivo: `ADO\buscarPorDocumCC1_Modified.sql`
4. Verifica que estás conectado a la base de datos **A_ZULIA_12**
5. Ejecuta el script completo (F5 o presiona el botón Execute)
6. Verifica que el mensaje indique: "Command(s) completed successfully."

### Opción 2: Solo ejecutar el ALTER PROCEDURE
Si prefieres ejecutar solo el comando ALTER PROCEDURE, copia y pega lo siguiente en SSMS:

```sql
USE [A_ZULIA_12]
GO

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
            iif(a.co_tipo_doc='N/CR',a.nro_orig,a.nro_doc) as nro_doc,
            a.doc_orig,
			a.fec_emis,
            FORMAT(a.fec_emis, 'dd/MM/yyyy') AS fec_emis1,
            DATEDIFF(DAY, a.fec_emis, GETDATE()) AS Dias,
            ROUND(iif(a.co_tipo_doc='N/CR',(a.saldo/a.tasa)*-1,(a.saldo/a.tasa)),2) as saldo,
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
            AND a.co_tipo_doc IN ('FACT', 'N/CR', 'PED')
            AND (b.cli_des LIKE '%' + @consulta + '%' OR a.co_cli LIKE '%' + @consulta + '%')
            AND (
                @usuario IN ('04', '05')
                OR a.co_ven = @usuario
            )
    )
    SELECT TOP (@cantidad) *
    FROM DocumentoCalculado
    ORDER BY Dias DESC;
END
GO
```

## VERIFICACIÓN

Después de ejecutar el script, puedes verificar que funcione correctamente con esta consulta:

```sql
-- Verificar que el SP ahora incluye PED
EXEC buscarPorDocumCC1 @consulta = '', @cantidad = 100, @usuario = '04'

-- Verificar cuántos registros hay de cada tipo
SELECT co_tipo_doc, COUNT(*) as cantidad
FROM (
    EXEC buscarPorDocumCC1 @consulta = '', @cantidad = 1000, @usuario = '04'
) AS resultado
GROUP BY co_tipo_doc
ORDER BY co_tipo_doc
```

## RESULTADO ESPERADO

Después de ejecutar el SP modificado:

✅ Al presionar el botón **"Todos"** en la aplicación, se mostrarán:
   - Documentos tipo **FACT** (facturas)
   - Documentos tipo **PED** (pedidos)
   - Documentos tipo **N/CR** (notas de crédito)

✅ Al usar el **cuadro de búsqueda**, también se incluirán todos estos tipos de documentos

✅ La columna **FACT/PED** mostrará el conteo correcto de ambos tipos de documentos

## IMPORTANTE

⚠️ **HAGA UN RESPALDO** del procedimiento almacenado original antes de ejecutar este script.

Para hacer el respaldo, ejecuta:
```sql
-- Guardar el SP actual
SELECT OBJECT_DEFINITION(OBJECT_ID('dbo.buscarPorDocumCC1'))
```

Guarda el resultado en un archivo de texto antes de continuar.

## PRUEBAS RECOMENDADAS

1. ✅ Verificar que el botón "Todos" muestre tanto FACT como PED
2. ✅ Verificar que la búsqueda por nombre de cliente funcione correctamente
3. ✅ Verificar que el conteo en la columna FACT/PED sea correcto
4. ✅ Verificar que al seleccionar un cliente se muestren todos sus documentos (FACT y PED)

---
**Fecha de modificación:** 09/10/2025
**Modificado por:** AI Assistant
**Motivo:** Incluir documentos tipo PED en todas las búsquedas y en el botón "Todos"

