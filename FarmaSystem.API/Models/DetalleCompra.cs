namespace FarmaSystem.API.Models;

public class DetalleCompra
{
    public int IdDetalle { get; set; }
    public int IdCompra { get; set; }
    public int IdMedicamento { get; set; }
    public int Cantidad { get; set; }
    public decimal CostoUnitario { get; set; }
    public decimal Subtotal { get; set; }

    public Compra Compra { get; set; } = null!;
    public Medicamento Medicamento { get; set; } = null!;
}
