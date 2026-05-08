# Instrucciones para Agregar la Columna 'Nombre' a la Tabla Usuarios

## Estado Actual de la Tabla
Tu tabla `Usuarios` actualmente tiene:
- Id
- Usuario
- Clave

Y contiene **6 registros** con sus datos intactos.

## ⚠️ IMPORTANTE
Este proceso **NO eliminará ningún dato** de tu tabla. Solo agregará una nueva columna.

## Pasos para Ejecutar el Script

### Opción 1: SQL Server Management Studio (SSMS)

1. Abre **SQL Server Management Studio**
2. Conéctate a tu servidor de base de datos
3. Abre el archivo: `ADO/AGREGAR_COLUMNA_NOMBRE.sql`
4. Asegúrate de estar en la base de datos correcta ejecutando:
   ```sql
   USE [NombreDeTuBaseDeDatos]
   GO
   ```
5. Ejecuta el script completo presionando **F5** o haciendo clic en "Ejecutar"

### Opción 2: Desde Visual Studio

1. En Visual Studio, ve al **Explorador de Servidores**
2. Expande tu conexión de base de datos
3. Haz clic derecho en la base de datos y selecciona **"Nueva consulta"**
4. Copia y pega el contenido del archivo `AGREGAR_COLUMNA_NOMBRE.sql`
5. Ejecuta la consulta

## Resultado Esperado

Después de ejecutar el script, verás el mensaje:
```
Columna Nombre agregada exitosamente a la tabla Usuarios
```

Y tu tabla quedará con la siguiente estructura:
```
Id | Usuario | Nombre | Clave
--------------------------------
2  | 04      | NULL   | 123*
3  | 02      | NULL   | *123*
6  | 03      | NULL   | *123*
7  | 05      | NULL   | *123*
8  | 10      | NULL   | 1234*
9  | 11      | NULL   | *123*
```

**Nota:** Los valores en la columna `Nombre` serán NULL hasta que los actualices manualmente o desde tu aplicación.

## Verificación

Para verificar que la columna fue agregada correctamente, ejecuta:
```sql
SELECT * FROM Usuarios;
```

También puedes verificar la estructura de la tabla con:
```sql
EXEC sp_columns 'Usuarios';
```

## Siguiente Paso (Opcional)

Si deseas agregar nombres a los usuarios existentes, puedes ejecutar:
```sql
UPDATE Usuarios SET Nombre = 'Nombre del Usuario 1' WHERE Id = 2;
UPDATE Usuarios SET Nombre = 'Nombre del Usuario 2' WHERE Id = 3;
-- Y así sucesivamente para cada usuario
```

## Seguridad de los Datos

✅ Tus datos actuales están **100% seguros**
✅ El comando `ALTER TABLE ADD` solo agrega una columna
✅ No se elimina ni modifica ningún dato existente
✅ Si la columna ya existe, el script no hace nada (evita errores)

## ¿Necesitas Hacer Rollback?

Si por alguna razón necesitas eliminar la columna agregada (NO RECOMENDADO), puedes ejecutar:
```sql
ALTER TABLE Usuarios
DROP COLUMN Nombre;
```
**⚠️ Advertencia:** Esto eliminará la columna y todos los datos que hayas agregado en ella.

