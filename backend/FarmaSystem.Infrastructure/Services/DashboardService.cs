using FarmaSystem.Core.DTOs;
using FarmaSystem.Core.Interfaces;
using FarmaSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FarmaSystem.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly FarmaSystemContext _context;
    private readonly IVentaService _ventaService;
    private readonly ICompraService _compraService;
    private readonly IInventarioService _inventarioService;

    public DashboardService(
        FarmaSystemContext context,
        IVentaService ventaService,
        ICompraService compraService,
        IInventarioService inventarioService)
    {
        _context = context;
        _ventaService = ventaService;
        _compraService = compraService;
        _inventarioService = inventarioService;
    }

    public async Task<DashboardResumenDTO> ObtenerResumenAsync()
    {
        var resumenVentas = await _ventaService.ObtenerResumenDiaAsync();
        var resumenCompras = await _compraService.ObtenerResumenMesAsync();
        var inventario = await _inventarioService.ObtenerEstadoStockAsync();

        return new DashboardResumenDTO
        {
            VentasHoy = resumenVentas.TotalVentas,
            ComprasMes = resumenCompras.TotalCompras,
            StockCritico = inventario.Count(i => i.EstadoStock == "Critico"),
            TotalClientes = await _context.Clientes.CountAsync()
        };
    }

    public async Task<List<VentasPorMesDTO>> ObtenerVentasPorMesAsync()
    {
        var desde = DateTime.Today.AddMonths(-11);
        desde = new DateTime(desde.Year, desde.Month, 1);

        var ventas = await _context.Ventas
            .Where(v => v.Fecha >= desde)
            .AsNoTracking()
            .ToListAsync();

        return ventas
            .GroupBy(v => new { v.Fecha.Year, v.Fecha.Month })
            .Select(g => new VentasPorMesDTO
            {
                Anio = g.Key.Year,
                Mes = g.Key.Month,
                Etiqueta = $"{g.Key.Month:00}/{g.Key.Year}",
                Total = g.Sum(v => v.Total),
                Cantidad = g.Count()
            })
            .OrderBy(x => x.Anio).ThenBy(x => x.Mes)
            .ToList();
    }

    public async Task<List<TopMedicamentoDTO>> ObtenerTopMedicamentosAsync()
    {
        return await _context.DetallesVenta
            .Include(d => d.Medicamento)
            .GroupBy(d => new { d.IdMedicamento, d.Medicamento.Nombre })
            .Select(g => new TopMedicamentoDTO
            {
                IdMedicamento = g.Key.IdMedicamento,
                Nombre = g.Key.Nombre,
                CantidadVendida = g.Sum(d => d.Cantidad),
                TotalVendido = g.Sum(d => d.Subtotal)
            })
            .OrderByDescending(x => x.CantidadVendida)
            .Take(5)
            .ToListAsync();
    }
}
