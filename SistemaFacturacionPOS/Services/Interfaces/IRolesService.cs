using SistemaFacturacionPOS.Models;

namespace SistemaFacturacionPOS.Services.Interfaces
{
    public interface IRolesService
    {
        Task<(bool ok, object? data, string msg)> GetRolesAsync();
        Task<(bool ok, string msg)>               CreateRolAsync(Rol r);
        Task<(bool ok, string msg)>               UpdateRolAsync(Guid id, Rol r);
        Task<(bool ok, string msg)>               DeleteRolAsync(Guid id);
    }
}
