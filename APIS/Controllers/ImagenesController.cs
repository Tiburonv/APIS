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
                    var resultado = ObtenerDocumentoConImagen(db, co_cli, cli_des);
                    var imagen = resultado?.Imagen;

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
                    var resultado = ObtenerDocumentoConImagen(db, co_cli, cli_des);
                    var imagen = resultado?.Imagen;

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

        private APIS.Models.DocumentoCC1ViewModel ObtenerDocumentoConImagen(A_ZULIA_12Entities db, string co_cli, string cli_des)
        {
            var usuario = User?.Identity?.Name ?? string.Empty;

            try
            {
                return db.Database.SqlQuery<APIS.Models.DocumentoCC1ViewModel>(
                    "EXEC buscarPorDocumCC1 @consulta, @cantidad, @usuario, @incluirImagen",
                    new SqlParameter("@consulta", $"{co_cli} {cli_des}"),
                    new SqlParameter("@cantidad", 1000),
                    new SqlParameter("@usuario", usuario),
                    new SqlParameter("@incluirImagen", true)
                ).FirstOrDefault(x => x.co_cli == co_cli && x.cli_des == cli_des);
            }
            catch (SqlException)
            {
                return db.Database.SqlQuery<APIS.Models.DocumentoCC1ViewModel>(
                    "EXEC buscarPorDocumCC1 @consulta, @cantidad, @usuario",
                    new SqlParameter("@consulta", $"{co_cli} {cli_des}"),
                    new SqlParameter("@cantidad", 1000),
                    new SqlParameter("@usuario", usuario)
                ).FirstOrDefault(x => x.co_cli == co_cli && x.cli_des == cli_des);
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
