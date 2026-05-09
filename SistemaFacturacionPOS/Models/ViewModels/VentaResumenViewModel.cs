using System;

namespace SistemaFacturacionPOS.Models.ViewModels
{
    public class VentaResumenViewModel
    {
        public string Folio { get; set; }
        public string Hora { get; set; }
        public decimal Total { get; set; }
    }
}
