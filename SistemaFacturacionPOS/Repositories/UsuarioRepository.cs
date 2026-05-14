using Microsoft.EntityFrameworkCore;
using SistemaFacturacionPOS.Contexto;
using SistemaFacturacionPOS.Models;
using SistemaFacturacionPOS.Repositories.Interfaces;

namespace SistemaFacturacionPOS.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly SistemaFacturacionPOSContext _context;

        public UsuarioRepository(SistemaFacturacionPOSContext context)
        {
            _context = context;
        }

        public Task<List<Usuario>> GetUsuariosAsync()
        {
            return _context.Usuarios
                .Include(u => u.Rol)
                .ToListAsync();
        }

        public Task<Usuario?> GetUsuarioAsync(Guid id)
        {
            return _context.Usuarios.FindAsync(id).AsTask();
        }

        public void AddUsuario(Usuario u)
        {
            _context.Usuarios.Add(u);
        }

        public Task SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }
    }
}
