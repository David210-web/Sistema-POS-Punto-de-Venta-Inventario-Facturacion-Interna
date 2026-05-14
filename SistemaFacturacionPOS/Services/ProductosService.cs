using SistemaFacturacionPOS.Models;
using SistemaFacturacionPOS.Repositories.Interfaces;
using SistemaFacturacionPOS.Services.Interfaces;

namespace SistemaFacturacionPOS.Services
{
    public class ProductosService : IProductosService
    {
        private readonly IProductosRepository _repo;

        public ProductosService(IProductosRepository repo)
        {
            _repo = repo;
        }

        public async Task<(bool ok, object? data, string msg)> GetProductosAsync()
        {
            try
            {
                var result = await _repo.GetProductosActivosAsync();
                return (true, result, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, null, ex.Message);
            }
        }

        public async Task<(bool ok, string msg)> AgregarProductoAsync(Producto p)
        {
            try
            {
                p.DeletedAt = null;
                if (p.StockActual == null) p.StockActual = 0;
                if (p.StockMinimo == null) p.StockMinimo = 0;
                _repo.AddProducto(p);
                await _repo.SaveChangesAsync();
                return (true, "Producto creado satisfactoriamente");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool ok, string msg)> ActualizarProductoAsync(Guid id, Producto p)
        {
            try
            {
                var existing = await _repo.GetProductoAsync(id);
                if (existing == null || existing.DeletedAt != null)
                    return (false, "Producto no encontrado");

                existing.Nombre       = p.Nombre;
                existing.StockMinimo  = p.StockMinimo;
                existing.CodigoBarras = p.CodigoBarras;
                existing.PrecioUnitario = p.PrecioUnitario;
                existing.CategoriaId  = p.CategoriaId;

                await _repo.SaveChangesAsync();
                return (true, "Producto actualizado exitosamente");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool ok, string msg)> EliminarProductoAsync(Guid id)
        {
            try
            {
                var existing = await _repo.GetProductoAsync(id);
                if (existing == null || existing.DeletedAt != null)
                    return (false, "Producto no encontrado");

                existing.DeletedAt = DateTimeOffset.Now;
                await _repo.SaveChangesAsync();
                return (true, "Producto eliminado exitosamente");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool ok, string msg)> AjustarStockAsync(Guid id, int cantidad, string justificacion, Guid? userId)
        {
            var transaction = await _repo.BeginTransactionAsync();
            try
            {
                var producto = await _repo.GetProductoAsync(id);
                if (producto == null || producto.DeletedAt != null)
                    return (false, "Producto no encontrado");

                if (string.IsNullOrWhiteSpace(justificacion))
                    return (false, "La justificación es obligatoria.");

                if (cantidad == 0)
                    return (false, "La cantidad a ajustar no puede ser cero.");

                string tipoMovimiento = cantidad > 0 ? "ENTRADA" : "AJUSTE_MERMA";
                int stockAnterior = producto.StockActual ?? 0;
                producto.StockActual = stockAnterior + cantidad;

                var movimiento = new InventarioMovimiento
                {
                    ProductoId    = id,
                    UsuarioId     = userId,
                    Tipo          = tipoMovimiento,
                    Cantidad      = cantidad,
                    Justificacion = justificacion,
                    CreatedAt     = DateTimeOffset.Now
                };
                _repo.AddMovimiento(movimiento);

                var auditoria = new AuditoriaLog
                {
                    UsuarioId      = userId,
                    TablaAfectada  = "productos",
                    Accion         = "AJUSTE_STOCK",
                    ValorAnterior  = stockAnterior.ToString(),
                    ValorNuevo     = producto.StockActual.ToString(),
                    FechaHora      = DateTimeOffset.Now
                };
                _repo.AddAuditoria(auditoria);

                await _repo.SaveChangesAsync();
                await transaction.CommitAsync();
                return (true, "Stock ajustado exitosamente");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, ex.Message);
            }
        }
    }
}
