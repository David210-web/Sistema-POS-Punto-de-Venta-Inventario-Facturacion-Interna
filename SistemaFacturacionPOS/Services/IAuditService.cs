using System;
using System.Collections.Generic;

namespace SistemaFacturacionPOS.Services
{
    public interface IAuditService
    {
        Guid? GetCurrentUserId();
        void LogManualAction(Guid? userId, string tabla, string accion, string valorAnterior, string valorNuevo);
    }
}
