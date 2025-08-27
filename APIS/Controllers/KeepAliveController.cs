using System.Web.Mvc;

namespace APIS.Controllers
{
    public class KeepAliveController : Controller
    {
        [HttpGet]
        public ActionResult Ping()
        {
            // Simplemente devolvemos un estado 200 OK
            return new HttpStatusCodeResult(200);
        }
    }
}
