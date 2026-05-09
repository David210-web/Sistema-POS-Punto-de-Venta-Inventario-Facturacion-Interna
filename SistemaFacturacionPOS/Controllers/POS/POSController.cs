using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaFacturacionPOS.Contexto;
using SistemaFacturacionPOS.Models;
using SistemaFacturacionPOS.Models.ViewModels;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SistemaFacturacionPOS.Controllers.POS
{
    [Authorize]
    public class POSController : Controller
    {
        private readonly SistemaFacturacionPOSContext _context;

        public POSController(SistemaFacturacionPOSContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out Guid userId))
            {
                return Unauthorized();
            }

            var sesionActiva = await _context.CajaSesiones
                .AnyAsync(c => c.UsuarioId == userId && c.Estado == true);

            if (!sesionActiva)
            {
                // Si la vista es solicitada por ajax, podemos indicar redireccion
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return PartialView("RequiereCaja");
                }
                return RedirectToAction("Index", "Caja");
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView();
            }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> BuscarProductos(string q)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Length < 3)
            {
                return Json(new object[] { });
            }

            var query = q.ToLower();

            var productos = await _context.Productos
                .Where(p => p.DeletedAt == null && 
                            (p.Nombre.ToLower().Contains(query) || p.CodigoBarras == q))
                .Select(p => new
                {
                    p.Id,
                    p.Nombre,
                    p.CodigoBarras,
                    p.PrecioUnitario,
                    p.StockActual
                })
                .Take(15)
                .ToListAsync();

            return Json(productos);
        }

        [HttpPost]
        public async Task<IActionResult> FinalizarVenta([FromBody] VentaRequestDTO request)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out Guid userId)) return Unauthorized();

            var sesionActiva = await _context.CajaSesiones
                .FirstOrDefaultAsync(c => c.UsuarioId == userId && c.Estado == true);

            if (sesionActiva == null)
            {
                return BadRequest("No existe una sesión de caja abierta.");
            }

            if (request.Detalles == null || !request.Detalles.Any())
            {
                return BadRequest("El carrito está vacío.");
            }

            // Iniciar transacción atómica (EF Core handle this inside SaveChanges, but we can be explicit or rely on it)
            // SaveChangesAsync is atomic by default. If any error occurs, it rolls back.

            var venta = new Venta
            {
                UsuarioId = userId,
                CajaSesionId = sesionActiva.Id,
                TotalNeto = request.Total,
                Impuestos = 0, // No especificaron manejo de impuestos aún
                TotalFinal = request.Total,
                MetodoPago = request.MetodoPago,
                Estado = "COMPLETADA",
                CreatedAt = DateTimeOffset.Now
            };

            foreach (var det in request.Detalles)
            {
                var producto = await _context.Productos.FindAsync(det.ProductoId);
                if (producto == null) return BadRequest($"Producto no encontrado.");

                if (producto.StockActual < det.Cantidad)
                {
                    return BadRequest($"No hay stock suficiente para: {producto.Nombre}. Stock actual: {producto.StockActual}");
                }

                producto.StockActual -= det.Cantidad;
                _context.Productos.Update(producto);

                venta.VentaDetalles.Add(new VentaDetalle
                {
                    ProductoId = producto.Id,
                    Cantidad = det.Cantidad,
                    PrecioUnitarioHistorico = det.PrecioUnitario
                });
            }

            _context.Ventas.Add(venta);

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { ventaId = venta.Id, message = "Venta registrada con éxito." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error al registrar la venta: " + ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Ticket(Guid id)
        {
            var venta = await _context.Ventas
                .Include(v => v.Usuario)
                .Include(v => v.VentaDetalles)
                .ThenInclude(vd => vd.Producto)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (venta == null)
            {
                return NotFound();
            }

            return View(venta); // We will return the plain HTML view without layout
        }
    }
}
