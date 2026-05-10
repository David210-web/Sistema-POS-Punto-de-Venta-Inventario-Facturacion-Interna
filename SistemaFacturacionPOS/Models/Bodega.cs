using System;
using System.Collections.Generic;

namespace SistemaFacturacionPOS.Models
{
    public class Bodega
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }

        public virtual ICollection<ProductoBodega> ProductoBodegas { get; set; } = new List<ProductoBodega>();
    }
}
