using SistemaFacturacionPOS.Models;

namespace SistemaFacturacionPOS.Repositories.Interfaces
{
    public interface ILoginRepository
    {
        Task<Usuario?> GetUsuarioActivoAsync(string username);
    }
}
