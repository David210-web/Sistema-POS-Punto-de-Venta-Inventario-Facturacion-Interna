using SistemaFacturacionPOS.Models;

namespace SistemaFacturacionPOS.Services.Interfaces
{
    public interface IProductosService
    {
        Task<(bool ok, object? data, string msg)> GetProductosAsync();
        Task<(bool ok, string msg)>               AgregarProductoAsync(Producto p);
        Task<(bool ok, string msg)>               ActualizarProductoAsync(Guid id, Producto p);
        Task<(bool ok, string msg)>               EliminarProductoAsync(Guid id);
        Task<(bool ok, string msg)>               AjustarStockAsync(Guid id, int cantidad, string justificacion, Guid? userId);
    }
}
