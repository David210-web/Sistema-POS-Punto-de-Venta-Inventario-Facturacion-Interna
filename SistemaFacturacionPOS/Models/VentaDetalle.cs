using System;

namespace SistemaFacturacionPOS.Models
{
    public class VentaDetalle
    {
        public Guid Id { get; set; }
        public Guid? VentaId { get; set; }
        public Guid? ProductoId { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitarioHistorico { get; set; }
        public decimal? Subtotal { get; set; }

        public virtual Venta Venta { get; set; }
        public virtual Producto Producto { get; set; }
    }
}
