using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaFacturacionPOS.Contexto;
using SistemaFacturacionPOS.Models;

namespace SistemaFacturacionPOS.Controllers.Inventario
{
    [Authorize(Roles = "Administrador")]
    public class BodegasController : Controller
    {
        private readonly SistemaFacturacionPOSContext context;

        public BodegasController(SistemaFacturacionPOSContext context)
        {
            this.context = context;
        }

        // Devuelve la vista (partial o completa según ajax)
        public IActionResult Index()
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView();
            return View();
        }

        // GET /Bodegas/GetBodegas
        [HttpGet]
        public async Task<IActionResult> GetBodegas()
        {
            try
            {
                var bodegas = await context.Bodegas
                    .Where(b => b.DeletedAt == null)
                    .Select(b => new { b.Id, b.Nombre, b.Descripcion })
                    .ToListAsync();
                return StatusCode(200, bodegas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener bodegas: {ex.Message}");
            }
        }

        // POST /Bodegas/AgregarBodega
        [HttpPost]
        public async Task<IActionResult> AgregarBodega([FromBody] Bodega bodega)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(bodega.Nombre))
                    return BadRequest("El nombre de la bodega es requerido.");

                bodega.DeletedAt = null;
                context.Bodegas.Add(bodega);
                await context.SaveChangesAsync();
                return StatusCode(200, "Bodega creada satisfactoriamente.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al crear la bodega: {ex.Message}");
            }
        }

        // PUT /Bodegas/ActualizarBodega/{id}
        [HttpPut]
        public async Task<IActionResult> ActualizarBodega(Guid id, [FromBody] Bodega bodega)
        {
            try
            {
                var existing = await context.Bodegas.FindAsync(id);
                if (existing == null || existing.DeletedAt != null)
                    return StatusCode(404, "Bodega no encontrada.");

                if (string.IsNullOrWhiteSpace(bodega.Nombre))
                    return BadRequest("El nombre de la bodega es requerido.");

                existing.Nombre = bodega.Nombre;
                existing.Descripcion = bodega.Descripcion;
                await context.SaveChangesAsync();
                return StatusCode(200, "Bodega actualizada exitosamente.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al actualizar la bodega: {ex.Message}");
            }
        }

        // DELETE /Bodegas/EliminarBodega/{id}
        [HttpDelete]
        public async Task<IActionResult> EliminarBodega(Guid id)
        {
            try
            {
                var existing = await context.Bodegas.FindAsync(id);
                if (existing == null || existing.DeletedAt != null)
                    return StatusCode(404, "Bodega no encontrada.");

                // Verificar si tiene productos asignados (Opción A: error controlado)
                bool tieneStock = await context.ProductoBodegas
                    .AnyAsync(pb => pb.BodegaId == id);
                if (tieneStock)
                    return StatusCode(400, "No se puede eliminar la bodega porque tiene productos con stock asignado. Reasigne o elimine las existencias primero.");

                existing.DeletedAt = DateTimeOffset.Now;
                await context.SaveChangesAsync();
                return StatusCode(200, "Bodega eliminada exitosamente.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al eliminar la bodega: {ex.Message}");
            }
        }
    }
}
