using SistemaFacturacionPOS.Models.ViewModels;
using SistemaFacturacionPOS.Repositories.Interfaces;
using SistemaFacturacionPOS.Services.Interfaces;

namespace SistemaFacturacionPOS.Services
{
    public class HomeService : IHomeService
    {
        private readonly IHomeRepository _repo;

        public HomeService(IHomeRepository repo) { _repo = repo; }

        public async Task<DashboardViewModel> GetDashboardAsync()
        {
            var hoy          = DateTimeOffset.Now.Date;
            var inicioSemana = DateTimeOffset.Now.AddDays(-7);
            var model        = new DashboardViewModel();

            var ventasHoy = await _repo.GetVentasDesdeAsync(hoy);
            model.VentasHoy        = ventasHoy.Sum(v => v.TotalFinal);
            model.TransaccionesHoy = ventasHoy.Count;

            model.ProductosTotales  = await _repo.CountProductosActivosAsync();
            model.ProductosStockBajo = await _repo.CountProductosStockBajoAsync();
            model.FacturasSemana    = await _repo.CountFacturasSemanaAsync(inicioSemana);

            var sesionActiva = await _repo.GetUltimaSesionAsync();
            if (sesionActiva != null)
            {
                var ventasSesion = sesionActiva.Ventas.Where(v => v.Estado == "COMPLETADA").Sum(v => v.TotalFinal);
                model.SaldoCaja = sesionActiva.MontoApertura + ventasSesion;
            }

            var logs = await _repo.GetLogsRecientesAsync(5);
            foreach (var log in logs)
            {
                model.Actividades.Add(new ActividadRecienteViewModel
                {
                    Titulo        = $"{log.accion}: {log.tabla_afectada}",
                    Descripcion   = $"Usuario: {log.username}",
                    Valor         = log.accion == "VENTA" ? "Transacción" : "Cambio sistema",
                    Tipo          = log.tabla_afectada,
                    Fecha         = log.fecha_hora ?? DateTimeOffset.Now,
                    TiempoRelativo = GetRelativeTime(log.fecha_hora ?? DateTimeOffset.Now)
                });
            }

            return model;
        }

        private static string GetRelativeTime(DateTimeOffset dateTime)
        {
            var timeSpan = DateTimeOffset.Now - dateTime;
            if (timeSpan <= TimeSpan.FromSeconds(60)) return "Hace un momento";
            if (timeSpan <= TimeSpan.FromMinutes(60)) return $"Hace {timeSpan.Minutes} min";
            if (timeSpan <= TimeSpan.FromHours(24))   return $"Hace {timeSpan.Hours} hrs";
            return dateTime.ToString("dd/MM/yyyy");
        }
    }
}
