using SistemaFacturacionPOS.Models;
using SistemaFacturacionPOS.Models.ViewModels;

namespace SistemaFacturacionPOS.Repositories.Interfaces
{
    public interface IHomeRepository
    {
        Task<List<Venta>>     GetVentasDesdeAsync(DateTimeOffset desde);
        Task<int>             CountProductosActivosAsync();
        Task<int>             CountProductosStockBajoAsync();
        Task<int>             CountFacturasSemanaAsync(DateTimeOffset desde);
        Task<CajaSesion?>     GetUltimaSesionAsync();
        Task<List<VistaLogs>> GetLogsRecientesAsync(int cantidad);
    }
}
