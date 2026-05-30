namespace FarmaSystem.Core.Views;

public class VwResumenCompras
{
    public int IdCompra { get; set; }
    public DateTime Fecha { get; set; }
    public decimal Total { get; set; }
    public string? NFactura { get; set; }
    public int IdProveedor { get; set; }
    public string? ProveedorNombre { get; set; }
    public int IdEmpleado { get; set; }
    public string? EmpleadoNombre { get; set; }
    public int? IdDetalle { get; set; }
    public int? IdMedicamento { get; set; }
    public string? Medicamento { get; set; }
    public int? Cantidad { get; set; }
    public decimal? CostoUnitario { get; set; }
    public decimal? Subtotal { get; set; }
}
