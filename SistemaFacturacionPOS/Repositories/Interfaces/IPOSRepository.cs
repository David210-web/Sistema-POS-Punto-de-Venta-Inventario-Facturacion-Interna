using SistemaFacturacionPOS.Models;

namespace SistemaFacturacionPOS.Repositories.Interfaces
{
    public interface IPOSRepository
    {
        Task<bool>           TieneSesionActivaAsync(Guid userId);
        Task<CajaSesion?>    GetSesionActivaAsync(Guid userId);
        Task<List<Producto>> BuscarProductosAsync(string? q);
        Task<Producto?>      GetProductoAsync(Guid id);
        Task<Venta?>         GetVentaTicketAsync(Guid id);
        void                 AddVenta(Venta venta);
        Task                 SaveChangesAsync();
    }
}
