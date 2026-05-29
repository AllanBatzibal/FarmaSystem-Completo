namespace FarmaSystem.API.Models;

public class Categoria
{
    public int IdCategoria { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }

    public ICollection<Medicamento> Medicamentos { get; set; } = new List<Medicamento>();
}
