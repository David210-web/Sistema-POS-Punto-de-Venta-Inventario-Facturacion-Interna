using SistemaFacturacionPOS.Models;

namespace SistemaFacturacionPOS.Services.Interfaces
{
    public interface IUsuarioService
    {
        Task<(bool ok, object? data, string msg)> GetUsuariosAsync();
        Task<(bool ok, string msg)>               CreateUsuarioAsync(Usuario u);
        Task<(bool ok, string msg)>               UpdateUsuarioAsync(Guid id, Usuario u);
        Task<(bool ok, string msg)>               PatchUsuarioAsync(Guid id, Usuario u);
        Task<(bool ok, string msg)>               RestablecerContraseñaAsync(Guid id);
    }
}
