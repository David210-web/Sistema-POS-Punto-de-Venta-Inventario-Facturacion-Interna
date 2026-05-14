using Microsoft.EntityFrameworkCore;
using SistemaFacturacionPOS.Contexto;
using SistemaFacturacionPOS.Models;
using SistemaFacturacionPOS.Repositories.Interfaces;

namespace SistemaFacturacionPOS.Repositories
{
    public class POSRepository : IPOSRepository
    {
        private readonly SistemaFacturacionPOSContext _context;

        public POSRepository(SistemaFacturacionPOSContext context)
        {
            _context = context;
        }

        public Task<bool> TieneSesionActivaAsync(Guid userId)
        {
            return _context.CajaSesiones
                .AnyAsync(c => c.UsuarioId == userId && c.Estado == true);
        }

        public Task<CajaSesion?> GetSesionActivaAsync(Guid userId)
        {
            return _context.CajaSesiones
                .FirstOrDefaultAsync(c => c.UsuarioId == userId && c.Estado == true);
        }

        public async Task<List<Producto>> BuscarProductosAsync(string? q)
        {
            var queryBase = _context.Productos.Where(p => p.DeletedAt == null);

            if (!string.IsNullOrWhiteSpace(q))
            {
                var query = q.ToLower();
                queryBase = queryBase.Where(p => p.Nombre.ToLower().Contains(query) || p.CodigoBarras == q);
            }

            return await queryBase
                .OrderBy(p => p.Nombre)
                .Take(20)
                .ToListAsync();
        }

        public Task<Producto?> GetProductoAsync(Guid id)
        {
            return _context.Productos.FindAsync(id).AsTask();
        }

        public Task<Venta?> GetVentaTicketAsync(Guid id)
        {
            return _context.Ventas
                .Include(v => v.Usuario)
                .Include(v => v.VentaDetalles)
                .ThenInclude(vd => vd.Producto)
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public void AddVenta(Venta venta)
        {
            _context.Ventas.Add(venta);
        }

        public Task SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }
    }
}
