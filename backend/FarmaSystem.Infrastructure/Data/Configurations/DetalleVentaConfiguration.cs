using FarmaSystem.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmaSystem.Infrastructure.Data.Configurations;

public class DetalleVentaConfiguration : IEntityTypeConfiguration<DetalleVenta>
{
    public void Configure(EntityTypeBuilder<DetalleVenta> e)
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
    }
}
