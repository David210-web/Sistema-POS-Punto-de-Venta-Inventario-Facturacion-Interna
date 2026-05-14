using Microsoft.EntityFrameworkCore;
using SistemaFacturacionPOS.Contexto;
using SistemaFacturacionPOS.Models;
using SistemaFacturacionPOS.Models.ViewModels;
using SistemaFacturacionPOS.Repositories.Interfaces;

namespace SistemaFacturacionPOS.Repositories
{
    public class HomeRepository : IHomeRepository
    {
        private readonly SistemaFacturacionPOSContext _context;

        public HomeRepository(SistemaFacturacionPOSContext context)
        {
            _context = context;
        }

        public Task<List<Venta>> GetVentasDesdeAsync(DateTimeOffset desde)
        {
            return _context.Ventas
                .Where(v => v.CreatedAt >= desde && v.Estado == "COMPLETADA")
                .ToListAsync();
        }

        public Task<int> CountProductosActivosAsync()
        {
            return _context.Productos.CountAsync(p => p.DeletedAt == null);
        }

        public Task<int> CountProductosStockBajoAsync()
        {
            return _context.Productos
                .CountAsync(p => p.DeletedAt == null && p.StockActual <= p.StockMinimo);
        }

        public Task<int> CountFacturasSemanaAsync(DateTimeOffset desde)
        {
            return _context.Ventas
                .CountAsync(v => v.CreatedAt >= desde && v.Estado == "COMPLETADA");
        }

        public Task<CajaSesion?> GetUltimaSesionAsync()
        {
            return _context.CajaSesiones
                .Include(s => s.Ventas)
                .OrderByDescending(s => s.AbiertaAt)
                .FirstOrDefaultAsync();
        }

        public Task<List<VistaLogs>> GetLogsRecientesAsync(int cantidad)
        {
            return _context.VistaLogs
                .OrderByDescending(l => l.fecha_hora)
                .Take(cantidad)
                .ToListAsync();
        }
    }
}
