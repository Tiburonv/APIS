# Funcionalidad de Detalle de Pedidos

## Descripción
Esta funcionalidad permite ver el detalle completo de un pedido cuando se hace clic en el número de documento en la lista de pedidos.

## Características Implementadas

### 1. Controlador (PedidosController.cs)
- **Método `obtenerDetallePedido(string doc_num)`**: Obtiene el encabezado y renglones de un pedido específico
- **Método `DetallePedido(string doc_num)`**: Acción que renderiza la vista de detalle
- Utiliza los stored procedures existentes:
  - `buscarPedidosActivos` para el encabezado
  - `buscarRengPE` para los renglones

### 2. Vista Principal (Ped.cshtml)
- El número de documento ahora es clickeable
- Se agregó un ícono de enlace externo para indicar que es clickeable
- Al hacer clic se ejecuta el método `verDetallePedido(doc_num)`
- Estilos CSS mejorados para el enlace

### 3. Vista de Detalle (DetallePedido.cshtml)
- **Encabezado del Pedido**: Muestra toda la información del pedido en un diseño de tarjetas
- **Tabla de Renglones**: Lista todos los artículos del pedido con sus detalles
- **Diseño Responsivo**: Optimizado para dispositivos móviles
- **Estados Visuales**: Indicadores visuales para pedidos anulados/activos
- **Totales**: Cálculo automático de totales de impuestos y neto

## Flujo de Funcionamiento

1. Usuario busca pedidos en la página principal
2. En la lista de resultados, el número de documento aparece como enlace clickeable
3. Al hacer clic, se navega a `/Pedidos/DetallePedido?doc_num=XXXX`
4. La página de detalle carga automáticamente:
   - Encabezado del pedido
   - Lista de renglones/artículos
5. El usuario puede regresar usando el botón "Volver"

## Estructura de Datos

### Encabezado del Pedido
- Número de documento
- Cliente (código y descripción)
- Fecha de emisión
- Estado (anulado/activo)
- Status
- Tasa
- Totales (bruto, impuestos, neto, saldo)

### Renglones del Pedido
- Número de renglón
- Código y descripción del artículo
- Cantidad
- Precio de venta
- Tipo de impuesto
- Monto de impuesto
- Neto del renglón

## Tecnologías Utilizadas

- **Backend**: ASP.NET MVC 5, Entity Framework 6
- **Frontend**: Vue.js 2, Bootstrap 5, Font Awesome
- **Base de Datos**: SQL Server con stored procedures
- **Diseño**: Mobile-first, responsive design

## Archivos Modificados/Creados

1. **APIS/Controllers/PedidosController.cs** - Agregados métodos para detalle
2. **APIS/Views/Pedidos/DetallePedido.cshtml** - Nueva vista de detalle
3. **APIS/Views/Pedidos/Ped.cshtml** - Modificada para hacer documentos clickeables
4. **APIS/Models/DetallePedidoViewModel.cs** - Nuevo modelo (opcional)

## Consideraciones de Rendimiento

- Los datos se cargan de forma asíncrona usando Axios
- Se implementa manejo de errores robusto
- Estados de carga para mejor experiencia de usuario
- Formateo de números y fechas en el cliente

## Compatibilidad

- Funciona con la estructura de base de datos existente
- Compatible con los stored procedures actuales
- Mantiene la funcionalidad existente intacta
- Diseño responsive para dispositivos móviles
