using System;

namespace SistemaFacturacionPOS.Models
{
    public class VentaAnulacionSolicitud
    {
        public Guid Id { get; set; }
        public Guid VentaId { get; set; }
        public Guid UsuarioSolicitaId { get; set; }
        public string Motivo { get; set; } = null!;
        public string Estado { get; set; } = "PENDIENTE";
        public Guid? UsuarioResuelveId { get; set; }
        public string? MotivoRechazo { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
        public DateTimeOffset? ResolvedAt { get; set; }

        public virtual Venta Venta { get; set; } = null!;
        public virtual Usuario UsuarioSolicita { get; set; } = null!;
        public virtual Usuario? UsuarioResuelve { get; set; }
    }
}
