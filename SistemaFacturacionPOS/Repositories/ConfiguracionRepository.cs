using Microsoft.EntityFrameworkCore;
using SistemaFacturacionPOS.Contexto;
using SistemaFacturacionPOS.Models;
using SistemaFacturacionPOS.Repositories.Interfaces;

namespace SistemaFacturacionPOS.Repositories
{
    public class ConfiguracionRepository : IConfiguracionRepository
    {
        private readonly SistemaFacturacionPOSContext _context;

        public ConfiguracionRepository(SistemaFacturacionPOSContext context)
        {
            _context = context;
        }

        public Task<Usuario?> GetUsuarioConRolAsync(Guid userId)
        {
            return _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public Task<Usuario?> GetUsuarioAsync(Guid userId)
        {
            return _context.Usuarios.FindAsync(userId).AsTask();
        }

        public Task<Empresa?> GetEmpresaAsync()
        {
            return _context.Empresa.FirstOrDefaultAsync();
        }

        public Task<int> CountSesionesActivasAsync()
        {
            return _context.CajaSesiones.CountAsync(s => s.Estado == true);
        }

        public void AddEmpresa(Empresa e)
        {
            _context.Empresa.Add(e);
        }

        public Task SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }
    }
}
