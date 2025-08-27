using System;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using APIS.ADO;

namespace APIS.Controllers
{
    public class ImagenesController : Controller
    {
        [HttpGet]
        public ActionResult ObtenerImagen(string co_cli, string cli_des)
        {
            try
            {
                if (string.IsNullOrEmpty(co_cli) || string.IsNullOrEmpty(cli_des))
                {
                    return HttpNotFound();
                }

                using (var db = new A_ZULIA_12Entities())
                {
                    // Buscar la imagen por código de cliente y descripción
                    var imagen = db.Database.SqlQuery<byte[]>(
                        "SELECT TOP 1 Imagen FROM buscarPorDocumCC1(@consulta, 1) WHERE co_cli = @co_cli AND cli_des = @cli_des",
                        new SqlParameter("@consulta", $"{co_cli} {cli_des}"),
                        new SqlParameter("@co_cli", co_cli),
                        new SqlParameter("@cli_des", cli_des)
                    ).FirstOrDefault();

                    if (imagen != null && imagen.Length > 0)
                    {
                        return File(imagen, "image/jpeg");
                    }
                    
                    return HttpNotFound();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al obtener imagen: {ex.Message}");
                return HttpNotFound();
            }
        }

        [HttpGet]
        public ActionResult ObtenerThumbnail(string co_cli, string cli_des)
        {
            try
            {
                if (string.IsNullOrEmpty(co_cli) || string.IsNullOrEmpty(cli_des))
                {
                    return HttpNotFound();
                }

                using (var db = new A_ZULIA_12Entities())
                {
                    // Buscar la imagen por código de cliente y descripción
                    var imagen = db.Database.SqlQuery<byte[]>(
                        "SELECT TOP 1 Imagen FROM buscarPorDocumCC1(@consulta, 1) WHERE co_cli = @co_cli AND cli_des = @cli_des",
                        new SqlParameter("@consulta", $"{co_cli} {cli_des}"),
                        new SqlParameter("@co_cli", co_cli),
                        new SqlParameter("@cli_des", cli_des)
                    ).FirstOrDefault();

                    if (imagen != null && imagen.Length > 0)
                    {
                        // Crear un thumbnail pequeño (50x50 píxeles)
                        var thumbnail = CrearThumbnail(imagen, 50, 50);
                        return File(thumbnail, "image/jpeg");
                    }
                    
                    return HttpNotFound();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al obtener thumbnail: {ex.Message}");
                return HttpNotFound();
            }
        }

        private byte[] CrearThumbnail(byte[] imagenOriginal, int ancho, int alto)
        {
            try
            {
                using (var stream = new System.IO.MemoryStream(imagenOriginal))
                using (var imagen = System.Drawing.Image.FromStream(stream))
                using (var thumbnail = imagen.GetThumbnailImage(ancho, alto, null, IntPtr.Zero))
                using (var thumbnailStream = new System.IO.MemoryStream())
                {
                    thumbnail.Save(thumbnailStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                    return thumbnailStream.ToArray();
                }
            }
            catch
            {
                // Si falla la creación del thumbnail, devolver la imagen original
                return imagenOriginal;
            }
        }
    }
}
