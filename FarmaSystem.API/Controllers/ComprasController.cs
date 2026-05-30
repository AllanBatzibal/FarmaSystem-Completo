using FarmaSystem.API.DTOs;
using FarmaSystem.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace FarmaSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ComprasController : ControllerBase
{
    private readonly ICompraService _compraService;

    public ComprasController(ICompraService compraService)
    {
        _compraService = compraService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CompraDTO>>> GetAll()
    {
        return Ok(await _compraService.ObtenerTodasAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CompraDTO>> GetById(int id)
    {
        var compra = await _compraService.ObtenerPorIdAsync(id);
        if (compra == null)
            return NotFound(new { message = $"Compra con ID {id} no encontrada." });
        return Ok(compra);
    }

    [HttpGet("resumen-mes")]
    public async Task<ActionResult<CompraResumenMesDTO>> GetResumenMes()
    {
        return Ok(await _compraService.ObtenerResumenMesAsync());
    }

    [HttpPost]
    public async Task<ActionResult<CompraDTO>> Create([FromBody] CompraCreateDTO dto)
    {
        try
        {
            var compra = await _compraService.RegistrarCompraAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = compra.Id }, compra);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ActualizarCompraDTO dto)
    {
        try
        {
            await _compraService.ActualizarCompraAsync(id, dto);
            return Ok(new { message = "Compra actualizada" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }
}
