using FarmaSystem.Core.DTOs;
using FarmaSystem.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FarmaSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VentasController : ControllerBase
{
    private readonly IVentaService _ventaService;

    public VentasController(IVentaService ventaService)
    {
        _ventaService = ventaService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<VentaDTO>>> GetAll(
        [FromQuery] int? limit,
        [FromQuery] DateTime? fecha)
    {
        var ventas = await _ventaService.ObtenerTodasAsync(limit, fecha);
        return Ok(ventas);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<VentaDTO>> GetById(int id)
    {
        var venta = await _ventaService.ObtenerPorIdAsync(id);
        if (venta == null)
            return NotFound(new { message = $"Venta con ID {id} no encontrada." });
        return Ok(venta);
    }

    [HttpGet("por-fecha")]
    public async Task<ActionResult<IEnumerable<VentaDTO>>> GetPorFecha(
        [FromQuery] DateTime inicio,
        [FromQuery] DateTime fin)
    {
        if (fin < inicio)
            return BadRequest(new { message = "La fecha fin debe ser mayor o igual a la fecha inicio." });

        var ventas = await _ventaService.ObtenerPorFechaAsync(inicio, fin);
        return Ok(ventas);
    }

    [HttpGet("hoy")]
    public async Task<ActionResult<IEnumerable<VentaDTO>>> GetHoy()
    {
        return Ok(await _ventaService.ObtenerVentasHoyAsync());
    }

    [HttpGet("resumen-dia")]
    public async Task<ActionResult<VentaResumenDTO>> GetResumenDia()
    {
        return Ok(await _ventaService.ObtenerResumenDiaAsync());
    }

    [HttpPost]
    public async Task<ActionResult<VentaDTO>> Create([FromBody] VentaCreateDTO dto)
    {
        try
        {
            var venta = await _ventaService.RegistrarVentaAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = venta.Id }, venta);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
