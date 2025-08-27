using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace APIS.Models
{
    public class RenglonDocumentoCP
    {
        public string co_art { get; set; }
        public string art_des { get; set; }
        public decimal total_art { get; set; }
        public decimal precio { get; set; }
        public decimal monto_imp { get; set; }
        public decimal monto_desc { get; set; }
        public decimal total { get; set; }
    }

    public class ProveedorInfo
    {
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string Direccion { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }
    }

    public class DetalleProveedorViewModel
    {
        public ProveedorInfo Proveedor { get; set; }
        public List<DocumentoCPViewModel> Documentos { get; set; }
        public decimal TotalSaldo { get; set; }
        public decimal TotalMontoBruto { get; set; }
        public decimal TotalImpuestos { get; set; }
    }

    public class DocumentoCPViewModel 
    {
        public string co_prov { get; set; }
        public string prov_des { get; set; }
        public string co_tipo_doc { get; set; }
        public string Doc { get; set; }
        public string nro_doc { get; set; }
        public string doc_orig { get; set; }
        public DateTime fec_emis { get; set; }
        public string fec_emis1 { get; set; }
        public int Dias { get; set; }
        public decimal saldo { get; set; }
        public decimal monto_bru { get; set; }
        public decimal monto_net { get; set; }
        public decimal monto_imp { get; set; }
        public decimal IVA25 { get; set; }
        public decimal IVA75 { get; set; }
        public decimal Ret25 { get; set; }
        public decimal Ret75 { get; set; }
        public decimal esperado_ret75 { get; set; }
        public decimal monto_ret_imp_convertido { get; set; }
        public string estado_retencion { get; set; }
        public string nro_fact { get; set; }
        public decimal tasa { get; set; }

    }
}