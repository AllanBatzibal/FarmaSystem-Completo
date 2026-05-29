namespace FarmaSystem.API.Models;

public class Compra
{
    public int IdCompra { get; set; }
    public DateTime Fecha { get; set; }
    public decimal Total { get; set; }
    public string? NFactura { get; set; }
    public int IdProveedor { get; set; }
    public int IdEmpleado { get; set; }

    public Proveedor Proveedor { get; set; } = null!;
    public Empleado Empleado { get; set; } = null!;
    public ICollection<DetalleCompra> Detalles { get; set; } = new List<DetalleCompra>();
}
