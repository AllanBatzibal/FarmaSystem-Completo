using FarmaSystem.Core.DTOs;
using FarmaSystem.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FarmaSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MedicamentosController : ControllerBase
{
    private readonly IMedicamentoService _medicamentoService;

    public MedicamentosController(IMedicamentoService medicamentoService)
    {
        _medicamentoService = medicamentoService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MedicamentoDTO>>> GetAll()
    {
        return Ok(await _medicamentoService.ObtenerTodosAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MedicamentoDTO>> GetById(int id)
    {
        var item = await _medicamentoService.ObtenerPorIdAsync(id);
        if (item == null)
            return NotFound(new { message = $"Medicamento con ID {id} no encontrado." });
        return Ok(item);
    }

    [HttpGet("stock-critico")]
    public async Task<ActionResult<IEnumerable<MedicamentoDTO>>> GetStockCritico()
    {
        return Ok(await _medicamentoService.ObtenerStockCriticoAsync());
    }

    [HttpGet("categoria/{id}")]
    public async Task<ActionResult<IEnumerable<MedicamentoDTO>>> GetByCategoria(int id)
    {
        return Ok(await _medicamentoService.ObtenerPorCategoriaAsync(id));
    }

    [HttpPost]
    public async Task<ActionResult<MedicamentoDTO>> Create([FromBody] MedicamentoCreateDTO dto)
    {
        try
        {
            var medicamento = await _medicamentoService.CrearAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = medicamento.Id }, medicamento);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<MedicamentoDTO>> Update(int id, [FromBody] MedicamentoCreateDTO dto)
    {
        try
        {
            var medicamento = await _medicamentoService.ActualizarAsync(id, dto);
            if (medicamento == null)
                return NotFound(new { message = $"Medicamento con ID {id} no encontrado." });
            return Ok(medicamento);
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
            await _medicamentoService.EliminarAsync(id);
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
