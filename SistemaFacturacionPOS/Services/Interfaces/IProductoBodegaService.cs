using SistemaFacturacionPOS.DTOs;

namespace SistemaFacturacionPOS.Services.Interfaces
{
    public interface IProductoBodegaService
    {
        Task<(bool ok, object? data, string msg)> GetExistenciasAsync(Guid productoId);
        Task<(bool ok, string msg)>               AgregarExistenciaAsync(ExistenciaRequest request);
        Task<(bool ok, string msg)>               ActualizarExistenciaAsync(Guid id, ExistenciaRequest request);
        Task<(bool ok, string msg)>               EliminarExistenciaAsync(Guid id);
    }
}
