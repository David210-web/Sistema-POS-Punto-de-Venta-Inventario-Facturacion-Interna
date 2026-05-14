using SistemaFacturacionPOS.Models;

namespace SistemaFacturacionPOS.Repositories.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<List<Usuario>> GetUsuariosAsync();
        Task<Usuario?>      GetUsuarioAsync(Guid id);
        void                AddUsuario(Usuario u);
        Task                SaveChangesAsync();
    }
}
