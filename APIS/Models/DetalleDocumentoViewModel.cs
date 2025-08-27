using System.Collections.Generic;
using APIS.ADO; // Ajusta esto según el espacio de nombres real.
using System.Linq; // Para usar métodos LINQ
namespace APIS.Models
{
    public class DetalleDocumentoViewModel
    {
        public DocumentoCC1ViewModel Documento { get; set; }
        public DocumentoCC1ViewModel Cliente { get; set; }
        public List<buscarRengCC_Result> Renglones { get; set; }

        // Propiedades para las sumas
       
        public decimal SumaMontoImp => Renglones.Sum(r => r.monto_imp);
        public decimal SumaRengNeto => Renglones.Sum(r => r.reng_neto);

        public DetalleDocumentoViewModel()
        {
            Renglones = new List<buscarRengCC_Result>();
        }
    }
}