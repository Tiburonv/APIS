using APIS.ADO;
using APIS.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;

namespace APIS.Controllers
{
    public class HomeController : Controller
    {

        public ActionResult Index(string co_cli, string cli_des)
        {
            // Limpiar y validar parámetros
            co_cli = co_cli?.Trim();
            cli_des = cli_des?.Trim();
            
            ViewBag.co_cli = co_cli;
            ViewBag.cli_des = cli_des;
            
            // Si hay parámetros, agregar un flag para indicar que debe cargar automáticamente
            if (!string.IsNullOrEmpty(co_cli) && !string.IsNullOrEmpty(cli_des))
            {
                ViewBag.CargarAutomaticamente = true;
            }
            
            return View();
        }

        public ActionResult Buscar()
        {
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult DetalleCliente(string co_cli, string cli_des)
        {
            try
            {
                // Limpiar y validar parámetros
                co_cli = co_cli?.Trim();
                cli_des = cli_des?.Trim();

                // Validar que los parámetros requeridos no estén vacíos
                if (string.IsNullOrEmpty(co_cli) || string.IsNullOrEmpty(cli_des))
                {
                    return RedirectToAction("Index");
                }

                using (var contexto = new A_ZULIA_12Entities())
                {
                    // Obtener todos los documentos del cliente
                    var documentos = ConsultasCuentasCobrar.BuscarPorDocumCC1(
                        contexto,
                        string.Empty,
                        1000,
                        User.Identity.Name ?? string.Empty,
                        incluirImagen: false)
                    .Where(x =>
                        string.Equals((x.co_cli ?? string.Empty).Trim(), co_cli, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals((x.cli_des ?? string.Empty).Trim(), cli_des, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                    if (!documentos.Any())
                    {
                        return RedirectToAction("Index");
                    }

                    // Obtener información del cliente del primer documento
                    var primerDoc = documentos.First();
                    var cliente = new ClienteInfo
                    {
                        Codigo = co_cli,
                        Nombre = cli_des,
                        Direccion = "", // Se puede obtener de otra tabla si es necesario
                        Telefono = "", // Se puede obtener de otra tabla si es necesario
                        Email = "" // Se puede obtener de otra tabla si es necesario
                    };

                    // Calcular totales
                    var totalSaldo = documentos.Sum(x => x.saldo);
                    var totalMontoBruto = documentos.Sum(x => x.monto_bru);
                    var totalImpuestos = documentos.Sum(x => x.monto_imp);

                    var viewModel = new DetalleClienteViewModel
                    {
                        Cliente = cliente,
                        Documentos = documentos,
                        TotalSaldo = totalSaldo,
                        TotalMontoBruto = totalMontoBruto,
                        TotalImpuestos = totalImpuestos
                    };

                    return View(viewModel);
                }
            }
            catch (Exception ex)
            {
                // Log del error para debugging
                System.Diagnostics.Debug.WriteLine($"Error en DetalleCliente: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                
                // En producción, redirigir a la página de error
                var message = "Error al cargar los detalles del cliente. Verifique que los parámetros sean correctos.";
                if (HttpContext != null && HttpContext.IsDebuggingEnabled)
                {
                    message = $"{message} Detalle técnico: {ex.Message}";
                }

                return RedirectToAction("Error", new { message });
            }
        }


        [HttpGet]
        public ActionResult DetalleDocumento(string co_tipo_doc, string nro_doc, string co_cli, string cli_des)
        {
            try
            {
                co_tipo_doc = co_tipo_doc?.Trim();
                nro_doc = nro_doc?.Trim();
                co_cli = co_cli?.Trim();
                cli_des = cli_des?.Trim();

                if (string.IsNullOrEmpty(co_tipo_doc) || string.IsNullOrEmpty(nro_doc))
                {
                    return RedirectToAction("Index", new { co_cli, cli_des });
                }

                using (var contexto = new A_ZULIA_12Entities())
                {
                    var usuario = User?.Identity?.Name ?? string.Empty;

                    // buscarPorDocumCC1 filtra por cliente, no por "N/CR-000...". Buscar por co_cli.
                    var documento = BuscarDocumentoEnCuentasPorCobrar(contexto, co_tipo_doc, nro_doc, co_cli, usuario);

                    if (documento == null)
                    {
                        TempData["MensajeError"] = "No se encontró el documento solicitado.";
                        return RedirectToAction("Index", new { co_cli, cli_des });
                    }

                    var codigoCliente = (co_cli ?? documento.co_cli ?? string.Empty).Trim();
                    var nroDocInterno = ResolverNroDocInterno(contexto, co_tipo_doc, nro_doc, codigoCliente);

                    var cliente = new DocumentoCC1ViewModel
                    {
                        co_cli = codigoCliente,
                        cli_des = cli_des ?? documento.cli_des,
                        tasa = documento.tasa
                    };

                    var renglones = ObtenerRenglonesDocumento(contexto, co_tipo_doc, nro_doc, nroDocInterno, codigoCliente);

                    var viewModel = new DetalleDocumentoViewModel
                    {
                        Documento = documento,
                        Cliente = cliente,
                        Renglones = renglones
                    };

                    return View(viewModel);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en DetalleDocumento: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");

                var message = "Error al cargar los detalles del documento. Verifique que los parámetros sean correctos.";
                if (HttpContext != null && HttpContext.IsDebuggingEnabled)
                {
                    message = $"{message} Detalle técnico: {ex.Message}";
                }

                return RedirectToAction("Error", new { message });
            }
        }

        private static DocumentoCC1ViewModel BuscarDocumentoEnCuentasPorCobrar(
            A_ZULIA_12Entities contexto,
            string co_tipo_doc,
            string nro_doc,
            string co_cli,
            string usuario)
        {
            var consultas = new List<string>();
            if (!string.IsNullOrWhiteSpace(co_cli))
            {
                consultas.Add(co_cli.Trim());
            }
            consultas.Add(string.Empty);

            foreach (var consulta in consultas.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var lista = ConsultasCuentasCobrar.BuscarPorDocumCC1(
                    contexto,
                    consulta ?? string.Empty,
                    1000,
                    usuario ?? string.Empty,
                    incluirImagen: false);

                var documento = lista.FirstOrDefault(x => CoincideDocumento(x, co_tipo_doc, nro_doc, co_cli));
                if (documento != null)
                {
                    return documento;
                }
            }

            return null;
        }

        private static bool CoincideDocumento(DocumentoCC1ViewModel documento, string co_tipo_doc, string nro_doc, string co_cli)
        {
            if (!string.Equals((documento.co_tipo_doc ?? string.Empty).Trim(), co_tipo_doc, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!string.Equals((documento.nro_doc ?? string.Empty).Trim(), nro_doc, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(co_cli) &&
                !string.Equals((documento.co_cli ?? string.Empty).Trim(), co_cli.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Para N/CR la grilla muestra nro_orig; buscarRengCC necesita el nro_doc real del documento.
        /// </summary>
        private static string ResolverNroDocInterno(
            A_ZULIA_12Entities contexto,
            string co_tipo_doc,
            string nro_doc,
            string co_cli)
        {
            var sql = @"SELECT TOP 1 LTRIM(RTRIM(a.nro_doc))
                        FROM saDocumentoVenta a
                        WHERE a.anulado = 0
                          AND LTRIM(RTRIM(a.co_tipo_doc)) = @tipo
                          AND (LTRIM(RTRIM(a.nro_doc)) = @nro OR LTRIM(RTRIM(ISNULL(a.nro_orig, ''))) = @nro)";

            var parametros = new List<SqlParameter>
            {
                new SqlParameter("@tipo", co_tipo_doc),
                new SqlParameter("@nro", nro_doc)
            };

            if (!string.IsNullOrWhiteSpace(co_cli))
            {
                sql += " AND LTRIM(RTRIM(a.co_cli)) = @cli";
                parametros.Add(new SqlParameter("@cli", co_cli));
            }

            var nroInterno = contexto.Database.SqlQuery<string>(sql, parametros.ToArray()).FirstOrDefault();
            return string.IsNullOrWhiteSpace(nroInterno) ? nro_doc : nroInterno.Trim();
        }

        private static List<buscarRengCC_Result> ObtenerRenglonesDocumento(
            A_ZULIA_12Entities contexto,
            string co_tipo_doc,
            string nro_doc,
            string nroDocInterno,
            string co_cli)
        {
            var consultasRenglon = new List<string>();
            if (!string.IsNullOrWhiteSpace(co_tipo_doc) && !string.IsNullOrWhiteSpace(nroDocInterno))
            {
                consultasRenglon.Add($"{co_tipo_doc}-{nroDocInterno}");
            }
            if (!string.IsNullOrWhiteSpace(co_tipo_doc) && !string.IsNullOrWhiteSpace(nro_doc))
            {
                consultasRenglon.Add($"{co_tipo_doc}-{nro_doc}");
            }
            if (!string.IsNullOrWhiteSpace(co_cli))
            {
                consultasRenglon.Add(co_cli);
            }

            foreach (var consulta in consultasRenglon.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var renglones = contexto.Database.SqlQuery<buscarRengCC_Result>(
                    "EXEC buscarRengCC @consulta",
                    new SqlParameter("@consulta", consulta)
                ).Where(r => CoincideRenglon(r, co_tipo_doc, nro_doc, nroDocInterno))
                 .ToList();

                if (renglones.Any())
                {
                    return renglones;
                }
            }

            return new List<buscarRengCC_Result>();
        }

        private static bool CoincideRenglon(buscarRengCC_Result renglon, string co_tipo_doc, string nro_doc, string nroDocInterno)
        {
            if (!string.Equals((renglon.co_tipo_doc ?? string.Empty).Trim(), co_tipo_doc, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var nroRenglon = (renglon.nro_doc ?? string.Empty).Trim();
            var docNumRenglon = (renglon.doc_num ?? string.Empty).Trim();

            return CoincideNroDocumento(nroRenglon, nroDocInterno)
                || CoincideNroDocumento(nroRenglon, nro_doc)
                || CoincideNroDocumento(docNumRenglon, nroDocInterno)
                || CoincideNroDocumento(docNumRenglon, nro_doc);
        }

        private static bool CoincideNroDocumento(string valorDb, string valorEntrada)
        {
            var db = (valorDb ?? string.Empty).Trim();
            var input = (valorEntrada ?? string.Empty).Trim();

            if (string.IsNullOrEmpty(db) || string.IsNullOrEmpty(input))
            {
                return false;
            }

            if (string.Equals(db, input, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return string.Equals(NormalizarNroDocumento(db), NormalizarNroDocumento(input), StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizarNroDocumento(string valor)
        {
            var limpio = (valor ?? string.Empty).Trim();
            if (limpio.Length == 0)
            {
                return string.Empty;
            }

            var sinCeros = limpio.TrimStart('0');
            return sinCeros.Length == 0 ? "0" : sinCeros;
        }

        // Usando modelo fuertemente tipado con manejo de respuestas grandes
        public ActionResult buscarPorDocumCC1(string nombre, int cantidad = 50, bool incluirImagen = false)
        {
            try
            {
                string usuario = User?.Identity?.Name ?? "";

                using (var contexto = new A_ZULIA_12Entities())
                {
                    System.Diagnostics.Debug.WriteLine($"Buscando documentos con: nombre='{nombre}', usuario='{usuario}', incluirImagen={incluirImagen}");
                    
                    int cantidadLimite = cantidad > 0 ? Math.Min(cantidad, 1000) : 100;
                    var resultados = ConsultasCuentasCobrar.BuscarPorDocumCC1(
                        contexto,
                        nombre ?? string.Empty,
                        cantidadLimite,
                        usuario,
                        incluirImagen)
                    .Select(item => new 
                    {
                        // Solo seleccionar los campos necesarios para la vista inicial
                        item.co_ven,
                        item.co_cli,
                        item.cli_des,
                        item.co_tipo_doc,
                        item.Doc,
                        item.nro_doc,
                        item.doc_orig,
                        fec_emis = item.fec_emis.ToString("yyyy-MM-dd"),
                        item.fec_emis1,
                        item.Dias,
                        item.estado_retencion,
                        item.Ret25,
                        item.Ret75,
                        item.saldo,
                        item.saldo1,
                        item.monto_bru,
                        item.monto_net,
                        item.monto_imp,
                        item.IVA25,
                        item.IVA75,
                        item.tiene_imagen,
                        Imagen = incluirImagen && item.Imagen != null ? Convert.ToBase64String(item.Imagen) : null,
                        // La imagen se convierte a base64 para permitir visualización
                    }).ToList();

                    System.Diagnostics.Debug.WriteLine($"Se encontraron {resultados.Count} resultados");

                    // Configurar el JsonResult para manejar respuestas grandes
                    var jsonResult = new JsonResult
                    {
                        Data = new { 
                            success = true, 
                            encabezados = resultados.FirstOrDefault()?.GetType().GetProperties().Select(p => p.Name).ToList() ?? new List<string>(),
                            datos = resultados,
                            total = resultados.Count,
                            mensaje = resultados.Count == 0 ? "No se encontraron resultados para la búsqueda." : null
                        },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        MaxJsonLength = int.MaxValue, // Aumentar el límite de tamaño
                        RecursionLimit = 100 // Aumentar el límite de recursión si es necesario
                    };

                    return jsonResult;
            }
            }
            catch (SqlException sqlEx)
            {
                System.Diagnostics.Debug.WriteLine($"Error SQL en buscarPorDocumCC1: {sqlEx.Message}");
                System.Diagnostics.Debug.WriteLine($"Número de error SQL: {sqlEx.Number}");
                System.Diagnostics.Debug.WriteLine($"Procedimiento: {sqlEx.Procedure}");
                System.Diagnostics.Debug.WriteLine($"Línea: {sqlEx.LineNumber}");
                
                return new JsonResult
                {
                    Data = new 
                    { 
                        success = false, 
                        error = "Error de base de datos al buscar documentos.",
                        // El detalle tecnico solo se expone en modo Debug (desarrollo)
                        details = HttpContext.IsDebuggingEnabled ? sqlEx.Message : null,
                        errorNumber = HttpContext.IsDebuggingEnabled ? (int?)sqlEx.Number : null,
                        procedure = HttpContext.IsDebuggingEnabled ? sqlEx.Procedure : null,
                        lineNumber = HttpContext.IsDebuggingEnabled ? (int?)sqlEx.LineNumber : null
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    MaxJsonLength = int.MaxValue
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en buscarPorDocumCC1: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Excepción interna: {ex.InnerException.Message}");
                    System.Diagnostics.Debug.WriteLine($"Stack Trace interno: {ex.InnerException.StackTrace}");
                }
                
                return new JsonResult
                {
                    Data = new 
                    { 
                        success = false, 
                        error = "Error al procesar la solicitud.",
                        // El detalle tecnico solo se expone en modo Debug (desarrollo)
                        details = HttpContext.IsDebuggingEnabled ? ex.Message : null,
                        stackTrace = HttpContext.IsDebuggingEnabled ? ex.StackTrace : null,
                        innerException = HttpContext.IsDebuggingEnabled ? ex.InnerException?.Message : null
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    MaxJsonLength = int.MaxValue
                };
            }
        }

        [HttpGet]
        public JsonResult ObtenerDatosRelacionados(string co_cli, string cli_des)
        {
            try
            {
                string usuario = User.Identity.Name ?? "";

                using (var contexto = new A_ZULIA_12Entities())
                {
                    var lista = ConsultasCuentasCobrar.BuscarPorDocumCC1(
                        contexto,
                        string.Empty,
                        1000,
                        usuario,
                        incluirImagen: false);

                    var datos = lista
                        .Where(x => x.co_cli == co_cli && x.cli_des == cli_des)
                        .Select(item => new Dictionary<string, object>
                        {
                            { "co_ven", item.co_ven },
                            { "co_cli", item.co_cli },
                            { "cli_des", item.cli_des },
                            { "co_tipo_doc", item.co_tipo_doc },
                            { "Doc", item.Doc },
                            { "nro_doc", item.nro_doc },
                            { "doc_orig", item.doc_orig },
                            { "fec_emis", item.fec_emis.ToString("yyyy-MM-dd") },
                            { "fec_emis1", item.fec_emis1 },
                            { "Dias", item.Dias },
                            { "estado_retencion", item.estado_retencion },
                            { "Ret25", item.Ret25 },
                            { "Ret75", item.Ret75 },
                            { "saldo", item.saldo },
                            { "saldo1", item.saldo1 },
                            { "monto_bru", item.monto_bru },
                            { "monto_net", item.monto_net },
                            { "monto_imp", item.monto_imp },
                            { "IVA25", item.IVA25 },
                            { "IMAGEN", item.Imagen != null ? Convert.ToBase64String(item.Imagen) : null },
                            { "tiene_imagen", item.tiene_imagen }
                        }).ToList();

                    if (datos.Count > 0)
                        return Json(datos, JsonRequestBehavior.AllowGet);
                    else
                        return Json(new { mensaje = "No se encontraron datos relacionados." }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en ObtenerDatosRelacionados: {ex.ToString()}");
                return Json(new { error = "Error al obtener los datos relacionados." }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult ObtenerRenglonesDocumento(string co_tipo_doc, string nro_doc)
        {
            try
            {
                using (var contexto = new A_ZULIA_12Entities())
                {
                    // Ejecutar el procedimiento almacenado para obtener los renglones del documento
                    var renglones = contexto.Database.SqlQuery<RengDocumentoViewModel>(
                        "EXEC buscarRengCC @consulta",
                        new SqlParameter("@consulta", $"{co_tipo_doc.Trim()}-{nro_doc.Trim()}")
                    ).ToList();

                    return Json(new { success = true, renglones }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en ObtenerRenglonesDocumento: {ex.ToString()}");
                return Json(new { success = false, error = "Error al obtener los renglones del documento." }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult ObtenerImagenDocumento(string co_tipo_doc, string nro_doc, string co_cli = null)
        {
            try
            {
                co_tipo_doc = co_tipo_doc?.Trim();
                nro_doc = nro_doc?.Trim();
                co_cli = co_cli?.Trim();

                if (string.IsNullOrEmpty(co_tipo_doc) || string.IsNullOrEmpty(nro_doc))
                {
                    return Json(new { success = false, message = "Parámetros inválidos." }, JsonRequestBehavior.AllowGet);
                }

                var usuario = User?.Identity?.Name ?? string.Empty;

                using (var contexto = new A_ZULIA_12Entities())
                {
                    var documento = BuscarDocumentoEnCuentasPorCobrar(contexto, co_tipo_doc, nro_doc, co_cli, usuario);

                    var imagenBytes = documento?.Imagen;
                    if (imagenBytes == null || imagenBytes.Length == 0)
                    {
                        imagenBytes = ObtenerImagenBinariaPorDocumento(contexto, co_tipo_doc, nro_doc, co_cli);
                    }

                    if (imagenBytes == null || imagenBytes.Length == 0)
                    {
                        return Json(new { success = false, message = "No hay imagen disponible para este documento." }, JsonRequestBehavior.AllowGet);
                    }

                    return Json(new
                    {
                        success = true,
                        imagen = Convert.ToBase64String(imagenBytes)
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en ObtenerImagenDocumento: {ex.ToString()}");
                return Json(new { success = false, message = "Error al obtener la imagen del documento." }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Fallback directo a fnFactImagen usando el nro_doc interno (importante para N/CR).
        /// </summary>
        private static byte[] ObtenerImagenBinariaPorDocumento(
            A_ZULIA_12Entities contexto,
            string co_tipo_doc,
            string nro_doc,
            string co_cli)
        {
            var nroDocInterno = ResolverNroDocInterno(contexto, co_tipo_doc, nro_doc, co_cli);

            var sql = @"SELECT TOP 1 f.picture
                        FROM saDocumentoVenta a
                        LEFT JOIN dbo.fnFactImagen() f ON a.nro_doc = f.doc_num
                        WHERE a.anulado = 0
                          AND LTRIM(RTRIM(a.co_tipo_doc)) = @tipo
                          AND LTRIM(RTRIM(a.nro_doc)) = @nroInterno
                          AND f.picture IS NOT NULL
                          AND DATALENGTH(f.picture) > 0";

            var parametros = new List<SqlParameter>
            {
                new SqlParameter("@tipo", co_tipo_doc),
                new SqlParameter("@nroInterno", nroDocInterno)
            };

            if (!string.IsNullOrWhiteSpace(co_cli))
            {
                sql += " AND LTRIM(RTRIM(a.co_cli)) = @cli";
                parametros.Add(new SqlParameter("@cli", co_cli));
            }

            return contexto.Database.SqlQuery<ImagenDocumentoRow>(sql, parametros.ToArray())
                .Select(x => x.picture)
                .FirstOrDefault();
        }

        private class ImagenDocumentoRow
        {
            public byte[] picture { get; set; }
        }

        [HttpGet]
        public ActionResult Error(string message = null)
        {
            ViewBag.ErrorMessage = message ?? "Ha ocurrido un error inesperado.";
            return View();
        }
    }
}