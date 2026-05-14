using SistemaFacturacionPOS.Models;
using SistemaFacturacionPOS.Repositories.Interfaces;
using SistemaFacturacionPOS.Services.Interfaces;

namespace SistemaFacturacionPOS.Services
{
    public class BodegasService : IBodegasService
    {
        private readonly IBodegasRepository _repo;

        public BodegasService(IBodegasRepository repo)
        {
            _repo = repo;
        }

        public async Task<(bool ok, object? data, string msg)> GetBodegasAsync()
        {
            try
            {
                var bodegas = await _repo.GetBodegasActivasAsync();
                return (true, bodegas, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, null, ex.Message);
            }
        }

        public async Task<(bool ok, string msg)> AgregarBodegaAsync(Bodega b)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(b.Nombre))
                    return (false, "El nombre de la bodega es requerido.");

                b.DeletedAt = null;
                _repo.AddBodega(b);
                await _repo.SaveChangesAsync();
                return (true, "Bodega creada satisfactoriamente.");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool ok, string msg)> ActualizarBodegaAsync(Guid id, Bodega b)
        {
            try
            {
                var existing = await _repo.GetBodegaAsync(id);
                if (existing == null || existing.DeletedAt != null)
                    return (false, "Bodega no encontrada.");

                if (string.IsNullOrWhiteSpace(b.Nombre))
                    return (false, "El nombre de la bodega es requerido.");

                existing.Nombre      = b.Nombre;
                existing.Descripcion = b.Descripcion;
                await _repo.SaveChangesAsync();
                return (true, "Bodega actualizada exitosamente.");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool ok, string msg)> EliminarBodegaAsync(Guid id)
        {
            try
            {
                var existing = await _repo.GetBodegaAsync(id);
                if (existing == null || existing.DeletedAt != null)
                    return (false, "Bodega no encontrada.");

                bool tieneStock = await _repo.TieneStockAsignadoAsync(id);
                if (tieneStock)
                    return (false, "No se puede eliminar la bodega porque tiene productos con stock asignado. Reasigne o elimine las existencias primero.");

                existing.DeletedAt = DateTimeOffset.Now;
                await _repo.SaveChangesAsync();
                return (true, "Bodega eliminada exitosamente.");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }
}
