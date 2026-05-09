using System;
using System.Collections.Generic;

namespace SistemaFacturacionPOS.Models.ViewModels
{
    public class CajaViewModel
    {
        public bool CajaAbierta { get; set; }
        public decimal MontoInicial { get; set; }
        public DateTimeOffset AbiertaDesde { get; set; }
        public decimal VentasDelDia { get; set; }
        public int CantidadTransacciones { get; set; }
        public string TiempoAbierta { get; set; }
        public List<VentaResumenViewModel> UltimasTransacciones { get; set; } = new List<VentaResumenViewModel>();
    }
}
