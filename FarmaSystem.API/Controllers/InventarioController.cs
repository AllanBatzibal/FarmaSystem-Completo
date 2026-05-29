using FarmaSystem.API.DTOs;
using FarmaSystem.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace FarmaSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventarioController : ControllerBase
{
    private readonly IInventarioService _inventarioService;

    public InventarioController(IInventarioService inventarioService)
    {
        _inventarioService = inventarioService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<InventarioItemDTO>>> GetEstadoStock()
    {
        return Ok(await _inventarioService.ObtenerEstadoStockAsync());
    }

    [HttpGet("movimientos")]
    public async Task<ActionResult<IEnumerable<MovimientoInventarioDTO>>> GetMovimientos()
    {
        return Ok(await _inventarioService.ObtenerMovimientosAsync());
    }

    [HttpGet("movimientos/{idMedicamento}")]
    public async Task<ActionResult<IEnumerable<MovimientoInventarioDTO>>> GetMovimientosPorMedicamento(int idMedicamento)
    {
        return Ok(await _inventarioService.ObtenerMovimientosPorMedicamentoAsync(idMedicamento));
    }
}
