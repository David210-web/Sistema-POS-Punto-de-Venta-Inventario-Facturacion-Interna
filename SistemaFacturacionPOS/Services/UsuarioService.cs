using SistemaFacturacionPOS.Managers;
using SistemaFacturacionPOS.Models;
using SistemaFacturacionPOS.Repositories.Interfaces;
using SistemaFacturacionPOS.Services.Interfaces;

namespace SistemaFacturacionPOS.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _repo;

        public UsuarioService(IUsuarioRepository repo) { _repo = repo; }

        public async Task<(bool ok, object? data, string msg)> GetUsuariosAsync()
        {
            try { var u = await _repo.GetUsuariosAsync(); return (true, u, string.Empty); }
            catch (Exception ex) { return (false, null, ex.Message); }
        }

        public async Task<(bool ok, string msg)> CreateUsuarioAsync(Usuario u)
        {
            try
            {
                u.PasswordHash = EncriptManager.Generate(u.PasswordHash);
                _repo.AddUsuario(u);
                await _repo.SaveChangesAsync();
                return (true, "Usuario creado exitosamente");
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        public async Task<(bool ok, string msg)> UpdateUsuarioAsync(Guid id, Usuario u)
        {
            try
            {
                var existing = await _repo.GetUsuarioAsync(id);
                if (existing == null) return (false, "Usuario no encontrado");
                existing.Username  = u.Username;
                existing.RolId     = u.RolId;
                existing.UpdatedAt = DateTimeOffset.Now;
                await _repo.SaveChangesAsync();
                return (true, "Usuario actualizado exitosamente");
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        public async Task<(bool ok, string msg)> PatchUsuarioAsync(Guid id, Usuario u)
        {
            try
            {
                var existing = await _repo.GetUsuarioAsync(id);
                if (existing == null) return (false, "Usuario no encontrado");
                existing.Activo    = u.Activo ?? existing.Activo;
                existing.UpdatedAt = DateTimeOffset.Now;
                await _repo.SaveChangesAsync();
                return (true, "Usuario actualizado exitosamente");
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        public async Task<(bool ok, string msg)> RestablecerContraseñaAsync(Guid id)
        {
            try
            {
                var existing = await _repo.GetUsuarioAsync(id);
                if (existing == null) return (false, "Usuario no encontrado");
                existing.PasswordHash = existing.Username;
                existing.UpdatedAt    = DateTimeOffset.Now;
                await _repo.SaveChangesAsync();
                return (true, "Contraseña restablecida exitosamente");
            }
            catch (Exception ex) { return (false, ex.Message); }
        }
    }
}
