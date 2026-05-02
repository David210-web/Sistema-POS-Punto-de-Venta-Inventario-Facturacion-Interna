using System;

namespace SistemaFacturacionPOS.Models
{
    public class InventarioMovimiento
    {
        public Guid Id { get; set; }
        public Guid? ProductoId { get; set; }
        public Guid? UsuarioId { get; set; }
        public string Tipo { get; set; }
        public int Cantidad { get; set; }
        public string Justificacion { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }

        public virtual Producto Producto { get; set; }
        public virtual Usuario Usuario { get; set; }
    }
}
