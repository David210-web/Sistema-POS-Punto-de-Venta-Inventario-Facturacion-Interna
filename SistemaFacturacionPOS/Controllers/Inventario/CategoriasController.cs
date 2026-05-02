using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaFacturacionPOS.Contexto;
using SistemaFacturacionPOS.Models;
using System;
using System.Threading.Tasks;

namespace SistemaFacturacionPOS.Controllers.Inventario
{
    [Authorize(Roles = "Administrador")]
    public class CategoriasController : Controller
    {
        private readonly SistemaFacturacionPOSContext context;

        public CategoriasController(SistemaFacturacionPOSContext context)
        {
            this.context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetCategorias()
        {
            try
            {
                var result = await context.Categorias.ToListAsync();
                return StatusCode(200, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Hubo un error en el servidor {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AgregarCategoria([FromBody] Categoria categoria)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(categoria.Nombre))
                {
                    return BadRequest("El nombre de la categoría es requerido.");
                }

                context.Categorias.Add(categoria);
                await context.SaveChangesAsync();
                return StatusCode(200, "Categoría creada satisfactoriamente");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Hubo un error en el servidor {ex.Message}");
            }
        }

        [HttpPut]
        public async Task<IActionResult> ActualizarCategoria(Guid id, [FromBody] Categoria categoria)
        {
            try
            {
                var existingCategoria = await context.Categorias.FindAsync(id);
                if (existingCategoria == null)
                {
                    return StatusCode(404, "Categoría no encontrada");
                }

                existingCategoria.Nombre = categoria.Nombre;
                await context.SaveChangesAsync();
                return StatusCode(200, "Categoría actualizada exitosamente");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Hubo un error en el servidor");
            }
        }

        [HttpDelete]
        public async Task<IActionResult> EliminarCategoria(Guid id)
        {
            try
            {
                var existingCategoria = await context.Categorias.FindAsync(id);
                if (existingCategoria == null)
                {
                    return StatusCode(404, "Categoría no encontrada");
                }

                // Check if it has products before deleting
                var tieneProductos = await context.Productos.AnyAsync(p => p.CategoriaId == id && p.DeletedAt == null);
                if (tieneProductos)
                {
                    return BadRequest("No se puede eliminar la categoría porque tiene productos activos asociados.");
                }

                context.Categorias.Remove(existingCategoria);
                await context.SaveChangesAsync();
                return StatusCode(200, "Categoría eliminada exitosamente");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al eliminar la categoría: {ex.Message}");
            }
        }
    }
}
