using FarmaSystem.Core.DTOs;
using FarmaSystem.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FarmaSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("resumen")]
    public async Task<ActionResult<DashboardResumenDTO>> GetResumen()
    {
        return Ok(await _dashboardService.ObtenerResumenAsync());
    }

    [HttpGet("ventas-por-mes")]
    public async Task<ActionResult<IEnumerable<VentasPorMesDTO>>> GetVentasPorMes()
    {
        return Ok(await _dashboardService.ObtenerVentasPorMesAsync());
    }

    [HttpGet("top-medicamentos")]
    public async Task<ActionResult<IEnumerable<TopMedicamentoDTO>>> GetTopMedicamentos()
    {
        return Ok(await _dashboardService.ObtenerTopMedicamentosAsync());
    }
}
