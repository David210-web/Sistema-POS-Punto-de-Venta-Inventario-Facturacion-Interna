using SistemaFacturacionPOS.Models;

namespace SistemaFacturacionPOS.Repositories.Interfaces
{
    public interface IRolesRepository
    {
        Task<List<Rol>> GetRolesAsync();
        Task<Rol?>      GetRolAsync(Guid id);
        void            AddRol(Rol r);
        void            RemoveRol(Rol r);
        Task            SaveChangesAsync();
    }
}
