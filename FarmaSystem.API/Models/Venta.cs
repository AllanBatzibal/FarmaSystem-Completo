namespace FarmaSystem.API.Models;

public class Venta
{
    public int IdVenta { get; set; }
    public DateTime Fecha { get; set; }
    public decimal Total { get; set; }
    public decimal Descuento { get; set; }
    public int IdCliente { get; set; }
    public int IdEmpleado { get; set; }

    public Cliente Cliente { get; set; } = null!;
    public Empleado Empleado { get; set; } = null!;
    public ICollection<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();
}
