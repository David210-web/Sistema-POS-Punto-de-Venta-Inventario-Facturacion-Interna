using System;
using System.Collections.Generic;

namespace SistemaFacturacionPOS.Models
{
    public class Rol
    {
        public Guid Id { get; set; }
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }

        public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    }
}
