using System.Text.Json.Serialization;

namespace FarmaSystem.API.DTOs;

public class EmpleadoDTO
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string? Puesto { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public decimal? Salario { get; set; }
}

public class EmpleadoCreateDTO
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string? Puesto { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public decimal? Salario { get; set; }
}
