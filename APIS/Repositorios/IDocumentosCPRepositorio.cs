using System.Collections.Generic;
using APIS.ADO;
using APIS.Models;

namespace APIS.Repositorios
{
    /// <summary>Acceso a datos del modulo de Documentos de Compra (CP) - Fase 2.</summary>
    public interface IDocumentosCPRepositorio
    {
        List<buscarPagosCP_Result> BuscarPagosCP(string nombre);
        List<DocumentoCPViewModel> BuscarPorDocumCP(string consulta, int cantidad, string usuario);
        List<buscarRengCP1_Result> BuscarRenglones(string consulta);
        bool TieneImagenPago(string cobNum);
        ImagenPagoDto ObtenerImagenPago(string cobNum);
    }

    /// <summary>Resultado de la imagen de un pago (SP buscarPagostImagen).</summary>
    public class ImagenPagoDto
    {
        public string cob_num { get; set; }
        public string des_imag { get; set; }
        public byte[] picture { get; set; }
    }
}
