using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SistemaFacturacionPOS.Contexto;
using SistemaFacturacionPOS.Models;
using SistemaFacturacionPOS.Models.ViewModels;
using SistemaFacturacionPOS.Repositories.Interfaces;

namespace SistemaFacturacionPOS.Repositories
{
    public class ProductoBodegaRepository : IProductoBodegaRepository
    {
        private readonly SistemaFacturacionPOSContext _context;

        public ProductoBodegaRepository(SistemaFacturacionPOSContext context)
        {
            _context = context;
        }

        public Task<List<VistaProductosBodegas>> GetExistenciasAsync(Guid productoId)
        {
            return _context.VistaProductosBodegas
                .Where(vpb => vpb.ProductoId == productoId)
                .ToListAsync();
        }

        public Task<bool> ExisteRelacionAsync(Guid productoId, Guid bodegaId)
        {
            return _context.ProductoBodegas
                .AnyAsync(pb => pb.ProductoId == productoId && pb.BodegaId == bodegaId);
        }

        public Task<ProductoBodega?> GetExistenciaAsync(Guid id)
        {
            return _context.ProductoBodegas.FindAsync(id).AsTask();
        }

        public async Task<int> SumStockByProductoAsync(Guid productoId)
        {
            return await _context.ProductoBodegas
                .Where(pb => pb.ProductoId == productoId)
                .SumAsync(pb => (int?)pb.Stock) ?? 0;
        }

        public Task<Producto?> GetProductoAsync(Guid id)
        {
            return _context.Productos.FindAsync(id).AsTask();
        }

        public void AddExistencia(ProductoBodega pb)
        {
            _context.ProductoBodegas.Add(pb);
        }

        public void RemoveExistencia(ProductoBodega pb)
        {
            _context.ProductoBodegas.Remove(pb);
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
