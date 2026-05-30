using FarmaSystem.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmaSystem.Infrastructure.Data.Configurations;

public class VentaConfiguration : IEntityTypeConfiguration<Venta>
{
    public void Configure(EntityTypeBuilder<Venta> e)
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
    }
}
