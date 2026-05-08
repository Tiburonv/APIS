(function (window) {
    'use strict';

    var configCache = null;
    var configPromise = null;
    var logoPdfCache = null;
    var logoPdfCacheSrc = null;

    function textoPdfMayus(texto) {
        if (texto == null || texto === '') return '';
        return texto.toString().toLocaleUpperCase('es');
    }

    function detectFormatFromDataUrl(dataUrl) {
        if (!dataUrl) return 'PNG';
        var lower = dataUrl.substring(0, 30).toLowerCase();
        if (lower.indexOf('image/jpeg') >= 0 || lower.indexOf('image/jpg') >= 0) return 'JPEG';
        return 'PNG';
    }

    function prepararLogoParaPdf(logoDataUrl) {
        if (!logoDataUrl) {
            return Promise.resolve(null);
        }
        if (logoPdfCacheSrc === logoDataUrl && logoPdfCache) {
            return Promise.resolve(logoPdfCache);
        }

        return new Promise(function (resolve) {
            var img = new Image();
            img.onload = function () {
                var w = img.naturalWidth || img.width;
                var h = img.naturalHeight || img.height;
                if (!w || !h) {
                    resolve({
                        dataUrl: logoDataUrl,
                        format: detectFormatFromDataUrl(logoDataUrl),
                        width: null,
                        height: null
                    });
                    return;
                }

                var canvas = document.createElement('canvas');
                canvas.width = w;
                canvas.height = h;
                var ctx = canvas.getContext('2d');
                ctx.fillStyle = '#FFFFFF';
                ctx.fillRect(0, 0, w, h);
                ctx.drawImage(img, 0, 0, w, h);

                var preparado = {
                    dataUrl: canvas.toDataURL('image/jpeg', 0.92),
                    format: 'JPEG',
                    width: w,
                    height: h
                };
                logoPdfCacheSrc = logoDataUrl;
                logoPdfCache = preparado;
                resolve(preparado);
            };
            img.onerror = function () {
                resolve({
                    dataUrl: logoDataUrl,
                    format: detectFormatFromDataUrl(logoDataUrl),
                    width: null,
                    height: null
                });
            };
            img.src = logoDataUrl;
        });
    }

    function calcularTamanoLogo(preparado, opciones) {
        var maxW = opciones.logoWidth != null ? opciones.logoWidth : 55;
        var maxH = opciones.logoMaxHeight != null ? opciones.logoMaxHeight : 40;

        if (opciones.logoWidth != null && opciones.logoHeight != null) {
            return { w: opciones.logoWidth, h: opciones.logoHeight };
        }

        if (!preparado || !preparado.width || !preparado.height) {
            return { w: maxW, h: opciones.logoHeight != null ? opciones.logoHeight : 28 };
        }

        var ratio = preparado.width / preparado.height;
        var w = maxW;
        var h = w / ratio;

        if (h > maxH) {
            h = maxH;
            w = h * ratio;
        }

        if (opciones.logoHeight != null && opciones.logoWidth == null) {
            h = opciones.logoHeight;
            w = h * ratio;
        }

        return { w: w, h: h };
    }

    window.obtenerConfigEmpresa = function () {
        if (configCache) {
            return Promise.resolve(configCache);
        }
        if (configPromise) {
            return configPromise;
        }

        configPromise = fetch('/ConfigEmpresa/Obtener')
            .then(function (response) {
                if (!response.ok) {
                    throw new Error('No se pudo cargar la configuracion');
                }
                return response.json();
            })
            .then(function (data) {
                configCache = data || {};
                if (configCache.logoBase64) {
                    window.logoBase64 = configCache.logoBase64;
                }
                return configCache;
            })
            .catch(function () {
                configCache = {
                    rif: '',
                    direccion: '',
                    telefono: '',
                    logoBase64: window.logoBase64 || null,
                    logoMimeType: null
                };
                return configCache;
            });

        return configPromise;
    };

    window.limpiarCacheConfigEmpresa = function () {
        configCache = null;
        configPromise = null;
        logoPdfCache = null;
        logoPdfCacheSrc = null;
    };

    window.agregarEncabezadoPdf = function (doc, opciones) {
        opciones = opciones || {};

        return window.obtenerConfigEmpresa().then(function (cfg) {
            var marginLeft = opciones.marginLeft != null ? opciones.marginLeft : 10;
            var yLogo = opciones.y != null ? opciones.y : 8;
            var mostrarDatos = opciones.mostrarDatos !== false;
            var logo = (cfg && cfg.logoBase64) ? cfg.logoBase64 : window.logoBase64;

            return prepararLogoParaPdf(logo).then(function (preparado) {
                var tamano = calcularTamanoLogo(preparado, opciones);
                var infoX = opciones.infoX != null ? opciones.infoX : (marginLeft + tamano.w + 5);

                if (preparado && preparado.dataUrl) {
                    try {
                        doc.addImage(
                            preparado.dataUrl,
                            preparado.format,
                            marginLeft,
                            yLogo,
                            tamano.w,
                            tamano.h,
                            undefined,
                            'FAST'
                        );
                    } catch (e) {
                        console.warn('No se pudo agregar el logo al PDF', e);
                    }
                }

                var lineSpacing = opciones.lineSpacing || 5;
                var fontSize = opciones.fontSize || 9;
                var maxW = opciones.direccionMaxWidth || 90;
                var lineas = [];

                if (mostrarDatos) {
                    if (cfg.rif) {
                        lineas.push(textoPdfMayus('RIF: ' + cfg.rif));
                    }
                    if (cfg.direccion) {
                        lineas = lineas.concat(doc.splitTextToSize(textoPdfMayus(cfg.direccion), maxW));
                    }
                    if (cfg.telefono) {
                        lineas.push(textoPdfMayus('Tel: ' + cfg.telefono));
                    }
                }

                var textY = yLogo + 4;
                if (lineas.length > 0) {
                    doc.setFontSize(fontSize);
                    var blockHeight = (lineas.length - 1) * lineSpacing + fontSize * 0.35;
                    var logoCenterY = yLogo + (tamano.h / 2);
                    textY = logoCenterY - (blockHeight / 2) + (fontSize * 0.28);

                    for (var i = 0; i < lineas.length; i++) {
                        doc.text(lineas[i], infoX, textY + (i * lineSpacing));
                    }
                }

                var lastTextY = lineas.length > 0
                    ? textY + ((lineas.length - 1) * lineSpacing) + 2
                    : yLogo;
                var startY = Math.max(yLogo + tamano.h + 4, lastTextY + 2);
                return startY;
            });
        });
    };
})(window);
