using SistemaFacturacionPOS.Models;
using SistemaFacturacionPOS.Models.ViewModels;

namespace SistemaFacturacionPOS.Services.Interfaces
{
    public interface IPOSService
    {
        Task<bool>                              TieneSesionActivaAsync(Guid userId);
        Task<IEnumerable<object>>               BuscarProductosAsync(string? q);
        Task<(bool ok, Guid ventaId, string error)> FinalizarVentaAsync(Guid userId, VentaRequestDTO request);
        Task<Venta?>                            GetTicketAsync(Guid id);
    }
}
