using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using APIS.ADO;
using APIS.Models;

namespace APIS.Controllers
{
    public class DocumentoCPController : Controller
    {
        public string Nro_doc { get; set; }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var usuario = Session["Usuario"] as string;
            if (usuario == "999")
            {
                base.OnActionExecuting(filterContext);
                return;
            }
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

        [HttpGet]
        public JsonResult buscarPagosCP(string nombre, int page = 1, int pageSize = 20)
        {
            try
            {
                using (var contexto = new A_ZULIA_12Entities())
                {
                    var lista = contexto.buscarPagosCP(nombre ?? string.Empty).ToList();
                    var totalRecords = lista.Count;
                    var pagedList = lista.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                    var datos = pagedList.Select(item => new Dictionary<string, object>
                    {
                        { "cob_num", item.cob_num ?? "" },
                        { "co_prov", item.co_prov ?? "" },
                        { "prov_des", item.prov_des ?? "" },
                        { "Fecha", item.Fecha.ToString("yyyy-MM-dd") },
                        { "Dias", item.Dias ?? 0 },
                        { "anulado", item.anulado },
                        { "co_tipo_doc", item.co_tipo_doc ?? "" },
                        { "nro_doc", item.nro_doc ?? "" },
                        { "num_doc", item.num_doc ?? "" },
                        { "Total", item.Total },
                        { "tasa", item.tasa ?? 0 },
                        { "descrip", item.descrip ?? "" },
                        { "tiene_imagen", VerificarTieneImagenPago(item.cob_num ?? "", contexto) }
                    }).ToList();

                    return Json(new { datos, totalRecords }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en buscarPagosCP: {ex.ToString()}");
                return Json(new { mensaje = "Error al buscar los pagos." }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult buscarPorDocumCP(string nombre, int page = 1, int pageSize = 20)
        {
            try
            {
                string usuario = User.Identity.Name ?? "";

                using (var contexto = new A_ZULIA_12Entities())
                {
                    var lista = contexto.Database.SqlQuery<DocumentoCPViewModel>(
                        "EXEC buscarPorDocumCP @consulta, @cantidad, @usuario",
                        new SqlParameter("@consulta", nombre ?? string.Empty),
                        new SqlParameter("@cantidad", 8000),
                        new SqlParameter("@usuario", usuario)
                    ).ToList();

                    var totalRecords = lista.Count;
                    var pagedList = lista.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                    var datos = pagedList.Select(item => new Dictionary<string, object>
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
                        return Json(new { encabezados, datos, totalRecords }, JsonRequestBehavior.AllowGet);
                    else
                        return Json(new { encabezados, datos, totalRecords = 0, mensaje = "No se encontraron resultados para la búsqueda." }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en buscarPorDocumCP: {ex.ToString()}");
                return Json(new { mensaje = "Error al buscar los documentos." }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult DetalleDocumento(string co_tipo_doc, string nro_doc, string co_prov, string prov_des)
        {
            try
            {
                co_tipo_doc = co_tipo_doc?.Trim();
                nro_doc = nro_doc?.Trim();
                co_prov = co_prov?.Trim();
                prov_des = prov_des?.Trim();

                if (string.IsNullOrEmpty(co_tipo_doc) || string.IsNullOrEmpty(nro_doc))
                {
                    return RedirectToAction("Index");
                }

                using (var contexto = new A_ZULIA_12Entities())
                {
                    var usuario = User?.Identity?.Name ?? string.Empty;
                    var consultaDocumento = $"{co_tipo_doc}-{nro_doc}";

                    var documento = contexto.Database.SqlQuery<DocumentoCPViewModel>(
                        "EXEC buscarPorDocumCP @consulta, @cantidad, @usuario",
                        new SqlParameter("@consulta", consultaDocumento),
                        new SqlParameter("@cantidad", 50),
                        new SqlParameter("@usuario", usuario)
                    ).ToList()
                    .FirstOrDefault(x =>
                        string.Equals((x.co_tipo_doc ?? string.Empty).Trim(), co_tipo_doc, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals((x.nro_doc ?? string.Empty).Trim(), nro_doc, StringComparison.OrdinalIgnoreCase));

                    if (documento == null)
                    {
                        documento = contexto.Database.SqlQuery<DocumentoCPViewModel>(
                            "EXEC buscarPorDocumCP @consulta, @cantidad, @usuario",
                            new SqlParameter("@consulta", ""),
                            new SqlParameter("@cantidad", 1000),
                            new SqlParameter("@usuario", usuario)
                        ).ToList()
                        .FirstOrDefault(x =>
                            string.Equals((x.co_tipo_doc ?? string.Empty).Trim(), co_tipo_doc, StringComparison.OrdinalIgnoreCase) &&
                            string.Equals((x.nro_doc ?? string.Empty).Trim(), nro_doc, StringComparison.OrdinalIgnoreCase));
                    }

                    if (documento == null)
                    {
                        return RedirectToAction("Index");
                    }

                    var proveedor = new DocumentoCPViewModel
                    {
                        co_prov = co_prov ?? documento.co_prov,
                        prov_des = prov_des ?? documento.prov_des,
                        tasa = documento.tasa
                    };

                    var renglones = contexto.Database.SqlQuery<buscarRengCP1_Result>(
                        "EXEC buscarRengCP1 @consulta",
                        new SqlParameter("@consulta", consultaDocumento)
                    ).Where(r =>
                        string.Equals((r.co_tipo_doc ?? string.Empty).Trim(), co_tipo_doc, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals((r.nro_doc ?? string.Empty).Trim(), nro_doc, StringComparison.OrdinalIgnoreCase))
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
                System.Diagnostics.Debug.WriteLine($"Error en DetalleDocumento: {ex.Message}");
                return RedirectToAction("Error", new { message = "Error al cargar los detalles del documento. Verifique que los parámetros sean correctos." });
            }
        }

        [HttpGet]
        public ActionResult Error(string message = null)
        {
            ViewBag.ErrorMessage = message ?? "Ha ocurrido un error inesperado.";
            return View();
        }

        private bool VerificarTieneImagenPago(string cob_num, A_ZULIA_12Entities contexto)
        {
            try
            {
                if (string.IsNullOrEmpty(cob_num))
                    return false;

                var existe = contexto.Database.SqlQuery<int>(
                    @"SELECT COUNT(1) 
                      FROM saPago a
                      INNER JOIN saDocumentoImagen b ON a.rowguid = b.rowguidDoc
                      WHERE a.cob_num = @p0 AND b.co_tipo_doc = 'PAGO'",
                    cob_num
                ).FirstOrDefault();

                return existe > 0;
            }
            catch
            {
                return false;
            }
        }

        [HttpGet]
        public JsonResult ObtenerImagenPago(string cob_num)
        {
            try
            {
                if (string.IsNullOrEmpty(cob_num))
                {
                    return Json(new { success = false, message = "Número de pago no especificado" }, JsonRequestBehavior.AllowGet);
                }

                using (var contexto = new A_ZULIA_12Entities())
                {
                    var connection = contexto.Database.Connection;
                    if (connection.State != System.Data.ConnectionState.Open)
                        connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "EXEC buscarPagostImagen @doc_num";
                        command.Parameters.Add(new SqlParameter("@doc_num", cob_num));

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                byte[] imageBytes = reader["picture"] as byte[];
                                
                                if (imageBytes != null && imageBytes.Length > 0)
                                {
                                    string base64Image = Convert.ToBase64String(imageBytes);

                                    return Json(new
                                    {
                                        success = true,
                                        cob_num = reader["cob_num"]?.ToString(),
                                        des_imag = reader["des_imag"]?.ToString() ?? "Imagen del pago",
                                        picture = base64Image
                                    }, JsonRequestBehavior.AllowGet);
                                }
                                else
                                {
                                    return Json(new { success = false, message = "La imagen está vacía o no es válida" }, JsonRequestBehavior.AllowGet);
                                }
                            }
                        }
                    }
                }

                return Json(new { success = false, message = "No se encontró imagen para este pago" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al obtener imagen del pago: {ex.Message}");
                return Json(new { success = false, message = "Error al cargar la imagen. Intente nuevamente." }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}