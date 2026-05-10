using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaFacturacionPOS.Contexto;
using SistemaFacturacionPOS.Models;
using SistemaFacturacionPOS.Models.ViewModels;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaFacturacionPOS.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SistemaFacturacionPOSContext _context;

        public HomeController(ILogger<HomeController> logger, SistemaFacturacionPOSContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var hoy = DateTimeOffset.Now.Date;
            var inicioSemana = DateTimeOffset.Now.AddDays(-7);

            var model = new DashboardViewModel();

            // Ventas de hoy
            var ventasHoy = await _context.Ventas
                .Where(v => v.CreatedAt >= hoy && v.Estado == "COMPLETADA")
                .ToListAsync();
            
            model.VentasHoy = ventasHoy.Sum(v => v.TotalFinal);
            model.TransaccionesHoy = ventasHoy.Count;

            // Productos
            model.ProductosTotales = await _context.Productos.CountAsync(p => p.DeletedAt == null);
            model.ProductosStockBajo = await _context.Productos
                .CountAsync(p => p.DeletedAt == null && p.StockActual <= p.StockMinimo);

            // Facturas de la semana
            model.FacturasSemana = await _context.Ventas
                .CountAsync(v => v.CreatedAt >= inicioSemana && v.Estado == "COMPLETADA");

            // Saldo de Caja (Última sesión abierta o última cerrada)
            var sesionActiva = await _context.CajaSesiones
                .Include(s => s.Ventas)
                .OrderByDescending(s => s.AbiertaAt)
                .FirstOrDefaultAsync();

            if (sesionActiva != null)
            {
                var ventasSesion = sesionActiva.Ventas.Where(v => v.Estado == "COMPLETADA").Sum(v => v.TotalFinal);
                model.SaldoCaja = sesionActiva.MontoApertura + ventasSesion;
            }

            // Actividad Reciente (Auditoría)
            var logs = await _context.VistaLogs
                .OrderByDescending(l => l.fecha_hora)
                .Take(5)
                .ToListAsync();

            foreach (var log in logs)
            {
                model.Actividades.Add(new ActividadRecienteViewModel
                {
                    Titulo = $"{log.accion}: {log.tabla_afectada}",
                    Descripcion = $"Usuario: {log.username}",
                    Valor = log.accion == "VENTA" ? "Transacción" : "Cambio sistema",
                    Tipo = log.tabla_afectada,
                    Fecha = log.fecha_hora ?? DateTimeOffset.Now,
                    TiempoRelativo = GetRelativeTime(log.fecha_hora ?? DateTimeOffset.Now)
                });
            }

            return View(model);
        }

        private string GetRelativeTime(DateTimeOffset dateTime)
        {
            var timeSpan = DateTimeOffset.Now - dateTime;

            if (timeSpan <= TimeSpan.FromSeconds(60))
                return "Hace un momento";
            if (timeSpan <= TimeSpan.FromMinutes(60))
                return $"Hace {timeSpan.Minutes} min";
            if (timeSpan <= TimeSpan.FromHours(24))
                return $"Hace {timeSpan.Hours} hrs";
            
            return dateTime.ToString("dd/MM/yyyy");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
