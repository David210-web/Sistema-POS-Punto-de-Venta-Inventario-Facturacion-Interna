using SistemaFacturacionPOS.Models.ViewModels;

namespace SistemaFacturacionPOS.Services.Interfaces
{
    public interface ILogsService
    {
        Task<List<VistaLogs>> GetLogsAsync(DateTime? startDate, DateTime? endDate);
    }
}
