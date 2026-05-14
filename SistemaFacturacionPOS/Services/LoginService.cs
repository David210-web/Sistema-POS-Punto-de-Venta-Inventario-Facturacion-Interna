using SistemaFacturacionPOS.Managers;
using SistemaFacturacionPOS.Models;
using SistemaFacturacionPOS.Repositories.Interfaces;
using SistemaFacturacionPOS.Services.Interfaces;

namespace SistemaFacturacionPOS.Services
{
    public class LoginService : ILoginService
    {
        private readonly ILoginRepository _repo;

        public LoginService(ILoginRepository repo)
        {
            _repo = repo;
        }

        public async Task<Usuario?> AutenticarAsync(string username, string password)
        {
            var usuario = await _repo.GetUsuarioActivoAsync(username);
            if (usuario == null) return null;

            if (!EncriptManager.Verify(password, usuario.PasswordHash)) return null;

            return usuario;
        }
    }
}
