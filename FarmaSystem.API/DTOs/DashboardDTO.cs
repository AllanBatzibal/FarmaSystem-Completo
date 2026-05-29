namespace FarmaSystem.API.DTOs;

public class DashboardResumenDTO
{
    public decimal VentasHoy { get; set; }
    public decimal ComprasMes { get; set; }
    public int StockCritico { get; set; }
    public int TotalClientes { get; set; }
}

public class VentasPorMesDTO
{
    public int Anio { get; set; }
    public int Mes { get; set; }
    public string Etiqueta { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public int Cantidad { get; set; }
}

public class TopMedicamentoDTO
{
    public int IdMedicamento { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int CantidadVendida { get; set; }
    public decimal TotalVendido { get; set; }
}
