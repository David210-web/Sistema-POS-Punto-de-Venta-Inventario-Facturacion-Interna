using System;
using System.Collections.Generic;

namespace SistemaFacturacionPOS.Models
{
    public class Usuario
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string? Nombre { get; set; }
        public string? Apellido { get; set; }
        public string PasswordHash { get; set; }
        public Guid? RolId { get; set; }
        public bool? Activo { get; set; }
        public DateTimeOffset? CreatedAt { get; set; } = DateTimeOffset.Now;
        public DateTimeOffset? UpdatedAt { get; set; } = DateTimeOffset.Now;

        public virtual Rol? Rol { get; set; }
        public virtual ICollection<CajaSesion> CajaSesiones { get; set; } = new List<CajaSesion>();
        public virtual ICollection<Venta> Ventas { get; set; } = new List<Venta>();
        public virtual ICollection<InventarioMovimiento> InventarioMovimientos { get; set; } = new List<InventarioMovimiento>();
        public virtual ICollection<AuditoriaLog> AuditoriaLogs { get; set; } = new List<AuditoriaLog>();
    }
}
