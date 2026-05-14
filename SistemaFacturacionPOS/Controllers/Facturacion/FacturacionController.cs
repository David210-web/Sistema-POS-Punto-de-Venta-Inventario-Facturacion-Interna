using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaFacturacionPOS.Services.Interfaces;
using System.Security.Claims;

namespace SistemaFacturacionPOS.Controllers.Facturacion
{
    [Authorize]
    public class FacturacionController : Controller
    {
        private readonly IFacturacionService _facturacionService;

        public FacturacionController(IFacturacionService facturacionService)
        {
            _facturacionService = facturacionService;
        }

        public async Task<IActionResult> Index()
        {
            // Mantener ViewBag.UserRole para la vista
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ViewBag.UserRole = User.FindFirstValue(ClaimTypes.Role);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView();
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetVentas(string? folio, string? date)
        {
            var data = await _facturacionService.GetVentasAsync(folio, date);
            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetSolicitudes()
        {
            var solicitudes = await _facturacionService.GetSolicitudesAsync();
            return Json(solicitudes);
        }

        [HttpPost]
        public async Task<IActionResult> SolicitarAnulacion([FromForm] Guid ventaId, [FromForm] string motivo)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out Guid userId)) return Unauthorized();

            var (ok, msg) = await _facturacionService.SolicitarAnulacionAsync(ventaId, userId, motivo);
            if (!ok) return BadRequest(msg);
            return Ok(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> AprobarAnulacion([FromForm] Guid solicitudId, [FromForm] string password)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out Guid userId)) return Unauthorized();

            var (ok, msg, notaId) = await _facturacionService.AprobarAnulacionAsync(solicitudId, userId, password);
            if (msg == "Acceso denegado.") return Forbid();
            if (!ok) return BadRequest(msg);
            return Ok(new { success = true, notaCreditoId = notaId });
        }

        [HttpPost]
        public async Task<IActionResult> RechazarAnulacion([FromForm] Guid solicitudId, [FromForm] string motivoRechazo)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out Guid userId)) return Unauthorized();

            var (ok, msg) = await _facturacionService.RechazarAnulacionAsync(solicitudId, userId, motivoRechazo);
            if (msg == "Acceso denegado.") return Forbid();
            if (!ok) return BadRequest(msg);
            return Ok(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> AnularDirecto([FromForm] Guid ventaId, [FromForm] string password)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out Guid userId)) return Unauthorized();

            var (ok, msg, notaId) = await _facturacionService.AnularDirectoAsync(ventaId, userId, password);
            if (msg == "Acceso denegado.") return Forbid();
            if (!ok) return BadRequest(msg);
            return Ok(new { success = true, notaCreditoId = notaId });
        }

        [HttpGet]
        public async Task<IActionResult> NotaCredito(Guid ventaId)
        {
            var nota = await _facturacionService.GetNotaCreditoAsync(ventaId);
            if (nota == null) return NotFound("Nota de crédito no encontrada.");
            return View(nota);
        }

        [HttpGet]
        public async Task<IActionResult> HasNotaCredito(Guid ventaId)
        {
            var (exists, id) = await _facturacionService.HasNotaCreditoAsync(ventaId);
            return Json(new { exists, id });
        }
    }
}
