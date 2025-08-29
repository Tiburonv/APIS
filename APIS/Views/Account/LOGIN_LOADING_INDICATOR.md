# Indicador de Carga - Login

## Descripción
Se ha implementado un indicador de carga en el formulario de inicio de sesión para mejorar la experiencia del usuario y prevenir múltiples envíos del formulario.

## Características implementadas:

### 1. Prevención de múltiples clics
- El botón se deshabilita inmediatamente al hacer clic
- Se agrega la clase `btn-loading` que previene eventos del mouse

### 2. Retroalimentación visual
- Spinner animado de Bootstrap 5
- Texto cambia de "Entrar" a "Iniciando sesión..."
- Opacidad del botón reducida durante la carga

### 3. Comportamiento
- **Clic en "Entrar"**: Botón deshabilitado + spinner visible
- **Login exitoso**: Redirección a la página principal
- **Login fallido**: Página recargada, botón vuelve a estado normal

## Archivos modificados:
- `/Views/Account/Login.cshtml`

## Tecnologías utilizadas:
- Bootstrap 5.3.0 (spinner-border)
- jQuery (manejo de eventos)
- CSS personalizado

## Código implementado:

### CSS:
```css
.spinner-border {
    width: 1rem;
    height: 1rem;
    margin-right: 0.5rem;
    vertical-align: middle;
}

.btn-loading {
    pointer-events: none;
    opacity: 0.8;
}
```

### JavaScript:
```javascript
$('#loginForm').on('submit', function(e) {
    var button = $('#loginButton');
    var buttonText = $('#buttonText');
    var loadingSpinner = $('#loadingSpinner');
    
    button.prop('disabled', true).addClass('btn-loading');
    buttonText.hide();
    loadingSpinner.show();
});
```

Esta implementación garantiza una mejor experiencia de usuario al proporcionar retroalimentación inmediata y prevenir envíos duplicados del formulario.