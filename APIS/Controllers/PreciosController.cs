using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using APIS.Models;
using APIS.Repositorios;

namespace APIS.Controllers
{
    public class PreciosController : Controller
    {
        private readonly IPreciosRepositorio _precios;

        public PreciosController() : this(new PreciosRepositorio())
        {
        }

        public PreciosController(IPreciosRepositorio precios)
        {
            _precios = precios;
        }
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
            var categorias = _precios.ListarCategorias();
            return Json(categorias, JsonRequestBehavior.AllowGet);
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

            var data = _precios.BuscarPrecios(busquedaLimpia, coCatLimpia)
                .Select(x => new
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