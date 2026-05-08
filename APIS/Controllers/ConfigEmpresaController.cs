using APIS.ADO;
using APIS.Models;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace APIS.Controllers
{
    public class ConfigEmpresaController : Controller
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var action = filterContext.ActionDescriptor.ActionName;
            if (string.Equals(action, "Obtener", StringComparison.OrdinalIgnoreCase))
            {
                base.OnActionExecuting(filterContext);
                return;
            }

            var usuario = Session["Usuario"] as string;
            if (usuario != "999")
            {
                filterContext.Result = RedirectToAction("Index", "Home");
                return;
            }

            base.OnActionExecuting(filterContext);
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public JsonResult Obtener()
        {
            try
            {
                using (var db = new A_ZULIA_12Entities())
                {
                    var cfg = db.Database.SqlQuery<ConfigEmpresaViewModel>(
                        "EXEC obtenerZConfigEmpresa"
                    ).FirstOrDefault();

                    if (cfg == null)
                    {
                        return Json(new
                        {
                            rif = string.Empty,
                            direccion = string.Empty,
                            telefono = string.Empty,
                            logoBase64 = (string)null,
                            logoMimeType = (string)null
                        }, JsonRequestBehavior.AllowGet);
                    }

                    string logoBase64 = null;
                    if (cfg.Logo != null && cfg.Logo.Length > 0)
                    {
                        var mime = string.IsNullOrWhiteSpace(cfg.LogoMimeType) ? "image/png" : cfg.LogoMimeType;
                        logoBase64 = "data:" + mime + ";base64," + Convert.ToBase64String(cfg.Logo);
                    }

                    return Json(new
                    {
                        rif = cfg.RIF ?? string.Empty,
                        direccion = cfg.Direccion ?? string.Empty,
                        telefono = cfg.Telefono ?? string.Empty,
                        logoBase64 = logoBase64,
                        logoMimeType = cfg.LogoMimeType
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en ConfigEmpresa Obtener: {ex.ToString()}");
                return Json(new
                {
                    rif = string.Empty,
                    direccion = string.Empty,
                    telefono = string.Empty,
                    logoBase64 = (string)null,
                    logoMimeType = (string)null,
                    error = "Error al obtener la configuración de la empresa."
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Guardar(string rif, string direccion, string telefono, HttpPostedFileBase logo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(rif) || string.IsNullOrWhiteSpace(direccion) || string.IsNullOrWhiteSpace(telefono))
                {
                    return Json(new { ok = false, mensaje = "RIF, dirección y teléfono son obligatorios." });
                }

                byte[] logoBytes = null;
                string logoMime = null;
                var actualizarLogo = false;

                if (logo != null && logo.ContentLength > 0)
                {
                    logoMime = ObtenerMimeLogo(logo);
                    if (logoMime == null)
                    {
                        return Json(new { ok = false, mensaje = "El logo debe ser PNG o JPG." });
                    }

                    using (var ms = new MemoryStream())
                    {
                        if (logo.InputStream.CanSeek)
                        {
                            logo.InputStream.Position = 0;
                        }
                        logo.InputStream.CopyTo(ms);
                        logoBytes = ms.ToArray();
                    }

                    if (logoBytes.Length == 0)
                    {
                        return Json(new { ok = false, mensaje = "No se pudo leer el archivo del logo." });
                    }

                    actualizarLogo = true;
                }

                var usuario = Session["Usuario"] as string ?? "999";

                using (var db = new A_ZULIA_12Entities())
                {
                    var connection = db.Database.Connection;
                    if (connection.State != ConnectionState.Open)
                    {
                        connection.Open();
                    }

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "EXEC guardarZConfigEmpresa @RIF, @Direccion, @Telefono, @Logo, @LogoMimeType, @ActualizarLogo, @Co_us_mo";
                        command.CommandType = CommandType.Text;

                        command.Parameters.Add(new SqlParameter("@RIF", SqlDbType.VarChar, 20) { Value = rif.Trim() });
                        command.Parameters.Add(new SqlParameter("@Direccion", SqlDbType.NVarChar, 250) { Value = direccion.Trim() });
                        command.Parameters.Add(new SqlParameter("@Telefono", SqlDbType.VarChar, 50) { Value = telefono.Trim() });

                        var logoParam = new SqlParameter("@Logo", SqlDbType.VarBinary, -1)
                        {
                            Value = (object)logoBytes ?? DBNull.Value
                        };
                        command.Parameters.Add(logoParam);

                        command.Parameters.Add(new SqlParameter("@LogoMimeType", SqlDbType.VarChar, 50)
                        {
                            Value = (object)logoMime ?? DBNull.Value
                        });
                        command.Parameters.Add(new SqlParameter("@ActualizarLogo", SqlDbType.Bit) { Value = actualizarLogo });
                        command.Parameters.Add(new SqlParameter("@Co_us_mo", SqlDbType.VarChar, 10) { Value = usuario });

                        command.ExecuteNonQuery();
                    }
                }

                return Json(new { ok = true, mensaje = "Configuración guardada correctamente." });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en ConfigEmpresa Guardar: {ex.ToString()}");
                return Json(new { ok = false, mensaje = "Error al guardar la configuración. Intente nuevamente." });
            }
        }

        private static string ObtenerMimeLogo(HttpPostedFileBase logo)
        {
            var mime = (logo.ContentType ?? string.Empty).ToLowerInvariant();
            if (mime == "image/png" || mime == "image/jpeg" || mime == "image/jpg" || mime == "image/pjpeg")
            {
                return mime == "image/jpg" || mime == "image/pjpeg" ? "image/jpeg" : mime;
            }

            var ext = Path.GetExtension(logo.FileName)?.ToLowerInvariant();
            if (ext == ".png") return "image/png";
            if (ext == ".jpg" || ext == ".jpeg") return "image/jpeg";

            return null;
        }
    }
}
