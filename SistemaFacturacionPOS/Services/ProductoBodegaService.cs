using SistemaFacturacionPOS.DTOs;
using SistemaFacturacionPOS.Models;
using SistemaFacturacionPOS.Repositories.Interfaces;
using SistemaFacturacionPOS.Services.Interfaces;

namespace SistemaFacturacionPOS.Services
{
    public class ProductoBodegaService : IProductoBodegaService
    {
        private readonly IProductoBodegaRepository _repo;

        public ProductoBodegaService(IProductoBodegaRepository repo)
        {
            _repo = repo;
        }

        public async Task<(bool ok, object? data, string msg)> GetExistenciasAsync(Guid productoId)
        {
            try { var e = await _repo.GetExistenciasAsync(productoId); return (true, e, string.Empty); }
            catch (Exception ex) { return (false, null, ex.Message); }
        }

        public async Task<(bool ok, string msg)> AgregarExistenciaAsync(ExistenciaRequest request)
        {
            var tx = await _repo.BeginTransactionAsync();
            try
            {
                if (request.Stock < 0) return (false, "El stock no puede ser negativo.");
                if (await _repo.ExisteRelacionAsync(request.ProductoId, request.BodegaId))
                    return (false, "Este producto ya tiene existencia registrada en esa bodega.");

                _repo.AddExistencia(new ProductoBodega { ProductoId = request.ProductoId, BodegaId = request.BodegaId, Stock = request.Stock });
                await _repo.SaveChangesAsync();
                await RecalcularStockTotalAsync(request.ProductoId);
                await _repo.SaveChangesAsync();
                await tx.CommitAsync();
                return (true, "Existencia agregada correctamente.");
            }
            catch (Exception ex) { await tx.RollbackAsync(); return (false, ex.Message); }
        }

        public async Task<(bool ok, string msg)> ActualizarExistenciaAsync(Guid id, ExistenciaRequest request)
        {
            var tx = await _repo.BeginTransactionAsync();
            try
            {
                if (request.Stock < 0) return (false, "El stock no puede ser negativo.");
                var existencia = await _repo.GetExistenciaAsync(id);
                if (existencia == null) return (false, "Existencia no encontrada.");
                existencia.Stock = request.Stock;
                await _repo.SaveChangesAsync();
                await RecalcularStockTotalAsync(existencia.ProductoId);
                await _repo.SaveChangesAsync();
                await tx.CommitAsync();
                return (true, "Existencia actualizada correctamente.");
            }
            catch (Exception ex) { await tx.RollbackAsync(); return (false, ex.Message); }
        }

        public async Task<(bool ok, string msg)> EliminarExistenciaAsync(Guid id)
        {
            var tx = await _repo.BeginTransactionAsync();
            try
            {
                var existencia = await _repo.GetExistenciaAsync(id);
                if (existencia == null) return (false, "Existencia no encontrada.");
                Guid productoId = existencia.ProductoId;
                _repo.RemoveExistencia(existencia);
                await _repo.SaveChangesAsync();
                await RecalcularStockTotalAsync(productoId);
                await _repo.SaveChangesAsync();
                await tx.CommitAsync();
                return (true, "Existencia eliminada correctamente.");
            }
            catch (Exception ex) { await tx.RollbackAsync(); return (false, ex.Message); }
        }

        private async Task RecalcularStockTotalAsync(Guid productoId)
        {
            var producto = await _repo.GetProductoAsync(productoId);
            if (producto == null) return;
            producto.StockActual = await _repo.SumStockByProductoAsync(productoId);
        }
    }
}
