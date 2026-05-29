namespace FarmaSystem.API.Models.Views;

public class VwInventarioActual
{
    public int IdMedicamento { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal Precio { get; set; }
    public int StockActual { get; set; }
    public int StockMinimo { get; set; }
    public int IdCategoria { get; set; }
    public string? Categoria { get; set; }
    public int IdProveedor { get; set; }
    public string? Proveedor { get; set; }
    public string? EstadoStock { get; set; }
}
