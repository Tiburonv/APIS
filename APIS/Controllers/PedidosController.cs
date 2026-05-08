using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using APIS.ADO;
using System.Data.Entity;

namespace APIS.Controllers
{
    public class PedidosController : Controller
    {
        private A_ZULIA_12Entities db = new A_ZULIA_12Entities();

        // GET: Pedidos
        public ActionResult Index()
        {
            return View();
        }

        // GET: Pedidos/Ped
        public ActionResult Ped(string co_cli = null, string cli_des = null)
        {
            ViewBag.co_cli = co_cli;
            ViewBag.cli_des = cli_des;
            ViewBag.CargarAutomaticamente = !string.IsNullOrEmpty(co_cli) && !string.IsNullOrEmpty(cli_des);
            return View();
        }

        // GET: Pedidos/DetallePedido
        public ActionResult DetallePedido(string doc_num)
        {
            if (string.IsNullOrEmpty(doc_num))
            {
                return RedirectToAction("Ped");
            }

            ViewBag.doc_num = doc_num;
            return View();
        }

        // POST: Pedidos/buscarPedidosActivos
        [HttpPost]
        public JsonResult buscarPedidosActivos(string consulta)
        {
            try
            {
                var resultados = db.buscarPedidosActivos(consulta).ToList();
                return Json(new { success = true, datos = resultados });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en buscarPedidosActivos: {ex.ToString()}");
                return Json(new { success = false, mensaje = "Error al buscar los pedidos activos." });
            }
        }

        // POST: Pedidos/buscarRengPedidos
        [HttpPost]
        public JsonResult buscarRengPedidos(string doc_num)
        {
            try
            {
                if (string.IsNullOrEmpty(doc_num))
                {
                    return Json(new { success = false, mensaje = "El número de documento es requerido" });
                }

                // Obtener los renglones del pedido
                var renglones = db.buscarRengPE2(doc_num).ToList();
                
                if (renglones.Any())
                {
                    // Obtener información adicional del cliente y fecha desde el primer renglón
                    var primerRenglon = renglones.First();
                    
                    // Buscar información del cliente usando buscarPedidosActivos
                    var pedidosCliente = db.buscarPedidosActivos(primerRenglon.co_cli).ToList();
                    var pedidoInfo = pedidosCliente.FirstOrDefault(p => p.doc_num == doc_num);
                    
                    // Crear una nueva lista con objetos anónimos que incluyan todos los campos
                    var renglonesCompletos = renglones.Select(renglon => new
                    {
                        doc_num = renglon.doc_num,
                        co_cli = renglon.co_cli,
                        tasa = renglon.tasa,
                        reng_num = renglon.reng_num,
                        co_art = renglon.co_art,
                        art_des = renglon.art_des,
                        total_art = renglon.total_art,
                        prec_vta = renglon.prec_vta,
                        tipo_imp = renglon.tipo_imp,
                        monto_imp = renglon.monto_imp,
                        reng_neto = renglon.reng_neto,
                        cli_des = pedidoInfo?.cli_des ?? "N/A",
                        fec_emis = pedidoInfo?.fec_emis
                    }).ToList();
                    
                    return Json(new { success = true, datos = renglonesCompletos });
                }
                
                return Json(new { success = true, datos = renglones });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en buscarRengPedidos: {ex.ToString()}");
                return Json(new { success = false, mensaje = "Error al obtener los renglones del pedido." });
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}