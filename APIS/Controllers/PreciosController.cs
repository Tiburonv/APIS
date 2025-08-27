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
        public JsonResult Buscar(string busqueda)
        {
            var resultados = new List<PreciosViewModel>();

            if (string.IsNullOrWhiteSpace(busqueda))
                return Json(resultados, JsonRequestBehavior.AllowGet);

            using (var db = new A_ZULIA_12Entities())
            {
                var param = new SqlParameter("@busqueda", busqueda);
                resultados = db.Database.SqlQuery<PreciosViewModel>(
                    "EXEC buscarPrecios @busqueda", param
                ).ToList();
            }

            var data = resultados.Select(x => new
            {
                co_art = x.co_art,
                art_des = x.art_des,
                tipo_imp = x.tipo_imp,
                iva = x.iva,
                precio1 = x.Precio1,
                precio2 = x.Precio2,
                precio3 = x.Precio3,
                precio4 = x.Precio4,
                precio5 = x.Precio5
            });

            return Json(data, JsonRequestBehavior.AllowGet);
        }
    }
}