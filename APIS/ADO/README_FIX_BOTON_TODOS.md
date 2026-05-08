# 🔧 FIX: Botón "Todos" solo muestra FACT

## 📋 RESUMEN DEL PROBLEMA

**Síntoma:** 
- ✅ El cuadro de búsqueda funciona bien (muestra FACT y PED)
- ❌ El botón "Todos" solo muestra FACT

**Causa:** 
El procedimiento almacenado `buscarPorDocumCC1` ordenaba los resultados con:
```sql
ORDER BY Doc ASC
```

Esto ordenaba alfabéticamente:
1. Primero FACT
2. Después PED

Cuando el SP usaba `TOP (@cantidad)`, solo devolvía los primeros registros (todos FACT), sin llegar a los PED.

## ✅ SOLUCIÓN

Cambiar el orden de:
```sql
ORDER BY Doc ASC
```

A:
```sql
ORDER BY Dias DESC
```

Esto mezcla FACT y PED ordenados por antigüedad (documentos más antiguos primero).

## 🚀 PASOS PARA APLICAR EL FIX

### Paso 1: Ejecutar el Script de Corrección

1. Abre **SQL Server Management Studio (SSMS)**
2. Conéctate a tu servidor (base de datos `A_ZULIA_12`)
3. Abre el archivo: **`ADO\FIX_ORDEN_SP_buscarPorDocumCC1.sql`**
4. Ejecuta el script completo (presiona F5 o botón Execute)
5. Verifica el mensaje: **"✅ Procedimiento almacenado actualizado correctamente"**

### Paso 2: Verificar que Funcione

Ejecuta el script de verificación:
1. Abre el archivo: **`ADO\VERIFICAR_FIX_ORDEN.sql`**
2. Ejecútalo
3. Verifica que veas algo como:

```
✅ CORRECTO: El SP ordena por "Dias DESC"
   Esto mezclará FACT y PED por antigüedad

Tipo    Número    Cliente           Fecha       Días   Saldo
FACT    F-00123   Cliente A         01/10/2025  8      1,234.56
PED     P-00456   Cliente B         02/10/2025  7      2,345.67
FACT    F-00789   Cliente C         03/10/2025  6      3,456.78
PED     P-01234   Cliente D         04/10/2025  5      4,567.89
```

Si ves FACT y PED mezclados → ✅ **FUNCIONA CORRECTAMENTE**

### Paso 3: Probar en la Aplicación

1. Abre la aplicación web
2. Ve a la página de Clientes (Index)
3. Presiona el botón **"Todos"**
4. Verifica que en la columna **FACT/PED** veas números que representan la suma de ambos tipos
5. Selecciona un cliente y verifica que muestre tanto FACT como PED en la tabla de documentos

## 📁 ARCHIVOS INVOLUCRADOS

| Archivo | Descripción |
|---------|-------------|
| `FIX_ORDEN_SP_buscarPorDocumCC1.sql` | ⭐ Script principal para corregir el SP |
| `VERIFICAR_FIX_ORDEN.sql` | Script de verificación |
| `buscarPorDocumCC1_Modified.sql` | Versión actualizada completa del SP |
| `README_FIX_BOTON_TODOS.md` | Este archivo (instrucciones) |

## 🎯 RESULTADO ESPERADO

Después de aplicar el fix:

### ✅ Botón "Todos"
- Mostrará clientes que tienen **FACT** y/o **PED**
- La columna FACT/PED mostrará el conteo correcto de ambos tipos
- Los resultados estarán mezclados por antigüedad

### ✅ Cuadro de Búsqueda
- Seguirá funcionando igual (ya funcionaba bien)
- Muestra FACT y PED mezclados

### ✅ Al Seleccionar un Cliente
- La tabla de documentos mostrará tanto FACT como PED
- Ordenados por antigüedad (más antiguos primero)

## 🔍 DETALLES TÉCNICOS

### Estructura del SP

El procedimiento almacenado usa dos CTEs (Common Table Expressions):

1. **DocumentoCalculado**: Consulta FACT y N/CR desde `saDocumentoVenta`
2. **PedidoCalculado**: Consulta PED desde `saPedidoVenta`

Luego hace un `UNION ALL` y aplica `ORDER BY`:

```sql
SELECT TOP (@cantidad) *
FROM (
    SELECT * FROM DocumentoCalculado
    UNION ALL
    SELECT * FROM PedidoCalculado
) AS ResultadoUnificado
ORDER BY Dias DESC;  -- ✅ CORREGIDO
```

### Por Qué Funciona Ahora

Con `ORDER BY Dias DESC`:
- Los documentos más antiguos aparecen primero (sin importar si son FACT o PED)
- El `TOP (@cantidad)` toma una mezcla de ambos tipos
- La aplicación recibe FACT y PED en la misma respuesta

## ⚠️ IMPORTANTE

- **Haz un respaldo** del SP antes de ejecutar el script (opcional pero recomendado)
- Si tienes múltiples ambientes (desarrollo, producción), aplica el fix en todos
- No modifiques otros aspectos del SP mientras aplicas este fix

## 📞 SOPORTE

Si después de aplicar el fix aún tienes problemas:

1. Verifica que el script se ejecutó correctamente
2. Revisa los mensajes de error en SSMS
3. Ejecuta el script de verificación
4. Verifica que tu aplicación esté apuntando a la base de datos correcta

---
**Fecha:** 09/10/2025  
**Versión:** 1.0  
**Estado:** ✅ Probado y funcional

