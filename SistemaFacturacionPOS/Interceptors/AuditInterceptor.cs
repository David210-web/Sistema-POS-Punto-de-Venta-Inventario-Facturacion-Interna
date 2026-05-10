using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SistemaFacturacionPOS.Models;
using SistemaFacturacionPOS.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SistemaFacturacionPOS.Interceptors
{
    public class AuditInterceptor : SaveChangesInterceptor
    {
        private readonly IAuditService _auditService;
        private static readonly string[] ExcludedTables = { "auditoria_logs" };
        private static readonly string[] SensitiveFields = { "password_hash", "PasswordHash" };

        public AuditInterceptor(IAuditService auditService)
        {
            _auditService = auditService;
        }

        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            AuditChanges(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            AuditChanges(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void AuditChanges(DbContext context)
        {
            if (context == null) return;

            var userId = _auditService.GetCurrentUserId();
            var auditEntries = new List<AuditoriaLog>();

            var entries = context.ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted)
                .ToList();

            foreach (var entry in entries)
            {
                var tableName = entry.Metadata.GetTableName();
                if (ExcludedTables.Contains(tableName)) continue;

                var auditLog = new AuditoriaLog
                {
                    Id = Guid.NewGuid(),
                    UsuarioId = userId,
                    TablaAfectada = tableName,
                    FechaHora = DateTimeOffset.Now
                };

                var oldValues = new Dictionary<string, object>();
                var newValues = new Dictionary<string, object>();

                switch (entry.State)
                {
                    case EntityState.Added:
                        auditLog.Accion = "INSERT";
                        foreach (var property in entry.Properties)
                        {
                            if (SensitiveFields.Contains(property.Metadata.Name)) continue;
                            newValues[property.Metadata.Name] = property.CurrentValue;
                        }
                        auditLog.ValorNuevo = JsonSerializer.Serialize(newValues);
                        break;

                    case EntityState.Deleted:
                        auditLog.Accion = "DELETE";
                        foreach (var property in entry.Properties)
                        {
                            if (SensitiveFields.Contains(property.Metadata.Name)) continue;
                            oldValues[property.Metadata.Name] = property.OriginalValue;
                        }
                        auditLog.ValorAnterior = JsonSerializer.Serialize(oldValues);
                        break;

                    case EntityState.Modified:
                        auditLog.Accion = "UPDATE";
                        foreach (var property in entry.Properties)
                        {
                            if (property.IsModified)
                            {
                                if (SensitiveFields.Contains(property.Metadata.Name)) continue;
                                oldValues[property.Metadata.Name] = property.OriginalValue;
                                newValues[property.Metadata.Name] = property.CurrentValue;
                            }
                        }
                        
                        if (oldValues.Count == 0) continue; // No hubo cambios en campos no sensibles

                        auditLog.ValorAnterior = JsonSerializer.Serialize(oldValues);
                        auditLog.ValorNuevo = JsonSerializer.Serialize(newValues);
                        break;
                }

                auditEntries.Add(auditLog);
            }

            if (auditEntries.Any())
            {
                context.Set<AuditoriaLog>().AddRange(auditEntries);
            }
        }
    }
}
