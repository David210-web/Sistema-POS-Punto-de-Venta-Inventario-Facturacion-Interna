using SistemaFacturacionPOS.Managers;
using SistemaFacturacionPOS.Models;
using SistemaFacturacionPOS.Repositories.Interfaces;
using SistemaFacturacionPOS.Services.Interfaces;

namespace SistemaFacturacionPOS.Services
{
    public class FacturacionService : IFacturacionService
    {
        private readonly IFacturacionRepository _repo;

        public FacturacionService(IFacturacionRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<object>> GetVentasAsync(string? folio, string? date)
        {
            var ventas = await _repo.GetVentasAsync(folio, date);
            return ventas.Select(v => new
            {
                id     = v.Id,
                folio  = v.FolioInterno,
                fecha  = v.CreatedAt?.ToString("dd 'de' MMM, hh:mm tt"),
                cajero = v.Usuario?.Username,
                metodo = v.MetodoPago,
                total  = v.TotalFinal.ToString("C"),
                estado = v.Estado
            });
        }

        public async Task<IEnumerable<object>> GetSolicitudesAsync()
        {
            var solicitudes = await _repo.GetSolicitudesPendientesAsync();
            return solicitudes.Select(s => new
            {
                id          = s.Id,
                ventaId     = s.VentaId,
                folioVenta  = s.Venta.FolioInterno,
                cajero      = s.UsuarioSolicita.Username,
                fecha       = s.CreatedAt.HasValue ? s.CreatedAt.Value.ToString("dd/MM/yyyy HH:mm") : "",
                motivo      = s.Motivo
            });
        }

        public async Task<(bool ok, string msg)> SolicitarAnulacionAsync(Guid ventaId, Guid userId, string motivo)
        {
            var venta = await _repo.GetVentaConDetallesAsync(ventaId);
            if (venta == null || venta.Estado == "ANULADA")
                return (false, "Venta no válida o ya anulada.");

            var solicitudes = await _repo.GetSolicitudesPendientesAsync();
            if (solicitudes.Any(s => s.VentaId == ventaId))
                return (false, "Ya existe una solicitud pendiente para esta venta.");

            var solicitud = new VentaAnulacionSolicitud
            {
                VentaId           = ventaId,
                UsuarioSolicitaId = userId,
                Motivo            = motivo,
                Estado            = "PENDIENTE",
                CreatedAt         = DateTimeOffset.Now
            };

            _repo.AddSolicitud(solicitud);
            await _repo.SaveChangesAsync();

            return (true, string.Empty);
        }

        public async Task<(bool ok, string msg, Guid? notaId)> AprobarAnulacionAsync(Guid solicitudId, Guid userId, string password)
        {
            var admin = await _repo.GetUsuarioConRolAsync(userId);
            if (admin == null || admin.Rol?.Nombre != "Administrador")
                return (false, "Acceso denegado.", null);

            password = password?.Trim() ?? "";
            bool isPasswordValid = EncriptManager.Verify(password, admin.PasswordHash);
            if (!isPasswordValid && EncriptManager.Encript(password) != admin.PasswordHash)
                return (false, "Contraseña incorrecta.", null);

            var solicitud = await _repo.GetSolicitudAsync(solicitudId);
            if (solicitud == null || solicitud.Estado != "PENDIENTE")
                return (false, "Solicitud inválida.", null);

            return await EjecutarAnulacionAsync(solicitud.VentaId, admin.Id, solicitudId);
        }

        public async Task<(bool ok, string msg)> RechazarAnulacionAsync(Guid solicitudId, Guid userId, string motivoRechazo)
        {
            var admin = await _repo.GetUsuarioConRolAsync(userId);
            if (admin == null || admin.Rol?.Nombre != "Administrador")
                return (false, "Acceso denegado.");

            var solicitud = await _repo.GetSolicitudAsync(solicitudId);
            if (solicitud == null || solicitud.Estado != "PENDIENTE")
                return (false, "Solicitud inválida.");

            solicitud.Estado           = "RECHAZADA";
            solicitud.UsuarioResuelveId = admin.Id;
            solicitud.MotivoRechazo    = motivoRechazo;
            solicitud.ResolvedAt       = DateTimeOffset.Now;

            await _repo.SaveChangesAsync();
            return (true, string.Empty);
        }

        public async Task<(bool ok, string msg, Guid? notaId)> AnularDirectoAsync(Guid ventaId, Guid userId, string password)
        {
            var admin = await _repo.GetUsuarioConRolAsync(userId);
            if (admin == null || admin.Rol?.Nombre != "Administrador")
                return (false, "Acceso denegado.", null);

            password = password?.Trim() ?? "";
            bool isPasswordValid = EncriptManager.Verify(password, admin.PasswordHash);
            if (!isPasswordValid && EncriptManager.Encript(password) != admin.PasswordHash)
                return (false, "Contraseña incorrecta.", null);

            return await EjecutarAnulacionAsync(ventaId, admin.Id, null);
        }

        private async Task<(bool ok, string msg, Guid? notaId)> EjecutarAnulacionAsync(Guid ventaId, Guid adminId, Guid? solicitudId)
        {
            using var transaction = await _repo.BeginTransactionAsync();
            try
            {
                var venta = await _repo.GetVentaConDetallesAsync(ventaId);
                if (venta == null || venta.Estado == "ANULADA")
                    throw new Exception("Venta inválida o ya anulada.");

                venta.Estado = "ANULADA";

                if (solicitudId.HasValue)
                {
                    var solicitud = await _repo.GetSolicitudAsync(solicitudId.Value);
                    if (solicitud != null)
                    {
                        solicitud.Estado            = "APROBADA";
                        solicitud.UsuarioResuelveId = adminId;
                        solicitud.ResolvedAt        = DateTimeOffset.Now;
                    }
                }

                foreach (var detalle in venta.VentaDetalles)
                {
                    if (detalle.ProductoId == null) continue;
                    var producto = await _repo.GetProductoAsync((Guid)detalle.ProductoId);
                    if (producto != null)
                        producto.StockActual += detalle.Cantidad;
                }

                var notaCredito = new NotaCredito
                {
                    VentaId       = venta.Id,
                    Folio         = "NC-" + venta.FolioInterno + "-" + DateTime.Now.ToString("fff"),
                    TotalDevuelto = venta.TotalFinal,
                    CreatedAt     = DateTimeOffset.Now
                };
                _repo.AddNotaCredito(notaCredito);

                await _repo.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, string.Empty, notaCredito.Id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, ex.Message, null);
            }
        }

        public Task<NotaCredito?> GetNotaCreditoAsync(Guid ventaId)
        {
            return _repo.GetNotaCreditoByVentaAsync(ventaId);
        }

        public async Task<(bool exists, Guid? id)> HasNotaCreditoAsync(Guid ventaId)
        {
            var nota = await _repo.GetNotaCreditoByVentaAsync(ventaId);
            return (nota != null, nota?.Id);
        }
    }
}
