using Microsoft.EntityFrameworkCore;
using SistemaFacturacionPOS.Contexto;
using SistemaFacturacionPOS.Models;
using SistemaFacturacionPOS.Repositories.Interfaces;

namespace SistemaFacturacionPOS.Repositories
{
    public class LoginRepository : ILoginRepository
    {
        private readonly SistemaFacturacionPOSContext _context;

        public LoginRepository(SistemaFacturacionPOSContext context)
        {
            _context = context;
        }

        public Task<Usuario?> GetUsuarioActivoAsync(string username)
        {
            return _context.Usuarios
                .Include(u => u.Rol)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username == username && u.Activo == true);
        }
    }
}
