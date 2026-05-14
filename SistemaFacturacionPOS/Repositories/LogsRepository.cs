using Microsoft.EntityFrameworkCore;
using SistemaFacturacionPOS.Contexto;
using SistemaFacturacionPOS.Models.ViewModels;
using SistemaFacturacionPOS.Repositories.Interfaces;

namespace SistemaFacturacionPOS.Repositories
{
    public class LogsRepository : ILogsRepository
    {
        private readonly SistemaFacturacionPOSContext _context;

        public LogsRepository(SistemaFacturacionPOSContext context)
        {
            _context = context;
        }

        public async Task<List<VistaLogs>> GetLogsAsync(DateTime? startDate, DateTime? endDate)
        {
            var query = _context.VistaLogs.AsQueryable();

            if (startDate.HasValue)
            {
                var start = startDate.Value.Date;
                query = query.Where(l => l.fecha_hora >= start);
            }
            else
            {
                var today = DateTime.Today;
                query = query.Where(l => l.fecha_hora >= today);
            }

            if (endDate.HasValue)
            {
                var end = endDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(l => l.fecha_hora <= end);
            }

            return await query.OrderByDescending(l => l.fecha_hora).ToListAsync();
        }
    }
}
