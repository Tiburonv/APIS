using System.Collections.Generic;

namespace APIS.Models
{
    public class DetalleClienteViewModel
    {
        public ClienteInfo Cliente { get; set; }
        public List<DocumentoCC1ViewModel> Documentos { get; set; }
        public decimal TotalSaldo { get; set; }
        public decimal TotalMontoBruto { get; set; }
        public decimal TotalImpuestos { get; set; }
    }

    public class ClienteInfo
    {
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string Direccion { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }
    }
}
