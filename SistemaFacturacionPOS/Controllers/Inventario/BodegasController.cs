using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaFacturacionPOS.Models;
using SistemaFacturacionPOS.Services.Interfaces;

namespace SistemaFacturacionPOS.Controllers.Inventario
{
    [Authorize(Roles = "Administrador")]
    public class BodegasController : Controller
    {
        private readonly IBodegasService _bodegasService;

        public BodegasController(IBodegasService bodegasService)
        {
            _bodegasService = bodegasService;
        }

        public IActionResult Index()
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView();
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetBodegas()
        {
            var (ok, data, msg) = await _bodegasService.GetBodegasAsync();
            if (!ok) return StatusCode(500, $"Error al obtener bodegas: {msg}");
            return StatusCode(200, data);
        }

        [HttpPost]
        public async Task<IActionResult> AgregarBodega([FromBody] Bodega bodega)
        {
            var (ok, msg) = await _bodegasService.AgregarBodegaAsync(bodega);
            if (msg == "El nombre de la bodega es requerido.") return BadRequest(msg);
            if (!ok) return StatusCode(500, $"Error al crear la bodega: {msg}");
            return StatusCode(200, msg);
        }

        [HttpPut]
        public async Task<IActionResult> ActualizarBodega(Guid id, [FromBody] Bodega bodega)
        {
            var (ok, msg) = await _bodegasService.ActualizarBodegaAsync(id, bodega);
            if (msg == "Bodega no encontrada.") return StatusCode(404, msg);
            if (msg == "El nombre de la bodega es requerido.") return BadRequest(msg);
            if (!ok) return StatusCode(500, $"Error al actualizar la bodega: {msg}");
            return StatusCode(200, msg);
        }

        [HttpDelete]
        public async Task<IActionResult> EliminarBodega(Guid id)
        {
            var (ok, msg) = await _bodegasService.EliminarBodegaAsync(id);
            if (msg == "Bodega no encontrada.") return StatusCode(404, msg);
            if (!ok) return StatusCode(400, msg);
            return StatusCode(200, msg);
        }
    }
}
