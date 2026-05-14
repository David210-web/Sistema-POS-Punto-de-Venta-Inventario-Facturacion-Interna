using SistemaFacturacionPOS.Models.ViewModels;

namespace SistemaFacturacionPOS.Repositories.Interfaces
{
    public interface ILogsRepository
    {
        Task<List<VistaLogs>> GetLogsAsync(DateTime? startDate, DateTime? endDate);
    }
}
