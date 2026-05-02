using System;

namespace SistemaFacturacionPOS.Models
{
    public class AuditoriaLog
    {
        public Guid Id { get; set; }
        public Guid? UsuarioId { get; set; }
        public string TablaAfectada { get; set; }
        public string Accion { get; set; }
        public string ValorAnterior { get; set; }
        public string ValorNuevo { get; set; }
        public DateTimeOffset? FechaHora { get; set; }

        public virtual Usuario Usuario { get; set; }
    }
}
