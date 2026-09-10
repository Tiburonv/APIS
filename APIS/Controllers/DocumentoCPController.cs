using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using APIS.Models;
using APIS.Repositorios;

namespace APIS.Controllers
{
    public class DocumentoCPController : Controller
    {
        private readonly IDocumentosCPRepositorio _docCP;

        public DocumentoCPController() : this(new DocumentosCPRepositorio())
        {
        }

        public DocumentoCPController(IDocumentosCPRepositorio docCP)
        {
            _docCP = docCP;
        }

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
                var lista = _docCP.BuscarPagosCP(nombre ?? string.Empty);
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
                    { "tiene_imagen", _docCP.TieneImagenPago(item.cob_num ?? "") }
                }).ToList();

                return Json(new { datos, totalRecords }, JsonRequestBehavior.AllowGet);
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

                var lista = _docCP.BuscarPorDocumCP(nombre ?? string.Empty, 8000, usuario);

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

                var usuario = User?.Identity?.Name ?? string.Empty;
                var consultaDocumento = $"{co_tipo_doc}-{nro_doc}";

                var documento = _docCP.BuscarPorDocumCP(consultaDocumento, 50, usuario)
                    .FirstOrDefault(x =>
                        string.Equals((x.co_tipo_doc ?? string.Empty).Trim(), co_tipo_doc, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals((x.nro_doc ?? string.Empty).Trim(), nro_doc, StringComparison.OrdinalIgnoreCase));

                if (documento == null)
                {
                    documento = _docCP.BuscarPorDocumCP("", 1000, usuario)
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

                var renglones = _docCP.BuscarRenglones(consultaDocumento)
                    .Where(r =>
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

        [HttpGet]
        public JsonResult ObtenerImagenPago(string cob_num)
        {
            try
            {
                if (string.IsNullOrEmpty(cob_num))
                {
                    return Json(new { success = false, message = "Número de pago no especificado" }, JsonRequestBehavior.AllowGet);
                }

                var imagen = _docCP.ObtenerImagenPago(cob_num);

                if (imagen == null)
                {
                    return Json(new { success = false, message = "No se encontró imagen para este pago" }, JsonRequestBehavior.AllowGet);
                }

                if (imagen.picture == null || imagen.picture.Length == 0)
                {
                    return Json(new { success = false, message = "La imagen está vacía o no es válida" }, JsonRequestBehavior.AllowGet);
                }

                return Json(new
                {
                    success = true,
                    cob_num = imagen.cob_num,
                    des_imag = imagen.des_imag ?? "Imagen del pago",
                    picture = Convert.ToBase64String(imagen.picture)
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al obtener imagen del pago: {ex.Message}");
                return Json(new { success = false, message = "Error al cargar la imagen. Intente nuevamente." }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}