namespace FarmaSystem.API.Models;

public class Empleado
{
    public int IdEmpleado { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string? Cargo { get; set; }
    public decimal? Salario { get; set; }
    public string? Telefono { get; set; }
    public DateTime? FechaIngreso { get; set; }
    public bool Activo { get; set; } = true;

    public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
    public ICollection<Compra> Compras { get; set; } = new List<Compra>();
    public Usuario? Usuario { get; set; }
}
