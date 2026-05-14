using SistemaFacturacionPOS.Models;

namespace SistemaFacturacionPOS.Services.Interfaces
{
    public interface IBodegasService
    {
        Task<(bool ok, object? data, string msg)> GetBodegasAsync();
        Task<(bool ok, string msg)>               AgregarBodegaAsync(Bodega b);
        Task<(bool ok, string msg)>               ActualizarBodegaAsync(Guid id, Bodega b);
        Task<(bool ok, string msg)>               EliminarBodegaAsync(Guid id);
    }
}
