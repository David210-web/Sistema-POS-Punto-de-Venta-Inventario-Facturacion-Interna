using SistemaFacturacionPOS.Managers;
using SistemaFacturacionPOS.Models;
using SistemaFacturacionPOS.Repositories.Interfaces;
using SistemaFacturacionPOS.Services.Interfaces;

namespace SistemaFacturacionPOS.Services
{
    public class ConfiguracionService : IConfiguracionService
    {
        private readonly IConfiguracionRepository _repo;

        public ConfiguracionService(IConfiguracionRepository repo) { _repo = repo; }

        public async Task<(Usuario? usuario, Empresa? empresa, int sesionesActivas)> GetDatosIndexAsync(Guid userId)
        {
            var usuario        = await _repo.GetUsuarioConRolAsync(userId);
            var empresa        = await _repo.GetEmpresaAsync();
            var sesionesActivas = await _repo.CountSesionesActivasAsync();
            return (usuario, empresa, sesionesActivas);
        }

        public async Task<(bool ok, string msg)> UpdateEmpresaAsync(string nombre, string nit, string direccion)
        {
            try
            {
                var existingEmpresa = await _repo.GetEmpresaAsync();
                if (existingEmpresa == null)
                {
                    _repo.AddEmpresa(new Empresa { Nombre = nombre, Nit = nit, Direccion = direccion });
                }
                else
                {
                    existingEmpresa.Nombre    = nombre;
                    existingEmpresa.Nit       = nit;
                    existingEmpresa.Direccion = direccion;
                }
                await _repo.SaveChangesAsync();
                return (true, "Datos de la empresa actualizados correctamente.");
            }
            catch (Exception ex) { return (false, "Error al actualizar los datos: " + ex.Message); }
        }

        public async Task<(bool ok, string msg)> UpdateProfileAsync(Guid userId, string nombre, string apellido)
        {
            try
            {
                var usuario = await _repo.GetUsuarioAsync(userId);
                if (usuario == null) return (false, "Usuario no encontrado.");
                usuario.Nombre    = nombre;
                usuario.Apellido  = apellido;
                usuario.UpdatedAt = DateTimeOffset.Now;
                await _repo.SaveChangesAsync();
                return (true, "Perfil actualizado correctamente.");
            }
            catch (Exception ex) { return (false, "Error al actualizar el perfil: " + ex.Message); }
        }

        public async Task<(bool ok, string msg)> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
        {
            try
            {
                var usuario = await _repo.GetUsuarioAsync(userId);
                if (usuario == null) return (false, "Usuario no encontrado.");
                if (!EncriptManager.Verify(currentPassword, usuario.PasswordHash))
                    return (false, "La contraseña actual es incorrecta.");
                usuario.PasswordHash = EncriptManager.Generate(newPassword);
                usuario.UpdatedAt    = DateTimeOffset.Now;
                await _repo.SaveChangesAsync();
                return (true, "Contraseña cambiada correctamente.");
            }
            catch (Exception ex) { return (false, "Error al cambiar la contraseña: " + ex.Message); }
        }
    }
}
