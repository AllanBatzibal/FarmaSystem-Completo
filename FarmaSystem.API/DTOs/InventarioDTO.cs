using System.Text.Json.Serialization;

namespace FarmaSystem.API.DTOs;

public class InventarioItemDTO
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;
    public string? Categoria { get; set; }
    public int StockActual { get; set; }
    public int StockMinimo { get; set; }
    public string EstadoStock { get; set; } = "OK";
    public decimal Precio { get; set; }
}

public class MovimientoInventarioDTO
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    public int IdMedicamento { get; set; }
    public string? MedicamentoNombre { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public DateTime Fecha { get; set; }
    public string? Motivo { get; set; }
    public string? Referencia { get; set; }
    public string? Observacion { get; set; }
}
