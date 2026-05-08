using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using APIS.ADO;

namespace APIS.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            if (User.Identity.IsAuthenticated && Session["Usuario"] != null)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index", "Home");
            }

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string username, string password, string returnUrl)
        {
            username = (username ?? string.Empty).Trim();
            password = (password ?? string.Empty).Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Usuario y contraseña son obligatorios.";
                ViewBag.Username = username;
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }

            try
            {
                using (var db = new A_ZULIA_12Entities())
                {
                    var usernameLower = username.ToLower();

                    // Buscar usuario por identificador de usuario o por nombre
                    var usuario = db.Usuarios.FirstOrDefault(u => 
                        u.Usuario.Trim().ToLower() == usernameLower || 
                        (u.Nombre != null && u.Nombre.Trim().ToLower() == usernameLower)
                    );

                    // Si no se encuentra y se ingresó un número sin ceros a la izquierda (ej: '4' en lugar de '04')
                    int numVal;
                    if (usuario == null && int.TryParse(username, out numVal))
                    {
                        var usernamePadded = username.PadLeft(2, '0');
                        usuario = db.Usuarios.FirstOrDefault(u => u.Usuario.Trim() == usernamePadded);
                    }

                    if (usuario != null)
                    {
                        var claveRegistrada = (usuario.Clave ?? string.Empty).Trim();

                        // Verificación híbrida y flexible:
                        // 1. Coincidencia exacta (ej. "123*", "*123*", "6553*")
                        // 2. Coincidencia sin asteriscos (si el usuario escribió "123" y en la BD está "123*" o "*123*")
                        // 3. Coincidencia de hash SHA-256
                        bool esValido = string.Equals(claveRegistrada, password, StringComparison.Ordinal) ||
                                        string.Equals(claveRegistrada.Replace("*", ""), password.Replace("*", ""), StringComparison.Ordinal) ||
                                        VerificarHash(password, claveRegistrada);

                        if (esValido)
                        {
                            var codigoUsuario = usuario.Usuario.Trim();
                            Session["Usuario"] = codigoUsuario;
                            Session["NombreUsuario"] = (usuario.Nombre ?? codigoUsuario).Trim();
                            FormsAuthentication.SetAuthCookie(codigoUsuario, false);

                            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                            {
                                return Redirect(returnUrl);
                            }

                            return RedirectToAction("Index", "Home");
                        }
                    }
                }

                ViewBag.Error = "Usuario o contraseña incorrectos.";
                ViewBag.Username = username;
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }
            catch (Exception ex)
            {
                // No exponer el detalle tecnico al usuario; solo registrarlo en el servidor
                System.Diagnostics.Debug.WriteLine("Error Login: " + ex.ToString());
                ViewBag.Error = "Error interno al validar el usuario. Intente nuevamente o contacte al administrador.";
                ViewBag.Username = username;
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Login");
        }

        private static bool VerificarHash(string inputPassword, string storedHash)
        {
            if (string.IsNullOrEmpty(storedHash) || storedHash.Length < 32)
            {
                return false;
            }

            try
            {
                using (var sha256 = SHA256.Create())
                {
                    byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(inputPassword));
                    var builder = new StringBuilder();
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        builder.Append(bytes[i].ToString("x2"));
                    }
                    return string.Equals(builder.ToString(), storedHash, StringComparison.OrdinalIgnoreCase);
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
