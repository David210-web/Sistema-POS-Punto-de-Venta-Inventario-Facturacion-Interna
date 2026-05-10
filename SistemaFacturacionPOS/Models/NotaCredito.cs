using System;

namespace SistemaFacturacionPOS.Models
{
    public class NotaCredito
    {
        public Guid Id { get; set; }
        public Guid VentaId { get; set; }
        public string Folio { get; set; } = null!;
        public decimal TotalDevuelto { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }

        public virtual Venta Venta { get; set; } = null!;
    }
}
