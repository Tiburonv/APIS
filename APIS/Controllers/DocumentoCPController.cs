//using APIS.ADO;
//using APIS.Models;
//using System;
//using System.Collections.Generic;
//using System.Data.SqlClient;
//using System.Linq;
//using System.Web.Mvc;
//using System.Data.Entity;

//public class DocumentoCPController : Controller
//{
//    public string Nro_doc { get; set; }

//    protected override void OnActionExecuting(ActionExecutingContext filterContext)
//    {
//        var usuario = Session["Usuario"] as string;
//        if (usuario != "04" && usuario != "05")
//        {
//            filterContext.Result = RedirectToAction("Index", "Home");
//            return;
//        }
//        base.OnActionExecuting(filterContext);
//    }

//    public ActionResult Index(string co_prov, string prov_des)
//    {
//        ViewBag.co_cli = co_prov;
//        ViewBag.cli_des = prov_des;
//        return View("Prov");
//    }

//    public ActionResult Buscar()
//    {
//        return View();
//    }

//    public JsonResult buscarPorDocumCP(string nombre, int cantidad)
//    {
//        try
//        {
//            string usuario = User.Identity.Name ?? "";

//            using (var contexto = new A_ZULIA_12Entities())
//            {
//                var lista = contexto.Database.SqlQuery<DocumentoCPViewModel>(
//                    "EXEC buscarPorDocumCP @consulta, @cantidad, @usuario",
//                    new SqlParameter("@consulta", nombre ?? string.Empty),
//                    new SqlParameter("@cantidad", cantidad),
//                    new SqlParameter("@usuario", usuario)
//                ).ToList();

//                var datos = lista.Select(item => new Dictionary<string, object>
//                {
//                    { "co_prov", item.co_prov },
//                    { "prov_des", item.prov_des },
//                    { "co_tipo_doc", item.co_tipo_doc },
//                    { "Doc", item.Doc },
//                    { "nro_doc", item.nro_doc },
//                    { "doc_orig", item.doc_orig },
//                    { "fec_emis", item.fec_emis.ToString("yyyy-MM-dd") },
//                    { "fec_emis1", item.fec_emis1 },
//                    { "Dias", item.Dias },
//                    { "estado_retencion", item.estado_retencion },
//                    { "Ret25", item.Ret25 },
//                    { "Ret75", item.Ret75 },
//                    { "saldo", item.saldo },
//                    { "monto_bru", item.monto_bru },
//                    { "monto_net", item.monto_net },
//                    { "monto_imp", item.monto_imp },
//                    { "IVA25", item.IVA25 },
//                    { "IVA75", item.IVA75 },
//                     { "nro_fact", item.nro_fact }
//                }).ToList();

//                var encabezados = datos.Count > 0 ? datos[0].Keys.ToList() : new List<string>();

//                if (datos.Count > 0)
//                    return Json(new { encabezados, datos }, JsonRequestBehavior.AllowGet);
//                else
//                    return Json(new { encabezados, datos, mensaje = "No se encontraron resultados para la búsqueda." }, JsonRequestBehavior.AllowGet);
//            }
//        }
//        catch (Exception ex)
//        {
//            return Json(new { mensaje = "Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
//        }

//        [HttpGet]
//        public ActionResult DetalleDocumento(string co_tipo_doc, string nro_doc, string co_prov, string prov_des)
//        {
//            try
//            {
//                // Limpiar y validar parámetros
//                co_tipo_doc = co_tipo_doc?.Trim();
//                nro_doc = nro_doc?.Trim();
//                co_prov = co_prov?.Trim();
//                prov_des = prov_des?.Trim();

//                // Validar que los parámetros requeridos no estén vacíos
//                if (string.IsNullOrEmpty(co_tipo_doc) || string.IsNullOrEmpty(nro_doc))
//                {
//                    return RedirectToAction("Index");
//                }

//                using (var contexto = new A_ZULIA_12Entities())
//                {
//                    // Obtener información del documento
//                    var documento = contexto.Database.SqlQuery<DocumentoCPViewModel>(
//                        "EXEC buscarPorDocumCP @consulta, @cantidad, @usuario",
//                        new SqlParameter("@consulta", ""),
//                        new SqlParameter("@cantidad", 1000),
//                        new SqlParameter("@usuario", User.Identity.Name ?? "")
//                    ).ToList()
//                    .FirstOrDefault(x => x.co_tipo_doc.Trim() == co_tipo_doc && x.nro_doc.Trim() == nro_doc);

//                    if (documento == null)
//                    {
//                        return RedirectToAction("Index");
//                    }

//                    // Obtener información del proveedor
//                    var proveedor = new DocumentoCPViewModel
//                    {
//                        co_prov = co_prov ?? documento.co_prov,
//                        prov_des = prov_des ?? documento.prov_des,
//                        tasa = documento.tasa // Asignar la tasa del documento
//                    };

//                    // Obtener renglones usando el SP buscarRengCP1
//                    var renglones = contexto.Database.SqlQuery<buscarRengCP1_Result>(
//                        "EXEC buscarRengCP1 @consulta",
//                        new SqlParameter("@consulta", documento.co_prov ?? string.Empty)
//                    ).Where(r => r.co_tipo_doc.Trim() == co_tipo_doc && r.nro_doc.Trim() == nro_doc)
//                     .ToList();

//                    var viewModel = new DetalleDocumentoCPViewModel
//                    {
//                        Documento = documento,
//                        Proveedor = proveedor,
//                        Renglones = renglones
//                    };

//                    return View(viewModel);
//                }
//            }
//            catch (Exception ex)
//            {
//                // Log del error para debugging
//                System.Diagnostics.Debug.WriteLine($"Error en DetalleDocumento: {ex.Message}");
//                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");

//                // En producción, redirigir a la página de error
//                return RedirectToAction("Error", new { message = "Error al cargar los detalles del documento. Verifique que los parámetros sean correctos." });
//            }
//        }

//        [HttpGet]
//        public ActionResult Error(string message = null)
//        {
//            ViewBag.ErrorMessage = message ?? "Ha ocurrido un error inesperado.";
//            return View();
//        }
//    }
//}

using APIS.ADO;
using APIS.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;

public class DocumentoCPController : Controller
{
    public string Nro_doc { get; set; }

    protected override void OnActionExecuting(ActionExecutingContext filterContext)
    {
        var usuario = Session["Usuario"] as string;
        if (usuario != "04" && usuario != "05")
        {
            filterContext.Result = RedirectToAction("Index", "Home");
            return;
        }
        base.OnActionExecuting(filterContext);
    }

    public ActionResult Index(string co_prov, string prov_des)
    {
        ViewBag.co_prov = co_prov;
        ViewBag.prov_des = prov_des;
        return View("Prov");
    }

    public ActionResult Buscar()
    {
        return View();
    }

    public JsonResult buscarPorDocumCP(string nombre, int cantidad)
    {
        try
        {
            string usuario = User.Identity.Name ?? "";

            using (var contexto = new A_ZULIA_12Entities())
            {
                var lista = contexto.Database.SqlQuery<DocumentoCPViewModel>(
                    "EXEC buscarPorDocumCP @consulta, @cantidad, @usuario",
                    new SqlParameter("@consulta", nombre ?? string.Empty),
                    new SqlParameter("@cantidad", cantidad),
                    new SqlParameter("@usuario", usuario)
                ).ToList();

                var datos = lista.Select(item => new Dictionary<string, object>
                {
                    { "co_prov", item.co_prov },
                    { "prov_des", item.prov_des },
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
                    { "monto_bru", item.monto_bru },
                    { "monto_net", item.monto_net },
                    { "monto_imp", item.monto_imp },
                    { "IVA25", item.IVA25 },
                    { "IVA75", item.IVA75 },
                    { "nro_fact", item.nro_fact }
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
            return Json(new { mensaje = "Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
        }
    }

    [HttpGet]
    public ActionResult DetalleDocumento(string co_tipo_doc, string nro_doc, string co_prov, string prov_des)
    {
        try
        {
            // Limpiar y validar parámetros
            co_tipo_doc = co_tipo_doc?.Trim();
            nro_doc = nro_doc?.Trim();
            co_prov = co_prov?.Trim();
            prov_des = prov_des?.Trim();

            // Validar que los parámetros requeridos no estén vacíos
            if (string.IsNullOrEmpty(co_tipo_doc) || string.IsNullOrEmpty(nro_doc))
            {
                return RedirectToAction("Index");
            }

            using (var contexto = new A_ZULIA_12Entities())
            {
                // Obtener información del documento
                var documento = contexto.Database.SqlQuery<DocumentoCPViewModel>(
                    "EXEC buscarPorDocumCP @consulta, @cantidad, @usuario",
                    new SqlParameter("@consulta", ""),
                    new SqlParameter("@cantidad", 1000),
                    new SqlParameter("@usuario", User.Identity.Name ?? "")
                ).ToList()
                .FirstOrDefault(x => x.co_tipo_doc.Trim() == co_tipo_doc && x.nro_doc.Trim() == nro_doc);

                if (documento == null)
                {
                    return RedirectToAction("Index");
                }

                // Obtener información del proveedor
                var proveedor = new DocumentoCPViewModel
                {
                    co_prov = co_prov ?? documento.co_prov,
                    prov_des = prov_des ?? documento.prov_des,
                    tasa = documento.tasa // Asignar la tasa del documento
                };

                // Obtener renglones usando el SP buscarRengCP1
                var renglones = contexto.Database.SqlQuery<buscarRengCP1_Result>(
                    "EXEC buscarRengCP1 @consulta",
                    new SqlParameter("@consulta", documento.co_prov ?? string.Empty)
                ).Where(r => r.co_tipo_doc.Trim() == co_tipo_doc && r.nro_doc.Trim() == nro_doc)
                 .ToList();

                var viewModel = new DetalleDocumentoCPViewModel
                {
                    Documento = documento,
                    Proveedor = proveedor,
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

    [HttpGet]
    public ActionResult Error(string message = null)
    {
        ViewBag.ErrorMessage = message ?? "Ha ocurrido un error inesperado.";
        return View();
    }
}