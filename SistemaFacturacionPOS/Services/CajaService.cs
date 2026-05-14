using SistemaFacturacionPOS.Models;
using SistemaFacturacionPOS.Models.ViewModels;
using SistemaFacturacionPOS.Repositories.Interfaces;
using SistemaFacturacionPOS.Services.Interfaces;

namespace SistemaFacturacionPOS.Services
{
    public class CajaService : ICajaService
    {
        private readonly ICajaRepository _repo;

        public CajaService(ICajaRepository repo)
        {
            _repo = repo;
        }

        public async Task<CajaViewModel> GetResumenAsync(Guid userId)
        {
            var sesionActiva = await _repo.GetSesionActivaConVentasAsync(userId);
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
                        Hora  = v.CreatedAt?.ToString("HH:mm") ?? "",
                        Total = v.TotalFinal
                    })
                    .ToList();
            }
            else
            {
                viewModel.CajaAbierta = false;
            }

            return viewModel;
        }

        public async Task<(bool ok, string msg)> AbrirCajaAsync(Guid userId, decimal monto)
        {
            if (monto <= 0)
                return (false, "El monto inicial debe ser mayor a 0.");

            if (monto > 9999999999.99m)
                return (false, "El monto inicial excede el límite permitido por el sistema.");

            var existeSesion = await _repo.TieneSesionActivaAsync(userId);
            if (existeSesion)
                return (false, "Ya existe una caja abierta para este usuario.");

            var nuevaSesion = new CajaSesion
            {
                UsuarioId    = userId,
                MontoApertura = monto,
                AbiertaAt    = DateTimeOffset.Now,
                Estado       = true
            };

            _repo.AddSesion(nuevaSesion);
            await _repo.SaveChangesAsync();

            return (true, "Caja abierta exitosamente.");
        }

        public async Task<(bool ok, string msg)> CerrarCajaAsync(Guid userId, decimal montoFisico)
        {
            if (montoFisico > 9999999999.99m)
                return (false, "El monto físico excede el límite permitido por el sistema.");

            var sesionActiva = await _repo.GetSesionActivaConVentasAsync(userId);
            if (sesionActiva == null)
                return (false, "No existe una caja abierta para este usuario.");

            var ventasDelDia = sesionActiva.Ventas.Where(v => v.Estado == "COMPLETADA").Sum(v => v.TotalFinal);

            sesionActiva.MontoCierreSistema = sesionActiva.MontoApertura + ventasDelDia;
            sesionActiva.MontoCierreFisico  = montoFisico;
            sesionActiva.Estado             = false;
            sesionActiva.CerradaAt          = DateTimeOffset.Now;

            await _repo.SaveChangesAsync();

            return (true, "Caja cerrada exitosamente.");
        }
    }
}
