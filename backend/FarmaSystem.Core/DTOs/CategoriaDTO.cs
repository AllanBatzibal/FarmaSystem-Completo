using System.Text.Json.Serialization;

namespace FarmaSystem.Core.DTOs;

public class CategoriaDTO
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
}
