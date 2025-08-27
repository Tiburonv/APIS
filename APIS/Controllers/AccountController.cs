using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using APIS.ADO; // Asegúrate de tener el namespace correcto para tu modelo

namespace APIS.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            using (var db = new A_ZULIA_12Entities())
            {
                // Busca el usuario en la base de datos
                var usuario = db.Usuarios
                    .FirstOrDefault(u => u.Usuario == username && u.Clave == password);

                if (usuario != null)
                {
                    // Asigna el código de usuario a la sesión
                    Session["Usuario"] = usuario.Usuario; // Aquí se guarda "04", "05", etc.
                    FormsAuthentication.SetAuthCookie(username, false);
                    return RedirectToAction("Index", "Home");
                }
            }

            ViewBag.Error = "Usuario o clave incorrectos";
            return View();
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            return RedirectToAction("Login");
        }
    }
}