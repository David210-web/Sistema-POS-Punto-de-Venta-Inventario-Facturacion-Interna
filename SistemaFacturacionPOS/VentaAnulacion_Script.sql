USE [SistemaFacturacionPOS] -- Ajusta el nombre de la base de datos si es diferente
GO

CREATE TABLE venta_anulacion_solicitudes (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    venta_id UNIQUEIDENTIFIER NOT NULL,
    usuario_solicita_id UNIQUEIDENTIFIER NOT NULL,
    motivo VARCHAR(500) NOT NULL,
    estado VARCHAR(20) DEFAULT 'PENDIENTE', -- PENDIENTE, APROBADA, RECHAZADA
    usuario_resuelve_id UNIQUEIDENTIFIER NULL,
    motivo_rechazo VARCHAR(500) NULL,
    created_at DATETIMEOFFSET DEFAULT SYSDATETIMEOFFSET(),
    resolved_at DATETIMEOFFSET NULL,
    CONSTRAINT FK_solicitud_venta FOREIGN KEY (venta_id) REFERENCES ventas(id),
    CONSTRAINT FK_solicitud_usuario_solicita FOREIGN KEY (usuario_solicita_id) REFERENCES usuarios(id),
    CONSTRAINT FK_solicitud_usuario_resuelve FOREIGN KEY (usuario_resuelve_id) REFERENCES usuarios(id)
);
GO

CREATE TABLE notas_credito (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    venta_id UNIQUEIDENTIFIER NOT NULL,
    folio VARCHAR(50) NOT NULL UNIQUE,
    total_devuelto DECIMAL(12, 2) NOT NULL,
    created_at DATETIMEOFFSET DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT FK_nota_credito_venta FOREIGN KEY (venta_id) REFERENCES ventas(id)
);
GO
