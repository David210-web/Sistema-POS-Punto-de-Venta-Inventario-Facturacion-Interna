-- ============================================================
-- MÓDULO: Control de Existencias por Bodega
-- Fecha  : 2026-05-09
-- Notas  : Ejecutar sobre la base de datos SistemaPOS
-- ============================================================

-- ============================================================
-- 1. TABLA bodegas
-- ============================================================
CREATE TABLE bodegas (
    id          UNIQUEIDENTIFIER PRIMARY KEY DEFAULT (NEWID()),
    nombre      VARCHAR(150)   NOT NULL,
    descripcion VARCHAR(500)   NULL,
    deleted_at  DATETIMEOFFSET NULL
);
GO

-- ============================================================
-- 2. TABLA producto_bodega
--    UNIQUE (producto_id, bodega_id) evita duplicados
--    FK sin ON DELETE CASCADE → eliminar bodega con stock asignado
--    genera error controlado (Opción A)
-- ============================================================
CREATE TABLE producto_bodega (
    id          UNIQUEIDENTIFIER PRIMARY KEY DEFAULT (NEWID()),
    producto_id UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT FK_pb_producto REFERENCES productos(id),
    bodega_id   UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT FK_pb_bodega  REFERENCES bodegas(id),
    stock       INT              NOT NULL DEFAULT 0,
    CONSTRAINT UQ_producto_bodega UNIQUE (producto_id, bodega_id)
);
GO

-- ============================================================
-- 3. DATOS DE EJEMPLO (bodegas)
-- ============================================================
INSERT INTO bodegas (nombre, descripcion)
VALUES
    ('Central',  'Bodega principal de almacenamiento'),
    ('Tienda',   'Vitrina y stock en tienda'),
    ('Bodega 2', 'Almacén secundario');
GO

-- ============================================================
-- 4. CONSULTAS DE REFERENCIA
-- ============================================================

-- 4a. Stock total de un producto
-- SELECT p.nombre, p.stock_actual AS total
-- FROM   productos p
-- WHERE  p.id = '<producto_id>';

-- 4b. Desglose por bodega de un producto
-- SELECT b.nombre AS bodega, pb.stock
-- FROM   producto_bodega pb
-- JOIN   bodegas b ON b.id = pb.bodega_id
-- WHERE  pb.producto_id = '<producto_id>';

-- 4c. Recalcular stock_actual de un producto (se ejecuta desde el backend)
-- UPDATE productos
-- SET    stock_actual = (
--     SELECT ISNULL(SUM(pb.stock), 0)
--     FROM   producto_bodega pb
--     WHERE  pb.producto_id = productos.id
-- )
-- WHERE  id = '<producto_id>';
GO
