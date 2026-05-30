using FarmaSystem.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmaSystem.Infrastructure.Data.Configurations;

public class MedicamentoConfiguration : IEntityTypeConfiguration<Medicamento>
{
    public void Configure(EntityTypeBuilder<Medicamento> e)
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
    }
}
