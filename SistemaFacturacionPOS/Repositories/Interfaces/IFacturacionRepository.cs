using Microsoft.EntityFrameworkCore.Storage;
using SistemaFacturacionPOS.Models;

namespace SistemaFacturacionPOS.Repositories.Interfaces
{
    public interface IFacturacionRepository
    {
        Task<List<Venta>>                   GetVentasAsync(string? folio, string? date);
        Task<List<VentaAnulacionSolicitud>> GetSolicitudesPendientesAsync();
        Task<Venta?>                        GetVentaConDetallesAsync(Guid ventaId);
        Task<VentaAnulacionSolicitud?>      GetSolicitudAsync(Guid solicitudId);
        Task<Usuario?>                      GetUsuarioConRolAsync(Guid userId);
        Task<Producto?>                     GetProductoAsync(Guid id);
        Task<NotaCredito?>                  GetNotaCreditoByVentaAsync(Guid ventaId);
        void                                AddSolicitud(VentaAnulacionSolicitud s);
        void                                AddNotaCredito(NotaCredito nc);
        Task<IDbContextTransaction>         BeginTransactionAsync();
        Task                                SaveChangesAsync();
    }
}
