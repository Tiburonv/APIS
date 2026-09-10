using System;
using System.Collections.Generic;
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
        // ====== Contraseñas seguras (PBKDF2) ======
        // Formato almacenado: pbkdf2$<iteraciones>$<salt_base64>$<hash_base64>
        private const int PBKDF2_ITERACIONES = 60000;
        private const int PBKDF2_TAM_SALT = 16;
        private const int PBKDF2_TAM_HASH = 32; // SHA-256
        private const string PBKDF2_PREFIJO = "pbkdf2$";

        // ====== Bloqueo por intentos fallidos ======
        private const int MAX_INTENTOS_FALLIDOS = 5;
        private const int MINUTOS_BLOQUEO = 5;
        // Usuarios que nunca se bloquean (evita quedar fuera del sistema por error)
        private static readonly string[] USUARIOS_EXENTOS = { "999" };

        private class RegistroIntento
        {
            public int Fallos;
            public DateTime HastaBloqueo;
        }
        private static readonly object _lockIntentos = new object();
        private static readonly Dictionary<string, RegistroIntento> _intentos = new Dictionary<string, RegistroIntento>();

        private static string ClaveIntentos(string username)
        {
            return (username ?? string.Empty).Trim().ToLowerInvariant();
        }

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
            // IMPORTANTE: la contrasena NO se recorta con Trim() (puede contener espacios validos)

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Usuario y contraseña son obligatorios.";
                ViewBag.Username = username;
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }

            var claveIntento = ClaveIntentos(username);
            bool esUsuarioExento = USUARIOS_EXENTOS.Any(e => string.Equals(e, username.Trim(), StringComparison.OrdinalIgnoreCase));

            // ===== Verificacion de bloqueo por intentos fallidos =====
            if (!esUsuarioExento && EstaBloqueado(claveIntento))
            {
                int minutos = MinutosRestantesBloqueo(claveIntento);
                ViewBag.Error = "Demasiados intentos fallidos. Cuenta temporalmente bloqueada. Intente nuevamente en " + minutos + " minuto(s).";
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
                        var claveRegistrada = usuario.Clave ?? string.Empty;

                        bool esValido = false;
                        bool requiereMigrar = false;

                        if (claveRegistrada.StartsWith(PBKDF2_PREFIJO, StringComparison.Ordinal))
                        {
                            // Formato nuevo: PBKDF2
                            esValido = VerificarPbkdf2(password, claveRegistrada);
                        }
                        else
                        {
                            // Formato legado (texto plano con asteriscos o SHA-256): se valida con compatibilidad
                            // y, si es correcta, se migra automaticamente al formato seguro PBKDF2.
                            var claveLegacy = claveRegistrada.Trim();
                            esValido = string.Equals(claveLegacy, password, StringComparison.Ordinal) ||
                                        string.Equals(claveLegacy.Replace("*", ""), password.Replace("*", ""), StringComparison.Ordinal) ||
                                        VerificarHashSha256Legacy(password, claveLegacy);
                            requiereMigrar = esValido;
                        }

                        if (esValido)
                        {
                            // Migrar a PBKDF2 en el primer login exitoso (claves legadas)
                            if (requiereMigrar)
                            {
                                try
                                {
                                    usuario.Clave = HashPasswordPbkdf2(password);
                                    db.SaveChanges();
                                }
                                catch (Exception ex)
                                {
                                    // Si falla la migracion no se debe impedir el acceso
                                    System.Diagnostics.Debug.WriteLine("Error al migrar clave a PBKDF2: " + ex.ToString());
                                }
                            }

                            LimpiarIntentosFallidos(claveIntento);

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

                // Login fallido: registrar intento (excepto usuarios exentos)
                if (!esUsuarioExento)
                {
                    RegistrarIntentoFallido(claveIntento);
                }

                ViewBag.Error = "Usuario o contraseña incorrectos.";
                ViewBag.Username = username;
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }
            catch (Exception ex)
            {
                // No exponer el detalle tecnico al usuario; solo registrarlo en el servidor
                APIS.Servicios.Log.Error("Error Login", ex);
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

        // ============================================================
        //  PBKDF2
        // ============================================================

        private static string HashPasswordPbkdf2(string password)
        {
            byte[] salt = new byte[PBKDF2_TAM_SALT];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }

            byte[] hash = DerivarPbkdf2(password, salt, PBKDF2_ITERACIONES);

            return PBKDF2_PREFIJO + PBKDF2_ITERACIONES + "$"
                   + Convert.ToBase64String(salt) + "$"
                   + Convert.ToBase64String(hash);
        }

        private static byte[] DerivarPbkdf2(string password, byte[] salt, int iteraciones)
        {
            using (var kdf = new Rfc2898DeriveBytes(password, salt, iteraciones, HashAlgorithmName.SHA256))
            {
                return kdf.GetBytes(PBKDF2_TAM_HASH);
            }
        }

        private static bool VerificarPbkdf2(string password, string almacenado)
        {
            try
            {
                var partes = almacenado.Split('$'); // [pbkdf2, iter, salt, hash]
                if (partes.Length != 4) return false;
                int iteraciones;
                if (!int.TryParse(partes[1], out iteraciones) || iteraciones <= 0) return false;

                byte[] salt = Convert.FromBase64String(partes[2]);
                byte[] hashEsperado = Convert.FromBase64String(partes[3]);
                byte[] hashCalculado = DerivarPbkdf2(password, salt, iteraciones);

                return IgualdadEnTiempoConstante(hashCalculado, hashEsperado);
            }
            catch
            {
                return false;
            }
        }

        private static bool IgualdadEnTiempoConstante(byte[] a, byte[] b)
        {
            if (a == null || b == null || a.Length != b.Length) return false;
            int diff = 0;
            for (int i = 0; i < a.Length; i++)
            {
                diff |= a[i] ^ b[i];
            }
            return diff == 0;
        }

        // ============================================================
        //  Legado (solo para validar claves antiguas y migrarlas)
        // ============================================================

        private static bool VerificarHashSha256Legacy(string inputPassword, string storedHash)
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

        // ============================================================
        //  Bloqueo por intentos fallidos (en memoria del servidor)
        // ============================================================

        private static bool EstaBloqueado(string claveIntento)
        {
            lock (_lockIntentos)
            {
                RegistroIntento reg;
                if (_intentos.TryGetValue(claveIntento, out reg))
                {
                    return reg.HastaBloqueo > DateTime.Now;
                }
                return false;
            }
        }

        private static int MinutosRestantesBloqueo(string claveIntento)
        {
            lock (_lockIntentos)
            {
                RegistroIntento reg;
                if (_intentos.TryGetValue(claveIntento, out reg) && reg.HastaBloqueo > DateTime.Now)
                {
                    return Math.Max(1, (int)Math.Ceiling((reg.HastaBloqueo - DateTime.Now).TotalMinutes));
                }
                return MINUTOS_BLOQUEO;
            }
        }

        private static void RegistrarIntentoFallido(string claveIntento)
        {
            lock (_lockIntentos)
            {
                RegistroIntento reg;
                if (!_intentos.TryGetValue(claveIntento, out reg))
                {
                    reg = new RegistroIntento();
                    _intentos[claveIntento] = reg;
                }

                if (reg.HastaBloqueo > DateTime.Now)
                {
                    return; // ya bloqueado
                }

                reg.Fallos++;
                if (reg.Fallos >= MAX_INTENTOS_FALLIDOS)
                {
                    reg.Fallos = 0;
                    reg.HastaBloqueo = DateTime.Now.AddMinutes(MINUTOS_BLOQUEO);
                }
            }
        }

        private static void LimpiarIntentosFallidos(string claveIntento)
        {
            lock (_lockIntentos)
            {
                _intentos.Remove(claveIntento);
            }
        }
    }
}
