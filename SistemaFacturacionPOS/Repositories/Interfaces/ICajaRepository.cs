using SistemaFacturacionPOS.Models;

namespace SistemaFacturacionPOS.Repositories.Interfaces
{
    public interface ICajaRepository
    {
        Task<CajaSesion?> GetSesionActivaConVentasAsync(Guid userId);
        Task<bool>        TieneSesionActivaAsync(Guid userId);
        void              AddSesion(CajaSesion sesion);
        Task              SaveChangesAsync();
    }
}
