using APIS.ADO;
using System;
using System.Collections.Generic;

namespace APIS.Models
{
    public class VercotizacionViewModel
    {
        public List<ZCotizacionNB> Cotizaciones { get; set; }
        public List<ZCotizacionRengNB> Renglones { get; set; }
        public Dictionary<string, string> Clientes { get; set; } = new Dictionary<string, string>();
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
    }

    public class DetalleCotizacionViewModel
    {
        public ZCotizacionNB Cotizacion { get; set; }
        public List<ZCotizacionRengNB> Renglones { get; set; }
        public string DescripcionCliente { get; set; }
        public Dictionary<string, string> DescripcionesArticulos { get; set; } = new Dictionary<string, string>();
    }
}