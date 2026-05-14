using SistemaFacturacionPOS.Models;

namespace SistemaFacturacionPOS.Services.Interfaces
{
    public interface ILoginService
    {
        Task<Usuario?> AutenticarAsync(string username, string password);
    }
}
