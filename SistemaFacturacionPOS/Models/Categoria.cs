using System;
using System.Collections.Generic;

namespace SistemaFacturacionPOS.Models
{
    public class Categoria
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; }

        public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();
    }
}
