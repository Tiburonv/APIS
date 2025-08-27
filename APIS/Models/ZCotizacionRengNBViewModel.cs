using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace APIS.Models
{
    public class ZCotizacionRengNBViewModel : Controller
    {
        public int reng_num { get; set; }
        public string co_art { get; set; }
        public decimal total_art { get; set; }
        public decimal prec_vta { get; set; }
        public string tipo_imp { get; set; }
        public decimal reng_neto { get; set; }

        // GET: ZCotizacionRengNBViewModel
        public ActionResult Index()
        {
            return View();
        }
    }
}