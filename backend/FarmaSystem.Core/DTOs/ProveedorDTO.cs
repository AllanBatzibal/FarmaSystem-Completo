using System.Text.Json.Serialization;

namespace FarmaSystem.Core.DTOs;

public class ProveedorDTO
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;
    public string? Contacto { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Direccion { get; set; }
}

public class ProveedorCreateDTO
{
    public string Nombre { get; set; } = string.Empty;
    public string? Contacto { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Direccion { get; set; }
}
