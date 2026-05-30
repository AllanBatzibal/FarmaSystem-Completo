using System.Text.Json.Serialization;

namespace FarmaSystem.Core.DTOs;

public class ClienteDTO
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Nit { get; set; }
    public string? Direccion { get; set; }
}

public class ClienteCreateDTO
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Nit { get; set; }
    public string? Direccion { get; set; }
}
