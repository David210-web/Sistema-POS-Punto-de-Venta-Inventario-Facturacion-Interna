using System;
using System.Collections.Generic;

namespace SistemaFacturacionPOS.Models
{
    public class CajaSesion
    {
        public Guid Id { get; set; }
        public Guid? UsuarioId { get; set; }
        public decimal MontoApertura { get; set; }
        public decimal? MontoCierreSistema { get; set; }
        public decimal? MontoCierreFisico { get; set; }
        public decimal? Diferencia { get; set; }
        public DateTimeOffset? AbiertaAt { get; set; }
        public DateTimeOffset? CerradaAt { get; set; }
        public bool? Estado { get; set; }

        public virtual Usuario Usuario { get; set; }
        public virtual ICollection<Venta> Ventas { get; set; } = new List<Venta>();
    }
}
