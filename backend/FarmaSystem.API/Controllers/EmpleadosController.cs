using FarmaSystem.Core.DTOs;
using FarmaSystem.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FarmaSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmpleadosController : ControllerBase
{
    private readonly IEmpleadoService _empleadoService;

    public EmpleadosController(IEmpleadoService empleadoService)
    {
        _empleadoService = empleadoService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmpleadoDTO>>> GetAll()
    {
        return Ok(await _empleadoService.ObtenerTodosAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EmpleadoDTO>> GetById(int id)
    {
        var item = await _empleadoService.ObtenerPorIdAsync(id);
        if (item == null)
            return NotFound(new { message = $"Empleado con ID {id} no encontrado." });
        return Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<EmpleadoDTO>> Create([FromBody] EmpleadoCreateDTO dto)
    {
        try
        {
            var empleado = await _empleadoService.CrearAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = empleado.Id }, empleado);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<EmpleadoDTO>> Update(int id, [FromBody] EmpleadoCreateDTO dto)
    {
        var empleado = await _empleadoService.ActualizarAsync(id, dto);
        if (empleado == null)
            return NotFound(new { message = $"Empleado con ID {id} no encontrado." });
        return Ok(empleado);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _empleadoService.EliminarAsync(id);
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
