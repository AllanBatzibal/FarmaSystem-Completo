namespace FarmaSystem.API.Models;

public class DetalleVenta
{
    public int IdDetalle { get; set; }
    public int IdVenta { get; set; }
    public int IdMedicamento { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }

    public Venta Venta { get; set; } = null!;
    public Medicamento Medicamento { get; set; } = null!;
}
