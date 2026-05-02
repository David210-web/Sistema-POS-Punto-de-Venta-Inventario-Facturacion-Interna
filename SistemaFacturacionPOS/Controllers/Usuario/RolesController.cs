using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaFacturacionPOS.Contexto;

namespace SistemaFacturacionPOS.Controllers.Usuario
{
    [Authorize(Roles = "Administrador")]
    public class RolesController : Controller
    {
        private readonly SistemaFacturacionPOSContext context;

        public RolesController(SistemaFacturacionPOSContext context)
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
        public async Task<IActionResult> GetRoles()
        {
            try
            {
                var roles = await context.Roles.ToListAsync();
                return StatusCode(200, roles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener los roles {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateRol([FromBody] Models.Rol rol)
        {
            try
            {
                context.Roles.Add(rol);
                await context.SaveChangesAsync();
                return StatusCode(201, "Rol creado exitosamente");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al crear el rol: {ex.Message}");
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateRol(Guid id, [FromBody] Models.Rol rol)
        {
            try
            {
                var existingRol = await context.Roles.FindAsync(id);
                if (existingRol == null)
                {
                    return StatusCode(404, "Rol no encontrado");
                }
                existingRol.Nombre = rol.Nombre;
                existingRol.Descripcion = rol.Descripcion;
                context.Roles.Update(existingRol);
                await context.SaveChangesAsync();
                return StatusCode(200, "Rol actualizado exitosamente");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al actualizar el rol: {ex.Message}");
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteRol(Guid id)
        {
            try
            {
                var existingRol = await context.Roles.FindAsync(id);
                if (existingRol == null)
                {
                    return StatusCode(404, "Rol no encontrado");
                }
                context.Roles.Remove(existingRol);
                await context.SaveChangesAsync();
                return StatusCode(200, "Rol eliminado exitosamente");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al eliminar el rol: {ex.Message}");
            }
        }
    }
}
