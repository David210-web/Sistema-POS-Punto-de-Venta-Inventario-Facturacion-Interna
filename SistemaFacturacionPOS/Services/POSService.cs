using SistemaFacturacionPOS.Models;
using SistemaFacturacionPOS.Models.ViewModels;
using SistemaFacturacionPOS.Repositories.Interfaces;
using SistemaFacturacionPOS.Services.Interfaces;

namespace SistemaFacturacionPOS.Services
{
    public class POSService : IPOSService
    {
        private readonly IPOSRepository _repo;

        public POSService(IPOSRepository repo)
        {
            _repo = repo;
        }

        public Task<bool> TieneSesionActivaAsync(Guid userId)
        {
            return _repo.TieneSesionActivaAsync(userId);
        }

        public async Task<IEnumerable<object>> BuscarProductosAsync(string? q)
        {
            var productos = await _repo.BuscarProductosAsync(q);
            return productos.Select(p => new
            {
                p.Id,
                p.Nombre,
                p.CodigoBarras,
                p.PrecioUnitario,
                p.StockActual
            });
        }

        public async Task<(bool ok, Guid ventaId, string error)> FinalizarVentaAsync(Guid userId, VentaRequestDTO request)
        {
            var sesionActiva = await _repo.GetSesionActivaAsync(userId);

            if (sesionActiva == null)
                return (false, Guid.Empty, "No existe una sesión de caja abierta.");

            if (request.Detalles == null || !request.Detalles.Any())
                return (false, Guid.Empty, "El carrito está vacío.");

            var venta = new Venta
            {
                UsuarioId    = userId,
                CajaSesionId = sesionActiva.Id,
                TotalNeto    = request.Total,
                Impuestos    = 0,
                TotalFinal   = request.Total,
                MetodoPago   = request.MetodoPago,
                Estado       = "COMPLETADA",
                CreatedAt    = DateTimeOffset.Now
            };

            foreach (var det in request.Detalles)
            {
                var producto = await _repo.GetProductoAsync(det.ProductoId);
                if (producto == null)
                    return (false, Guid.Empty, "Producto no encontrado.");

                if (producto.StockActual < det.Cantidad)
                    return (false, Guid.Empty, $"No hay stock suficiente para: {producto.Nombre}. Stock actual: {producto.StockActual}");

                producto.StockActual -= det.Cantidad;

                venta.VentaDetalles.Add(new VentaDetalle
                {
                    ProductoId              = producto.Id,
                    Cantidad                = det.Cantidad,
                    PrecioUnitarioHistorico = det.PrecioUnitario
                });
            }

            _repo.AddVenta(venta);
            await _repo.SaveChangesAsync();

            return (true, venta.Id, string.Empty);
        }

        public Task<Venta?> GetTicketAsync(Guid id)
        {
            return _repo.GetVentaTicketAsync(id);
        }
    }
}
