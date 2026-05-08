# INSTRUCCIONES PARA EJECUTAR EL SP MODIFICADO

## 📋 **Paso a Paso para Actualizar el SP**

### **1. Ejecutar el SP Modificado**
```sql
-- Copiar y pegar todo el contenido del archivo buscarPorDocumCC1_Modified.sql
-- en SQL Server Management Studio (SSMS) y ejecutarlo
```

### **2. Verificar la Modificación**
```sql
-- Verificar que el SP se haya actualizado correctamente
EXEC sp_helptext 'buscarPorDocumCC1'
```

### **3. Probar el SP Modificado**
```sql
-- Ejecutar una consulta de prueba
EXEC buscarPorDocumCC1 @consulta = '', @cantidad = 10, @usuario = '01'

-- Verificar que aparezca el campo tiene_imagen con valores 1 o 0
```

## 🔍 **Cambios Realizados en el SP**

### **Campo Agregado:**
- **`tiene_imagen`**: Campo calculado que devuelve:
  - **1**: Cuando la factura tiene imagen disponible
  - **0**: Cuando la factura no tiene imagen

### **Lógica del Campo:**
```sql
CASE 
    WHEN f.picture IS NOT NULL AND DATALENGTH(f.picture) > 0 THEN 1
    ELSE 0
END AS tiene_imagen
```

### **Beneficios:**
1. **Performance**: No necesita procesar los datos de imagen para determinar si existe
2. **Eficiencia**: La aplicación puede mostrar iconos sin verificar la longitud del campo Imagen
3. **Claridad**: Lógica más directa y fácil de entender
4. **Compatibilidad**: Mantiene el campo Imagen original

## ⚠️ **Consideraciones Importantes**

### **Antes de Ejecutar:**
- Hacer backup de la base de datos
- Verificar que no haya usuarios activos ejecutando el SP
- Probar en un ambiente de desarrollo primero

### **Después de Ejecutar:**
- Verificar que la aplicación funcione correctamente
- Comprobar que los iconos de imagen se muestren correctamente
- Validar que el contador de imágenes funcione

## 🧪 **Pruebas Recomendadas**

### **Prueba 1: Verificar Campo tiene_imagen**
```sql
-- Debe devolver registros con tiene_imagen = 1 o 0
SELECT co_tipo_doc, nro_doc, tiene_imagen, Imagen
FROM buscarPorDocumCC1('', 20, '01')
WHERE tiene_imagen = 1
```

### **Prueba 2: Contar Facturas con Imagen**
```sql
-- Debe devolver el conteo correcto
SELECT 
    COUNT(*) as TotalFacturas,
    SUM(tiene_imagen) as ConImagen,
    COUNT(*) - SUM(tiene_imagen) as SinImagen
FROM buscarPorDocumCC1('', 1000, '01')
```

### **Prueba 3: Verificar Rendimiento**
```sql
-- Comparar tiempos de ejecución antes y después
SET STATISTICS TIME ON
EXEC buscarPorDocumCC1 @consulta = '', @cantidad = 100, @usuario = '01'
SET STATISTICS TIME OFF
```

## 📞 **Soporte**

Si encuentras algún problema:
1. Revisar los logs de SQL Server
2. Verificar que la función `fnFactImagen()` esté funcionando
3. Comprobar que los permisos de usuario sean correctos
4. Validar que la tabla `saDocumentoVenta` tenga datos válidos
