using Microsoft.EntityFrameworkCore;
using SistemaFacturacionPOS.Contexto;
using SistemaFacturacionPOS.Models;
using SistemaFacturacionPOS.Repositories.Interfaces;

namespace SistemaFacturacionPOS.Repositories
{
    public class BodegasRepository : IBodegasRepository
    {
        private readonly SistemaFacturacionPOSContext _context;

        public BodegasRepository(SistemaFacturacionPOSContext context)
        {
            _context = context;
        }

        public Task<List<Bodega>> GetBodegasActivasAsync()
        {
            return _context.Bodegas
                .Where(b => b.DeletedAt == null)
                .Select(b => new Bodega { Id = b.Id, Nombre = b.Nombre, Descripcion = b.Descripcion })
                .ToListAsync();
        }

        public Task<Bodega?> GetBodegaAsync(Guid id)
        {
            return _context.Bodegas.FindAsync(id).AsTask();
        }

        public Task<bool> TieneStockAsignadoAsync(Guid bodegaId)
        {
            return _context.ProductoBodegas.AnyAsync(pb => pb.BodegaId == bodegaId);
        }

        public void AddBodega(Bodega b)
        {
            _context.Bodegas.Add(b);
        }

        public Task SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }
    }
}
