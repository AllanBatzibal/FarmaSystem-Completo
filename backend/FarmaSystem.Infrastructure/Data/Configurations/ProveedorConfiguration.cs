using FarmaSystem.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmaSystem.Infrastructure.Data.Configurations;

public class ProveedorConfiguration : IEntityTypeConfiguration<Proveedor>
{
    public void Configure(EntityTypeBuilder<Proveedor> e)
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
    }
}
