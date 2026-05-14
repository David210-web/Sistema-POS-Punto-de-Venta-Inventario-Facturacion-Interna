using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaFacturacionPOS.Models.ViewModels;
using SistemaFacturacionPOS.Services.Interfaces;
using System.Security.Claims;

namespace SistemaFacturacionPOS.Controllers.POS
{
    [Authorize]
    public class POSController : Controller
    {
        private readonly IPOSService _posService;

        public POSController(IPOSService posService)
        {
            _posService = posService;
        }

        public async Task<IActionResult> Index()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out Guid userId)) return Unauthorized();

            var tieneSesion = await _posService.TieneSesionActivaAsync(userId);
            if (!tieneSesion)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return PartialView("RequiereCaja");
                return RedirectToAction("Index", "Caja");
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView();
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> BuscarProductos(string q)
        {
            var productos = await _posService.BuscarProductosAsync(q);
            return Json(productos);
        }

        [HttpPost]
        public async Task<IActionResult> FinalizarVenta([FromBody] VentaRequestDTO request)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out Guid userId)) return Unauthorized();

            try
            {
                var (ok, ventaId, error) = await _posService.FinalizarVentaAsync(userId, request);
                if (!ok) return BadRequest(error);
                return Ok(new { ventaId, message = "Venta registrada con éxito." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error al registrar la venta: " + ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Ticket(Guid id)
        {
            var venta = await _posService.GetTicketAsync(id);
            if (venta == null) return NotFound();
            return View(venta);
        }
    }
}
