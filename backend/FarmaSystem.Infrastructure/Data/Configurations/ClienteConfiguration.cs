using FarmaSystem.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmaSystem.Infrastructure.Data.Configurations;

public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> e)
    {
        e.ToTable("Cliente");
        e.HasKey(x => x.IdCliente);
        e.Property(x => x.IdCliente).HasColumnName("idCliente");
        e.Property(x => x.Nombre).HasColumnName("nombre");
        e.Property(x => x.Apellido).HasColumnName("apellido");
        e.Property(x => x.Telefono).HasColumnName("telefono");
        e.Property(x => x.Correo).HasColumnName("correo");
        e.Property(x => x.Direccion).HasColumnName("direccion");
        e.Property(x => x.FechaRegistro).HasColumnName("fechaRegistro");
    }
}
