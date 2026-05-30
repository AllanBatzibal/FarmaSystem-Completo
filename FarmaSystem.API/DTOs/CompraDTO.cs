using System.Text.Json.Serialization;

namespace FarmaSystem.API.DTOs;

public class CompraDTO
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    public int IdProveedor { get; set; }
    public string? ProveedorNombre { get; set; }
    public int IdEmpleado { get; set; }
    public string? EmpleadoNombre { get; set; }
    public string? NumeroFactura { get; set; }
    public DateTime Fecha { get; set; }
    public DateTime FechaCompra => Fecha;
    public decimal Total { get; set; }
    public List<DetalleCompraDTO> Detalles { get; set; } = new();
}

public class DetalleCompraDTO
{
    public int IdMedicamento { get; set; }
    public string? Nombre { get; set; }
    public int Cantidad { get; set; }
    public decimal CostoUnitario { get; set; }
    public decimal Subtotal { get; set; }
}

public class DetalleCompraCreateDTO
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

    [JsonPropertyName("costo")]
    public decimal Costo { get; set; }

    [JsonPropertyName("precioUnitario")]
    public decimal PrecioUnitario
    {
        get => Costo;
        set => Costo = value;
    }

    public decimal? Subtotal { get; set; }
}

public class CompraCreateDTO
{
    [JsonPropertyName("idProveedor")]
    public int IdProveedor { get; set; }

    [JsonPropertyName("proveedorId")]
    public int ProveedorId
    {
        get => IdProveedor;
        set => IdProveedor = value;
    }

    [JsonPropertyName("idEmpleado")]
    public int IdEmpleado { get; set; } = 1;

    [JsonPropertyName("nFactura")]
    public string? NumeroFactura { get; set; }

    public decimal? Total { get; set; }
    public List<DetalleCompraCreateDTO> Detalles { get; set; } = new();
}

public class CompraResumenMesDTO
{
    public decimal TotalCompras { get; set; }
    public int Cantidad { get; set; }
}

public class ActualizarCompraDTO
{
    [JsonPropertyName("idProveedor")]
    public int IdProveedor { get; set; }

    [JsonPropertyName("nFactura")]
    public string NFactura { get; set; } = string.Empty;

    public decimal Total { get; set; }
}
