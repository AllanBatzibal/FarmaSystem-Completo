using FarmaSystem.Core.DTOs;
using FarmaSystem.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FarmaSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientesController : ControllerBase
{
    private readonly IClienteService _clienteService;

    public ClientesController(IClienteService clienteService)
    {
        _clienteService = clienteService;
    }

    [HttpGet("test-conexion")]
    public async Task<IActionResult> TestConexion()
    {
        try
        {
            return Ok(await _clienteService.TestConexionAsync());
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClienteDTO>>> GetAll()
    {
        return Ok(await _clienteService.ObtenerTodosAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ClienteDTO>> GetById(int id)
    {
        var cliente = await _clienteService.ObtenerPorIdAsync(id);
        if (cliente == null)
            return NotFound(new { message = $"Cliente con ID {id} no encontrado." });
        return Ok(cliente);
    }

    [HttpPost]
    public async Task<ActionResult<ClienteDTO>> Create([FromBody] ClienteCreateDTO dto)
    {
        try
        {
            var cliente = await _clienteService.CrearAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = cliente.Id }, cliente);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ClienteDTO>> Update(int id, [FromBody] ClienteCreateDTO dto)
    {
        try
        {
            var cliente = await _clienteService.ActualizarAsync(id, dto);
            if (cliente == null)
                return NotFound(new { message = $"Cliente con ID {id} no encontrado." });
            return Ok(cliente);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _clienteService.EliminarAsync(id);
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
