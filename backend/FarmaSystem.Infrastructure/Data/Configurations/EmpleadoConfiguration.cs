using FarmaSystem.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmaSystem.Infrastructure.Data.Configurations;

public class EmpleadoConfiguration : IEntityTypeConfiguration<Empleado>
{
    public void Configure(EntityTypeBuilder<Empleado> e)
    {
        e.ToTable("Empleado");
        e.HasKey(x => x.IdEmpleado);
        e.Property(x => x.IdEmpleado).HasColumnName("idEmpleado");
        e.Property(x => x.Nombre).HasColumnName("nombre");
        e.Property(x => x.Apellido).HasColumnName("apellido");
        e.Property(x => x.Cargo).HasColumnName("cargo");
        e.Property(x => x.Salario).HasColumnName("salario").HasColumnType("decimal(18,2)");
        e.Property(x => x.Telefono).HasColumnName("telefono");
        e.Property(x => x.FechaIngreso).HasColumnName("fechaIngreso");
        e.Property(x => x.Activo).HasColumnName("activo");
    }
}
