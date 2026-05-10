using System;
using System.Collections.Generic;

namespace SistemaFacturacionPOS.Models
{
    public class Producto
    {
        public Guid Id { get; set; }
        public string CodigoBarras { get; set; }
        public string Nombre { get; set; }
        public decimal PrecioUnitario { get; set; }
        public int? StockActual { get; set; }
        public int? StockMinimo { get; set; }
        public Guid? CategoriaId { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }

        public virtual Categoria Categoria { get; set; }
        public virtual ICollection<VentaDetalle> VentaDetalles { get; set; } = new List<VentaDetalle>();
        public virtual ICollection<InventarioMovimiento> InventarioMovimientos { get; set; } = new List<InventarioMovimiento>();
        public virtual ICollection<ProductoBodega> ProductoBodegas { get; set; } = new List<ProductoBodega>();
    }
}
