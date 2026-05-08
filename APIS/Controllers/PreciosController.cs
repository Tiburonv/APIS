using APIS.ADO;
using APIS.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;

namespace APIS.Controllers
{
    public class PreciosController : Controller
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var usuario = Session["Usuario"] as string;
            if (usuario == "999")
            {
                base.OnActionExecuting(filterContext);
                return;
            }
            if (usuario != "04" && usuario != "05" && usuario != "11")
            {
                filterContext.Result = RedirectToAction("Index", "Home");
                return;
            }
            base.OnActionExecuting(filterContext);
        }

        //public ActionResult Index(string co_prov, string prov_des)
        //{
        //    ViewBag.co_prov = co_prov;
        //    ViewBag.prov_des = prov_des;
        //    return View("Precios");
        //}

        // GET: Precios/Precios
        public ActionResult Precios()


        {
            return View();
        }

        [HttpGet]
        public JsonResult ListarCategorias()
        {
            using (var db = new A_ZULIA_12Entities())
            {
                var categorias = db.Database.SqlQuery<CategoriaViewModel>(
                    "SELECT RTRIM(co_cat) AS co_cat, RTRIM(cat_des) AS cat_des " +
                    "FROM saCatArticulo " +
                    "ORDER BY cat_des"
                ).ToList();

                return Json(categorias, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult Buscar(string busqueda, string co_cat = null)
        {
            var resultados = new List<PreciosViewModel>();

            if (string.IsNullOrWhiteSpace(busqueda) && string.IsNullOrWhiteSpace(co_cat))
                return Json(resultados, JsonRequestBehavior.AllowGet);

            var busquedaLimpia = string.IsNullOrWhiteSpace(busqueda) ? null : busqueda.Trim();
            var coCatLimpia = string.IsNullOrWhiteSpace(co_cat) ? null : co_cat.Trim();

            // Si hay texto de búsqueda, ignorar la categoría
            if (!string.IsNullOrEmpty(busquedaLimpia))
            {
                coCatLimpia = null;
            }

            using (var db = new A_ZULIA_12Entities())
            {
                var paramBusqueda = new SqlParameter("@busqueda", (object)busquedaLimpia ?? DBNull.Value);
                var paramCoCat = new SqlParameter("@co_cat", (object)coCatLimpia ?? DBNull.Value);
                resultados = db.Database.SqlQuery<PreciosViewModel>(
                    "EXEC buscarPrecios @busqueda, @co_cat", paramBusqueda, paramCoCat
                ).ToList();
            }

            var data = resultados.Select(x => new
            {
                co_art = x.co_art,
                art_des = x.art_des,
                co_cat = x.co_cat,
                cat_des = x.cat_des,
                tipo_imp = x.tipo_imp,
                iva = x.iva,
                precio1 = x.Precio1,
                precio2 = x.Precio2,
                precio3 = x.Precio3,
                precio4 = x.Precio4,
                precio5 = x.Precio5,
                tasa = x.tasa
            });

            return Json(data, JsonRequestBehavior.AllowGet);
        }
    }
}