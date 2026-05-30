using FarmaSystem.Core.Models;
using FarmaSystem.Core.Views;
using Microsoft.EntityFrameworkCore;

namespace FarmaSystem.Infrastructure.Data;

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
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FarmaSystemContext).Assembly);
        MapVistas(modelBuilder);
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
