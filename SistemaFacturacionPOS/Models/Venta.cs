using System;
using System.Collections.Generic;

namespace SistemaFacturacionPOS.Models
{
    public class Venta
    {
        public Guid Id { get; set; }
        public int FolioInterno { get; set; }
        public Guid? UsuarioId { get; set; }
        public Guid? CajaSesionId { get; set; }
        public decimal TotalNeto { get; set; }
        public decimal Impuestos { get; set; }
        public decimal TotalFinal { get; set; }
        public string MetodoPago { get; set; }
        public string Estado { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }

        public virtual Usuario Usuario { get; set; }
        public virtual CajaSesion CajaSesion { get; set; }
        public virtual ICollection<VentaDetalle> VentaDetalles { get; set; } = new List<VentaDetalle>();
    }
}
