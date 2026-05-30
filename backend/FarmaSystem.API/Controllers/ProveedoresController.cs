using FarmaSystem.Core.DTOs;
using FarmaSystem.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FarmaSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProveedoresController : ControllerBase
{
    private readonly IProveedorService _proveedorService;

    public ProveedoresController(IProveedorService proveedorService)
    {
        _proveedorService = proveedorService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProveedorDTO>>> GetAll()
    {
        return Ok(await _proveedorService.ObtenerTodosAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProveedorDTO>> GetById(int id)
    {
        var item = await _proveedorService.ObtenerPorIdAsync(id);
        if (item == null)
            return NotFound(new { message = $"Proveedor con ID {id} no encontrado." });
        return Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<ProveedorDTO>> Create([FromBody] ProveedorCreateDTO dto)
    {
        try
        {
            var proveedor = await _proveedorService.CrearAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = proveedor.Id }, proveedor);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ProveedorDTO>> Update(int id, [FromBody] ProveedorCreateDTO dto)
    {
        var proveedor = await _proveedorService.ActualizarAsync(id, dto);
        if (proveedor == null)
            return NotFound(new { message = $"Proveedor con ID {id} no encontrado." });
        return Ok(proveedor);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _proveedorService.EliminarAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("no encontrado"))
                return NotFound(new { message = ex.Message });
            return BadRequest(new { message = ex.Message });
        }
    }
}
