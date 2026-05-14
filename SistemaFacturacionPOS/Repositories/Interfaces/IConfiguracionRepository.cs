using SistemaFacturacionPOS.Models;

namespace SistemaFacturacionPOS.Repositories.Interfaces
{
    public interface IConfiguracionRepository
    {
        Task<Usuario?>  GetUsuarioConRolAsync(Guid userId);
        Task<Usuario?>  GetUsuarioAsync(Guid userId);
        Task<Empresa?>  GetEmpresaAsync();
        Task<int>       CountSesionesActivasAsync();
        void            AddEmpresa(Empresa e);
        Task            SaveChangesAsync();
    }
}
