using SistemaFacturacionPOS.Models.ViewModels;

namespace SistemaFacturacionPOS.Services.Interfaces
{
    public interface ICajaService
    {
        Task<CajaViewModel>         GetResumenAsync(Guid userId);
        Task<(bool ok, string msg)> AbrirCajaAsync(Guid userId, decimal monto);
        Task<(bool ok, string msg)> CerrarCajaAsync(Guid userId, decimal montoFisico);
    }
}
