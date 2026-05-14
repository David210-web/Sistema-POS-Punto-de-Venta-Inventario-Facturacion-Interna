using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaFacturacionPOS.Services.Interfaces;
using System.Security.Claims;

namespace SistemaFacturacionPOS.Controllers.Configuracion
{
    [Authorize]
    public class ConfiguracionController : Controller
    {
        private readonly IConfiguracionService _configuracionService;

        public ConfiguracionController(IConfiguracionService configuracionService)
        {
            _configuracionService = configuracionService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Index", "Login");

                var userId = Guid.Parse(userIdStr);
                var (usuario, empresa, sesionesActivas) = await _configuracionService.GetDatosIndexAsync(userId);

                ViewBag.Usuario            = usuario;
                ViewBag.Empresa            = empresa;
                ViewBag.ActiveSessionsCount = sesionesActivas;

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return PartialView("Index");
                return View();
            }
            catch (Exception ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return StatusCode(500, $"Error interno: {ex.Message} - {ex.InnerException?.Message}");
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateEmpresa(string nombre, string nit, string direccion)
        {
            var (ok, msg) = await _configuracionService.UpdateEmpresaAsync(nombre, nit, direccion);
            return Json(new { success = ok, message = msg });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(string nombre, string apellido)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr))
                return Json(new { success = false, message = "Sesión no válida." });

            var userId = Guid.Parse(userIdStr);
            var (ok, msg) = await _configuracionService.UpdateProfileAsync(userId, nombre, apellido);
            return Json(new { success = ok, message = msg });
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userId = Guid.Parse(userIdStr!);
            var (ok, msg) = await _configuracionService.ChangePasswordAsync(userId, currentPassword, newPassword);
            return Json(new { success = ok, message = msg });
        }
    }
}
