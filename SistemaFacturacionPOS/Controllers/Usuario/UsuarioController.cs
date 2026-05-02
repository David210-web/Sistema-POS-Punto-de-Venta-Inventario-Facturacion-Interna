using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaFacturacionPOS.Contexto;
using SistemaFacturacionPOS.Managers;
using SistemaFacturacionPOS.Models;

namespace SistemaFacturacionPOS.Controllers.Usuario
{
    [Authorize(Roles = "Administrador")]
    public class UsuarioController : Controller
    {
        private readonly SistemaFacturacionPOSContext context;

        public UsuarioController(SistemaFacturacionPOSContext context)
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
        public async Task<IActionResult> GetUsuarios()
        {
            try
            {
                var usuarios = await context.Usuarios
                    .Include(u => u.Rol)
                    .ToListAsync();
                return StatusCode(200, usuarios);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener los usuarios: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateUsuario([FromBody] Models.Usuario usuario)
        {
            try
            {
                var passwordHashed =  EncriptManager.Generate(usuario.PasswordHash);
                usuario.PasswordHash = passwordHashed;
                context.Usuarios.Add(usuario);
                await context.SaveChangesAsync();
                return StatusCode(201, "Usuario creado exitosamente");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al crear el usuario: {ex.Message}");
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateUsuario(Guid id, [FromBody] Models.Usuario usuario)
        {
            try
            {
                var existingUsuario = await context.Usuarios.FindAsync(id);
                if (existingUsuario == null)
                {
                    return StatusCode(404, "Usuario no encontrado");
                }
                existingUsuario.Username = usuario.Username;
                existingUsuario.RolId = usuario.RolId;
                existingUsuario.UpdatedAt = DateTimeOffset.Now;
                context.Usuarios.Update(existingUsuario);
                await context.SaveChangesAsync();
                return StatusCode(200, "Usuario actualizado exitosamente");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al actualizar el usuario: {ex.Message}");
            }
        }

        [HttpPatch]
        public async Task<IActionResult> PatchUsuario(Guid id, [FromBody] Models.Usuario usuario)
        {
            try
            {
                var existingUsuario = await context.Usuarios.FindAsync(id);
                if (existingUsuario == null)
                {
                    return StatusCode(404, "Usuario no encontrado");
                }

                existingUsuario.Activo = usuario.Activo ?? existingUsuario.Activo;
                existingUsuario.UpdatedAt = DateTimeOffset.Now;
                context.Usuarios.Update(existingUsuario);
                await context.SaveChangesAsync();
                return StatusCode(200, "Usuario actualizado exitosamente");

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al actualizar el usuario: {ex.Message}");
            }
        }

        [HttpPut]
        public async Task<IActionResult> RestablecerContraseña(Guid id) { 
            try
            {
                var existingUsuario = await context.Usuarios.FindAsync(id);
                if (existingUsuario == null)
                {
                    return StatusCode(404, "Usuario no encontrado");
                }

                // Lógica para restablecer la contraseña
                existingUsuario.PasswordHash = existingUsuario.Username; // Reemplazar con la lógica real
                existingUsuario.UpdatedAt = DateTimeOffset.Now;
                context.Usuarios.Update(existingUsuario);
                await context.SaveChangesAsync();
                return StatusCode(200, "Contraseña restablecida exitosamente");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al restablecer la contraseña: {ex.Message}");
            }
        }
    }
}
