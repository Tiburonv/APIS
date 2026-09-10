using System;
using System.Linq;
using System.Web.Mvc;
using APIS.Repositorios;

namespace APIS.Controllers
{
    public class ImagenesController : Controller
    {
        private readonly IImagenesRepositorio _imagenes;

        public ImagenesController() : this(new ImagenesRepositorio())
        {
        }

        public ImagenesController(IImagenesRepositorio imagenes)
        {
            _imagenes = imagenes;
        }
        [HttpGet]
        public ActionResult ObtenerImagen(string co_cli, string cli_des)
        {
            try
            {
                if (string.IsNullOrEmpty(co_cli) || string.IsNullOrEmpty(cli_des))
                {
                    return HttpNotFound();
                }

                // Buscar la imagen por código de cliente y descripción
                var resultado = ObtenerDocumentoConImagen(co_cli, cli_des);
                var imagen = resultado != null ? resultado.Imagen : null;

                if (imagen != null && imagen.Length > 0)
                {
                    return File(imagen, "image/jpeg");
                }
                
                return HttpNotFound();
            }
            catch (Exception ex)
            {
                APIS.Servicios.Log.Error("Error al obtener imagen", ex);
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

                // Buscar la imagen por código de cliente y descripción
                var resultado = ObtenerDocumentoConImagen(co_cli, cli_des);
                var imagen = resultado != null ? resultado.Imagen : null;

                if (imagen != null && imagen.Length > 0)
                {
                    // Crear un thumbnail pequeño (50x50 píxeles)
                    var thumbnail = CrearThumbnail(imagen, 50, 50);
                    return File(thumbnail, "image/jpeg");
                }
                
                return HttpNotFound();
            }
            catch (Exception ex)
            {
                APIS.Servicios.Log.Error("Error al obtener thumbnail", ex);
                return HttpNotFound();
            }
        }

        private APIS.Models.DocumentoCC1ViewModel ObtenerDocumentoConImagen(string co_cli, string cli_des)
        {
            var usuario = User != null && User.Identity != null ? (User.Identity.Name ?? string.Empty) : string.Empty;

            try
            {
                return _imagenes.ObtenerDocumentoConImagen(co_cli, cli_des, usuario);
            }
            catch (Exception ex)
            {
                APIS.Servicios.Log.Error("Error en ImagenesController.ObtenerDocumentoConImagen", ex);
                return null;
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
