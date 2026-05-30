using FarmaSystem.API.Data;
using FarmaSystem.API.DTOs;
using FarmaSystem.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FarmaSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly FarmaSystemContext _context;
    private readonly IVentaService _ventaService;
    private readonly ICompraService _compraService;
    private readonly IInventarioService _inventarioService;

    public DashboardController(
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

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        try
        {
            var hoy = DateTime.Today;

            var totalVentas = await _context.Ventas
                .SumAsync(v => (decimal?)v.Total) ?? 0;

            var ventasHoy = await _context.Ventas
                .Where(v => v.Fecha.Date == hoy)
                .SumAsync(v => (decimal?)v.Total) ?? 0;

            var totalCompras = await _context.Compras
                .SumAsync(c => (decimal?)c.Total) ?? 0;

            var totalClientes = await _context.Clientes
                .CountAsync();

            var totalMedicamentos = await _context.Medicamentos
                .CountAsync();

            var totalEmpleados = await _context.Empleados
                .CountAsync(e => e.Activo);

            var productosAgotando = await _context.Medicamentos
                .Where(m => m.StockActual <= m.StockMinimo)
                .Select(m => new
                {
                    m.IdMedicamento,
                    m.Nombre,
                    m.StockActual,
                    m.StockMinimo
                })
                .OrderBy(m => m.StockActual)
                .ToListAsync();

            var productosMasVendidos = await _context.DetallesVenta
                .GroupBy(d => new
                {
                    d.IdMedicamento,
                    d.Medicamento.Nombre
                })
                .Select(g => new
                {
                    IdMedicamento = g.Key.IdMedicamento,
                    Nombre = g.Key.Nombre,
                    TotalVendido = g.Sum(d => d.Cantidad)
                })
                .OrderByDescending(x => x.TotalVendido)
                .Take(5)
                .ToListAsync();

            var mejorVendedor = await _context.Ventas
                .GroupBy(v => new
                {
                    v.IdEmpleado,
                    v.Empleado.Nombre,
                    v.Empleado.Apellido,
                    v.Empleado.Cargo
                })
                .Select(g => new
                {
                    IdEmpleado = g.Key.IdEmpleado,
                    Nombre = g.Key.Nombre + " " + g.Key.Apellido,
                    Cargo = g.Key.Cargo,
                    TotalVentas = g.Sum(v => v.Total)
                })
                .OrderByDescending(x => x.TotalVentas)
                .FirstOrDefaultAsync();

            return Ok(new
            {
                totalVentas,
                ventasHoy,
                totalCompras,
                totalClientes,
                totalMedicamentos,
                totalEmpleados,
                productosAgotando,
                productosMasVendidos,
                mejorVendedor
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpGet("resumen")]
    public async Task<ActionResult<DashboardResumenDTO>> GetResumen()
    {
        var resumenVentas = await _ventaService.ObtenerResumenDiaAsync();
        var resumenCompras = await _compraService.ObtenerResumenMesAsync();
        var inventario = await _inventarioService.ObtenerEstadoStockAsync();

        return Ok(new DashboardResumenDTO
        {
            VentasHoy = resumenVentas.TotalVentas,
            ComprasMes = resumenCompras.TotalCompras,
            StockCritico = inventario.Count(i => i.EstadoStock == "Critico"),
            TotalClientes = await _context.Clientes.CountAsync()
        });
    }

    [HttpGet("ventas-por-mes")]
    public async Task<ActionResult<IEnumerable<VentasPorMesDTO>>> GetVentasPorMes()
    {
        var desde = DateTime.Today.AddMonths(-11);
        desde = new DateTime(desde.Year, desde.Month, 1);

        var ventas = await _context.Ventas
            .Where(v => v.Fecha >= desde)
            .AsNoTracking()
            .ToListAsync();

        var agrupado = ventas
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

        return Ok(agrupado);
    }

    [HttpGet("top-medicamentos")]
    public async Task<ActionResult<IEnumerable<TopMedicamentoDTO>>> GetTopMedicamentos()
    {
        var top = await _context.DetallesVenta
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

        return Ok(top);
    }
}
