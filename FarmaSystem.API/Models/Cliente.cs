namespace FarmaSystem.API.Models;

public class Cliente
{
    public int IdCliente { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Correo { get; set; }
    public string? Direccion { get; set; }
    public DateTime? FechaRegistro { get; set; }

    public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
}
