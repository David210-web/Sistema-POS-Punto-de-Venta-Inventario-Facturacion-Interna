using SistemaFacturacionPOS.Models;

namespace SistemaFacturacionPOS.Services.Interfaces
{
    public interface IFacturacionService
    {
        Task<IEnumerable<object>>                        GetVentasAsync(string? folio, string? date);
        Task<IEnumerable<object>>                        GetSolicitudesAsync();
        Task<(bool ok, string msg)>                      SolicitarAnulacionAsync(Guid ventaId, Guid userId, string motivo);
        Task<(bool ok, string msg, Guid? notaId)>        AprobarAnulacionAsync(Guid solicitudId, Guid userId, string password);
        Task<(bool ok, string msg)>                      RechazarAnulacionAsync(Guid solicitudId, Guid userId, string motivoRechazo);
        Task<(bool ok, string msg, Guid? notaId)>        AnularDirectoAsync(Guid ventaId, Guid userId, string password);
        Task<NotaCredito?>                               GetNotaCreditoAsync(Guid ventaId);
        Task<(bool exists, Guid? id)>                   HasNotaCreditoAsync(Guid ventaId);
    }
}
