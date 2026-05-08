using System;

namespace APIS.Models
{
    public class ElementoPagoViewModel
    {
        public string CodigoProveedor { get; set; }
        public string NombreProveedor { get; set; }
        public string TipoDoc { get; set; }
        public string NroProv { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Saldo { get; set; }
        
        // Propiedades adicionales para formateo
        public string FechaFormateada 
        { 
            get 
            { 
                return Fecha.ToString("dd/MM/yyyy"); 
            } 
        }
        
        public string SaldoFormateado 
        { 
            get 
            { 
                return Saldo.ToString("N2"); 
            } 
        }
    }
}
