using FarmaSystem.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmaSystem.Infrastructure.Data.Configurations;

public class DetalleCompraConfiguration : IEntityTypeConfiguration<DetalleCompra>
{
    public void Configure(EntityTypeBuilder<DetalleCompra> e)
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
    }
}
