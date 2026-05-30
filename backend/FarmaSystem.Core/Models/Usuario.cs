namespace FarmaSystem.Core.Models;

public class Usuario
{
    public int IdUsuario { get; set; }
    public string UsuarioNombre { get; set; } = string.Empty;
    public string Contrasena { get; set; } = string.Empty;
    public int IdEmpleado { get; set; }
    public int IdRol { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    public DateTime? UltimoAcceso { get; set; }

    public Empleado Empleado { get; set; } = null!;
    public Rol Rol { get; set; } = null!;
}
