using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaFacturacionPOS.DTOs;
using SistemaFacturacionPOS.Models;
using SistemaFacturacionPOS.Services.Interfaces;
using System.Security.Claims;

namespace SistemaFacturacionPOS.Controllers.Inventario
{
    [Authorize(Roles = "Administrador")]
    public class ProductosController : Controller
    {
        private readonly IProductosService _productosService;

        public ProductosController(IProductosService productosService)
        {
            _productosService = productosService;
        }

        public IActionResult Index()
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView();
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetProductos()
        {
            var (ok, data, msg) = await _productosService.GetProductosAsync();
            if (!ok) return StatusCode(500, $"Hubo un error en el servidor {msg}");
            return StatusCode(200, data);
        }

        [HttpPost]
        public async Task<IActionResult> AgregarProductos([FromBody] Producto producto)
        {
            var (ok, msg) = await _productosService.AgregarProductoAsync(producto);
            if (!ok) return StatusCode(500, $"Hubo un error en el servidor {msg}");
            return StatusCode(200, msg);
        }

        [HttpPut]
        public async Task<IActionResult> ActualizarProducto(Guid id, [FromBody] Producto producto)
        {
            var (ok, msg) = await _productosService.ActualizarProductoAsync(id, producto);
            if (msg == "Producto no encontrado") return StatusCode(404, msg);
            if (!ok) return StatusCode(500, "Hubo un error en el servidor");
            return StatusCode(200, msg);
        }

        [HttpDelete]
        public async Task<IActionResult> EliminarProducto(Guid id)
        {
            var (ok, msg) = await _productosService.EliminarProductoAsync(id);
            if (msg == "Producto no encontrado") return StatusCode(404, msg);
            if (!ok) return StatusCode(500, $"Error al eliminar el producto: {msg}");
            return StatusCode(200, msg);
        }

        [HttpPost]
        public async Task<IActionResult> AjustarStock(Guid id, [FromBody] AjusteStockRequest request)
        {
            Guid? userId = null;
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdClaim, out Guid parsedId)) userId = parsedId;

            var (ok, msg) = await _productosService.AjustarStockAsync(id, request.Cantidad, request.Justificacion, userId);
            if (msg == "Producto no encontrado") return StatusCode(404, msg);
            if (msg == "La justificación es obligatoria.") return BadRequest(msg);
            if (msg == "La cantidad a ajustar no puede ser cero.") return BadRequest(msg);
            if (!ok) return StatusCode(500, $"Error al ajustar el stock: {msg}");
            return StatusCode(200, msg);
        }
    }
}
