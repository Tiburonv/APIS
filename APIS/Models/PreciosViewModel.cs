using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace APIS.Models
{
    /// <summary>
    /// ViewModel para los resultados del procedimiento almacenado buscarPrecios.
    /// </summary>
    public class PreciosViewModel
    {
        public string co_art { get; set; }
        public string art_des { get; set; }
        public string tipo_imp { get; set; }
        public string iva { get; set; }
        public decimal Precio1 { get; set; }
        public decimal Precio2 { get; set; }
        public decimal Precio3 { get; set; }
        public decimal Precio4 { get; set; }
        public decimal Precio5 { get; set; }
    }
}