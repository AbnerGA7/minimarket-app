using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace MinimarketApp.Models;

public partial class MinimarketContext : DbContext
{
    public MinimarketContext()
    {
    }

    public MinimarketContext(DbContextOptions<MinimarketContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CajaMovimiento> CajaMovimientos { get; set; }

    public virtual DbSet<Categoria> Categorias { get; set; }

    public virtual DbSet<DetalleVentum> DetalleVenta { get; set; }

    public virtual DbSet<Lote> Lotes { get; set; }

    public virtual DbSet<MediosPago> MediosPagos { get; set; }

    public virtual DbSet<Producto> Productos { get; set; }

    public virtual DbSet<Proveedore> Proveedores { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<Venta> Ventas { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=.\\SQLEXPRESS;Database=minimarket-app;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CajaMovimiento>(entity =>
        {
            entity.HasKey(e => e.IdMovimiento).HasName("PK__CajaMovi__881A6AE008ECFB27");

            entity.Property(e => e.FechaMovimiento)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Monto).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Observacion)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.TipoMovimiento)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.CajaMovimientos)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("FK__CajaMovim__IdUsu__5812160E");
        });

        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.HasKey(e => e.IdCategoria).HasName("PK__Categori__A3C02A10E61DB7A2");

            entity.Property(e => e.Descripcion)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<DetalleVentum>(entity =>
        {
            entity.HasKey(e => e.IdDetalle).HasName("PK__DetalleV__E43646A5082DF7F5");

            entity.Property(e => e.PrecioVentaUnitario).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.SubTotal)
                .HasComputedColumnSql("([Cantidad]*[PrecioVentaUnitario])", false)
                .HasColumnType("decimal(21, 2)");

            entity.HasOne(d => d.IdLoteNavigation).WithMany(p => p.DetalleVenta)
                .HasForeignKey(d => d.IdLote)
                .HasConstraintName("FK__DetalleVe__IdLot__5535A963");

            entity.HasOne(d => d.IdVentaNavigation).WithMany(p => p.DetalleVenta)
                .HasForeignKey(d => d.IdVenta)
                .HasConstraintName("FK__DetalleVe__IdVen__5441852A");
        });

        modelBuilder.Entity<Lote>(entity =>
        {
            entity.HasKey(e => e.IdLote).HasName("PK__Lotes__38C4EE90427ACA7D");

            entity.Property(e => e.CostoCompraUnitario).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.FechaRecepcion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NumeroLote)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.IdProductoNavigation).WithMany(p => p.Lotes)
                .HasForeignKey(d => d.IdProducto)
                .HasConstraintName("FK__Lotes__IdProduct__47DBAE45");

            entity.HasOne(d => d.IdProveedorNavigation).WithMany(p => p.Lotes)
                .HasForeignKey(d => d.IdProveedor)
                .HasConstraintName("FK__Lotes__IdProveed__48CFD27E");
        });

        modelBuilder.Entity<MediosPago>(entity =>
        {
            entity.HasKey(e => e.IdMedioPago).HasName("PK__MediosPa__0A55C859B0632C82");

            entity.ToTable("MediosPago");

            entity.Property(e => e.Nombre)
                .HasMaxLength(30)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(e => e.IdProducto).HasName("PK__Producto__09889210CAA5ABBF");

            entity.HasIndex(e => e.CodigoBarras, "UQ__Producto__F61589C82BD848C1").IsUnique();

            entity.Property(e => e.CodigoBarras)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ImagenUrl).IsUnicode(false);
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.PrecioVenta).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.StockMinimo).HasDefaultValue(5);
            entity.Property(e => e.UnidadMedida)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("UNIDAD");

            entity.HasOne(d => d.IdCategoriaNavigation).WithMany(p => p.Productos)
                .HasForeignKey(d => d.IdCategoria)
                .HasConstraintName("FK__Productos__IdCat__4316F928");
        });

        modelBuilder.Entity<Proveedore>(entity =>
        {
            entity.HasKey(e => e.IdProveedor).HasName("PK__Proveedo__E8B631AFB149B423");

            entity.Property(e => e.Direccion)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.RazonSocial)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.RucDni)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("RUC_DNI");
            entity.Property(e => e.Telefono)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.IdRol).HasName("PK__Roles__2A49584C0A9E1996");

            entity.Property(e => e.NombreRol)
                .HasMaxLength(30)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("PK__Usuarios__5B65BF974900ECF2");

            entity.HasIndex(e => e.Username, "UQ__Usuarios__536C85E4B9D76029").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.NombreCompleto)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.IdRolNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdRol)
                .HasConstraintName("FK__Usuarios__IdRol__3A81B327");
        });

        modelBuilder.Entity<Venta>(entity =>
        {
            entity.HasKey(e => e.IdVenta).HasName("PK__Ventas__BC1240BD7515C00A");

            entity.Property(e => e.FechaVenta)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.MontoPagado).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TotalVenta).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Vuelto).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.IdMedioPagoNavigation).WithMany(p => p.Venta)
                .HasForeignKey(d => d.IdMedioPago)
                .HasConstraintName("FK__Ventas__IdMedioP__5165187F");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Venta)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("FK__Ventas__IdUsuari__5070F446");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
