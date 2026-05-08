using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace APIS.Models
{
    public class DocumentoCC1ViewModel
    {
        public string co_ven { get; set; }
        public string co_cli { get; set; }
        public string cli_des { get; set; }
        public decimal tasa { get; set; }
        public string co_tipo_doc { get; set; }
        public string Doc { get; set; }
        public string nro_doc { get; set; }
        public string doc_orig { get; set; }
        //public string nro_orig { get; set; }
        public DateTime fec_emis { get; set; }
        public string fec_emis1 { get; set; }
        public int Dias { get; set; }
        public decimal Ret25 { get; set; }
        public decimal Ret75 { get; set; }
        public decimal saldo { get; set; }
        public decimal saldo1 { get; set; }
        public decimal monto_bru { get; set; }
        public decimal monto_net { get; set; }
        public decimal monto_imp { get; set; }
        public decimal IVA25 { get; set; }
        public decimal IVA75 { get; set; }
        public decimal? saldo_moneda { get; set; }
        public decimal esperado_ret75 { get; set; }
        public decimal monto_ret_imp_convertido { get; set; }
        public string estado_retencion { get; set; }
        public byte[] Imagen { get; set; } // Cambiado de string a byte[]
        public int tiene_imagen { get; set; } // Indica si la factura tiene imagen (1 = sí, 0 = no)


    }
}
