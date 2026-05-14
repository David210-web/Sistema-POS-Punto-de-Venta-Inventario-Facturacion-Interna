using SistemaFacturacionPOS.Models.ViewModels;

namespace SistemaFacturacionPOS.Services.Interfaces
{
    public interface IHomeService
    {
        Task<DashboardViewModel> GetDashboardAsync();
    }
}
