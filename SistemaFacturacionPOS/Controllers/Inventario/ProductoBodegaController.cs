using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaFacturacionPOS.Contexto;
using SistemaFacturacionPOS.Models;

namespace SistemaFacturacionPOS.Controllers.Inventario
{
    [Authorize(Roles = "Administrador")]
    public class ProductoBodegaController : Controller
    {
        private readonly SistemaFacturacionPOSContext context;

        public ProductoBodegaController(SistemaFacturacionPOSContext context)
        {
            this.context = context;
        }

        // GET /ProductoBodega/GetExistencias/{productoId}
        // Devuelve las bodegas con stock del producto indicado
        [HttpGet]
        public async Task<IActionResult> GetExistencias(Guid productoId)
        {
            try
            {
                var existencias = await context.VistaProductosBodegas
                    .Where(vpb => vpb.ProductoId == productoId).ToListAsync();


                return StatusCode(200, existencias);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener existencias: {ex.Message}");
            }
        }

        // Request model compartido para agregar y actualizar
        public class ExistenciaRequest
        {
            public Guid ProductoId { get; set; }
            public Guid BodegaId { get; set; }
            public int Stock { get; set; }
        }

        // POST /ProductoBodega/AgregarExistencia
        // Asigna un producto a una bodega con stock inicial
        [HttpPost]
        public async Task<IActionResult> AgregarExistencia([FromBody] ExistenciaRequest request)
        {
            var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                if (request.Stock < 0)
                    return BadRequest("El stock no puede ser negativo.");

                // Verificar que no exista ya la relación
                bool yaExiste = await context.ProductoBodegas
                    .AnyAsync(pb => pb.ProductoId == request.ProductoId && pb.BodegaId == request.BodegaId);
                if (yaExiste)
                    return StatusCode(400, "Este producto ya tiene existencia registrada en esa bodega. Use 'Editar' para modificar el stock.");

                var nueva = new ProductoBodega
                {
                    ProductoId = request.ProductoId,
                    BodegaId   = request.BodegaId,
                    Stock      = request.Stock
                };
                context.ProductoBodegas.Add(nueva);
                await context.SaveChangesAsync();

                // Recalcular stock_actual en productos
                await RecalcularStockTotal(request.ProductoId);
                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                return StatusCode(200, "Existencia agregada correctamente.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Error al agregar existencia: {ex.Message}");
            }
        }

        // PUT /ProductoBodega/ActualizarExistencia/{id}
        // Edita el stock de una relación producto-bodega existente
        [HttpPut]
        public async Task<IActionResult> ActualizarExistencia(Guid id, [FromBody] ExistenciaRequest request)
        {
            var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                if (request.Stock < 0)
                    return BadRequest("El stock no puede ser negativo.");

                var existencia = await context.ProductoBodegas.FindAsync(id);
                if (existencia == null)
                    return StatusCode(404, "Existencia no encontrada.");

                existencia.Stock = request.Stock;
                await context.SaveChangesAsync();

                // Recalcular stock_actual en productos
                await RecalcularStockTotal(existencia.ProductoId);
                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                return StatusCode(200, "Existencia actualizada correctamente.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Error al actualizar existencia: {ex.Message}");
            }
        }

        // DELETE /ProductoBodega/EliminarExistencia/{id}
        // Elimina la relación producto-bodega y recalcula el total
        [HttpDelete]
        public async Task<IActionResult> EliminarExistencia(Guid id)
        {
            var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                var existencia = await context.ProductoBodegas.FindAsync(id);
                if (existencia == null)
                    return StatusCode(404, "Existencia no encontrada.");

                Guid productoId = existencia.ProductoId;
                context.ProductoBodegas.Remove(existencia);
                await context.SaveChangesAsync();

                // Recalcular stock_actual en productos
                await RecalcularStockTotal(productoId);
                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                return StatusCode(200, "Existencia eliminada correctamente.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Error al eliminar existencia: {ex.Message}");
            }
        }

        // Método auxiliar: recalcula productos.stock_actual como SUM de producto_bodega.stock
        private async Task RecalcularStockTotal(Guid productoId)
        {
            var producto = await context.Productos.FindAsync(productoId);
            if (producto == null) return;

            int total = await context.ProductoBodegas
                .Where(pb => pb.ProductoId == productoId)
                .SumAsync(pb => (int?)pb.Stock) ?? 0;

            producto.StockActual = total;
        }
    }
}
