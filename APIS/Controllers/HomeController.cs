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
        public string Nro_doc { get; set; }

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
            return View();
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
                    var documentos = contexto.Database.SqlQuery<DocumentoCC1ViewModel>(
                        "EXEC buscarPorDocumCC1 @consulta, @cantidad, @usuario",
                        new SqlParameter("@consulta", ""),
                        new SqlParameter("@cantidad", 1000),
                        new SqlParameter("@usuario", User.Identity.Name ?? "")
                    ).ToList()
                    .Where(x => x.co_cli.Trim() == co_cli && x.cli_des.Trim() == cli_des)
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
                return RedirectToAction("Error", new { message = "Error al cargar los detalles del cliente. Verifique que los parámetros sean correctos." });
            }
        }


        [HttpGet]
        public ActionResult DetalleDocumento(string co_tipo_doc, string nro_doc, string co_cli, string cli_des)
        {
            try
            {
                // Limpiar y validar parámetros
                co_tipo_doc = co_tipo_doc?.Trim();
                nro_doc = nro_doc?.Trim();
                co_cli = co_cli?.Trim();
                cli_des = cli_des?.Trim();

                // Validar que los parámetros requeridos no estén vacíos
                if (string.IsNullOrEmpty(co_tipo_doc) || string.IsNullOrEmpty(nro_doc))
                {
                    return RedirectToAction("Index");
                }

                using (var contexto = new A_ZULIA_12Entities())
                {
                    // Obtener información del documento
                    var documento = contexto.Database.SqlQuery<DocumentoCC1ViewModel>(
                        "EXEC buscarPorDocumCC1 @consulta, @cantidad, @usuario",
                        new SqlParameter("@consulta", ""),
                        new SqlParameter("@cantidad", 1000),
                        new SqlParameter("@usuario", User.Identity.Name ?? "")
                    ).ToList()
                    .FirstOrDefault(x => x.co_tipo_doc.Trim() == co_tipo_doc && x.nro_doc.Trim() == nro_doc);

                    if (documento == null)
                    {
                        return RedirectToAction("Index");
                    }

                    // Obtener información del cliente
                    var cliente = new DocumentoCC1ViewModel
                    {
                        co_cli = co_cli ?? documento.co_cli,
                        cli_des = cli_des ?? documento.cli_des,
                        tasa = documento.tasa // Asignar la tasa del documento
                    };

                    // Obtener renglones usando el SP buscarRengCC
                    // Primero obtenemos el código de cliente del documento
                    var codigoCliente = documento.co_cli?.Trim();
                    
                    // Luego buscamos los renglones usando el código de cliente
                    var renglones = contexto.Database.SqlQuery<buscarRengCC_Result>(
                        "EXEC buscarRengCC @consulta",
                        new SqlParameter("@consulta", codigoCliente ?? string.Empty)
                    ).Where(r => r.co_tipo_doc.Trim() == co_tipo_doc && r.nro_doc.Trim() == nro_doc)
                     .ToList();

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
                // Log del error para debugging
                System.Diagnostics.Debug.WriteLine($"Error en DetalleDocumento: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                
                // En producción, redirigir a la página de error
                return RedirectToAction("Error", new { message = "Error al cargar los detalles del documento. Verifique que los parámetros sean correctos." });
            }
        }

        // Usando modelo fuertemente tipado
        public JsonResult buscarPorDocumCC1(string nombre, int cantidad)
        {
            try
            {
                string usuario = User.Identity.Name ?? "";

                using (var contexto = new A_ZULIA_12Entities())
                {
                    var lista = contexto.Database.SqlQuery<DocumentoCC1ViewModel>(
                        "EXEC buscarPorDocumCC1 @consulta, @cantidad, @usuario",
                        new SqlParameter("@consulta", nombre ?? string.Empty),
                        new SqlParameter("@cantidad", cantidad),
                        new SqlParameter("@usuario", usuario)
                    ).ToList();

                    var datos = lista.Select(item => new Dictionary<string, object>
{
    { "co_ven", item.co_ven },
    { "co_cli", item.co_cli },
    { "cli_des", item.cli_des },
    { "co_tipo_doc", item.co_tipo_doc },
    { "Doc", item.Doc },
    { "nro_doc", item.nro_doc },
    { "doc_orig", item.doc_orig },
    //{ "doc_orig", item.nro_orig },
    { "fec_emis", item.fec_emis.ToString("yyyy-MM-dd") },
    { "fec_emis1", item.fec_emis1 },
    { "Dias", item.Dias },
    { "estado_retencion", item.estado_retencion }, // <-- AGREGA ESTA LÍNEA
    { "Ret25", item.Ret25 },
    { "Ret75", item.Ret75 },
    { "saldo", item.saldo },
    { "saldo1", item.saldo1 },
    { "monto_bru", item.monto_bru },
    { "monto_net", item.monto_net },
    { "monto_imp", item.monto_imp },
    { "IVA25", item.IVA25 },
    { "IVA75", item.IVA75 },
   { "IMAGEN", item.Imagen != null ? Convert.ToBase64String(item.Imagen) : null }
}).ToList();

                    var encabezados = datos.Count > 0 ? datos[0].Keys.ToList() : new List<string>();

                    if (datos.Count > 0)
                        return Json(new { encabezados, datos }, JsonRequestBehavior.AllowGet);
                    else
                        return Json(new { encabezados, datos, mensaje = "No se encontraron resultados para la búsqueda." }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { encabezados = new List<string>(), datos = new List<object>(), error = ex.Message }, JsonRequestBehavior.AllowGet);
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
                    var lista = contexto.Database.SqlQuery<DocumentoCC1ViewModel>(
                        "EXEC buscarPorDocumCC1 @consulta, @cantidad, @usuario",
                        new SqlParameter("@consulta", ""),
                        new SqlParameter("@cantidad", 1000),
                        new SqlParameter("@usuario", usuario)
                    ).ToList();

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
                            { "IMAGEN", item.Imagen != null ? Convert.ToBase64String(item.Imagen) : null }
                        }).ToList();

                    if (datos.Count > 0)
                        return Json(datos, JsonRequestBehavior.AllowGet);
                    else
                        return Json(new { mensaje = "No se encontraron datos relacionados." }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
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
                return Json(new { success = false, error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult Error(string message = null)
        {
            ViewBag.ErrorMessage = message ?? "Ha ocurrido un error inesperado.";
            return View();
        }
    }
}