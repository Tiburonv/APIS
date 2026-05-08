using System;

namespace APIS.Models
{
    public class ConfigEmpresaViewModel
    {
        public int Id { get; set; }
        public string RIF { get; set; }
        public string Direccion { get; set; }
        public string Telefono { get; set; }
        public byte[] Logo { get; set; }
        public string LogoMimeType { get; set; }
        public DateTime? Fe_us_mo { get; set; }
        public string Co_us_mo { get; set; }
    }
}
