using FarmaSystem.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmaSystem.Infrastructure.Data.Configurations;

public class RolConfiguration : IEntityTypeConfiguration<Rol>
{
    public void Configure(EntityTypeBuilder<Rol> e)
    {
        e.ToTable("Rol");
        e.HasKey(x => x.IdRol);
        e.Property(x => x.IdRol).HasColumnName("idRol");
        e.Property(x => x.Nombre).HasColumnName("nombre");
        e.Property(x => x.Descripcion).HasColumnName("descripcion");
        e.Property(x => x.Activo).HasColumnName("activo");
    }
}
