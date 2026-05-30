using FarmaSystem.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmaSystem.Infrastructure.Data.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> e)
    {
        e.ToTable("Usuario");
        e.HasKey(x => x.IdUsuario);
        e.Property(x => x.IdUsuario).HasColumnName("idUsuario");
        e.Property(x => x.UsuarioNombre).HasColumnName("usuario");
        e.Property(x => x.Contrasena).HasColumnName("contrasena");
        e.Property(x => x.IdEmpleado).HasColumnName("idEmpleado");
        e.Property(x => x.IdRol).HasColumnName("idRol");
        e.Property(x => x.Activo).HasColumnName("activo");
        e.Property(x => x.FechaCreacion).HasColumnName("fechaCreacion");
        e.Property(x => x.UltimoAcceso).HasColumnName("ultimoAcceso");
        e.HasOne(x => x.Empleado)
            .WithOne(em => em.Usuario)
            .HasForeignKey<Usuario>(x => x.IdEmpleado);
        e.HasOne(x => x.Rol)
            .WithMany(r => r.Usuarios)
            .HasForeignKey(x => x.IdRol);
    }
}
