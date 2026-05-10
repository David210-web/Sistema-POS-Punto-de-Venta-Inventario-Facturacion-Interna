using System;

namespace SistemaFacturacionPOS.Models.ViewModels
{
    public class VistaLogs
    {
        public Guid? id { get;set; }
        public string? username { get; set; }
        public string? tabla_afectada { get; set; }
        public string? accion { get; set; }
        public string? valor_anterior { get; set; }
        public string? valor_nuevo { get; set; }
        public DateTimeOffset? fecha_hora { get; set; }
    }
}
