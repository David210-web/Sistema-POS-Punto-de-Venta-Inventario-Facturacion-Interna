using System;
using System.Collections.Generic;

namespace SistemaFacturacionPOS.Models.ViewModels
{
    public class DashboardViewModel
    {
        public decimal VentasHoy { get; set; }
        public int TransaccionesHoy { get; set; }
        public int ProductosTotales { get; set; }
        public int ProductosStockBajo { get; set; }
        public int FacturasSemana { get; set; }
        public decimal SaldoCaja { get; set; }
        public List<ActividadRecienteViewModel> Actividades { get; set; } = new List<ActividadRecienteViewModel>();
    }

    public class ActividadRecienteViewModel
    {
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public string Valor { get; set; }
        public string Tipo { get; set; } // Venta, Inventario, Caja
        public DateTimeOffset Fecha { get; set; }
        public string TiempoRelativo { get; set; }
    }
}
