using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaFacturacionPOS.Models;
using SistemaFacturacionPOS.Services.Interfaces;

namespace SistemaFacturacionPOS.Controllers.Usuario
{
    [Authorize(Roles = "Administrador")]
    public class UsuarioController : Controller
    {
        private readonly IUsuarioService _usuarioService;

        public UsuarioController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        public IActionResult Index()
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView();
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetUsuarios()
        {
            var (ok, data, msg) = await _usuarioService.GetUsuariosAsync();
            if (!ok) return StatusCode(500, $"Error al obtener los usuarios: {msg}");
            return StatusCode(200, data);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUsuario([FromBody] Models.Usuario usuario)
        {
            var (ok, msg) = await _usuarioService.CreateUsuarioAsync(usuario);
            if (!ok) return StatusCode(500, $"Error al crear el usuario: {msg}");
            return StatusCode(201, msg);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateUsuario(Guid id, [FromBody] Models.Usuario usuario)
        {
            var (ok, msg) = await _usuarioService.UpdateUsuarioAsync(id, usuario);
            if (msg == "Usuario no encontrado") return StatusCode(404, msg);
            if (!ok) return StatusCode(500, $"Error al actualizar el usuario: {msg}");
            return StatusCode(200, msg);
        }

        [HttpPatch]
        public async Task<IActionResult> PatchUsuario(Guid id, [FromBody] Models.Usuario usuario)
        {
            var (ok, msg) = await _usuarioService.PatchUsuarioAsync(id, usuario);
            if (msg == "Usuario no encontrado") return StatusCode(404, msg);
            if (!ok) return StatusCode(500, $"Error al actualizar el usuario: {msg}");
            return StatusCode(200, msg);
        }

        [HttpPut]
        public async Task<IActionResult> RestablecerContraseña(Guid id)
        {
            var (ok, msg) = await _usuarioService.RestablecerContraseñaAsync(id);
            if (msg == "Usuario no encontrado") return StatusCode(404, msg);
            if (!ok) return StatusCode(500, $"Error al restablecer la contraseña: {msg}");
            return StatusCode(200, msg);
        }
    }
}
