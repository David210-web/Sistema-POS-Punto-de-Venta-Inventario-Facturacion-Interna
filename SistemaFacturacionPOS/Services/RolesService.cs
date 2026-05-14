using SistemaFacturacionPOS.Models;
using SistemaFacturacionPOS.Repositories.Interfaces;
using SistemaFacturacionPOS.Services.Interfaces;

namespace SistemaFacturacionPOS.Services
{
    public class RolesService : IRolesService
    {
        private readonly IRolesRepository _repo;

        public RolesService(IRolesRepository repo) { _repo = repo; }

        public async Task<(bool ok, object? data, string msg)> GetRolesAsync()
        {
            try { var r = await _repo.GetRolesAsync(); return (true, r, string.Empty); }
            catch (Exception ex) { return (false, null, ex.Message); }
        }

        public async Task<(bool ok, string msg)> CreateRolAsync(Rol r)
        {
            try
            {
                _repo.AddRol(r);
                await _repo.SaveChangesAsync();
                return (true, "Rol creado exitosamente");
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        public async Task<(bool ok, string msg)> UpdateRolAsync(Guid id, Rol r)
        {
            try
            {
                var existing = await _repo.GetRolAsync(id);
                if (existing == null) return (false, "Rol no encontrado");
                existing.Nombre      = r.Nombre;
                existing.Descripcion = r.Descripcion;
                await _repo.SaveChangesAsync();
                return (true, "Rol actualizado exitosamente");
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        public async Task<(bool ok, string msg)> DeleteRolAsync(Guid id)
        {
            try
            {
                var existing = await _repo.GetRolAsync(id);
                if (existing == null) return (false, "Rol no encontrado");
                _repo.RemoveRol(existing);
                await _repo.SaveChangesAsync();
                return (true, "Rol eliminado exitosamente");
            }
            catch (Exception ex) { return (false, ex.Message); }
        }
    }
}
