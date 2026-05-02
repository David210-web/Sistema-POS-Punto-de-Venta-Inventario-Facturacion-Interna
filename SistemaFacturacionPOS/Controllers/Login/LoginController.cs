using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaFacturacionPOS.Contexto;
using SistemaFacturacionPOS.Managers;
using System.Security.Claims;

namespace SistemaFacturacionPOS.Controllers.Login
{
    public class LoginController : Controller
    {
        private readonly SistemaFacturacionPOSContext _context;

        public LoginController(SistemaFacturacionPOSContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Ingresar(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Debe ingresar usuario y contraseña.";
                return View("Index");
            }

            // Usar AsNoTracking() para que la consulta sea lo más rápida posible (Requisito: < 2 segundos)
            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username == username && u.Activo == true);

            if (usuario == null)
            {
                ViewBag.Error = "Usuario no encontrado o inactivo.";
                return View("Index");
            }

            // Verificación usando Bcrypt
            if (!EncriptManager.Verify(password, usuario.PasswordHash))
            {
                ViewBag.Error = "Contraseña incorrecta.";
                return View("Index");
            }

            // Crear Claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Username),
                new Claim(ClaimTypes.Role, usuario.Rol.Nombre), // Agregamos el nombre del rol como claim nativo
                new Claim("RolId", usuario.RolId.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            // Iniciar sesión
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTime.UtcNow.AddHours(8)
            });

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Login");
        }

        [HttpGet]
        public IActionResult AccesoDenegado()
        {
            return View();
        }
    }
}
