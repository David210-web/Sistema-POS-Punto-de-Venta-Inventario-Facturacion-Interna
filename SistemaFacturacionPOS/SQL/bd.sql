USE master;
GO

-- 1. CREACIÓN DE LA BASE DE DATOS
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'SistemaPOS')
BEGIN
    CREATE DATABASE SistemaPOS;
END
GO

USE SistemaPOS;
GO

-- 2. TABLAS BASE (Sin dependencias)

CREATE TABLE SistemaPOS.dbo.roles (
	id uniqueidentifier DEFAULT newid() NOT NULL,
	nombre varchar(50) COLLATE Modern_Spanish_CI_AS NOT NULL,
	descripcion nvarchar(MAX) COLLATE Modern_Spanish_CI_AS NULL,
	CONSTRAINT PK_roles PRIMARY KEY (id),
	CONSTRAINT UQ_roles_nombre UNIQUE (nombre)
);

CREATE TABLE SistemaPOS.dbo.categorias (
	id uniqueidentifier DEFAULT newid() NOT NULL,
	nombre varchar(100) COLLATE Modern_Spanish_CI_AS NOT NULL,
	CONSTRAINT PK_categorias PRIMARY KEY (id),
	CONSTRAINT UQ_categorias_nombre UNIQUE (nombre)
);

CREATE TABLE SistemaPOS.dbo.bodegas (
	id uniqueidentifier DEFAULT newid() NOT NULL,
	nombre varchar(150) COLLATE Modern_Spanish_CI_AS NOT NULL,
	descripcion varchar(500) COLLATE Modern_Spanish_CI_AS NULL,
	deleted_at datetimeoffset NULL,
	CONSTRAINT PK_bodegas PRIMARY KEY (id)
);
GO

-- 3. INSERCIÓN DE ROLES (Necesarios para crear usuarios)
INSERT INTO SistemaPOS.dbo.roles (id, nombre, descripcion)
VALUES (N'E68F943E-FD41-4A79-873E-99F76CE191DE', N'Cajero', N'Es el cajero del sistema'),
       (N'FEA1F9F4-7371-4BF6-8C30-D24BD6A8C00A', N'Administrador', N'Rol de administrador');
GO

-- 4. TABLA DE USUARIOS E INSERCIONES
CREATE TABLE SistemaPOS.dbo.usuarios (
	id uniqueidentifier DEFAULT newid() NOT NULL,
	username varchar(50) COLLATE Modern_Spanish_CI_AS NOT NULL,
	password_hash nvarchar(MAX) COLLATE Modern_Spanish_CI_AS NOT NULL,
	rol_id uniqueidentifier NULL,
	activo bit NULL,
	created_at datetimeoffset NULL,
	updated_at datetimeoffset NULL,
	CONSTRAINT PK_usuarios PRIMARY KEY (id),
	CONSTRAINT UQ_usuarios_username UNIQUE (username),
	CONSTRAINT FK_usuarios_roles FOREIGN KEY (rol_id) REFERENCES SistemaPOS.dbo.roles(id)
);
GO

INSERT INTO SistemaPOS.dbo.usuarios (id, username, password_hash, rol_id, activo, created_at, updated_at)
VALUES 
(N'9B422BED-F9B0-457D-B695-254A2D12AE62', N'David Castillo', N'$2a$11$i.BxS8J3lVDREoSQ/8LLKO3HGjEUJSjQB0lPyk9g3Vfsbzogtn.by', N'FEA1F9F4-7371-4BF6-8C30-D24BD6A8C00A', 1, '2026-05-08T20:06:24.319-06:00', '2026-05-08T20:06:24.319-06:00'),
(N'028A3FC4-77B9-48A6-BC4D-A0786F697549', N'Manuel Hernandez', N'$2a$11$qlWDQKyAwhXCHWWRHCB43O2PitqPIHqwq7Y4DYfya9a7prKX0N3S2', N'FEA1F9F4-7371-4BF6-8C30-D24BD6A8C00A', 1, '2026-05-09T15:34:01.985-06:00', '2026-05-10T16:49:34.698-06:00');
GO

-- 5. TABLAS DE PRODUCTOS E INVENTARIO
CREATE TABLE SistemaPOS.dbo.productos (
	id uniqueidentifier DEFAULT newid() NOT NULL,
	codigo_barras varchar(100) COLLATE Modern_Spanish_CI_AS NULL,
	nombre varchar(255) COLLATE Modern_Spanish_CI_AS NOT NULL,
	precio_unitario decimal(12,2) NOT NULL,
	stock_actual int NULL,
	stock_minimo int NULL,
	categoria_id uniqueidentifier NULL,
	deleted_at datetimeoffset NULL,
	CONSTRAINT PK_productos PRIMARY KEY (id),
	CONSTRAINT UQ_productos_codigo UNIQUE (codigo_barras),
	CONSTRAINT FK_productos_categorias FOREIGN KEY (categoria_id) REFERENCES SistemaPOS.dbo.categorias(id)
);

CREATE TABLE SistemaPOS.dbo.producto_bodega (
	id uniqueidentifier DEFAULT newid() NOT NULL,
	producto_id uniqueidentifier NOT NULL,
	bodega_id uniqueidentifier NOT NULL,
	stock int DEFAULT 0 NOT NULL,
	CONSTRAINT PK_producto_bodega PRIMARY KEY (id),
	CONSTRAINT UQ_producto_bodega UNIQUE (producto_id,bodega_id),
	CONSTRAINT FK_pb_bodegas FOREIGN KEY (bodega_id) REFERENCES SistemaPOS.dbo.bodegas(id),
	CONSTRAINT FK_pb_productos FOREIGN KEY (producto_id) REFERENCES SistemaPOS.dbo.productos(id)
);
GO

-- 6. SESIONES Y VENTAS
CREATE TABLE SistemaPOS.dbo.caja_sesiones (
	id uniqueidentifier DEFAULT newid() NOT NULL,
	usuario_id uniqueidentifier NULL,
	monto_apertura decimal(12,2) NOT NULL,
	monto_cierre_sistema decimal(12,2) NULL,
	monto_cierre_fisico decimal(12,2) NULL,
	diferencia AS ([monto_cierre_fisico]-[monto_cierre_sistema]) PERSISTED,
	abierta_at datetimeoffset DEFAULT sysdatetimeoffset() NULL,
	cerrada_at datetimeoffset NULL,
	estado bit DEFAULT 1 NULL,
	CONSTRAINT PK_caja_sesiones PRIMARY KEY (id),
	CONSTRAINT FK_caja_usuario FOREIGN KEY (usuario_id) REFERENCES SistemaPOS.dbo.usuarios(id)
);

CREATE TABLE SistemaPOS.dbo.ventas (
	id uniqueidentifier DEFAULT newid() NOT NULL,
	folio_interno int IDENTITY(1,1) NOT NULL,
	usuario_id uniqueidentifier NULL,
	caja_sesion_id uniqueidentifier NULL,
	total_neto decimal(12,2) NOT NULL,
	impuestos decimal(12,2) NOT NULL,
	total_final decimal(12,2) NOT NULL,
	metodo_pago varchar(50) COLLATE Modern_Spanish_CI_AS NULL,
	estado varchar(20) COLLATE Modern_Spanish_CI_AS NULL,
	created_at datetimeoffset NULL,
	CONSTRAINT PK_ventas PRIMARY KEY (id),
	CONSTRAINT UQ_ventas_folio UNIQUE (folio_interno),
	CONSTRAINT FK_ventas_usuario FOREIGN KEY (usuario_id) REFERENCES SistemaPOS.dbo.usuarios(id),
	CONSTRAINT FK_ventas_sesion FOREIGN KEY (caja_sesion_id) REFERENCES SistemaPOS.dbo.caja_sesiones(id)
);

CREATE TABLE SistemaPOS.dbo.venta_detalles (
	id uniqueidentifier DEFAULT newid() NOT NULL,
	venta_id uniqueidentifier NULL,
	producto_id uniqueidentifier NULL,
	cantidad int NOT NULL,
	precio_unitario_historico decimal(12,2) NOT NULL,
	subtotal AS ([cantidad]*[precio_unitario_historico]) PERSISTED,
	CONSTRAINT PK_venta_detalles PRIMARY KEY (id),
	CONSTRAINT FK_detalles_venta FOREIGN KEY (venta_id) REFERENCES SistemaPOS.dbo.ventas(id),
	CONSTRAINT FK_detalles_producto FOREIGN KEY (producto_id) REFERENCES SistemaPOS.dbo.productos(id)
);
GO

-- 7. AUDITORÍA Y LOGS
CREATE TABLE SistemaPOS.dbo.auditoria_logs (
	id uniqueidentifier DEFAULT newid() NOT NULL,
	usuario_id uniqueidentifier NULL,
	tabla_afectada varchar(100) COLLATE Modern_Spanish_CI_AS NULL,
	accion varchar(20) COLLATE Modern_Spanish_CI_AS NULL,
	valor_anterior nvarchar(MAX) COLLATE Modern_Spanish_CI_AS NULL,
	valor_nuevo nvarchar(MAX) COLLATE Modern_Spanish_CI_AS NULL,
	fecha_hora datetimeoffset DEFAULT sysdatetimeoffset() NULL,
	CONSTRAINT PK_auditoria PRIMARY KEY (id)
);

CREATE TABLE SistemaPOS.dbo.inventario_movimientos (
	id uniqueidentifier DEFAULT newid() NOT NULL,
	producto_id uniqueidentifier NULL,
	usuario_id uniqueidentifier NULL,
	tipo varchar(20) COLLATE Modern_Spanish_CI_AS NOT NULL,
	cantidad int NOT NULL,
	justificacion nvarchar(MAX) COLLATE Modern_Spanish_CI_AS NULL,
	created_at datetimeoffset NULL,
	CONSTRAINT PK_movimientos PRIMARY KEY (id),
	CONSTRAINT FK_mov_producto FOREIGN KEY (producto_id) REFERENCES SistemaPOS.dbo.productos(id)
);
GO

-- 8. ÍNDICES (Para mejorar rendimiento)
CREATE NONCLUSTERED INDEX idx_inventario_producto ON SistemaPOS.dbo.inventario_movimientos (producto_id ASC);
CREATE NONCLUSTERED INDEX idx_producto_codigo ON SistemaPOS.dbo.productos (codigo_barras ASC);
CREATE NONCLUSTERED INDEX idx_venta_fecha ON SistemaPOS.dbo.ventas (created_at ASC);
GO

-- 9. VISTAS (Cada una con su propio GO)
CREATE OR ALTER VIEW [dbo].[vista_alertas_stock] AS
SELECT id, nombre, stock_actual, stock_minimo
FROM SistemaPOS.dbo.productos
WHERE stock_actual <= stock_minimo
AND deleted_at IS NULL;
GO

CREATE OR ALTER VIEW [dbo].[vista_logs] AS
SELECT al.id, u.username, al.tabla_afectada, al.accion, al.valor_anterior, al.valor_nuevo, al.fecha_hora 
FROM SistemaPOS.dbo.auditoria_logs al
INNER JOIN SistemaPOS.dbo.usuarios u ON al.usuario_id = u.id;
GO