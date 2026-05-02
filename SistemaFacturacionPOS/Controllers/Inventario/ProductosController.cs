using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaFacturacionPOS.Contexto;
using SistemaFacturacionPOS.Models;
using System.Security.Claims;

namespace SistemaFacturacionPOS.Controllers.Inventario
{
    [Authorize(Roles = "Administrador")]
    public class ProductosController : Controller
    {
        private readonly SistemaFacturacionPOSContext context; 

        public ProductosController(SistemaFacturacionPOSContext context)
        {
            this.context = context;
        }

        public IActionResult Index()
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView();
            }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetProductos()
        {
            try
            {
                var result = await context.Productos
                    .Include(p => p.Categoria)
                    .Where(p => p.DeletedAt == null)
                    .ToListAsync();
                return StatusCode(200,result);
            }catch(Exception ex)
            {
                return StatusCode(500, $"Hubo un error en el servidor {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AgregarProductos([FromBody] Models.Producto producto)
        {
            try
            {
                producto.DeletedAt = null;
                if(producto.StockActual == null) producto.StockActual = 0;
                if(producto.StockMinimo == null) producto.StockMinimo = 0;
                context.Productos.Add(producto);
                await context.SaveChangesAsync();
                return StatusCode(200, "Producto creado satisfactoriamente");
            }catch(Exception ex)
            {
                return StatusCode(500,$"Hubo un error en el servidor {ex.Message}");
            }
        }

        [HttpPut]
        public async Task<IActionResult> ActualizarProducto(Guid id, [FromBody] Models.Producto producto)
        {
            try
            {
                var existingProducto = await context.Productos.FindAsync(id);
                if (existingProducto == null || existingProducto.DeletedAt != null)
                {
                    return StatusCode(404, "Producto no encontrado");
                }
                
                existingProducto.Nombre = producto.Nombre;
                existingProducto.StockMinimo = producto.StockMinimo;
                existingProducto.CodigoBarras = producto.CodigoBarras;
                existingProducto.PrecioUnitario = producto.PrecioUnitario;
                existingProducto.CategoriaId = producto.CategoriaId;
                await context.SaveChangesAsync();
                return StatusCode(200, "Producto actualizado exitosamente");
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Hubo un error en el servidor");
            }
        }

        [HttpDelete]
        public async Task<IActionResult> EliminarProducto(Guid id)
        {
            try
            {
                var existingProducto = await context.Productos.FindAsync(id);
                if (existingProducto == null || existingProducto.DeletedAt != null)
                {
                    return StatusCode(404, "Producto no encontrado");
                }
                existingProducto.DeletedAt = DateTimeOffset.Now;
                await context.SaveChangesAsync();
                return StatusCode(200, "Producto eliminado exitosamente");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al eliminar el producto: {ex.Message}");
            }
        }

        public class AjusteStockRequest
        {
            public int Cantidad { get; set; }
            public string Justificacion { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> AjustarStock(Guid id, [FromBody] AjusteStockRequest request)
        {
            var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                var producto = await context.Productos.FindAsync(id);
                if (producto == null || producto.DeletedAt != null)
                {
                    return StatusCode(404, "Producto no encontrado");
                }

                if (string.IsNullOrWhiteSpace(request.Justificacion))
                {
                    return BadRequest("La justificación es obligatoria.");
                }

                // Determinar el tipo de ajuste basado en las restricciones de base de datos
                string tipoMovimiento = request.Cantidad > 0 ? "ENTRADA" : "AJUSTE_MERMA";
                if (request.Cantidad == 0) return BadRequest("La cantidad a ajustar no puede ser cero.");

                int stockAnterior = producto.StockActual ?? 0;
                producto.StockActual = stockAnterior + request.Cantidad;

                // Obtener ID del usuario actual de los claims (se asume que se usa el claim NameIdentifier)
                Guid? usuarioId = null;
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (Guid.TryParse(userIdClaim, out Guid parsedId))
                {
                    usuarioId = parsedId;
                }
                else
                {
                    // Si no está en NameIdentifier, buscar un claim de usuario por nombre (esto depende de cómo configures la cookie)
                    var user = await context.Usuarios.FirstOrDefaultAsync(u => u.Username == User.Identity.Name);
                    usuarioId = user?.Id;
                }

                var movimiento = new InventarioMovimiento
                {
                    ProductoId = id,
                    UsuarioId = usuarioId,
                    Tipo = tipoMovimiento,
                    Cantidad = request.Cantidad,
                    Justificacion = request.Justificacion,
                    CreatedAt = DateTimeOffset.Now
                };

                context.InventarioMovimientos.Add(movimiento);

                var auditoria = new AuditoriaLog
                {
                    UsuarioId = usuarioId,
                    TablaAfectada = "productos",
                    Accion = "AJUSTE_STOCK",
                    ValorAnterior = stockAnterior.ToString(),
                    ValorNuevo = producto.StockActual.ToString(),
                    FechaHora = DateTimeOffset.Now
                };

                context.AuditoriaLogs.Add(auditoria);

                await context.SaveChangesAsync();
                await transaction.CommitAsync();
                return StatusCode(200, "Stock ajustado exitosamente");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Error al ajustar el stock: {ex.Message}");
            }
        }
    }
}
