using APIS.ADO;
using APIS.Models;

namespace APIS.Repositorios
{
    /// <summary>Acceso a datos de imagenes de documentos (Fase 2).</summary>
    public interface IImagenesRepositorio
    {
        DocumentoCC1ViewModel ObtenerDocumentoConImagen(string coCli, string cliDes, string usuario);
    }

    public class ImagenesRepositorio : IImagenesRepositorio
    {
        /// <summary>
        /// Busca el documento del cliente incluyendo la imagen (bitmap) cuando existe.
        /// Reutiliza el helper compartido de Cuentas por Cobrar.
        /// </summary>
        public DocumentoCC1ViewModel ObtenerDocumentoConImagen(string coCli, string cliDes, string usuario)
        {
            using (var db = new A_ZULIA_12Entities())
            {
                var lista = ConsultasCuentasCobrar.BuscarPorDocumCC1(
                    db,
                    coCli + " " + cliDes,
                    1000,
                    usuario,
                    incluirImagen: true
                );

                foreach (var item in lista)
                {
                    if (item.co_cli == coCli && item.cli_des == cliDes)
                    {
                        return item;
                    }
                }

                return null;
            }
        }
    }
}
