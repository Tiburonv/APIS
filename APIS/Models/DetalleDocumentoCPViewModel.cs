using System.Collections.Generic;
using APIS.ADO;
using System.Linq;

namespace APIS.Models
{
    public class DetalleDocumentoCPViewModel
    {
        public DocumentoCPViewModel Documento { get; set; }
        public DocumentoCPViewModel Proveedor { get; set; }
        public List<buscarRengCP1_Result> Renglones { get; set; }

        // Propiedades para las sumas
        public decimal SumaMontoImp => Renglones?.Sum(r => r.monto_imp) ?? 0;
        public decimal SumaRengNeto => Renglones?.Sum(r => r.reng_neto) ?? 0;

        public DetalleDocumentoCPViewModel()
        {
            Renglones = new List<buscarRengCP1_Result>();
        }
    }
}

