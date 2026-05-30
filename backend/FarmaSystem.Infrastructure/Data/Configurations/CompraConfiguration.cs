using FarmaSystem.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmaSystem.Infrastructure.Data.Configurations;

public class CompraConfiguration : IEntityTypeConfiguration<Compra>
{
    public void Configure(EntityTypeBuilder<Compra> e)
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
    }
}
