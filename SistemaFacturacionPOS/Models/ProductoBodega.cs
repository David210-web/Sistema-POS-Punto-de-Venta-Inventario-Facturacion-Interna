using System;

namespace SistemaFacturacionPOS.Models
{
    public class ProductoBodega
    {
        public Guid Id { get; set; }
        public Guid ProductoId { get; set; }
        public Guid BodegaId { get; set; }
        public int Stock { get; set; }

        public virtual Producto Producto { get; set; }
        public virtual Bodega Bodega { get; set; }
    }
}
