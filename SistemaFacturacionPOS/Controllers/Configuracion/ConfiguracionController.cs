using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaFacturacionPOS.Contexto;
using SistemaFacturacionPOS.Models;
using SistemaFacturacionPOS.Managers;
using System.Security.Claims;

namespace SistemaFacturacionPOS.Controllers.Configuracion
{
    [Authorize]
    public class ConfiguracionController : Controller
    {
        private readonly SistemaFacturacionPOSContext _context;

        public ConfiguracionController(SistemaFacturacionPOSContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Index", "Login");

                var userId = Guid.Parse(userIdStr);
                var usuario = await _context.Usuarios
                    .Include(u => u.Rol)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                var empresa = await _context.Empresa.FirstOrDefaultAsync();

                // Count active sessions (state = 1)
                var activeSessions = await _context.CajaSesiones.CountAsync(s => s.Estado == true);

                ViewBag.Usuario = usuario;
                ViewBag.Empresa = empresa;
                ViewBag.ActiveSessionsCount = activeSessions;

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return PartialView("Index");
                }

                return View();
            }
            catch (Exception ex)
            {
                // Si es una petición AJAX, devolver el error como texto para que SweetAlert lo muestre o se vea en consola
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return StatusCode(500, $"Error interno: {ex.Message} - {ex.InnerException?.Message}");
                }
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateEmpresa(string nombre, string nit, string direccion)
        {
            try
            {
                var existingEmpresa = await _context.Empresa.FirstOrDefaultAsync();
                if (existingEmpresa == null)
                {
                    existingEmpresa = new Empresa
                    {
                        Nombre = nombre,
                        Nit = nit,
                        Direccion = direccion
                    };
                    _context.Empresa.Add(existingEmpresa);
                }
                else
                {
                    existingEmpresa.Nombre = nombre;
                    existingEmpresa.Nit = nit;
                    existingEmpresa.Direccion = direccion;
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Datos de la empresa actualizados correctamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al actualizar los datos: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(string nombre, string apellido)
        {
            try
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdStr)) return Json(new { success = false, message = "Sesión no válida." });

                var userId = Guid.Parse(userIdStr);
                var usuario = await _context.Usuarios.FindAsync(userId);
                if (usuario == null) return Json(new { success = false, message = "Usuario no encontrado." });

                usuario.Nombre = nombre;
                usuario.Apellido = apellido;
                usuario.UpdatedAt = DateTimeOffset.Now;

                _context.Usuarios.Update(usuario);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Perfil actualizado correctamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al actualizar el perfil: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword)
        {
            try
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userId = Guid.Parse(userIdStr);
                var usuario = await _context.Usuarios.FindAsync(userId);

                if (usuario == null) return Json(new { success = false, message = "Usuario no encontrado." });

                if (!EncriptManager.Verify(currentPassword, usuario.PasswordHash))
                {
                    return Json(new { success = false, message = "La contraseña actual es incorrecta." });
                }

                usuario.PasswordHash = EncriptManager.Generate(newPassword);
                usuario.UpdatedAt = DateTimeOffset.Now;

                _context.Usuarios.Update(usuario);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Contraseña cambiada correctamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al cambiar la contraseña: " + ex.Message });
            }
        }
    }
}
