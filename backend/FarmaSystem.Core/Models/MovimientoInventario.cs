namespace FarmaSystem.Core.Models;

public class MovimientoInventario
{
    public int IdMovimiento { get; set; }
    public int IdMedicamento { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public DateTime Fecha { get; set; }
    public string? Motivo { get; set; }

    public Medicamento Medicamento { get; set; } = null!;
}
