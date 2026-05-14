using SistemaFacturacionPOS.Models;

namespace SistemaFacturacionPOS.Repositories.Interfaces
{
    public interface IBodegasRepository
    {
        Task<List<Bodega>> GetBodegasActivasAsync();
        Task<Bodega?>      GetBodegaAsync(Guid id);
        Task<bool>         TieneStockAsignadoAsync(Guid bodegaId);
        void               AddBodega(Bodega b);
        Task               SaveChangesAsync();
    }
}
