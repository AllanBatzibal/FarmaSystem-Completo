namespace FarmaSystem.Core.Models;

public class Proveedor
{
    public int IdProveedor { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Contacto { get; set; }
    public string? Telefono { get; set; }
    public string? Correo { get; set; }
    public string? Direccion { get; set; }
    public bool Activo { get; set; } = true;

    public ICollection<Medicamento> Medicamentos { get; set; } = new List<Medicamento>();
    public ICollection<Compra> Compras { get; set; } = new List<Compra>();
}
