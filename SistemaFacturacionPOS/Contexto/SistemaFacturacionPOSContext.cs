using Microsoft.EntityFrameworkCore;
using SistemaFacturacionPOS.Models;

namespace SistemaFacturacionPOS.Contexto
{
    public partial class SistemaFacturacionPOSContext : DbContext
    {
        public SistemaFacturacionPOSContext() { }

        public SistemaFacturacionPOSContext(DbContextOptions<SistemaFacturacionPOSContext> options) : base(options)
        {
        }

        public virtual DbSet<Rol> Roles { get; set; }
        public virtual DbSet<Usuario> Usuarios { get; set; }
        public virtual DbSet<Categoria> Categorias { get; set; }
        public virtual DbSet<Producto> Productos { get; set; }
        public virtual DbSet<CajaSesion> CajaSesiones { get; set; }
        public virtual DbSet<Venta> Ventas { get; set; }
        public virtual DbSet<VentaDetalle> VentaDetalles { get; set; }
        public virtual DbSet<InventarioMovimiento> InventarioMovimientos { get; set; }
        public virtual DbSet<AuditoriaLog> AuditoriaLogs { get; set; }
        public virtual DbSet<VistaAlertasStock> VistaAlertasStocks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Rol>(entity =>
            {
                entity.ToTable("roles");
                entity.HasIndex(e => e.Nombre).IsUnique();
                entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("(newid())");
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(50).IsUnicode(false).HasColumnName("nombre");
                entity.Property(e => e.Descripcion).HasColumnName("descripcion");
            });

            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("usuarios");
                entity.HasIndex(e => e.Username).IsUnique();
                entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("(newid())");
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50).IsUnicode(false).HasColumnName("username");
                entity.Property(e => e.PasswordHash).IsRequired().HasColumnName("password_hash");
                entity.Property(e => e.RolId).HasColumnName("rol_id");
                entity.Property(e => e.Activo).HasColumnName("activo").HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("(sysdatetimeoffset())");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("(sysdatetimeoffset())");

                entity.HasOne(d => d.Rol)
                    .WithMany(p => p.Usuarios)
                    .HasForeignKey(d => d.RolId)
                    .HasConstraintName("FK_usuarios_roles");
            });

            modelBuilder.Entity<Categoria>(entity =>
            {
                entity.ToTable("categorias");
                entity.HasIndex(e => e.Nombre).IsUnique();
                entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("(newid())");
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100).IsUnicode(false).HasColumnName("nombre");
            });

            modelBuilder.Entity<Producto>(entity =>
            {
                entity.ToTable("productos");
                entity.HasIndex(e => e.CodigoBarras).IsUnique();
                entity.HasIndex(e => e.Nombre).HasDatabaseName("idx_producto_nombre");
                entity.HasIndex(e => e.CodigoBarras).HasDatabaseName("idx_producto_codigo");
                entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("(newid())");
                entity.Property(e => e.CodigoBarras).HasMaxLength(100).IsUnicode(false).HasColumnName("codigo_barras");
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(255).IsUnicode(false).HasColumnName("nombre");
                entity.Property(e => e.PrecioUnitario).HasColumnType("decimal(12, 2)").HasColumnName("precio_unitario");
                entity.Property(e => e.StockActual).HasColumnName("stock_actual").HasDefaultValue(0);
                entity.Property(e => e.StockMinimo).HasColumnName("stock_minimo").HasDefaultValue(0);
                entity.Property(e => e.CategoriaId).HasColumnName("categoria_id");
                entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");

                entity.HasOne(d => d.Categoria)
                    .WithMany(p => p.Productos)
                    .HasForeignKey(d => d.CategoriaId)
                    .HasConstraintName("FK_productos_categoria");
            });

            modelBuilder.Entity<CajaSesion>(entity =>
            {
                entity.ToTable("caja_sesiones");
                entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("(newid())");
                entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");
                entity.Property(e => e.MontoApertura).HasColumnType("decimal(12, 2)").HasColumnName("monto_apertura");
                entity.Property(e => e.MontoCierreSistema).HasColumnType("decimal(12, 2)").HasColumnName("monto_cierre_sistema");
                entity.Property(e => e.MontoCierreFisico).HasColumnType("decimal(12, 2)").HasColumnName("monto_cierre_fisico");
                entity.Property(e => e.Diferencia).HasColumnType("decimal(12, 2)").HasColumnName("diferencia").HasComputedColumnSql("([monto_cierre_fisico]-[monto_cierre_sistema])", true);
                entity.Property(e => e.AbiertaAt).HasColumnName("abierta_at").HasDefaultValueSql("(sysdatetimeoffset())");
                entity.Property(e => e.CerradaAt).HasColumnName("cerrada_at");
                entity.Property(e => e.Estado).HasColumnName("estado").HasDefaultValue(true);

                entity.HasOne(d => d.Usuario)
                    .WithMany(p => p.CajaSesiones)
                    .HasForeignKey(d => d.UsuarioId)
                    .HasConstraintName("FK_caja_usuario");
            });

            modelBuilder.Entity<Venta>(entity =>
            {
                entity.ToTable("ventas");
                entity.HasIndex(e => e.FolioInterno).IsUnique();
                entity.HasIndex(e => e.CreatedAt).HasDatabaseName("idx_venta_fecha");
                entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("(newid())");
                entity.Property(e => e.FolioInterno).ValueGeneratedOnAdd().HasColumnName("folio_interno");
                entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");
                entity.Property(e => e.CajaSesionId).HasColumnName("caja_sesion_id");
                entity.Property(e => e.TotalNeto).HasColumnType("decimal(12, 2)").HasColumnName("total_neto");
                entity.Property(e => e.Impuestos).HasColumnType("decimal(12, 2)").HasColumnName("impuestos");
                entity.Property(e => e.TotalFinal).HasColumnType("decimal(12, 2)").HasColumnName("total_final");
                entity.Property(e => e.MetodoPago).HasMaxLength(50).IsUnicode(false).HasColumnName("metodo_pago");
                entity.Property(e => e.Estado).HasMaxLength(20).IsUnicode(false).HasColumnName("estado").HasDefaultValueSql("('COMPLETADA')");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("(sysdatetimeoffset())");

                entity.HasOne(d => d.Usuario)
                    .WithMany(p => p.Ventas)
                    .HasForeignKey(d => d.UsuarioId)
                    .HasConstraintName("FK_ventas_usuario");

                entity.HasOne(d => d.CajaSesion)
                    .WithMany(p => p.Ventas)
                    .HasForeignKey(d => d.CajaSesionId)
                    .HasConstraintName("FK_ventas_caja");
            });

            modelBuilder.Entity<VentaDetalle>(entity =>
            {
                entity.ToTable("venta_detalles");
                entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("(newid())");
                entity.Property(e => e.VentaId).HasColumnName("venta_id");
                entity.Property(e => e.ProductoId).HasColumnName("producto_id");
                entity.Property(e => e.Cantidad).HasColumnName("cantidad");
                entity.Property(e => e.PrecioUnitarioHistorico).HasColumnType("decimal(12, 2)").HasColumnName("precio_unitario_historico");
                entity.Property(e => e.Subtotal).HasColumnType("decimal(12, 2)").HasColumnName("subtotal").HasComputedColumnSql("([cantidad]*[precio_unitario_historico])", true);

                entity.HasOne(d => d.Producto)
                    .WithMany(p => p.VentaDetalles)
                    .HasForeignKey(d => d.ProductoId)
                    .HasConstraintName("FK_detalle_producto");

                entity.HasOne(d => d.Venta)
                    .WithMany(p => p.VentaDetalles)
                    .HasForeignKey(d => d.VentaId)
                    .HasConstraintName("FK_detalle_venta");
            });

            modelBuilder.Entity<InventarioMovimiento>(entity =>
            {
                entity.ToTable("inventario_movimientos");
                entity.HasIndex(e => e.ProductoId).HasDatabaseName("idx_inventario_producto");
                entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("(newid())");
                entity.Property(e => e.ProductoId).HasColumnName("producto_id");
                entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");
                entity.Property(e => e.Tipo).IsRequired().HasMaxLength(20).IsUnicode(false).HasColumnName("tipo");
                entity.Property(e => e.Cantidad).HasColumnName("cantidad");
                entity.Property(e => e.Justificacion).HasColumnName("justificacion");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("(sysdatetimeoffset())");

                entity.HasOne(d => d.Producto)
                    .WithMany(p => p.InventarioMovimientos)
                    .HasForeignKey(d => d.ProductoId)
                    .HasConstraintName("FK_mov_producto");

                entity.HasOne(d => d.Usuario)
                    .WithMany(p => p.InventarioMovimientos)
                    .HasForeignKey(d => d.UsuarioId)
                    .HasConstraintName("FK_mov_usuario");
            });

            modelBuilder.Entity<AuditoriaLog>(entity =>
            {
                entity.ToTable("auditoria_logs");
                entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("(newid())");
                entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");
                entity.Property(e => e.TablaAfectada).HasMaxLength(100).IsUnicode(false).HasColumnName("tabla_afectada");
                entity.Property(e => e.Accion).HasMaxLength(20).IsUnicode(false).HasColumnName("accion");
                entity.Property(e => e.ValorAnterior).HasColumnName("valor_anterior");
                entity.Property(e => e.ValorNuevo).HasColumnName("valor_nuevo");
                entity.Property(e => e.FechaHora).HasColumnName("fecha_hora").HasDefaultValueSql("(sysdatetimeoffset())");

                entity.HasOne(d => d.Usuario)
                    .WithMany(p => p.AuditoriaLogs)
                    .HasForeignKey(d => d.UsuarioId)
                    .HasConstraintName("FK_audit_usuario");
            });

            modelBuilder.Entity<VistaAlertasStock>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("vista_alertas_stock");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Nombre).HasMaxLength(255).IsUnicode(false).HasColumnName("nombre");
                entity.Property(e => e.StockActual).HasColumnName("stock_actual");
                entity.Property(e => e.StockMinimo).HasColumnName("stock_minimo");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
