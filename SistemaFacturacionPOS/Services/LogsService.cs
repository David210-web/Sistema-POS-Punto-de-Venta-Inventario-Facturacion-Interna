using SistemaFacturacionPOS.Models.ViewModels;
using SistemaFacturacionPOS.Repositories.Interfaces;
using SistemaFacturacionPOS.Services.Interfaces;

namespace SistemaFacturacionPOS.Services
{
    public class LogsService : ILogsService
    {
        private readonly ILogsRepository _repo;

        public LogsService(ILogsRepository repo) { _repo = repo; }

        public Task<List<VistaLogs>> GetLogsAsync(DateTime? startDate, DateTime? endDate)
        {
            return _repo.GetLogsAsync(startDate, endDate);
        }
    }
}
