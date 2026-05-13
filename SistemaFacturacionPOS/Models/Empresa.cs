using System;

namespace SistemaFacturacionPOS.Models
{
    public class Empresa
    {
        public Guid Id { get; set; }
        public string? Nombre { get; set; }
        public string? Nit { get; set; }
        public string? Direccion { get; set; }
    }
}
