using FarmaSystem.API.Models;
using FarmaSystem.API.Models.Views;
using Microsoft.EntityFrameworkCore;

namespace FarmaSystem.API.Data;

public class FarmaSystemContext : DbContext
{
    public FarmaSystemContext(DbContextOptions<FarmaSystemContext> options) : base(options)
    {
    }

    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Proveedor> Proveedores => Set<Proveedor>();
    public DbSet<Empleado> Empleados => Set<Empleado>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Medicamento> Medicamentos => Set<Medicamento>();
    public DbSet<Venta> Ventas => Set<Venta>();
    public DbSet<DetalleVenta> DetallesVenta => Set<DetalleVenta>();
    public DbSet<Compra> Compras => Set<Compra>();
    public DbSet<DetalleCompra> DetallesCompra => Set<DetalleCompra>();
    public DbSet<MovimientoInventario> MovimientosInventario => Set<MovimientoInventario>();

    public DbSet<VwInventarioActual> VwInventarioActual => Set<VwInventarioActual>();
    public DbSet<VwMovimientosCompletos> VwMovimientosCompletos => Set<VwMovimientosCompletos>();
    public DbSet<VwResumenVentas> VwResumenVentas => Set<VwResumenVentas>();
    public DbSet<VwResumenCompras> VwResumenCompras => Set<VwResumenCompras>();

    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        MapTablas(modelBuilder);
        MapVistas(modelBuilder);
    }

    private static void MapTablas(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Categoria>(e =>
        {
            e.ToTable("Categoria");
            e.HasKey(x => x.IdCategoria);
            e.Property(x => x.IdCategoria).HasColumnName("idCategoria");
            e.Property(x => x.Nombre).HasColumnName("nombre");
            e.Property(x => x.Descripcion).HasColumnName("descripcion");
        });

        modelBuilder.Entity<Cliente>(e =>
        {
            e.ToTable("Cliente");
            e.HasKey(x => x.IdCliente);
            e.Property(x => x.IdCliente).HasColumnName("idCliente");
            e.Property(x => x.Nombre).HasColumnName("nombre");
            e.Property(x => x.Apellido).HasColumnName("apellido");
            e.Property(x => x.Telefono).HasColumnName("telefono");
            e.Property(x => x.Correo).HasColumnName("correo");
            e.Property(x => x.Direccion).HasColumnName("direccion");
            e.Property(x => x.FechaRegistro).HasColumnName("fechaRegistro");
        });

        modelBuilder.Entity<Proveedor>(e =>
        {
            e.ToTable("Proveedor");
            e.HasKey(x => x.IdProveedor);
            e.Property(x => x.IdProveedor).HasColumnName("idProveedor");
            e.Property(x => x.Nombre).HasColumnName("nombre");
            e.Property(x => x.Contacto).HasColumnName("contacto");
            e.Property(x => x.Telefono).HasColumnName("telefono");
            e.Property(x => x.Correo).HasColumnName("correo");
            e.Property(x => x.Direccion).HasColumnName("direccion");
            e.Property(x => x.Activo).HasColumnName("activo");
        });

        modelBuilder.Entity<Empleado>(e =>
        {
            e.ToTable("Empleado");
            e.HasKey(x => x.IdEmpleado);
            e.Property(x => x.IdEmpleado).HasColumnName("idEmpleado");
            e.Property(x => x.Nombre).HasColumnName("nombre");
            e.Property(x => x.Apellido).HasColumnName("apellido");
            e.Property(x => x.Cargo).HasColumnName("cargo");
            e.Property(x => x.Salario).HasColumnName("salario").HasColumnType("decimal(18,2)");
            e.Property(x => x.Telefono).HasColumnName("telefono");
            e.Property(x => x.FechaIngreso).HasColumnName("fechaIngreso");
            e.Property(x => x.Activo).HasColumnName("activo");
        });

        modelBuilder.Entity<Medicamento>(e =>
        {
            e.ToTable("Medicamento");
            e.HasKey(x => x.IdMedicamento);
            e.Property(x => x.IdMedicamento).HasColumnName("idMedicamento");
            e.Property(x => x.Nombre).HasColumnName("nombre");
            e.Property(x => x.Descripcion).HasColumnName("descripcion");
            e.Property(x => x.Precio).HasColumnName("precio").HasColumnType("decimal(18,2)");
            e.Property(x => x.StockActual).HasColumnName("stockActual");
            e.Property(x => x.StockMinimo).HasColumnName("stockMinimo");
            e.Property(x => x.IdCategoria).HasColumnName("idCategoria");
            e.Property(x => x.IdProveedor).HasColumnName("idProveedor");
            e.HasOne(x => x.Categoria).WithMany(c => c.Medicamentos).HasForeignKey(x => x.IdCategoria);
            e.HasOne(x => x.Proveedor).WithMany(p => p.Medicamentos).HasForeignKey(x => x.IdProveedor);
        });

        modelBuilder.Entity<Venta>(e =>
        {
            e.ToTable("Venta");
            e.HasKey(x => x.IdVenta);
            e.Property(x => x.IdVenta).HasColumnName("idVenta");
            e.Property(x => x.Fecha).HasColumnName("fecha");
            e.Property(x => x.Total).HasColumnName("total").HasColumnType("decimal(18,2)");
            e.Property(x => x.Descuento).HasColumnName("descuento").HasColumnType("decimal(18,2)");
            e.Property(x => x.IdCliente).HasColumnName("idCliente");
            e.Property(x => x.IdEmpleado).HasColumnName("idEmpleado");
            e.HasOne(x => x.Cliente).WithMany(c => c.Ventas).HasForeignKey(x => x.IdCliente);
            e.HasOne(x => x.Empleado).WithMany(em => em.Ventas).HasForeignKey(x => x.IdEmpleado);
        });

        modelBuilder.Entity<DetalleVenta>(e =>
        {
            e.ToTable("DetalleVenta");
            e.HasKey(x => x.IdDetalle);
            e.Property(x => x.IdDetalle).HasColumnName("idDetalle");
            e.Property(x => x.IdVenta).HasColumnName("idVenta");
            e.Property(x => x.IdMedicamento).HasColumnName("idMedicamento");
            e.Property(x => x.Cantidad).HasColumnName("cantidad");
            e.Property(x => x.PrecioUnitario).HasColumnName("precioUnitario").HasColumnType("decimal(18,2)");
            e.Property(x => x.Subtotal)
                .HasColumnName("subtotal")
                .HasColumnType("decimal(18,2)")
                .HasComputedColumnSql("([cantidad]*[precioUnitario])", stored: true);
            e.HasOne(x => x.Venta).WithMany(v => v.Detalles).HasForeignKey(x => x.IdVenta);
            e.HasOne(x => x.Medicamento).WithMany(m => m.DetallesVenta).HasForeignKey(x => x.IdMedicamento);
        });

        modelBuilder.Entity<Compra>(e =>
        {
            e.ToTable("Compra");
            e.HasKey(x => x.IdCompra);
            e.Property(x => x.IdCompra).HasColumnName("idCompra");
            e.Property(x => x.Fecha).HasColumnName("fecha");
            e.Property(x => x.Total).HasColumnName("total").HasColumnType("decimal(18,2)");
            e.Property(x => x.NFactura).HasColumnName("nFactura");
            e.Property(x => x.IdProveedor).HasColumnName("idProveedor");
            e.Property(x => x.IdEmpleado).HasColumnName("idEmpleado");
            e.HasOne(x => x.Proveedor).WithMany(p => p.Compras).HasForeignKey(x => x.IdProveedor);
            e.HasOne(x => x.Empleado).WithMany(em => em.Compras).HasForeignKey(x => x.IdEmpleado);
        });

        modelBuilder.Entity<DetalleCompra>(e =>
        {
            e.ToTable("DetalleCompra");
            e.HasKey(x => x.IdDetalle);
            e.Property(x => x.IdDetalle).HasColumnName("idDetalle");
            e.Property(x => x.IdCompra).HasColumnName("idCompra");
            e.Property(x => x.IdMedicamento).HasColumnName("idMedicamento");
            e.Property(x => x.Cantidad).HasColumnName("cantidad");
            e.Property(x => x.CostoUnitario).HasColumnName("costoUnitario").HasColumnType("decimal(18,2)");
            e.Property(x => x.Subtotal)
                .HasColumnName("subtotal")
                .HasColumnType("decimal(18,2)")
                .HasComputedColumnSql("([cantidad]*[costoUnitario])", stored: true);
            e.HasOne(x => x.Compra).WithMany(c => c.Detalles).HasForeignKey(x => x.IdCompra);
            e.HasOne(x => x.Medicamento).WithMany(m => m.DetallesCompra).HasForeignKey(x => x.IdMedicamento);
        });

        modelBuilder.Entity<MovimientoInventario>(e =>
        {
            e.ToTable("MovimientoInventario");
            e.HasKey(x => x.IdMovimiento);
            e.Property(x => x.IdMovimiento).HasColumnName("idMovimiento");
            e.Property(x => x.IdMedicamento).HasColumnName("idMedicamento");
            e.Property(x => x.Tipo).HasColumnName("tipo");
            e.Property(x => x.Cantidad).HasColumnName("cantidad");
            e.Property(x => x.Fecha).HasColumnName("fecha");
            e.Property(x => x.Motivo).HasColumnName("motivo");
            e.HasOne(x => x.Medicamento).WithMany(m => m.Movimientos).HasForeignKey(x => x.IdMedicamento);
        });

        modelBuilder.Entity<Rol>(e =>
        {
            e.ToTable("Rol");
            e.HasKey(x => x.IdRol);
            e.Property(x => x.IdRol).HasColumnName("idRol");
            e.Property(x => x.Nombre).HasColumnName("nombre");
            e.Property(x => x.Descripcion).HasColumnName("descripcion");
            e.Property(x => x.Activo).HasColumnName("activo");
        });

        modelBuilder.Entity<Usuario>(e =>
        {
            e.ToTable("Usuario");
            e.HasKey(x => x.IdUsuario);
            e.Property(x => x.IdUsuario).HasColumnName("idUsuario");
            e.Property(x => x.UsuarioNombre).HasColumnName("usuario");
            e.Property(x => x.Contrasena).HasColumnName("contrasena");
            e.Property(x => x.IdEmpleado).HasColumnName("idEmpleado");
            e.Property(x => x.IdRol).HasColumnName("idRol");
            e.Property(x => x.Activo).HasColumnName("activo");
            e.Property(x => x.FechaCreacion).HasColumnName("fechaCreacion");
            e.Property(x => x.UltimoAcceso).HasColumnName("ultimoAcceso");
            e.HasOne(x => x.Empleado)
                .WithOne(em => em.Usuario)
                .HasForeignKey<Usuario>(x => x.IdEmpleado);
            e.HasOne(x => x.Rol)
                .WithMany(r => r.Usuarios)
                .HasForeignKey(x => x.IdRol);
        });
    }

    private static void MapVistas(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<VwInventarioActual>(e =>
        {
            e.HasNoKey();
            e.ToView("vw_InventarioActual");
        });

        modelBuilder.Entity<VwMovimientosCompletos>(e =>
        {
            e.HasNoKey();
            e.ToView("vw_MovimientosCompletos");
        });

        modelBuilder.Entity<VwResumenVentas>(e =>
        {
            e.HasNoKey();
            e.ToView("vw_ResumenVentas");
        });

        modelBuilder.Entity<VwResumenCompras>(e =>
        {
            e.HasNoKey();
            e.ToView("vw_ResumenCompras");
        });
    }
}
