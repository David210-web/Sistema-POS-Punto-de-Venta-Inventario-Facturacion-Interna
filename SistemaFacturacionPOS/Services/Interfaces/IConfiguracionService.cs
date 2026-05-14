using SistemaFacturacionPOS.Models;

namespace SistemaFacturacionPOS.Services.Interfaces
{
    public interface IConfiguracionService
    {
        Task<(Usuario? usuario, Empresa? empresa, int sesionesActivas)> GetDatosIndexAsync(Guid userId);
        Task<(bool ok, string msg)> UpdateEmpresaAsync(string nombre, string nit, string direccion);
        Task<(bool ok, string msg)> UpdateProfileAsync(Guid userId, string nombre, string apellido);
        Task<(bool ok, string msg)> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);
    }
}
