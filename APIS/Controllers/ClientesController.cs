using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace APIS.Controllers
{
    public class ClientesController : Controller
    {
        // Acción principal para mostrar la página de clientes
        public ActionResult Index()
        {
            return View();
        }
    }
}