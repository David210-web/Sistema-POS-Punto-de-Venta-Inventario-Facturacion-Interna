using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SistemaFacturacionPOS.Contexto;
using SistemaFacturacionPOS.Models;
using SistemaFacturacionPOS.Repositories.Interfaces;

namespace SistemaFacturacionPOS.Repositories
{
    public class FacturacionRepository : IFacturacionRepository
    {
        private readonly SistemaFacturacionPOSContext _context;

        public FacturacionRepository(SistemaFacturacionPOSContext context)
        {
            _context = context;
        }

        public async Task<List<Venta>> GetVentasAsync(string? folio, string? date)
        {
            var query = _context.Ventas
                .Include(v => v.Usuario)
                .Include(v => v.CajaSesion)
                .AsQueryable();

            if (!string.IsNullOrEmpty(folio))
            {
                var folioLimpio = folio.ToUpper().Replace("V", "").TrimStart('0');
                if (string.IsNullOrEmpty(folioLimpio)) folioLimpio = "0";
                if (int.TryParse(folioLimpio, out int folioInt))
                {
                    query = query.Where(v => v.FolioInterno == folioInt);
                }
            }

            if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out DateTime parsedDate))
            {
                query = query.Where(v => v.CreatedAt.HasValue && v.CreatedAt.Value.Date == parsedDate.Date);
            }

            return await query.OrderByDescending(v => v.CreatedAt).ToListAsync();
        }

        public Task<List<VentaAnulacionSolicitud>> GetSolicitudesPendientesAsync()
        {
            return _context.VentaAnulacionSolicitudes
                .Include(s => s.Venta)
                .Include(s => s.UsuarioSolicita)
                .Where(s => s.Estado == "PENDIENTE")
                .ToListAsync();
        }

        public Task<Venta?> GetVentaConDetallesAsync(Guid ventaId)
        {
            return _context.Ventas
                .Include(v => v.VentaDetalles)
                .FirstOrDefaultAsync(v => v.Id == ventaId);
        }

        public Task<VentaAnulacionSolicitud?> GetSolicitudAsync(Guid solicitudId)
        {
            return _context.VentaAnulacionSolicitudes.FindAsync(solicitudId).AsTask();
        }

        public Task<Usuario?> GetUsuarioConRolAsync(Guid userId)
        {
            return _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public Task<Producto?> GetProductoAsync(Guid id)
        {
            return _context.Productos.FindAsync(id).AsTask();
        }

        public Task<NotaCredito?> GetNotaCreditoByVentaAsync(Guid ventaId)
        {
            return _context.NotasCredito
                .Include(n => n.Venta)
                .ThenInclude(v => v.VentaDetalles)
                .ThenInclude(vd => vd.Producto)
                .Include(n => n.Venta.Usuario)
                .FirstOrDefaultAsync(n => n.VentaId == ventaId);
        }

        public void AddSolicitud(VentaAnulacionSolicitud s)
        {
            _context.VentaAnulacionSolicitudes.Add(s);
        }

        public void AddNotaCredito(NotaCredito nc)
        {
            _context.NotasCredito.Add(nc);
        }

        public Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return _context.Database.BeginTransactionAsync();
        }

        public Task SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }
    }
}
