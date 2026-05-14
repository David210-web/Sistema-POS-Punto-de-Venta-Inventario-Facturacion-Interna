using Microsoft.EntityFrameworkCore;
using SistemaFacturacionPOS.Contexto;
using SistemaFacturacionPOS.Models;
using SistemaFacturacionPOS.Repositories.Interfaces;

namespace SistemaFacturacionPOS.Repositories
{
    public class RolesRepository : IRolesRepository
    {
        private readonly SistemaFacturacionPOSContext _context;

        public RolesRepository(SistemaFacturacionPOSContext context)
        {
            _context = context;
        }

        public Task<List<Rol>> GetRolesAsync()
        {
            return _context.Roles.ToListAsync();
        }

        public Task<Rol?> GetRolAsync(Guid id)
        {
            return _context.Roles.FindAsync(id).AsTask();
        }

        public void AddRol(Rol r)
        {
            _context.Roles.Add(r);
        }

        public void RemoveRol(Rol r)
        {
            _context.Roles.Remove(r);
        }

        public Task SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }
    }
}
