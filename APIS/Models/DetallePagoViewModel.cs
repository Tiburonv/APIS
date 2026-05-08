using System;
using System.Collections.Generic;

namespace APIS.Models
{
    public class DetallePagoViewModel
    {
        public string cob_num { get; set; }
        public string co_prov { get; set; }
        public string prov_des { get; set; }
        public DateTime Fecha { get; set; }
        public int? Dias { get; set; }
        public bool anulado { get; set; }
        public decimal? tasa { get; set; }
        public List<DetallePagoRenglonViewModel> Renglones { get; set; }
    }

    public class DetallePagoRenglonViewModel
    {
        public string cob_num { get; set; }
        public string co_prov { get; set; }
        public string prov_des { get; set; }
        public DateTime Fecha { get; set; }
        public int? Dias { get; set; }
        public bool anulado { get; set; }
        public string co_tipo_doc { get; set; }
        public string nro_doc { get; set; }
        public decimal? Total { get; set; }
        public string forma_pag { get; set; }
        public string cod_cta { get; set; }
        public string num_doc { get; set; }
        public decimal? tasa { get; set; }
    }
}
