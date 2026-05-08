using System;
using System.Collections.Generic;

namespace APIS.Models
{
    public class PagoValidadoViewModel
    {
        public int Id { get; set; }
        public DateTime FechaValidacion { get; set; }
        public string CodigoProveedor { get; set; }
        public string NombreProveedor { get; set; }
        public decimal TotalPago { get; set; }
        public int CantidadElementos { get; set; }
        public string UsuarioValidacion { get; set; }
        public List<ElementoPagoViewModel> Elementos { get; set; }
        
        // Propiedades adicionales para formateo
        public string FechaValidacionFormateada 
        { 
            get 
            { 
                return FechaValidacion.ToString("dd/MM/yyyy HH:mm"); 
            } 
        }
        
        public string TotalPagoFormateado 
        { 
            get 
            { 
                return TotalPago.ToString("N2"); 
            } 
        }
        
        public string FechaSolo 
        { 
            get 
            { 
                return FechaValidacion.ToString("dd/MM/yyyy"); 
            } 
        }
    }
}
