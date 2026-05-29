namespace FarmaSystem.API.Models.Views;

public class VwMovimientosCompletos
{
    public int IdMovimiento { get; set; }
    public int IdMedicamento { get; set; }
    public string? Medicamento { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public DateTime Fecha { get; set; }
    public string? Motivo { get; set; }
}
