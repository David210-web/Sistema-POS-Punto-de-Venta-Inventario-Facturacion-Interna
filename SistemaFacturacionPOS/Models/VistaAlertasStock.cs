using System;

namespace SistemaFacturacionPOS.Models
{
    public class VistaAlertasStock
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; }
        public int? StockActual { get; set; }
        public int? StockMinimo { get; set; }
    }
}
