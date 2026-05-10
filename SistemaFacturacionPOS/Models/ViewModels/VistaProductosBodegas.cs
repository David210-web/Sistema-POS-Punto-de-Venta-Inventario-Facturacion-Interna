using System;
using System.Collections.Generic;

namespace SistemaFacturacionPOS.Models.ViewModels
{
    public class VistaProductosBodegas
    {
        public Guid Id { get; set; }
        public Guid ProductoId { get; set; }
        public Guid BodegaId { get; set; }
        public string bodegaNombre { get; set; }
        public int Stock { get; set; }
    }
}