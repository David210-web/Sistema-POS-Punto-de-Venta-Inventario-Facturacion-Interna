using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SistemaFacturacionPOS.Contexto;
using SistemaFacturacionPOS.Models;
using SistemaFacturacionPOS.Repositories.Interfaces;

namespace SistemaFacturacionPOS.Repositories
{
    public class ProductosRepository : IProductosRepository
    {
        private readonly SistemaFacturacionPOSContext _context;

        public ProductosRepository(SistemaFacturacionPOSContext context)
        {
            _context = context;
        }

        public Task<List<Producto>> GetProductosActivosAsync()
        {
            return _context.Productos
                .Include(p => p.Categoria)
                .Where(p => p.DeletedAt == null)
                .ToListAsync();
        }

        public Task<Producto?> GetProductoAsync(Guid id)
        {
            return _context.Productos.FindAsync(id).AsTask();
        }

        public void AddProducto(Producto p)
        {
            _context.Productos.Add(p);
        }

        public void AddMovimiento(InventarioMovimiento m)
        {
            _context.InventarioMovimientos.Add(m);
        }

        public void AddAuditoria(AuditoriaLog a)
        {
            _context.AuditoriaLogs.Add(a);
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
