using Microsoft.EntityFrameworkCore.Storage;
using SistemaFacturacionPOS.Models;
using SistemaFacturacionPOS.Models.ViewModels;

namespace SistemaFacturacionPOS.Repositories.Interfaces
{
    public interface IProductoBodegaRepository
    {
        Task<List<VistaProductosBodegas>> GetExistenciasAsync(Guid productoId);
        Task<bool>                        ExisteRelacionAsync(Guid productoId, Guid bodegaId);
        Task<ProductoBodega?>             GetExistenciaAsync(Guid id);
        Task<int>                         SumStockByProductoAsync(Guid productoId);
        Task<Producto?>                   GetProductoAsync(Guid id);
        void                              AddExistencia(ProductoBodega pb);
        void                              RemoveExistencia(ProductoBodega pb);
        Task<IDbContextTransaction>       BeginTransactionAsync();
        Task                              SaveChangesAsync();
    }
}
