namespace FarmaSystem.Core.Views;

public class VwResumenVentas
{
    public int IdVenta { get; set; }
    public DateTime Fecha { get; set; }
    public decimal Total { get; set; }
    public decimal Descuento { get; set; }
    public int IdCliente { get; set; }
    public string? ClienteNombre { get; set; }
    public int IdEmpleado { get; set; }
    public string? EmpleadoNombre { get; set; }
    public int? IdDetalle { get; set; }
    public int? IdMedicamento { get; set; }
    public string? Medicamento { get; set; }
    public int? Cantidad { get; set; }
    public decimal? PrecioUnitario { get; set; }
    public decimal? Subtotal { get; set; }
}
