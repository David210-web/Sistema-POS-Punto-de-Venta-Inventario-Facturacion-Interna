using Microsoft.EntityFrameworkCore.Storage;
using SistemaFacturacionPOS.Models;

namespace SistemaFacturacionPOS.Repositories.Interfaces
{
    public interface IProductosRepository
    {
        Task<List<Producto>>        GetProductosActivosAsync();
        Task<Producto?>             GetProductoAsync(Guid id);
        void                        AddProducto(Producto p);
        void                        AddMovimiento(InventarioMovimiento m);
        void                        AddAuditoria(AuditoriaLog a);
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task                        SaveChangesAsync();
    }
}
