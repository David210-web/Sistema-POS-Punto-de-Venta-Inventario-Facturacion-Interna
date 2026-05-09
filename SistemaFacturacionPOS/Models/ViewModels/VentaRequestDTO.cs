using System;
using System.Collections.Generic;

namespace SistemaFacturacionPOS.Models.ViewModels
{
    public class VentaRequestDTO
    {
        public string MetodoPago { get; set; }
        public decimal Total { get; set; }
        public decimal DineroRecibido { get; set; }
        public string UltimosDigitosTarjeta { get; set; }
        public List<VentaDetalleRequestDTO> Detalles { get; set; }
    }

    public class VentaDetalleRequestDTO
    {
        public Guid ProductoId { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
    }
}
