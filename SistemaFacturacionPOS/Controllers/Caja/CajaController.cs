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

namespace SistemaFacturacionPOS.Controllers.Caja
{
    [Authorize]
    public class CajaController : Controller
    {
        private readonly SistemaFacturacionPOSContext _context;

        public CajaController(SistemaFacturacionPOSContext context)
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
                .Include(c => c.Ventas)
                .Where(c => c.UsuarioId == userId && c.Estado == true)
                .OrderByDescending(c => c.AbiertaAt)
                .FirstOrDefaultAsync();

            var viewModel = new CajaViewModel();

            if (sesionActiva != null)
            {
                viewModel.CajaAbierta = true;
                viewModel.MontoInicial = sesionActiva.MontoApertura;
                viewModel.AbiertaDesde = sesionActiva.AbiertaAt ?? DateTimeOffset.Now;
                
                var ventasDelDia = sesionActiva.Ventas.Where(v => v.Estado == "COMPLETADA").ToList();
                viewModel.VentasDelDia = ventasDelDia.Sum(v => v.TotalFinal);
                viewModel.CantidadTransacciones = ventasDelDia.Count;

                var tiempo = DateTimeOffset.Now - viewModel.AbiertaDesde;
                viewModel.TiempoAbierta = $"{(int)tiempo.TotalHours}h {tiempo.Minutes}m";

                viewModel.UltimasTransacciones = ventasDelDia
                    .OrderByDescending(v => v.CreatedAt)
                    .Take(5)
                    .Select(v => new VentaResumenViewModel
                    {
                        Folio = "V" + v.FolioInterno.ToString().PadLeft(7, '0'),
                        Hora = v.CreatedAt?.ToString("HH:mm") ?? "",
                        Total = v.TotalFinal
                    })
                    .ToList();
            }
            else
            {
                viewModel.CajaAbierta = false;
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView(viewModel);
            }

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AbrirCaja([FromBody] AbrirCajaRequest request)
        {
            try
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!Guid.TryParse(userIdStr, out Guid userId))
                {
                    return Unauthorized();
                }

                if (request.MontoInicial <= 0)
                {
                    return BadRequest("El monto inicial debe ser mayor a 0.");
                }

                // Verificar si ya hay una caja abierta
                var existeSesion = await _context.CajaSesiones
                    .AnyAsync(c => c.UsuarioId == userId && c.Estado == true);

                if (existeSesion)
                {
                    return BadRequest("Ya existe una caja abierta para este usuario.");
                }

                var nuevaSesion = new CajaSesion
                {
                    UsuarioId = userId,
                    MontoApertura = request.MontoInicial,
                    AbiertaAt = DateTimeOffset.Now,
                    Estado = true
                };

                _context.CajaSesiones.Add(nuevaSesion);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Caja abierta exitosamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al abrir la caja: {ex.Message}");
            }
        }
        [HttpPost]
        public async Task<IActionResult> CerrarCaja([FromBody] CerrarCajaRequest request)
        {
            try
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!Guid.TryParse(userIdStr, out Guid userId))
                {
                    return Unauthorized();
                }

                var sesionActiva = await _context.CajaSesiones
                    .Include(c => c.Ventas)
                    .Where(c => c.UsuarioId == userId && c.Estado == true)
                    .FirstOrDefaultAsync();

                if (sesionActiva == null)
                {
                    return BadRequest("No existe una caja abierta para este usuario.");
                }

                var ventasDelDia = sesionActiva.Ventas.Where(v => v.Estado == "COMPLETADA").Sum(v => v.TotalFinal);
                
                sesionActiva.MontoCierreSistema = sesionActiva.MontoApertura + ventasDelDia;
                sesionActiva.MontoCierreFisico = request.MontoFisico;
                // Diferencia is a computed column in the DB, so we don't set it explicitly unless EF requires it,
                // but wait, EF core maps computed columns to not be updatable. Let's just set the physical amount.
                sesionActiva.Estado = false;
                sesionActiva.CerradaAt = DateTimeOffset.Now;

                _context.CajaSesiones.Update(sesionActiva);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Caja cerrada exitosamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al cerrar la caja: {ex.Message}");
            }
        }
    }

    public class AbrirCajaRequest
    {
        public decimal MontoInicial { get; set; }
    }

    public class CerrarCajaRequest
    {
        public decimal MontoFisico { get; set; }
    }
}
