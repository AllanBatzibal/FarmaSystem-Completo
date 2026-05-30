using System.Text.Json.Serialization;

namespace FarmaSystem.Core.DTOs;

public class VentaDTO
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    public int IdCliente { get; set; }
    public string? ClienteNombre { get; set; }
    public int IdEmpleado { get; set; }
    public string? EmpleadoNombre { get; set; }
    public DateTime Fecha { get; set; }
    public DateTime FechaVenta => Fecha;
    public decimal Descuento { get; set; }
    public decimal Total { get; set; }
    public List<DetalleVentaDTO> Detalles { get; set; } = new();
}

public class DetalleVentaDTO
{
    public int IdMedicamento { get; set; }
    public string? Nombre { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
}

public class DetalleVentaCreateDTO
{
    [JsonPropertyName("idMedicamento")]
    public int IdMedicamento { get; set; }

    [JsonPropertyName("medicamentoId")]
    public int MedicamentoId
    {
        get => IdMedicamento;
        set => IdMedicamento = value;
    }

    public int Cantidad { get; set; }
    public decimal? PrecioUnitario { get; set; }
    public decimal? Subtotal { get; set; }
}

public class VentaCreateDTO
{
    [JsonPropertyName("idCliente")]
    public int IdCliente { get; set; }

    [JsonPropertyName("clienteId")]
    public int ClienteId
    {
        get => IdCliente;
        set => IdCliente = value;
    }

    [JsonPropertyName("idEmpleado")]
    public int IdEmpleado { get; set; }

    [JsonPropertyName("empleadoId")]
    public int EmpleadoId
    {
        get => IdEmpleado;
        set => IdEmpleado = value;
    }

    public decimal Descuento { get; set; }
    public decimal? Total { get; set; }

    [JsonPropertyName("nombreClienteNuevo")]
    public string? NombreClienteNuevo { get; set; }

    public List<DetalleVentaCreateDTO> Detalles { get; set; } = new();
}

public class VentaResumenDTO
{
    public decimal TotalVentas { get; set; }
    public int Cantidad { get; set; }
}
