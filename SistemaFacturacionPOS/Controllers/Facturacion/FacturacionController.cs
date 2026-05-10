using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaFacturacionPOS.Contexto;
using SistemaFacturacionPOS.Models;
using SistemaFacturacionPOS.Managers;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SistemaFacturacionPOS.Controllers.Facturacion
{
    [Authorize]
    public class FacturacionController : Controller
    {
        private readonly SistemaFacturacionPOSContext _context;

        public FacturacionController(SistemaFacturacionPOSContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out Guid userId)) return Unauthorized();

            var user = await _context.Usuarios.Include(u => u.Rol).FirstOrDefaultAsync(u => u.Id == userId);
            ViewBag.UserRole = user?.Rol?.Nombre;

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView();
            }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetVentas(string? folio, string? date)
        {
            var query = _context.Ventas
                .Include(v => v.Usuario)
                .Include(v => v.CajaSesion)
                .AsQueryable();

            if (!string.IsNullOrEmpty(folio))
            {
                var folioLimpio = folio.ToUpper().Replace("V", "").TrimStart('0');
                if (string.IsNullOrEmpty(folioLimpio)) folioLimpio = "0";
                if (int.TryParse(folioLimpio, out int folioInt))
                {
                    query = query.Where(v => v.FolioInterno == folioInt);
                }
            }

            if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out DateTime parsedDate))
            {
                query = query.Where(v => v.CreatedAt.HasValue && v.CreatedAt.Value.Date == parsedDate.Date);
            }

            var ventas = await query.OrderByDescending(v => v.CreatedAt).ToListAsync();

            var data = ventas.Select(v => new {
                id = v.Id,
                folio = v.FolioInterno,
                fecha = v.CreatedAt?.ToString("dd 'de' MMM, hh:mm tt"),
                cajero = v.Usuario?.Username,
                metodo = v.MetodoPago,
                total = v.TotalFinal.ToString("C"),
                estado = v.Estado
            });

            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetSolicitudes()
        {
            var solicitudes = await _context.VentaAnulacionSolicitudes
                .Include(s => s.Venta)
                .Include(s => s.UsuarioSolicita)
                .Where(s => s.Estado == "PENDIENTE")
                .Select(s => new {
                    id = s.Id,
                    ventaId = s.VentaId,
                    folioVenta = s.Venta.FolioInterno,
                    cajero = s.UsuarioSolicita.Username,
                    fecha = s.CreatedAt.HasValue ? s.CreatedAt.Value.ToString("dd/MM/yyyy HH:mm") : "",
                    motivo = s.Motivo
                }).ToListAsync();

            return Json(solicitudes);
        }

        [HttpPost]
        public async Task<IActionResult> SolicitarAnulacion([FromForm] Guid ventaId, [FromForm] string motivo)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out Guid userId)) return Unauthorized();

            var venta = await _context.Ventas.FindAsync(ventaId);
            if (venta == null || venta.Estado == "ANULADA") return BadRequest("Venta no válida o ya anulada.");

            var exists = await _context.VentaAnulacionSolicitudes.AnyAsync(s => s.VentaId == ventaId && s.Estado == "PENDIENTE");
            if (exists) return BadRequest("Ya existe una solicitud pendiente para esta venta.");

            var solicitud = new VentaAnulacionSolicitud
            {
                VentaId = ventaId,
                UsuarioSolicitaId = userId,
                Motivo = motivo,
                Estado = "PENDIENTE",
                CreatedAt = DateTimeOffset.Now
            };

            _context.VentaAnulacionSolicitudes.Add(solicitud);
            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> AprobarAnulacion([FromForm] Guid solicitudId, [FromForm] string password)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out Guid userId)) return Unauthorized();

            var admin = await _context.Usuarios.Include(u => u.Rol).FirstOrDefaultAsync(u => u.Id == userId);
            if (admin == null || admin.Rol?.Nombre != "Administrador") return Forbid();

            password = password?.Trim() ?? "";

            // Verificar password. Intentar con EncriptManager.Verify (BCrypt) o Encript (SHA256) dependiendo de tu sistema
            // Asumo EncriptManager.Verify si se usó BCrypt.
            bool isPasswordValid = EncriptManager.Verify(password, admin.PasswordHash);
            if (!isPasswordValid) 
            {
                // Fallback a SHA256 por si acaso
                if (EncriptManager.Encript(password) != admin.PasswordHash) 
                {
                    return BadRequest("Contraseña incorrecta.");
                }
            }

            var solicitud = await _context.VentaAnulacionSolicitudes.FindAsync(solicitudId);
            if (solicitud == null || solicitud.Estado != "PENDIENTE") return BadRequest("Solicitud inválida.");

            return await EjecutarAnulacion(solicitud.VentaId, admin.Id, solicitudId);
        }

        [HttpPost]
        public async Task<IActionResult> RechazarAnulacion([FromForm] Guid solicitudId, [FromForm] string motivoRechazo)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out Guid userId)) return Unauthorized();

            var admin = await _context.Usuarios.Include(u => u.Rol).FirstOrDefaultAsync(u => u.Id == userId);
            if (admin == null || admin.Rol?.Nombre != "Administrador") return Forbid();

            var solicitud = await _context.VentaAnulacionSolicitudes.FindAsync(solicitudId);
            if (solicitud == null || solicitud.Estado != "PENDIENTE") return BadRequest("Solicitud inválida.");

            solicitud.Estado = "RECHAZADA";
            solicitud.UsuarioResuelveId = admin.Id;
            solicitud.MotivoRechazo = motivoRechazo;
            solicitud.ResolvedAt = DateTimeOffset.Now;

            _context.VentaAnulacionSolicitudes.Update(solicitud);
            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> AnularDirecto([FromForm] Guid ventaId, [FromForm] string password)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out Guid userId)) return Unauthorized();

            var admin = await _context.Usuarios.Include(u => u.Rol).FirstOrDefaultAsync(u => u.Id == userId);
            if (admin == null || admin.Rol?.Nombre != "Administrador") return Forbid();

            password = password?.Trim() ?? "";

            bool isPasswordValid = EncriptManager.Verify(password, admin.PasswordHash);
            if (!isPasswordValid) 
            {
                if (EncriptManager.Encript(password) != admin.PasswordHash) 
                {
                    return BadRequest("Contraseña incorrecta.");
                }
            }

            return await EjecutarAnulacion(ventaId, admin.Id, null);
        }

        private async Task<IActionResult> EjecutarAnulacion(Guid ventaId, Guid adminId, Guid? solicitudId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var venta = await _context.Ventas.Include(v => v.VentaDetalles).FirstOrDefaultAsync(v => v.Id == ventaId);
                if (venta == null || venta.Estado == "ANULADA") throw new Exception("Venta inválida o ya anulada.");

                venta.Estado = "ANULADA";

                _context.Attach(venta);
                venta.Estado = "ANULADA";
                _context.Entry(venta).Property(v => v.Estado).IsModified = true;
        
                // Asegurar que FolioInterno NO se marque como modificado
                _context.Entry(venta).Property(v => v.FolioInterno).IsModified = false;

                if (solicitudId.HasValue)
                {
                    var solicitud = await _context.VentaAnulacionSolicitudes.FindAsync(solicitudId.Value);
                    solicitud.Estado = "APROBADA";
                    solicitud.UsuarioResuelveId = adminId;
                    solicitud.ResolvedAt = DateTimeOffset.Now;
                    _context.VentaAnulacionSolicitudes.Update(solicitud);
                }

                // Devolver stock
                foreach (var detalle in venta.VentaDetalles)
                {
                    var producto = await _context.Productos.FindAsync(detalle.ProductoId);
                    if (producto != null)
                    {
                        producto.StockActual += detalle.Cantidad;
                        _context.Productos.Update(producto);
                    }
                }

                // El interceptor capturará automáticamente el cambio de estado de la venta
                // y el incremento de stock de los productos.

                var notaCredito = new NotaCredito
                {
                    VentaId = venta.Id,
                    Folio = "NC-" + venta.FolioInterno + "-" + DateTime.Now.ToString("fff"), // Simple folio generator
                    TotalDevuelto = venta.TotalFinal,
                    CreatedAt = DateTimeOffset.Now
                };
                _context.NotasCredito.Add(notaCredito);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { success = true, notaCreditoId = notaCredito.Id });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> NotaCredito(Guid ventaId)
        {
            var nota = await _context.NotasCredito
                .Include(n => n.Venta)
                .ThenInclude(v => v.VentaDetalles)
                .ThenInclude(vd => vd.Producto)
                .Include(n => n.Venta.Usuario)
                .FirstOrDefaultAsync(n => n.VentaId == ventaId);

            if (nota == null) return NotFound("Nota de crédito no encontrada.");

            return View(nota);
        }
        
        [HttpGet]
        public async Task<IActionResult> HasNotaCredito(Guid ventaId)
        {
            var nota = await _context.NotasCredito.FirstOrDefaultAsync(n => n.VentaId == ventaId);
            return Json(new { exists = nota != null, id = nota?.Id });
        }
    }
}
