using Microsoft.EntityFrameworkCore;
using SistemaFacturacionPOS.Contexto;
using SistemaFacturacionPOS.Models;
using SistemaFacturacionPOS.Repositories.Interfaces;

namespace SistemaFacturacionPOS.Repositories
{
    public class CajaRepository : ICajaRepository
    {
        private readonly SistemaFacturacionPOSContext _context;

        public CajaRepository(SistemaFacturacionPOSContext context)
        {
            _context = context;
        }

        public Task<CajaSesion?> GetSesionActivaConVentasAsync(Guid userId)
        {
            return _context.CajaSesiones
                .Include(c => c.Ventas)
                .Where(c => c.UsuarioId == userId && c.Estado == true)
                .OrderByDescending(c => c.AbiertaAt)
                .FirstOrDefaultAsync();
        }

        public Task<bool> TieneSesionActivaAsync(Guid userId)
        {
            return _context.CajaSesiones
                .AnyAsync(c => c.UsuarioId == userId && c.Estado == true);
        }

        public void AddSesion(CajaSesion sesion)
        {
            _context.CajaSesiones.Add(sesion);
        }

        public Task SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }
    }
}
