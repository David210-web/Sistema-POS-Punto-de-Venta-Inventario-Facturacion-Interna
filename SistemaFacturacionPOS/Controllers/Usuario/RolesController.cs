using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaFacturacionPOS.Models;
using SistemaFacturacionPOS.Services.Interfaces;

namespace SistemaFacturacionPOS.Controllers.Usuario
{
    [Authorize(Roles = "Administrador")]
    public class RolesController : Controller
    {
        private readonly IRolesService _rolesService;

        public RolesController(IRolesService rolesService)
        {
            _rolesService = rolesService;
        }

        public IActionResult Index()
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView();
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetRoles()
        {
            var (ok, data, msg) = await _rolesService.GetRolesAsync();
            if (!ok) return StatusCode(500, $"Error al obtener los roles {msg}");
            return StatusCode(200, data);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRol([FromBody] Models.Rol rol)
        {
            var (ok, msg) = await _rolesService.CreateRolAsync(rol);
            if (!ok) return StatusCode(500, $"Error al crear el rol: {msg}");
            return StatusCode(201, msg);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateRol(Guid id, [FromBody] Models.Rol rol)
        {
            var (ok, msg) = await _rolesService.UpdateRolAsync(id, rol);
            if (msg == "Rol no encontrado") return StatusCode(404, msg);
            if (!ok) return StatusCode(500, $"Error al actualizar el rol: {msg}");
            return StatusCode(200, msg);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteRol(Guid id)
        {
            var (ok, msg) = await _rolesService.DeleteRolAsync(id);
            if (msg == "Rol no encontrado") return StatusCode(404, msg);
            if (!ok) return StatusCode(500, $"Error al eliminar el rol: {msg}");
            return StatusCode(200, msg);
        }
    }
}
