using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaFacturacionPOS.DTOs;
using SistemaFacturacionPOS.Services.Interfaces;
using System.Security.Claims;

namespace SistemaFacturacionPOS.Controllers.Caja
{
    [Authorize]
    public class CajaController : Controller
    {
        private readonly ICajaService _cajaService;

        public CajaController(ICajaService cajaService)
        {
            _cajaService = cajaService;
        }

        public async Task<IActionResult> Index()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out Guid userId)) return Unauthorized();

            var viewModel = await _cajaService.GetResumenAsync(userId);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView(viewModel);
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AbrirCaja([FromBody] AbrirCajaRequest request)
        {
            try
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!Guid.TryParse(userIdStr, out Guid userId)) return Unauthorized();

                var (ok, msg) = await _cajaService.AbrirCajaAsync(userId, request.MontoInicial);
                if (!ok) return BadRequest(msg);
                return Ok(new { message = msg });
            }
            catch (Exception ex)
            {
                var message = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return StatusCode(500, $"Error al abrir la caja: {message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CerrarCaja([FromBody] CerrarCajaRequest request)
        {
            try
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!Guid.TryParse(userIdStr, out Guid userId)) return Unauthorized();

                var (ok, msg) = await _cajaService.CerrarCajaAsync(userId, request.MontoFisico);
                if (!ok) return BadRequest(msg);
                return Ok(new { message = msg });
            }
            catch (Exception ex)
            {
                var message = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return StatusCode(500, $"Error al cerrar la caja: {message}");
            }
        }
    }
}
