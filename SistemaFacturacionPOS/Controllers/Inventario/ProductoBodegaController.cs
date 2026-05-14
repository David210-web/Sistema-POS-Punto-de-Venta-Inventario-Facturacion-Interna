using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaFacturacionPOS.DTOs;
using SistemaFacturacionPOS.Services.Interfaces;

namespace SistemaFacturacionPOS.Controllers.Inventario
{
    [Authorize(Roles = "Administrador")]
    public class ProductoBodegaController : Controller
    {
        private readonly IProductoBodegaService _productoBodegaService;

        public ProductoBodegaController(IProductoBodegaService productoBodegaService)
        {
            _productoBodegaService = productoBodegaService;
        }

        [HttpGet]
        public async Task<IActionResult> GetExistencias(Guid productoId)
        {
            var (ok, data, msg) = await _productoBodegaService.GetExistenciasAsync(productoId);
            if (!ok) return StatusCode(500, $"Error al obtener existencias: {msg}");
            return StatusCode(200, data);
        }

        [HttpPost]
        public async Task<IActionResult> AgregarExistencia([FromBody] ExistenciaRequest request)
        {
            var (ok, msg) = await _productoBodegaService.AgregarExistenciaAsync(request);
            if (!ok) return msg.StartsWith("El stock") ? BadRequest(msg) : StatusCode(ok ? 200 : 400, msg);
            return StatusCode(200, msg);
        }

        [HttpPut]
        public async Task<IActionResult> ActualizarExistencia(Guid id, [FromBody] ExistenciaRequest request)
        {
            var (ok, msg) = await _productoBodegaService.ActualizarExistenciaAsync(id, request);
            if (msg == "Existencia no encontrada.") return StatusCode(404, msg);
            if (!ok) return StatusCode(500, $"Error al actualizar existencia: {msg}");
            return StatusCode(200, msg);
        }

        [HttpDelete]
        public async Task<IActionResult> EliminarExistencia(Guid id)
        {
            var (ok, msg) = await _productoBodegaService.EliminarExistenciaAsync(id);
            if (msg == "Existencia no encontrada.") return StatusCode(404, msg);
            if (!ok) return StatusCode(500, $"Error al eliminar existencia: {msg}");
            return StatusCode(200, msg);
        }
    }
}
