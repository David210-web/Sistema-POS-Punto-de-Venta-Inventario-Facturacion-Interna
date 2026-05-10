using Microsoft.AspNetCore.Http;
using SistemaFacturacionPOS.Contexto;
using SistemaFacturacionPOS.Models;
using System;
using System.Security.Claims;

namespace SistemaFacturacionPOS.Services
{
    public class AuditService : IAuditService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IServiceProvider _serviceProvider;

        public AuditService(IHttpContextAccessor httpContextAccessor, IServiceProvider serviceProvider)
        {
            _httpContextAccessor = httpContextAccessor;
            _serviceProvider = serviceProvider;
        }

        public Guid? GetCurrentUserId()
        {
            var userIdStr = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(userIdStr, out Guid userId))
            {
                return userId;
            }
            return null;
        }

        public void LogManualAction(Guid? userId, string tabla, string accion, string valorAnterior, string valorNuevo)
        {
            // Nota: Este método es para auditoría manual explícita.
            // Para que funcione, necesitamos una instancia del contexto.
            // Dado que el interceptor ya captura SaveChanges, podríamos simplemente 
            // insertar un registro en la tabla de auditoría manualmente si es necesario.
            
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<SistemaFacturacionPOSContext>();
            
            var log = new AuditoriaLog
            {
                Id = Guid.NewGuid(),
                UsuarioId = userId ?? GetCurrentUserId(),
                TablaAfectada = tabla,
                Accion = accion,
                ValorAnterior = valorAnterior,
                ValorNuevo = valorNuevo,
                FechaHora = DateTimeOffset.Now
            };

            context.AuditoriaLogs.Add(log);
            context.SaveChanges();
        }
    }
}
