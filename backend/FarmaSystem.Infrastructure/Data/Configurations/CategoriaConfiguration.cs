using FarmaSystem.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmaSystem.Infrastructure.Data.Configurations;

public class CategoriaConfiguration : IEntityTypeConfiguration<Categoria>
{
    public void Configure(EntityTypeBuilder<Categoria> e)
    {
        e.ToTable("Categoria");
        e.HasKey(x => x.IdCategoria);
        e.Property(x => x.IdCategoria).HasColumnName("idCategoria");
        e.Property(x => x.Nombre).HasColumnName("nombre");
        e.Property(x => x.Descripcion).HasColumnName("descripcion");
    }
}
