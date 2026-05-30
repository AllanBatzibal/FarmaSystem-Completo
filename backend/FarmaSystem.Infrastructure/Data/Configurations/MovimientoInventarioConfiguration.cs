using FarmaSystem.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmaSystem.Infrastructure.Data.Configurations;

public class MovimientoInventarioConfiguration : IEntityTypeConfiguration<MovimientoInventario>
{
    public void Configure(EntityTypeBuilder<MovimientoInventario> e)
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
    }
}
